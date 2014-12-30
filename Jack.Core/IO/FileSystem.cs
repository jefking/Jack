using System;
using System.Collections.Generic;
using System.IO;

using Jack.Core.IO.Data;
using Jack.Core.IO.Storage;

using Jack.Logger;

namespace Jack.Core.IO
{
    /// <summary>
    /// Public interface to storing data
    /// </summary>
    public class FileSystem : IDataOperations
        , ILifetime
        , IGetBlock
    {
        #region Events
        /// <summary>
        /// Manifest Update
        /// </summary>
        internal event EventHandler<FileManifest> ManifestUpdated;
        #endregion

        #region Members
        /// <summary>
        /// File System Singleton
        /// </summary>
        private static readonly FileSystem s_instance;
        /// <summary>
        /// Storage Manager
        /// </summary>
        private StorageManager m_storageManager;
        /// <summary>
        /// Disposed
        /// </summary>
        private bool m_disposed = false;
        /// <summary>
        /// Data Store
        /// </summary>
        private DataStore m_dataStore;
        #endregion

        #region Constructor
        /// <summary>
        /// Static Constructor
        /// </summary>
        static FileSystem()
        {
            using (var log = new TraceContext())
            {
                s_instance = new FileSystem();
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        private FileSystem()
            : base()
        {
            using (var log = new TraceContext())
            {
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get Block
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <returns>Block Data</returns>
        public byte[] GetBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                    , identifier);

                return this.m_storageManager.GetBlock(identifier);
            }
        }
        /// <summary>
        /// Synchronize Block
        /// </summary>
        /// <param name="identifier">Identifier</param>
        internal bool SynchronizeBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                    , identifier);

                bool syched = false;
                if (null != this.m_storageManager
                    && Guid.Empty != identifier)
                {
                    byte[] block = this.m_storageManager.GetBlock(identifier
                        , Location.Remote);

                    if (null != block)
                    {
                        this.m_storageManager.SaveBlock(identifier
                            , block);

                        syched = true;
                    }
                }
                return syched;
            }
        }
        /// <summary>
        /// Add Get Block
        /// </summary>
        /// <param name="getBlock">Get Block</param>
        public void AddGetBlock(IGetBlock getBlock)
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_storageManager
                    && null != getBlock)
                {
                    this.m_storageManager.AddGetBlock(getBlock);
                }
            }
        }
        /// <summary>
        /// Create Version
        /// </summary>
        /// <param name="unc">UNC</param>
        /// <param name="unencryptedFileLength">Unencrypted File Length</param>
        /// <param name="blocks">Blocks</param>
        /// <returns>File Manifest</returns>
        private FileManifest CreateVersion(string unc
            , long unencryptedFileLength
            , IEnumerable<IBlock> blocks)
        {
            using (var log = new TraceContext())
            {
                log.Debug("unc={0},unencryptedFileLength={1},blocks={2}"
                    , unc
                    , unencryptedFileLength
                    , blocks);

                VersionManifest version = new VersionManifest();
                version.Identifier = Guid.NewGuid();
                version.UnencryptedFileLength = unencryptedFileLength;

                foreach (IBlock block in blocks)
                {
                    version.Blocks.Add(block.Identifier);
                }

                FileManifest file = this.m_dataStore.GetManifest(unc);
                if (null == file)
                {
                    file = new FileManifest();
                    file.Identifier = Guid.NewGuid();
                    file.UniversalNamingConvention = unc;
                }

                file.Versions.Push(version);

                this.m_dataStore.Put(file);

                return file;
            }
        }

        #region IDataOperations Members
        /// <summary>
        /// Store Data
        /// </summary>
        /// <param name="unc">UNC</param>
        /// <param name="payload">Pay Load</param>
        public void Store(string unc
            , byte[] payload)
        {
            using (var log = new TraceContext())
            {
                log.Debug("unc={0}"
                    , unc);

                if (payload.LongLength > Constants.MaxFileSize)
                {
                    string error = string.Format("File is {0} long, which exceeds maximum {1}."
                        , payload.LongLength
                        , Constants.MaxFileSize);

                    throw new FileLoadException(error);
                }
                else if (string.IsNullOrEmpty(unc))
                {
                    throw new FileLoadException("No UNC path specified");
                }
                else if (null == payload
                    || 0 == payload.LongLength)
                {
                    throw new FileLoadException("No payload specified"); ;
                }

                IEnumerable<IBlock> blocks = Block.CreateBlocks(payload);

                FileManifest fileManifest = this.CreateVersion(unc
                    , payload.LongLength
                    , blocks);

                foreach (IBlock block in blocks)
                {
                    this.m_storageManager.SaveBlock(block.Identifier
                        , block.Data);
                }

                EventHandler<FileManifest> handler = this.ManifestUpdated;
                if (null != handler)
                {
                    handler(this
                        , new EventArguments<FileManifest>(fileManifest));
                }
            }
        }
        /// <summary>
        /// File Update
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Args</param>
        public void FileUpdate(object sender
            , EventArguments<FileManifest> args)
        {
            using (var log = new TraceContext())
            {
                if (null == args.Data)
                {
                    throw new NullReferenceException("File Manifest is null.");
                }
                else
                {
                    this.m_dataStore.Put(args.Data);
                }
            }
        }
        /// <summary>
        /// Retrieve
        /// </summary>
        /// <param name="unc">UNC</param>
        /// <param name="version">Version</param>
        /// <returns>File</returns>
        public IFile Retrieve(string unc)
        {
            using (var log = new TraceContext())
            {
                log.Info("unc={0}"
                    , unc);

                if (null == this.m_dataStore)
                {
                    throw new ApplicationException("Data store is null.");
                }
                else if (null == this.m_storageManager)
                {
                    throw new ApplicationException("Storage manager is null.");
                }
                else if (string.IsNullOrEmpty(unc))
                {
                    throw new ApplicationException("UNC path is null.");
                }

                FileManifest manifest = this.m_dataStore.GetManifest(unc);
                if (null == manifest)
                {
                    log.Warn("Manifest cannot be found");
                    return null;
                }
                else if (null == manifest.Versions)
                {
                    log.Warn("Manifest versions is null");
                    return null;
                }
                else if (0 == manifest.Versions.Count)
                {
                    log.Warn("Manifest has no versions");
                    return null;
                }
                else
                {
                    return this.m_storageManager.Retrieve(manifest.Versions.Peek());
                }
            }
        }
        #endregion

        #region ILifeTime Members
        /// <summary>
        /// Initializes Storage Manager
        /// </summary>
        public void Initialize()
        {
            using (var log = new TraceContext())
            {
                this.m_dataStore = this.m_dataStore ?? DataStore.Instance;

                this.m_storageManager = this.m_storageManager ?? new StorageManager();
                this.m_storageManager.Initialize();
            }
        }
        /// <summary>
        /// Adds Local Filers to Storage Manager
        /// </summary>
        public void Load()
        {
            using (var log = new TraceContext())
            {
                //Should add a Local storage class for each disk
                this.m_storageManager.AddFiler(new Local());

                this.m_storageManager.AddFiler(new Memory());
            }
        }
        /// <summary>
        /// Unload Service
        /// </summary>
        public void Unload()
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_storageManager)
                {
                    this.m_storageManager.Dispose();
                    this.m_storageManager = null;
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
                this.Dispose(true);

                GC.SuppressFinalize(this);
            }
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            using (var log = new TraceContext())
            {
                log.Info("disposing={0}"
                    , disposing);
                if (!(this.m_disposed))
                {
                    if (disposing)
                    {
                        this.ManifestUpdated = null;

                        if (null != this.m_storageManager)
                        {
                            this.m_storageManager = null;
                        }
                        this.m_disposed = true;
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// File System Singleton
        /// </summary>
        public static FileSystem Instance
        {
            get
            {
                return s_instance;
            }
        }
        /// <summary>
        /// Local Store Loaded
        /// </summary>
        internal bool LocalStoreLoaded
        {
            get
            {
                return null != this.m_storageManager
                    && this.m_storageManager.LocalStoreLoaded;
            }
        }
        /// <summary>
        /// Local Stored Block Identifiers
        /// </summary>
        internal HashSet<Guid> LocalStoredBlockIdentifiers
        {
            get
            {
                return new HashSet<Guid>(this.m_storageManager.StoredBlockIdentifiers);
            }
        }
        /// <summary>
        /// Identifier
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return this.m_storageManager.Identifier;
            }
        }
        #endregion
    }
}