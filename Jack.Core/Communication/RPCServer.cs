using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using Jack.Core.Configuration;
using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// RPC Server
    /// </summary>
    /// <remarks>
    /// Loads a TCP channel for Communication
    /// </remarks>
    public sealed class RPCServer : ILifetime
        , IUnique<Guid>
    {
        #region Members
        /// <summary>
        /// TCP Channel, As RPC Sink
        /// </summary>
        private static TcpChannel s_channel = null;
        /// <summary>
        /// Port
        /// </summary>
        private short m_port;
        /// <summary>
        /// Disposed
        /// </summary>
        private bool m_disposed;
        /// <summary>
        /// Uniquely Identifies the Server
        /// </summary>
        private static readonly Guid s_identifier = Guid.NewGuid();
        #endregion

        #region Constructor
        /// <summary>
        /// RPC Server
        /// </summary>
        public RPCServer(short port)
            : base()
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
        #region ILifeTime Members
        /// <summary>
        /// Initializes Lifetime members
        /// </summary>
        public void Initialize()
        {
            using (var log = new TraceContext())
            {
                if (null == s_channel)
                {
                    s_channel = new TcpChannel(this.m_port);

                    log.Debug("Created TcpChannel on port={0}"
                        , this.m_port);
                }
            }
        }
        /// <summary>
        /// Loads TCP Channel for Communication
        /// </summary>
        public void Load()
        {
            using (var log = new TraceContext())
            {
                bool isRegistered = false;
                foreach (IChannel channel in ChannelServices.RegisteredChannels)
                {
                    if (channel.ChannelName == s_channel.ChannelName)
                    {
                        log.Debug("Channel already registered={0}"
                            , s_channel.ChannelName);

                        isRegistered = true;
                        break;
                    }
                }

                try
                {
                    if (!(isRegistered))
                    {
                        ChannelServices.RegisterChannel(s_channel
                            , false);

                        log.Info("Registered channel on port {0}"
                            , this.m_port);
                    }
                }
                catch (System.Exception ex)
                {
                    log.Error("Failed to Register server on port {0}, exception: {1}"
                        , this.m_port
                        , ex);
                }

                if (!(isRegistered))
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    foreach (Type type in assembly.GetTypes())
                    {
                        foreach (Communicator attribute in type.GetCustomAttributes(typeof(Communicator), false))
                        {
                            RemotingConfiguration.RegisterWellKnownServiceType(type
                                    , type.Name
                                    , System.Runtime.Remoting.WellKnownObjectMode.Singleton);

                            log.Debug("Registered service {0} on endpoint {1}"
                                , type
                                , type.Name);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Deregisters TCP channel from serving requests
        /// </summary>
        public void Unload()
        {
            using (var log = new TraceContext())
            {
                if (null != s_channel)
                {
                    foreach (IChannel channel in ChannelServices.RegisteredChannels)
                    {
                        if (channel.ChannelName == s_channel.ChannelName)
                        {
                            ChannelServices.UnregisterChannel(channel);
                        }
                    }
                    s_channel.StopListening(this.m_port);
                    s_channel = null;
                    log.Debug("Stopped listening on port={0}"
                        , this.m_port);
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
        private void Dispose(bool disposing)
        {
            using (var log = new TraceContext())
            {
                log.Debug("disposing={0}"
                    , disposing);

                if (!(this.m_disposed))
                {
                    if (disposing)
                    {
                        this.Unload();

                        this.m_disposed = true;
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Uniquely Identifies Server
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return s_identifier;
            }
        }
        /// <summary>
        /// Uniquely Identifies Server
        /// </summary>
        public static Guid ServerIdentifier
        {
            get
            {
                return s_identifier;
            }
        }
        #endregion
    }
}