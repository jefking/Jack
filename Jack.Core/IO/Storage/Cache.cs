using System;
using System.Collections.Generic;

namespace Jack.Core.IO.Storage
{
    /// <summary>
    /// For plaintext versions of files currently in use
    /// </summary>
    internal class Cache : IDisposable
    {
        #region Classes
        /// <summary>
        /// Cache Activity
        /// </summary>
        internal class CacheActivity
        {
            public Guid FileID;
            public long Version;
            public TimeSpan TimeInCache;
        }
        /// <summary>
        /// Entry
        /// </summary>
        internal class Entry
        {
            public Guid FileID;
            public long Version;
            public byte[] Data;
            /// <summary>
            /// When added to cache
            /// </summary>
            public DateTime Created;
        }
        #endregion

        #region Events
        /// <summary>
        /// Cache Replacement Event
        /// </summary>
        public event EventHandler<CacheActivity> CacheReplacement;
        #endregion

        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly object m_filesMutex = new object();
        /// <summary>
        /// Files
        /// </summary>
        private readonly Dictionary<Guid, Entry> m_files = new Dictionary<Guid,Entry>();
        #endregion

        #region Methods
        /// <summary>
        /// Put Entry
        /// </summary>
        /// <param name="entry">Entry</param>
        public void Put(Entry entry)
        {
            Entry old = null;
            entry.Created = DateTime.Now;

            lock(this.m_filesMutex)
            {
                if (this.m_files.ContainsKey(entry.FileID))
                {
                    old = this.m_files[entry.FileID];
                }
                this.m_files[entry.FileID] = entry;
            }

            if (old != null) //raise event
            {
                var a = new CacheActivity 
                {
                    FileID = old.FileID,
                    Version = old.Version,
                    TimeInCache = DateTime.Now - old.Created
                };

                EventHandler<CacheActivity> handler = this.CacheReplacement;
                if (handler != null)
                {
                    handler(this
                        , new EventArguments<CacheActivity>(a));
                }
             }
        }

        #region IDisposable Members
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.CacheReplacement = null;
        }
        #endregion
        #endregion
    }
}