using System;

using Jack.Core.Communication;

using Jack.Logger;

namespace Jack.Core.Metadata
{
    /// <summary>
    /// Server Discriptor
    /// </summary>
    /// <remarks>
    /// Used during communication initialization to ensure that remote client can access server
    /// -Describe server for client loop-back
    /// </remarks>
    [Serializable]
    public class Server : MetaData
        , IUnique<Guid>
    {
        #region Members
        /// <summary>
        /// Server Identifier
        /// </summary>
        private readonly Guid m_identifier;
        /// <summary>
        /// Connection information
        /// </summary>
        private readonly IRPCConnector m_connector;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Server()
            : base()
        {
            using (var log = new TraceContext())
            {
                this.m_identifier = RPCServer.ServerIdentifier;
                this.m_connector = new ManifestConnector();

                log.Debug("m_identifier={0}, m_connector={1}"
                    , this.m_identifier
                    , this.m_connector);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Connection information of Server
        /// </summary>
        public IRPCConnector Connector
        {
            get
            {
                return this.m_connector;
            }
        }

        #region IUnique Members
        /// <summary>
        /// Unique Identifier
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
