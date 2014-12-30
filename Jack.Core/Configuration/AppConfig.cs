using System;
using System.Configuration;
using System.Linq;

using Jack.Logger;

namespace Jack.Core.Configuration
{
    /// <summary>
    /// App Config
    /// </summary>
    public class AppConfig
    {
        #region Constants
        /// <summary>
        /// Client Id Key
        /// </summary>
        private const string c_clientIdKey = "ClientId";
        /// <summary>
        /// Default Port Key
        /// </summary>
        private const string c_port = "Port";
        /// <summary>
        /// IP Address Key
        /// </summary>
        private const string c_ipAddress = "IPAddress";
        /// <summary>
        /// Pipe Prefix Key
        /// </summary>
        private const string s_pipePrefix = "PipePrefix";
        /// <summary>
        /// Peers Key
        /// </summary>
        private const string s_peersKey = "Peers";
        #endregion

        #region Methods
        /// <summary>
        /// Client Id
        /// </summary>
        public static string ClientId
        {
            get
            {
                using (var log = new TraceContext())
                {
                    return ConfigurationManager.AppSettings[c_clientIdKey];
                }
            }
        }
        /// <summary>
        /// Pipe Prefix
        /// </summary>
        public static string PipePrefix
        {
            get
            {
                using (var log = new TraceContext())
                {
                    return ConfigurationManager.AppSettings[s_pipePrefix];
                }
            }
        }
        /// <summary>
        /// Server IP Address to bind to
        /// </summary>
        public static string IPAddress
        {
            get
            {
                using (var log = new TraceContext())
                {
                    return ConfigurationManager.AppSettings[c_ipAddress];
                }
            }
        }
        /// <summary>
        /// Server Port
        /// </summary>
        public static short Port
        {
            get
            {
                using (var log = new TraceContext())
                {
                    return Convert.ToInt16(ConfigurationManager.AppSettings[c_port]);
                }
            }
        }
        /// <summary>
        /// Peers
        /// </summary>
        public static short[] Peers
        {
            get
            {
                using (var log = new TraceContext())
                {
                    string peers = ConfigurationManager.AppSettings[s_peersKey];
                    if (string.IsNullOrEmpty(peers))
                    {
                        return new short[] { 0 };
                    }
                    else if (peers.Contains(","))
                    {
                        string[] peer = peers.Split(',');
                        var peerPorts = peer.Select(x => short.Parse(x));
                        return peerPorts.ToArray();
                    }
                    else
                    {
                        return new short[] { short.Parse(peers) };
                    }
                }
            }
        }
        #endregion
    }
}