using System;

using Jack.Core.Metadata;
using Jack.Core.IO;
using Jack.Core.IO.Data;
using Jack.Core.Metric;

using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Remote
    /// </summary>
    [Communicator]
    public class ManifestTransferor : Transferor
    {
        #region Members
        /// <summary>
        /// Data Store
        /// </summary>
        private readonly DataStore m_dataStore;
        /// <summary>
        /// Time Queue
        /// </summary>
        private readonly TimeQueue m_timeQueue;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks>
        /// Used for Remoting Instantiation
        /// </remarks>
        public ManifestTransferor()
            : base()
        {
            using (var log = new TraceContext())
            {
                this.m_timeQueue = new TimeQueue();

                this.m_dataStore = DataStore.Instance;

                log.Debug("m_timeQueue={0} m_dataStore={1}"
                    , this.m_timeQueue
                    , this.m_dataStore);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determine Latency
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override TimeSpan Determine(Jack.Core.Metric.LatencyType type)
        {
            using (var log = new TraceContext())
            {
                log.Debug("type={0}"
                    , type);

                switch (type)
                {
                    case LatencyType.Network:
                        return this.m_timeQueue.Average;
                    default:
                        return TimeSpan.Zero;
                }
            }
        }
        /// <summary>
        /// Initialize Communication
        /// </summary>
        /// <param name="connector">To Estabilsh Communication Bi-Directionally</param>
        public void InitializeCommunication(Server server)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;

                log.Debug("server={0},startCall={1}"
                    , server
                    , startCall);

                if (server != null
                    && server.Connector != null)
                {
                    Peers p = Peers.Instance;
                    if (!(p.AlreadyConnected(server.Identifier)))
                    {
                        log.Warn("Not Connected.");
                    }

                    this.m_timeQueue.AddTime(startCall);
                }
            }
        }
        /// <summary>
        /// Send Manifest
        /// </summary>
        /// <param name="metaData">Manifest</param>
        public void Push(FileManifest manifest)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;

                log.Debug("manifest={0},startCall={1}"
                    , manifest
                    , startCall);

                this.m_dataStore.Put(manifest);

                this.m_timeQueue.AddTime(startCall);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Server Identifier
        /// </summary>
        /// <remarks>
        /// This should come back as initial connection information...
        /// </remarks>
        public Guid ServerIdentifier
        {
            get
            {
                return RPCServer.ServerIdentifier;
            }
        }
        #endregion
    }
}