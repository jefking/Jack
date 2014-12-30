using System;

using Jack.Core.Configuration;

using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Byte Connector
    /// </summary>
    [Serializable]
    internal class ByteConnector : Connector<ByteTransferor>
    {
        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        internal ByteConnector()
            : base(null
                , null
                , short.MinValue)
        {
            using (var log = new TraceContext()){ }
        }
        /// <summary>
        /// Constructor Specifying Port
        /// </summary>
        /// <param name="port">Port Numer</param>
        /// <remarks>
        /// Used to Connect to Remote Hosts;
        /// Currently just Port, as we are testing on single machines, this will be built out.
        /// </remarks>
        internal ByteConnector(short port)
            : base(null
                , null
                , port)
        {
            using (var log = new TraceContext()) { }
        }
        #endregion
    }
}