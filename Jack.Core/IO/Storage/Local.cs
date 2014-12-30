using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

using Jack.Core.Metric;
using Jack.Core.Threading;
using Jack.Logger;

namespace Jack.Core.IO.Storage
{
    /// <summary>
    /// Local
    /// </summary>
    public class Local : IFiler
        , ILatency
        , IInitialize
        , ILoad
    {
        #region Members
        /// <summary>
        /// Storage Sub-System
        /// </summary>
        private const string c_file = "storage.dat";
        /// <summary>
        /// Full File Path
        /// </summary>
        private static readonly string s_fullFilePath;
        /// <summary>
        /// Lock object for Block Identifiers
        /// </summary>
        private readonly object m_blockIdentifiersLock = new object();
        /// <summary>
        /// File Object Lock
        /// </summary>
        private readonly object m_fileAccessLock = new object();
        /// <summary>
        /// Store Identifier
        /// </summary>
        private readonly Guid m_storeIdentifier;
        /// <summary>
        /// IO Operation Duration
        /// </summary>
        private readonly TimeQueue m_fileOperationDurations;
        /// <summary>
        /// Block Identifiers
        /// </summary>
        private IDictionary<Guid, long> m_blocks;
        /// <summary>
        /// Empty Slots, fragmented file
        /// </summary>
        private Stack<long> m_emptySlots;
        /// <summary>
        /// File Loader
        /// </summary>
        private Executor m_fileLoader;
        /// <summary>
        /// Disposed
        /// </summary>
        private bool m_disposed;
        #endregion

        #region Constructors
        /// <summary>
        /// Local
        /// </summary>
        static Local()
        {
            using (var log = new TraceContext())
            {
                string path = string.Format("{0}\\"
                    , FileHelper.BinDirectory);

                FileHelper.EnsureFolderExists(path);

                s_fullFilePath = string.Format("{0}{1}"
                    , path
                    , c_file);

                log.Debug("path={0},s_fullFilePath={1}"
                    , path
                    , s_fullFilePath);

                Local.InitializeStorage(s_fullFilePath);
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Local()
            : base()
        {
            using (var log = new TraceContext())
            {
                this.m_storeIdentifier = Guid.NewGuid();

                this.m_emptySlots = new Stack<long>();

                this.m_fileOperationDurations = new TimeQueue();

                log.Debug("m_storeIdentifier={0},m_fileOperationDurations={1},m_emptySlots={2}"
                    , this.m_storeIdentifier
                    , this.m_fileOperationDurations
                    , this.m_emptySlots);

                this.Initialize();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize Storage
        /// </summary>
        private static void InitializeStorage(string file)
        {
            using (var log = new TraceContext())
            {
                log.Debug("file={0}"
                    , file);

                if (System.IO.File.Exists(file))
                {
                    log.Debug("Storage file already exists: {0}"
                        , file);
                }
                else
                {
                    log.Debug("Storage file doesn't already exist, creating: {0}"
                        , file);

                    using (FileStream fs = System.IO.File.Create(file))
                    {
                        //Security etc?
                    }
                }
            }
        }
        /// <summary>
        /// Initialize Class
        /// </summary>
        public void Initialize()
        {
            using (var log = new TraceContext())
            {
                this.m_fileLoader = new Executor(this.LoadFiles
                    , this.LoadFilesCompleted);

                Executor.Run(this.m_fileLoader);

                log.Debug("m_fileLoader={0}"
                    , this.m_fileLoader);
            }
        }
        /// <summary>
        /// Load Files
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        private void LoadFiles(object sender
            , DoWorkEventArgs e)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;
                IDictionary<Guid, long> blocks = new Dictionary<Guid, long>();
                byte[] guid = Cloneable.Guid.Clone() as byte[];
                Guid g;
                FileInfo fi = new FileInfo(s_fullFilePath);
                long length = fi.Length;
                if (0 < length)
                {
                    for (long offset = 0; offset < length; offset += Constants.StorageBlockSize)
                    {
                        g = new Guid(this.ReadIdentifier(offset));
                        if (g == Guid.Empty)
                        {
                            this.m_emptySlots.Push(offset);
                            log.Debug("Fragment at;offset={0}"
                                , offset);
                        }
                        else
                        {
                            blocks.Add(g
                                , offset);
                        }

                        log.Debug("offset={0}"
                            , offset);
                    }

                    this.m_fileOperationDurations.AddTime(startCall);
                }
                else
                {
                    log.Warn("Empty storage file;nothing to load.");
                }

                if (0 < blocks.Count)
                {
                    e.Result = blocks;
                }
            }
        }
        /// <summary>
        /// Load Files Completed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        private void LoadFilesCompleted(object sender
            , RunWorkerCompletedEventArgs e)
        {
            using (var log = new TraceContext())
            {
                this.m_blocks = e.Result as IDictionary<Guid, long>;
                if (null == this.m_blocks)
                {
                    FileInfo fi = new FileInfo(s_fullFilePath);
                    int initBlocks = 100;
                    if (0 < fi.Length)
                    {
                        initBlocks += (int)(fi.Length / Constants.StorageBlockSize);
                    }
                    this.m_blocks = new Dictionary<Guid, long>(initBlocks);
                }
            }
        }

        #region ILatency Members
        /// <summary>
        /// Determine Latency
        /// </summary>
        /// <param name="type">Latency Type</param>
        /// <returns>Time Span</returns>
        public TimeSpan Determine(LatencyType type)
        {
            using (var log = new TraceContext())
            {
                log.Debug("type={0}"
                    , type);

                switch (type)
                {
                    case LatencyType.Storage:
                        return this.m_fileOperationDurations.Average;
                    default:
                        log.Warn("Invalid");
                        return TimeSpan.Zero;
                }
            }
        }
        #endregion

        #region IFiler Members
        /// <summary>
        /// Get Block from hard disk
        /// </summary>
        /// <remarks>
        /// -Expect Null Back when not stored
        /// -Will sleep calling thread if store hasn't been loaded
        /// </remarks>
        /// <param name="identifier">Identifier</param>
        /// <returns>Payload</returns>
        public byte[] GetBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;

                log.Debug("identifier={0},startCall={1}"
                    , identifier
                    , startCall);

                if (Guid.Empty == identifier)
                {
                    log.Error("identifier is empty");
                    throw new InvalidOperationException();
                }
                else
                {
                    this.WaitUntilLocalStoreIsLoaded();

                    bool exists = false;
                    lock (this.m_blockIdentifiersLock)
                    {
                        exists = this.m_blocks.ContainsKey(identifier);
                    }

                    byte[] payload = null;
                    if (exists)
                    {
                        long offset = this.m_blocks[identifier] + Constants.GuidLength;

                        payload = this.ReadBlock(offset);

                        if (null != payload)
                        {
                            this.m_fileOperationDurations.AddTime(startCall);
                        }
                        else
                        {
                            log.Error("Read Block Failed;offset={0},identifier={1}"
                                , offset
                                , identifier);
                        }
                    }
                    else
                    {
                        log.Error("Guid doesn't exist, identifier={0}"
                            , identifier);
                    }

                    return payload;
                }
            }
        }
        /// <summary>
        /// Wait Until Local Store is Loaded
        /// </summary>
        /// <remarks>
        /// Sleeps current thread to ensure that actions take place at correct time
        /// -This is a blocking call
        /// -Sleeps executing thread for 100 miliseconds
        /// </remarks>
        private void WaitUntilLocalStoreIsLoaded()
        {
            using (var log = new TraceContext())
            {
                while (this.m_fileLoader.LoadingState == LoadingState.Loading)
                {
                    log.Debug("Sleeping thread while local store loads.");
                    Thread.Sleep(100);
                }
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
                DateTime startCall = DateTime.Now;

                log.Debug("identifier={0},startCall={1}"
                    , identifier
                    , startCall);

                if (Guid.Empty == identifier)
                {
                    log.Error("identifier is empty");
                    throw new InvalidOperationException();
                }
                else if (null == block)
                {
                    log.Error("block is null");
                    throw new InvalidOperationException();
                }
                else if (Constants.BlockSize != block.LongLength)
                {
                    log.Error("block is wrong length");
                    throw new InvalidOperationException();
                }
                else if (null == this.m_blocks)
                {
                    //Before blocks can be interacted with; we have to ensure they are loaded.
                    this.WaitUntilLocalStoreIsLoaded();
                }
                //--^
                if (this.m_blocks.Count + 1 * Constants.StorageBlockSize > Constants.MaxFileSize)
                {
                    log.Error("Unable to save block to store: store is full.");
                    throw new InvalidOperationException();
                }
                else
                {
                    bool exists = false;
                    lock (this.m_blockIdentifiersLock)
                    {
                        exists = this.m_blocks.ContainsKey(identifier);
                    }

                    if (exists)
                    {
                        log.Warn("block already exists in storage; not storing");
                    }
                    else
                    {
                        byte[] toStore = Cloneable.StorageBlock.Clone() as byte[];

                        Array.Copy(identifier.ToByteArray()
                            , toStore
                            , Constants.GuidLength);

                        Array.Copy(block
                            , 0
                            , toStore
                            , Constants.GuidLength
                            , Constants.BlockSize);

                        long index = long.MinValue;
                        FileMode fileMode = FileMode.Append;
                        if (null != this.m_emptySlots
                            && this.m_emptySlots.Count > 0)
                        {
                            index = this.m_emptySlots.Pop();
                            fileMode = FileMode.Open;
                        }

                        bool success = this.WriteBlock(ref index
                            , fileMode
                            , toStore);

                        if (success)
                        {
                            lock (this.m_blockIdentifiersLock)
                            {
                                this.m_blocks.Add(identifier
                                    , index);
                            }

                            this.m_fileOperationDurations.AddTime(startCall);
                        }
                        else
                        {
                            if (long.MinValue != index)
                            {
                                //Re-Add Index
                                this.m_emptySlots.Push(index);
                            }

                            log.Error("Save Block Failed;index={0},identifier={1},fileMode={2}"
                                , index
                                , identifier
                                , fileMode);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Delete Block
        /// </summary>
        /// <remarks>
        /// Writes empty block to deleted item
        /// this needs a clean up routine (Defragment)
        /// </remarks>
        /// <param name="identifier">Identifier</param>
        public void DeleteBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;

                log.Debug("identifier={0},startCall={1}"
                    , identifier
                    , startCall);

                if (Guid.Empty == identifier)
                {
                    log.Error("identifier is empty");
                    throw new InvalidOperationException();
                }
                else
                {
                    this.WaitUntilLocalStoreIsLoaded();

                    bool exists = false;
                    lock (this.m_blockIdentifiersLock)
                    {
                        exists = this.m_blocks.ContainsKey(identifier);
                    }

                    if (exists)
                    {
                        lock (this.m_blockIdentifiersLock)
                        {
                            long index = this.m_blocks[identifier];

                            bool success = this.WriteBlock(ref index
                                , FileMode.Open
                                , Cloneable.StorageBlock.Clone() as byte[]);

                            if (success)
                            {
                                this.m_blocks.Remove(identifier);

                                this.m_emptySlots.Push(index);

                                this.m_fileOperationDurations.AddTime(startCall);
                            }
                            else
                            {
                                log.Error("Delete Block Failed;index={0},identifier={1}"
                                    , index
                                    , identifier);
                            }
                        }
                    }
                    else
                    {
                        log.Warn("Identifier isn't in store;identifier={0}"
                            , identifier);
                    }
                }
            }
        }
        /// <summary>
        /// Write Block to Disk
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="fileMode">File Mode</param>
        /// <param name="payload">Payload</param>
        private bool WriteBlock(ref long index
            , FileMode fileMode
            , byte[] payload)
        {
            using (var log = new TraceContext())
            {
                bool success = false;

                log.Debug("index={0},fileMode={1}"
                    , index
                    , fileMode);

                lock (this.m_fileAccessLock)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(s_fullFilePath
                            , fileMode))
                        {
                            if (long.MinValue != index)
                            {
                                fs.Position = index;
                            }
                            else
                            {
                                index = fs.Length;
                            }

                            fs.Write(payload
                                , 0
                                , Constants.StorageBlockSize);

                            success = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
                return success;
            }
        }
        /// <summary>
        /// Read Block
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Block Payload</returns>
        private byte[] ReadBlock(long offset)
        {
            using (var log = new TraceContext())
            {
                return this.Read(offset
                    , Cloneable.Block.Clone() as byte[]);
            }
        }
        /// <summary>
        /// Read Identifier
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Identifier Payload</returns>
        private byte[] ReadIdentifier(long offset)
        {
            using (var log = new TraceContext())
            {
                return this.Read(offset
                    , Cloneable.Guid.Clone() as byte[]);
            }
        }
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="payload">Payload</param>
        /// <returns>Bytes</returns>
        private byte[] Read(long offset
            , byte[] payload)
        {
            using (var log = new TraceContext())
            {
                log.Debug("offset={0}"
                    , offset);

                if (offset < 0)
                {
                    log.Warn("Offset less than 0;offset={0}"
                        , offset);

                    return null;
                }
                else
                {
                    lock (this.m_fileAccessLock)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(s_fullFilePath
                                , FileMode.Open))
                            {
                                //Seek to location
                                fs.Seek(offset
                                    , SeekOrigin.Begin);

                                //Read Data
                                fs.Read(payload
                                    , 0
                                    , payload.Length);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                            payload = null;
                        }
                    }
                    return payload;
                }
            }
        }

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
                log.Debug("disposing={0}"
                    , disposing);

                if (!(this.m_disposed))
                {
                    if (disposing)
                    {
                        if (null != this.m_fileLoader)
                        {
                            Executor.Cancel(this.m_fileLoader);
                            this.m_fileLoader.Dispose();
                            this.m_fileLoader = null;
                        }

                        if (null != this.m_blocks)
                        {
                            this.m_blocks = null;
                        }

                        if (null != this.m_emptySlots)
                        {
                            this.m_emptySlots = null;
                        }

                        this.m_disposed = true;
                    }
                }
            }
        }
        #endregion
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Load State
        /// </summary>
        public LoadingState LoadState
        {
            get
            {
                return (null == this.m_fileLoader)
                    ? LoadingState.NotLoaded
                    : this.m_fileLoader.LoadingState;
            }
        }
        /// <summary>
        /// Client Id
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return this.m_storeIdentifier;
            }
        }
        /// <summary>
        /// Identifiers
        /// </summary>
        public IEnumerable<Guid> Identifiers
        {
            get
            {
                lock (this.m_blockIdentifiersLock)
                {
                    return this.m_blocks.Keys;
                }
            }
        }
        #endregion
    }
}