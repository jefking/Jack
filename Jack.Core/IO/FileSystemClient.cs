using System;

using Jack.Core.Communication;

using Jack.Logger;

namespace Jack.Core.IO
{
    /// <summary>
    /// File System Client
    /// </summary>
    /// <remarks>
    /// Class used for interaction between 'clients' and the file system
    /// Load using RPC and interact with the underlying file system
    /// </remarks>
    [Communicator]
    public class FileSystemClient : MarshalByRefObject
        , IDataOperations
    {
        #region Classes
        /// <summary>
        /// Connector
        /// </summary>
        private class Connector : Connector<FileSystemClient>
        {
            #region Constructors
            /// <summary>
            /// Connector
            /// </summary>
            /// <param name="machine">Machine</param>
            /// <param name="port">Port</param>
            internal Connector(string machine
                    , short port)
                :base(null
                    , machine
                    , port)
            {
            }
            #endregion
        }
        #endregion

        #region Members
        /// <summary>
        /// File System
        /// </summary>
        private readonly IDataOperations m_fs;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public FileSystemClient()
        {
            using (var log = new TraceContext())
            {
                this.m_fs = FileSystem.Instance;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Is Connected
        /// </summary>
        /// <remarks>
        /// Used as a basic test for connectivity; This is throw an exception if the client isn't connected.
        /// </remarks>
        /// <returns>True</returns>
        public bool IsConnected()
        {
            using (var log = new TraceContext())
            {
                log.Debug("type={0}"
                    , this.GetType());

                return true;
            }
        }

        #region IDataOperations Members
        /// <summary>
        /// Store
        /// </summary>
        /// <param name="unc">UNC</param>
        /// <param name="payload">Payload</param>
        public void Store(string unc
            , byte[] payload)
        {
            using (var log = new TraceContext())
            {
                log.Info("unc={0}"
                    , unc);

                if (string.IsNullOrEmpty(unc))
                {
                    throw new InvalidOperationException("UNC is null");
                }
                else if (null == payload)
                {
                    throw new InvalidOperationException("Payload is null");
                }
                else
                {
                    this.m_fs.Store(unc
                        , payload);
                }
            }
        }
        /// <summary>
        /// Retrieve
        /// </summary>
        /// <param name="filename">File Name</param>
        /// <param name="version">Version</param>
        /// <returns>File</returns>
        public IFile Retrieve(string unc)
        {
            using (var log = new TraceContext())
            {
                log.Info("unc={0}"
                    , unc);

                if (string.IsNullOrEmpty(unc))
                {
                    throw new InvalidOperationException("UNC is null");
                }
                else
                {
                    return this.m_fs.Retrieve(unc);
                }
            }
        }
        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <param name="port">Port</param>
        /// <returns>File System Client</returns>
        public static FileSystemClient Connect(string machine
            , short port)
        {
            using (var log = new TraceContext())
            {
                log.Info("machine={0},port={1}"
                    , machine
                    , port);

                return Connector<FileSystemClient>.EstablishConnection<FileSystemClient>(new Connector(machine
                    , port));
            }
        }
        /// <summary>
        /// Connect
        /// </summary>
        /// <remarks>
        /// Connects to Localhost on default (app config) serving port.
        /// </remarks>
        /// <returns>File System Client</returns>
        public static FileSystemClient Connect()
        {
            using (var log = new TraceContext())
            {
                return FileSystemClient.Connect(Jack.Core.Network.Constants.Localhost
                    , Jack.Core.Configuration.AppConfig.Port);
            }
        }
        #endregion
        #endregion
    }
}