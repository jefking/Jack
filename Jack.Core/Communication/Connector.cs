using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jack.Core.Configuration;
using Jack.Core.LDAP;
using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Connector
    /// </summary>
    [Serializable]
    public class Connector<T> : IRPCConnector
    {
        #region Members
        /// <summary>
        /// Port
        /// </summary>
        private readonly short m_port;
        /// <summary>
        /// Machine
        /// </summary>
        private readonly string m_machine;
        /// <summary>
        /// Protocol
        /// </summary>
        private readonly string m_pipePrefix;
        #endregion

        #region Constructors
        /// <summary>
        /// Connector
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <param name="port">Port</param>
        /// <param name="endPoint">End Point</param>
        protected Connector(string pipePrefix
            , string machine
            , short port)
            : base()
        {
            using (var log = new TraceContext())
            {
                this.m_pipePrefix = pipePrefix
                    ?? AppConfig.PipePrefix;

                this.m_machine = machine
                    ?? ActiveDirectory.MachineName;

                this.m_port = (ushort.MinValue == port)
                    ? AppConfig.Port
                    : port;

                log.Debug("m_port={0},type={1},m_pipePrefix={2},m_machine={3}"
                    , this.m_port
                    , typeof(T)
                    , this.m_pipePrefix
                    , this.m_machine);
            }
        }
        #endregion

        #region Static
        /// <summary>
        /// Establishes Connection To Server
        /// </summary>
        /// <param name="RPCConnector">RPC Connector</param>
        /// <returns>Connected Object</returns>
        public static Client EstablishConnection<Client>(IRPCConnector RPCConnector)
        {
            using (var log = new TraceContext())
            {
                try
                {
                    log.Debug("Requesting {0} On Port {1}."
                        , typeof(Client)
                        , RPCConnector.Port);

                    return (Client)Activator.GetObject(typeof(Client)
                        , RPCConnector.ProxyString);
                }
                catch (System.Exception ex)
                {
                    log.Error(ex.Message);
                    return default(Client);
                }
            }
        }
        #endregion

        #region IRPCConnector Members
        /// <summary>
        /// Port
        /// </summary>
        public short Port
        {
            get
            {
                return this.m_port;
            }
        }
        /// <summary>
        /// Proxy String
        /// </summary>
        public string ProxyString
        {
            get
            {
                using (var log = new TraceContext())
                {
                    string proxyString = string.Format("{0}://{1}:{2}/{3}"
                        , this.m_pipePrefix
                        , this.m_machine
                        , this.m_port
                        , this.EndPoint);
                    log.Debug("proxyString={0}"
                        , proxyString);
                    return proxyString;
                }
            }
        }
        /// <summary>
        /// End Point
        /// </summary>
        public string EndPoint
        {
            get
            {
                return typeof(T).Name;
            }
        }
        #endregion
    }
}