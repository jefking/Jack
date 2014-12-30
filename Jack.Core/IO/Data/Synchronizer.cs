using System;
using System.Collections.Generic;
using System.Threading;

using Jack.Core.XML;

using Jack.Logger;

namespace Jack.Core.IO.Data
{
    /// <summary>
    /// Synchronizer
    /// </summary>
    internal class Synchronizer : ILifetime
    {
        #region Members
        /// <summary>
        /// Download Timer
        /// </summary>
        private Timer m_downloadTimer;
        /// <summary>
        /// Download Frequency
        /// </summary>
        private static readonly TimeSpan s_downloadFrequency = new TimeSpan(0, 0, 5);
        /// <summary>
        /// Storage Checker
        /// </summary>
        private Timer m_storageChecker;
        /// <summary>
        /// Download Frequency
        /// </summary>
        private static readonly TimeSpan s_checkFrequency = new TimeSpan(0, 1, 0);
        /// <summary>
        /// Not Yet Synchronized
        /// </summary>
        private Stack<Guid> m_notYetSychronized;
        /// <summary>
        /// Not Yet Synchronized Locker
        /// </summary>
        private readonly object m_notYetSychronizedLocker = new object();
        #endregion

        #region Constructors
        /// <summary>
        /// Synchronizer
        /// </summary>
        public Synchronizer()
            : base()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Check Store
        /// </summary>
        /// <param name="state">State</param>
        private void CheckStore(object state)
        {
            using (var log = new TraceContext())
            {
                log.Debug("state={0}"
                    , state);

                FileSystem fs = FileSystem.Instance;
                if (fs.LocalStoreLoaded)
                {
                    HashSet<Guid> localStore = fs.LocalStoredBlockIdentifiers;
                    ManifestXml xml = ManifestXml.Instance;
                    IList<FileManifest> files = xml.ReadAll();
                    foreach (FileManifest fm in files)
                    {
                        foreach (VersionManifest vm in fm.Versions)
                        {
                            foreach (Guid identifier in vm.Blocks)
                            {
                                if (!(localStore.Contains(identifier))
                                    && null != this.m_notYetSychronized
                                    && (!this.m_notYetSychronized.Contains(identifier)))
                                {
                                    log.Debug("identifier={0}"
                                        , identifier);

                                    lock (this.m_notYetSychronizedLocker)
                                    {
                                        this.m_notYetSychronized.Push(identifier);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Download Block
        /// </summary>
        /// <param name="state">State</param>
        private void DownloadBlock(object state)
        {
            using (var log = new TraceContext())
            {
                log.Debug("UnSynched={0},state={1}"
                    , (null != this.m_notYetSychronized) ? this.m_notYetSychronized.Count : 0
                    , state);

                if (null != this.m_notYetSychronized
                    && 0 < this.m_notYetSychronized.Count)
                {
                    Guid identifier = Guid.Empty;
                    lock (this.m_notYetSychronizedLocker)
                    {
                        identifier = this.m_notYetSychronized.Pop();

                        if (identifier != Guid.Empty)
                        {
                            log.Debug("Synch'ing Identifier={0}"
                                , identifier);

                            FileSystem fs = FileSystem.Instance;
                            if (!(fs.SynchronizeBlock(identifier)))
                            {
                                log.Warn("Re-Adding Block; unable to retrieve from any remote store");
                                this.m_notYetSychronized.Push(identifier);
                            }
                        }
                    }
                }
            }
        }

        #region ILifetime Members
        /// <summary>
        /// Load
        /// </summary>
        public void Load()
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_downloadTimer)
                {
                    this.m_downloadTimer.Change(TimeSpan.Zero
                        , s_downloadFrequency);
                }
                if (null != this.m_downloadTimer)
                {
                    this.m_storageChecker.Change(TimeSpan.Zero
                       , s_checkFrequency);
                }
            }
        }
        /// <summary>
        /// Unload
        /// </summary>
        public void Unload()
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_downloadTimer)
                {
                    this.m_downloadTimer.Change(0
                        , Timeout.Infinite);
                }
                if (null != this.m_downloadTimer)
                {
                    this.m_storageChecker.Change(0
                        , Timeout.Infinite);
                }
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            using (var log = new TraceContext())
            {
                this.Unload();

                this.m_downloadTimer = null;

                this.m_storageChecker = null;

                lock (this.m_notYetSychronizedLocker)
                {
                    this.m_notYetSychronized = null;
                }
            }
        }
        #endregion

        #region IInitialize Members
        /// <summary>
        /// Initialize
        /// </summary>
        public void Initialize()
        {
            using (var log = new TraceContext())
            {
                this.m_notYetSychronized = this.m_notYetSychronized ?? new Stack<Guid>(100);

                this.m_downloadTimer = this.m_downloadTimer ?? new Timer(this.DownloadBlock);

                this.m_storageChecker = this.m_storageChecker ?? new Timer(this.CheckStore);
            }
        }
        #endregion
        #endregion
    }
}