using Jack.Core.Communication;
using Jack.Core.IO;
using Jack.Core.IO.Data;

using Jack.Logger;

namespace Jack.Core.Windows.Services
{
    /// <summary>
    /// Jack Service
    /// </summary>
    public class JackService : Service
        , ILifetime
    {
        #region Members
        /// <summary>
        /// Port
        /// </summary>
        private readonly short m_port;
        /// <summary>
        /// RPC Server
        /// </summary>
        private ILifetime m_server;
        /// <summary>
        /// File System
        /// </summary>
        private FileSystem m_fileSystem;
        /// <summary>
        /// Peers
        /// </summary>
        private Peers m_peers;
        /// <summary>
        /// Synchronizer
        /// </summary>
        private ILifetime m_synchronizer;
        #endregion

        #region Constructor
        /// <summary>
        /// Jack Service
        /// </summary>
        public JackService(short port)
            : base(Common.ServiceName)
        {
            using (var log = new TraceContext())
            {
                this.m_port = port;

                log.Debug("m_port={0}"
                    , this.m_port);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// When Service Is Started
        /// </summary>
        protected override void OnStart(string[] args)
        {
            using (var log = new TraceContext())
            {
                this.Initialize();
                this.Load();
            }
        }
        /// <summary>
        /// When Service is Stopped
        /// </summary>
        /// <remarks>
        /// Unwinds from On Start
        /// </remarks>
        protected override void OnStop()
        {
            using (var log = new TraceContext())
            {
                this.Unload();
            }
        }

        #region ILifeTime Members
        /// <summary>
        /// Initializes all corresponding services
        /// </summary>
        public void Initialize()
        {
            using (var log = new TraceContext())
            {
                //Get File System Ready
                if (null == this.m_fileSystem)
                {
                    this.m_fileSystem = FileSystem.Instance;
                    this.m_fileSystem.Initialize();
                    this.m_fileSystem.ManifestUpdated += this.FileUpdate;
                }

                //Get Server Ready
                this.m_server = this.m_server ?? new RPCServer(this.m_port);
                this.m_server.Initialize();

                //Accept Peer Activity
                if (null == this.m_peers)
                {
                    this.m_peers = Peers.Instance;
                    this.m_peers.Initialize();

                    // this.m_peers.ProxyConnected += this.ProxyConnected;
                    this.m_peers.RemoteStoreConnected += this.RemoteStoreConnected;
                }

                this.m_synchronizer = new Synchronizer();
                this.m_synchronizer.Initialize();
            }
        }
        /// <summary>
        /// Remote Store Connected
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Get Block Event Arguments</param>
        private void RemoteStoreConnected(object sender
            , EventArguments<IGetBlock> args)
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_fileSystem
                    && null != args.Data)
                {
                    this.m_fileSystem.AddGetBlock(args.Data);
                }
            }
        }
        /// <summary>
        /// File Update
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">File Manifest Event Arguments</param>
        private void FileUpdate(object sender
            , EventArguments<FileManifest> args)
        {
            if (null != this.m_peers
                && null != args.Data)
            {
                this.m_peers.Push(args.Data);
            }
        }
        /// <summary>
        /// Loads Corresponding Services
        /// </summary>
        public void Load()
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_fileSystem)
                {
                    //Load File System
                    this.m_fileSystem.Load();
                }

                if (null != this.m_server)
                {
                    //Start Serving, Ensures manifest type is registered
                    this.m_server.Load();
                }

                if (null != this.m_peers)
                {
                    //Accept Client Requests
                    this.m_peers.Load();
                }

                if (null != this.m_synchronizer)
                {
                    //Check Storage
                    this.m_synchronizer.Load();
                }
            }
        }
        /// <summary>
        /// Unloads services
        /// </summary>
        public void Unload()
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_synchronizer)
                {
                    //Unload Storage
                    this.m_synchronizer.Unload();
                }

                if (null != this.m_peers)
                {
                    //Drop Peers
                    this.m_peers.Unload();
                }

                if (null != this.m_server)
                {
                    //Stop Serving
                    this.m_server.Unload();
                }

                if (null != this.m_fileSystem)
                {
                    //Disconnect File System
                    this.m_fileSystem.Unload();
                }
            }
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Cleanly Disposes of Objects
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected override void Dispose(bool disposing)
        {
            using (var log = new TraceContext())
            {
                log.Debug("disposing={0}"
                    , disposing);
                if (disposing)
                {
                    if (null != this.m_synchronizer)
                    {
                        this.m_synchronizer.Dispose();
                        this.m_synchronizer = null;
                    }

                    if (null != this.m_peers)
                    {
                        this.m_peers.RemoteStoreConnected -= this.RemoteStoreConnected;

                        this.m_peers.Dispose();
                        this.m_peers = null;
                    }

                    if (null != this.m_server)
                    {
                        this.m_server.Dispose();
                        this.m_server = null;
                    }

                    if (null != this.m_fileSystem)
                    {
                        this.m_fileSystem.Dispose();
                        this.m_fileSystem = null;
                    }

                    System.GC.SuppressFinalize(this);
                }

                base.Dispose(disposing);
            }
        }
        #endregion
        #endregion
    }
}