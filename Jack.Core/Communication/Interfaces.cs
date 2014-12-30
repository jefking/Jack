namespace Jack.Core.Communication
{
    #region Interfaces
    /// <summary>
    /// Allows RPC Server Connections To Be Initiated
    /// </summary>
    public interface IRPCConnector
    {
        #region Properties
        /// <summary>
        /// Port Number
        /// </summary>
        short Port
        {
            get;
        }
        /// <summary>
        /// Proxy String
        /// </summary>
        string ProxyString
        {
            get;
        }
        /// <summary>
        /// Messaging End Point
        /// </summary>
        string EndPoint
        {
            get;
        }
        #endregion
    }
    #endregion
}