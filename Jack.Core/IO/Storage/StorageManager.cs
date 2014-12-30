using System;
using System.Collections.Generic;

using Jack.Core.Threading;

using Jack.Logger;

namespace Jack.Core.IO.Storage
{
    /// <summary>
    /// Storage Manager
    /// </summary>
    internal class StorageManager : IDisposable
        , IFiler
        , IInitialize
    {
        #region Members
        /// <summary>
        /// Unique Identifier
        /// </summary>
        private readonly Guid m_identifier;
        /// <summary>
        /// Local Filers
        /// </summary>
        private IDictionary<Guid, IFiler> m_localFilers;
        /// <summary>
        /// Local Lock
        /// </summary>
        private readonly object m_localLock = new object();
        /// <summary>
        /// Remote Filers
        /// </summary>
        private IDictionary<Guid, IGetBlock> m_remoteStores;
        /// <summary>
        /// Remote Lock
        /// </summary>
        private readonly object m_remoteLock = new object();
        /// <summary>
        /// Disposed
        /// </summary>
        private bool m_disposed = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public StorageManager()
            : base()
        {
            using (var log = new TraceContext())
            {
                this.m_identifier = Guid.NewGuid();

                log.Debug("m_identifier={0},disposed={1}"
                    , this.m_identifier
                    , this.m_disposed);
            }
        }
        #endregion

        #region Methods
        #region IInitialize Members
        /// <summary>
        /// Initialize
        /// </summary>
        public void Initialize()
        {
            using (var log = new TraceContext())
            {
                lock (this.m_localLock)
                {
                    if (null == this.m_localFilers)
                    {
                        this.m_localFilers = new Dictionary<Guid, IFiler>(2);//Set capacity to disk/memory
                    }
                }

                lock (this.m_remoteLock)
                {
                    if (null == this.m_remoteStores)
                    {
                        this.m_remoteStores = new Dictionary<Guid, IGetBlock>(3);//Set capacity to minimum
                    }
                }
            }
        }
        #endregion

        #region IDataOperations Members
        /// <summary>
        /// Retrieve a file
        /// </summary>
        public IFile Retrieve(VersionManifest manifest)
        {
            using (var log = new TraceContext())
            {
                log.Debug("manifest={0}"
                    , manifest);

                Block block;
                File file = new File();
                foreach (Guid blockId in manifest.Blocks)
                {
                    block = new Block();
                    block.Identifier = blockId;
                    block.Data = this.GetBlock(blockId);
                    file.AddBlock(block);
                }

                long length = (long)manifest.UnencryptedFileLength % Constants.BlockSize;
                if (0 < length) // 0 Means full last block
                {
                    file.TrimLastBlock(length);
                }
                return file;
            }
        }
        /// <summary>
        /// Add Filer
        /// </summary>
        /// <param name="filer">Filer</param>
        public void AddFiler(IFiler filer)
        {
            using (var log = new TraceContext())
            {
                log.Debug("filer={0}"
                    , filer);

                lock (this.m_localLock)
                {
                    if (!(this.m_localFilers.ContainsKey(filer.Identifier)))
                    {
                        this.m_localFilers.Add(filer.Identifier
                            , filer);
                    }
                }
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
                log.Debug("getBlock={0}"
                    , getBlock);

                lock (this.m_remoteLock)
                {
                    if (!(this.m_remoteStores.ContainsKey(getBlock.Identifier)))
                    {
                        this.m_remoteStores.Add(getBlock.Identifier
                            , getBlock);
                    }
                }
            }
        }
        /// <summary>
        /// Remove Filer
        /// </summary>
        /// <param name="storeId">Store Identifier</param>
        public void Remove(IUnique<Guid> unique)
        {
            using (var log = new TraceContext())
            {
                log.Debug("unique={0}"
                    , unique);

                bool removed = false;
                lock (this.m_localLock)
                {
                    if (!(this.m_localFilers.ContainsKey(unique.Identifier)))
                    {
                        this.m_localFilers.Remove(unique.Identifier);
                        removed = true;
                    }
                }

                if (!(removed))
                {
                    lock (this.m_remoteLock)
                    {
                        if (!(this.m_remoteStores.ContainsKey(unique.Identifier)))
                        {
                            this.m_remoteStores.Remove(unique.Identifier);
                        }
                    }
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
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!(this.m_disposed))
            {
                if (disposing)
                {
                    foreach (IDisposable disposable in this.m_localFilers.Values)
                    {
                        if (null != disposable)
                        {
                            disposable.Dispose();
                        }
                    }

                    foreach (IDisposable disposable in this.m_remoteStores.Values)
                    {
                        if (null != disposable)
                        {
                            disposable.Dispose();
                        }
                    }

                    this.m_disposed = true;
                }
            }
        }
        #endregion

        /// <summary>
        /// Get Block
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="location">Location</param>
        /// <returns>Block</returns>
        public byte[] GetBlock(Guid identifier
            , Location location)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0},location={1}"
                    , identifier
                    , location);

                byte[] block = null;
                if (location == Location.Any
                    || location == Location.Local)
                {
                    //Order By Latency
                    foreach (IFiler filer in this.m_localFilers.Values)
                    {
                        block = filer.GetBlock(identifier);
                        if (null != block)
                        {
                            return block;
                        }
                    }
                }

                if (location == Location.Any
                    || location == Location.Remote)
                {
                    //Order By Latency
                    foreach (IGetBlock getBlock in this.m_remoteStores.Values)
                    {
                        block = getBlock.GetBlock(identifier);
                        if (null != block)
                        {
                            return block;
                        }
                    }
                }
                return null;
            }
        }

        #region IFiler Members
        /// <summary>
        /// Get Block
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Block</returns>
        public byte[] GetBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                    , identifier);

                return this.GetBlock(identifier
                    , Location.Any);
            }
        }
        /// <summary>
        /// Save Block
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="block">Block</param>
        public void SaveBlock(Guid identifier
            , byte[] block)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                    , identifier);

                foreach (IFiler filer in this.m_localFilers.Values)
                {
                    filer.SaveBlock(identifier
                        , block);
                }
            }
        }
        /// <summary>
        /// Delete Block
        /// </summary>
        /// <param name="identifier">Identifier</param>
        public void DeleteBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                    , identifier);

                foreach (IFiler filer in this.m_localFilers.Values)
                {
                    filer.DeleteBlock(identifier);
                }
            }
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Identifiers
        /// </summary>
        public IEnumerable<Guid> Identifiers
        {
            get
            {
                List<Guid> ids = new List<Guid>(this.m_localFilers.Count + this.m_remoteStores.Values.Count);
                ids.AddRange(this.m_localFilers.Keys);
                ids.AddRange(this.m_remoteStores.Keys);
                return ids;
            }
        }
        /// <summary>
        /// Identifiers
        /// </summary>
        public ICollection<Guid> StoredBlockIdentifiers
        {
            get
            {
                List<Guid> identifiers = new List<Guid>();
                foreach (IFiler filer in this.m_localFilers.Values)
                {
                    identifiers.AddRange(filer.Identifiers);
                }
                return identifiers;
            }
        }
        /// <summary>
        /// Local Store Loaded
        /// </summary>
        internal bool LocalStoreLoaded
        {
            get
            {
                bool isLoaded = false;
                if (null != this.m_localFilers)
                {
                    isLoaded = true;
                    ILoad load;
                    foreach (object obj in this.m_localFilers.Values)
                    {
                        load = obj as ILoad;
                        if (null != load)
                        {
                            isLoaded &= load.LoadState == LoadingState.Loaded;
                        }
                    }
                }
                return isLoaded;
            }
        }

        #region IUnique Members
        /// <summary>
        /// Identifier
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return this.m_identifier;
            }
        }
        #endregion
        #endregion
    }
}