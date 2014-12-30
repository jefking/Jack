using System;
using System.Collections.Generic;
using System.Threading;

using Jack.Core.IO;
using Jack.Core.Metadata;

using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Clien Communication Wrapper
    /// </summary>
    internal class Client : ILifetime
        , IUnique<Guid>
    {
        #region Events
        /// <summary>
        /// Remote Store Connected
        /// </summary>
        public event EventHandler<IGetBlock> RemoteStoreConnected;
        /// <summary>
        /// Occurs when object isn't remotely connected any more
        /// </summary>
        public event EventHandler<Guid> RemoteObjectDisconnected;
        #endregion

        #region Members
        /// <summary>
        /// Transferors
        /// </summary>
        private IDictionary<Guid, Transferor> m_transferors;
        /// <summary>
        /// Duration for checking availablity of remote object
        /// </summary>
        private static readonly TimeSpan s_checkDuration = new TimeSpan(0, 1, 0);
        /// <summary>
        /// Server
        /// </summary>
        private static readonly Server s_server;
        /// <summary>
        /// Port for connection
        /// </summary>
        private readonly short m_port;
        /// <summary>
        /// Byte Identifier, Cached
        /// </summary>
        private Guid m_byteIdentifier;
        /// <summary>
        /// Byte Connector
        /// </summary>
        private ByteConnector m_byteConnector;
        /// <summary>
        /// Manifest Identifier, Cached
        /// </summary>
        private Guid m_manifestIdentifier;
        /// <summary>
        /// Manifest Connector
        /// </summary>
        private ManifestConnector m_manifestConnector;
        /// <summary>
        /// Guid, Identifier
        /// </summary>
        /// <remarks>
        /// Make stateful
        /// </remarks>
        private Guid m_identifier = Guid.NewGuid();
        /// <summary>
        /// Manifest Connection Checker
        /// </summary>
        private Timer m_manifestConnectionChecker;
        /// <summary>
        /// Byte Connection Checker
        /// </summary>
        private Timer m_byteConnectionChecker;
        #endregion

        #region Constructors
        /// <summary>
        /// Static Constructor
        /// </summary>
        static Client()
        {
            using (var log = new TraceContext())
            {
                s_server = new Server();
            }
        }
        /// <summary>
        /// Client Constructor
        /// </summary>
        /// <param name="port">Port</param>
        public Client(short port)
        {
            using (var log = new TraceContext())
            {
                log.Debug("port={0}"
                    , port);

                this.m_port = port;
            }
        }
        #endregion

        #region Methods
        #region ILifetime Members
        /// <summary>
        /// Load
        /// </summary>
        public void Load()
        {
            using (var log = new TraceContext())
            {
                ManifestTransferor manifestTransferor = Connector<ManifestTransferor>.EstablishConnection<ManifestTransferor>(this.m_manifestConnector);
                if (Client.IsConnected(manifestTransferor))
                {
                    this.m_manifestIdentifier = manifestTransferor.Identifier;

                    this.m_transferors.Add(this.m_manifestIdentifier
                        , manifestTransferor);

                    this.m_manifestConnectionChecker = new Timer(this.TimerCallback
                        , this.m_manifestIdentifier
                        , s_checkDuration
                        , s_checkDuration);

                    if (Peers.Instance.Register(this))
                    {
                        try
                        {
                            manifestTransferor.InitializeCommunication(s_server);
                            ByteTransferor byteTransferor = Connector<ByteTransferor>.EstablishConnection<ByteTransferor>(this.m_byteConnector);
                            if (Client.IsConnected(byteTransferor))
                            {
                                this.m_byteIdentifier = byteTransferor.Identifier;

                                this.m_transferors.Add(this.m_byteIdentifier
                                    , byteTransferor);

                                this.m_byteConnectionChecker = new Timer(this.TimerCallback
                                    , this.m_byteIdentifier
                                    , s_checkDuration
                                    , s_checkDuration);

                                EventHandler<IGetBlock> handler = this.RemoteStoreConnected;
                                if (null != handler)
                                {
                                    handler(this
                                        , new EventArguments<IGetBlock>(byteTransferor));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Timer Callback
        /// </summary>
        /// <param name="state">State</param>
        private void TimerCallback(object state)
        {
            using (var log = new TraceContext())
            {
                Guid id = (Guid)state;
                if (Client.IsConnected(this.m_transferors[id]))
                {
                    log.Debug("Is Connected.");
                }
                else
                {
                    log.Debug("Is Disconnected.");
                    EventHandler<Guid> handler = this.RemoteObjectDisconnected;
                    if (null != handler)
                    {
                        handler(this
                            , new EventArguments<Guid>(id));
                    }
                    this.StopAssociatedTimer(id);
                }
            }
        }
        /// <summary>
        /// Stop Associated Timer
        /// </summary>
        /// <param name="identifier">Identifier</param>
        private void StopAssociatedTimer(Guid identifier)
        {
            if (identifier == this.m_manifestIdentifier)
            {
                this.m_manifestConnectionChecker.Dispose();
            }
            else if (identifier == this.m_byteIdentifier)
            {
                this.m_byteConnectionChecker.Dispose();
            }
        }
        /// <summary>
        /// Is Connected to remote Client
        /// </summary>
        /// <param name="t">Transferor</param>
        /// <returns>Is Connected</returns>
        private static bool IsConnected(Transferor t)
        {
            using (var log = new TraceContext())
            {
                bool isConnected = false;
                try
                {
                    isConnected = t.IsConnected();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
                return isConnected;
            }
        }
        /// <summary>
        /// Unload
        /// </summary>
        public void Unload()
        {
            using (var log = new TraceContext())
            {
                if (null != this.m_manifestConnectionChecker)
                {
                    this.m_manifestConnectionChecker.Dispose();
                    this.m_manifestConnectionChecker = null;
                }
                if (null != this.m_byteConnectionChecker)
                {
                    this.m_byteConnectionChecker.Dispose();
                    this.m_byteConnectionChecker = null;
                }

                if (null != this.m_transferors)
                {
                    foreach (Transferor t in this.m_transferors.Values)
                    {
                        if (null != t)
                        {
                            t.Dispose();
                        }
                    }

                    this.m_transferors = null;
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

                this.RemoteStoreConnected = null;
                this.RemoteObjectDisconnected = null;
            }
        }
        #endregion

        #region IInitialize Members
        /// <summary>
        /// Initialize Members
        /// </summary>
        public void Initialize()
        {
            using (var log = new TraceContext())
            {
                this.m_byteConnector = this.m_byteConnector
                    ?? new ByteConnector(this.m_port);

                this.m_manifestConnector = this.m_manifestConnector
                    ?? new ManifestConnector(this.m_port);

                this.m_transferors = new Dictionary<Guid, Transferor>(2);
            }
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Remote Manifest Transferor
        /// </summary>
        internal ManifestTransferor Manifest
        {
            get
            {
                return this.m_transferors[this.m_manifestIdentifier] as ManifestTransferor;
            }
        }
        /// <summary>
        /// Remote Byte Transferor
        /// </summary>
        internal ByteTransferor Byte
        {
            get
            {
                return this.m_transferors[this.m_byteIdentifier] as ByteTransferor;
            }
        }

        #region IUnique Members
        /// <summary>
        /// Client Identifier
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