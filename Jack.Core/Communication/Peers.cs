using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using Jack.Core.Configuration;
using Jack.Core.IO;
using Jack.Core.Threading;

using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Peers
    /// </summary>
    internal class Peers : ILifetime
    {
        #region Events
        /// <summary>
        /// Occurs When a Proxy is Connected
        /// </summary>
        internal event EventHandlerNoReturn<Client> ProxyConnected;
        /// <summary>
        /// Remote Store Connected
        /// </summary>
        internal event EventHandler<IGetBlock> RemoteStoreConnected;
        #endregion

        #region Members
        /// <summary>
        /// Peers Instance
        /// </summary>
        private static Peers s_peers;
        /// <summary>
        /// Proxy Objects
        /// </summary>
        private IDictionary<Guid, Client> m_proxy;
        /// <summary>
        /// Proxy Lock
        /// </summary>
        private readonly object m_proxyLock = new object();
        /// <summary>
        /// Discovery Thread
        /// </summary>
        private Executor m_discovery;
        #endregion

        #region Constructors
        /// <summary>
        /// Static Constructor
        /// </summary>
        static Peers()
        {
            using (var log = new TraceContext())
            {
                s_peers = new Peers();
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        private Peers()
            : base()
        {
            using (var log = new TraceContext())
            {
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Push manifest to all clients
        /// </summary>
        /// <param name="manifest">File Manifest</param>
        public void Push(FileManifest manifest)
        {
            using (var log = new TraceContext())
            {
                log.Debug("manifest={0}"
                    , manifest);

                lock (m_proxyLock)
                {
                    foreach (Client client in this.m_proxy.Values)
                    {
                        client.Manifest.Push(manifest);
                    }
                }
            }
        }
        /// <summary>
        /// Add Client
        /// </summary>
        /// <param name="proxy">Client</param>
        public void InitClient(Client proxy)
        {
            using (var log = new TraceContext())
            {
                log.Debug("proxy={0}"
                    , proxy);

                proxy.RemoteStoreConnected += this.ExecuteRemoteStoreConnected;

                proxy.Initialize();

                proxy.Load();

                //Should we leave connected?
                //If we drop and reconnect?
                proxy.RemoteStoreConnected -= this.ExecuteRemoteStoreConnected;
            }
        }
        /// <summary>
        /// Remote Store Connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ExecuteRemoteStoreConnected(object sender
            , EventArguments<IGetBlock> args)
        {
            using (var log = new TraceContext())
            {
                EventHandler<IGetBlock> handler = this.RemoteStoreConnected;
                if (null != handler)
                {
                    handler(this
                        , args);
                }
            }
        }
        /// <summary>
        /// Register Client Proxy
        /// </summary>
        /// <param name="proxy">Proxy</param>
        /// <returns>Is Newly Registered</returns>
        public bool Register(Client proxy)
        {
            using (var log = new TraceContext())
            {
                bool isNew = false;
                if (null != proxy)
                {
                    if (!(this.m_proxy.ContainsKey(proxy.Identifier)))
                    {
                        lock (this.m_proxyLock)
                        {
                            this.m_proxy.Add(proxy.Identifier
                                , proxy);
                        }

                        EventHandlerNoReturn<Client> handler = this.ProxyConnected;
                        if (null != handler)
                        {
                            handler(proxy);
                        }

                        isNew = true;
                    }
                }
                return isNew;
            }
        }
        /// <summary>
        /// Already Connected to Client
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Already Connected</returns>
        public bool AlreadyConnected(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                return null != this.m_proxy
                    && this.m_proxy.ContainsKey(identifier);
            }
        }

        #region Threaded
        /// <summary>
        /// Load Known Clients
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadKnownClients(object sender
            , DoWorkEventArgs e)
        {
            using (var log = new TraceContext())
            {
                short[] ports = (short[])e.Argument;
                log.Debug("ports={0}"
                    , ports);
                foreach (short port in ports)
                {
                    if (0 != port)
                    {
                        this.InitClient(new Client(port));
                    }
                }
            }
        }
        #endregion

        #region ILifeTime Members
        /// <summary>
        /// Initializes list of proxies
        /// </summary>
        public void Initialize()
        {
            using (var log = new TraceContext())
            {
                if (null == this.m_proxy)
                {
                    lock (this.m_proxyLock)
                    {
                        this.m_proxy = new Dictionary<Guid, Client>(3);
                    }
                }

                this.m_discovery = this.m_discovery ?? new Executor(this.LoadKnownClients
                    , null);
            }
        }
        /// <summary>
        /// Run
        /// </summary>
        /// <remarks>
        /// Loads Known Peers
        /// Starts Discovery (To Do)
        /// </remarks>
        public void Load()
        {
            using (var log = new TraceContext())
            {
                Executor.Run(this.m_discovery
                    , AppConfig.Peers);
            }
        }
        /// <summary>
        /// Stop tracking servers/clients
        /// </summary>
        public void Unload()
        {
            using (var log = new TraceContext())
            {
                Executor.Cancel(this.m_discovery);

                if (null != this.m_proxy)
                {
                    lock (this.m_proxyLock)
                    {
                        this.m_proxy = null;
                    }
                }
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Clean up
        /// </summary>
        public void Dispose()
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_discovery)
                {
                    Executor.Cancel(this.m_discovery);
                    this.m_discovery.Dispose();
                    this.m_discovery = null;
                }

                this.ProxyConnected = null;

                this.Unload();
            }
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Peers Instance
        /// </summary>
        internal static Peers Instance
        {
            get
            {
                return s_peers;
            }
        }
        #endregion
    }
}