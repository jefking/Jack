using System;

using Jack.Core.Configuration;
using Jack.Core.Communication;
using Jack.Core.IO;

namespace Jack.Service
{
    /// <summary>
    /// Jack Service
    /// </summary>
    public class JackService : Jack.Core.Windows.Services.Service
    {
        #region Members
        /// <summary>
        /// Server
        /// </summary>
        private RPCServer m_server;
        protected FileSystem m_fileSystem;
        #endregion

        #region Constructor
        /// <summary>
        /// Jack Service
        /// </summary>
        public JackService()
            : base(Common.ServiceName)
        {
            this.Initialize();
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            ServerConnector sc = new ServerConnector(AppConfig.ServerPort
                , AppConfig.EndPoint);

            this.m_server = new RPCServer(sc);
        }
        /// <summary>
        /// When Service Is Started
        /// </summary>
        protected override void OnStart(string[] args)
        {
            this.m_server.Start();
            
            var peers = new Peers(AppConfig.Peers
                , AppConfig.EndPoint);

            m_fileSystem = new FileSystem(peers);
        }
        /// <summary>
        /// When Service is Stopped
        /// </summary>
        protected override void OnStop()
        {
            this.m_server.Stop();
        }
        /// <summary>
        /// Cleanly Disposes of Objects
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (null != this.m_server)
            {
                this.m_server.Dispose();
                this.m_server = null;
            }

            base.Dispose(disposing);

            if (disposing)
            {
                System.GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}