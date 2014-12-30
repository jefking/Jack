using System;

using Jack.Core.Configuration;

using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Manifest Connector
    /// </summary>
    /// <remarks>
    /// Allows Connections for transfering meta data
    /// </remarks>
    [Serializable]
    internal class ManifestConnector : Connector<ManifestTransferor>
    {
        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks>
        /// Used to Initalize Server
        /// </remarks>
        internal ManifestConnector()
            : base(null
                , null
                , short.MinValue)
        {
            using (var log = new TraceContext())
            {
            }
        }
        /// <summary>
        /// Constructor Specifying Port
        /// </summary>
        /// <param name="port">Port Number</param>
        /// <remarks>
        /// Used to Connect to Remote Hosts;
        /// Currently just Port, as we are testing on single machines, this will be built out.
        /// </remarks>
        internal ManifestConnector(short port)
            : base(null
                , null
                , port)
        {
            using (var log = new TraceContext())
            {
            }
        }
        #endregion
    }
}