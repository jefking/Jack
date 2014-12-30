using System;
using System.Diagnostics;
using System.ServiceProcess;

using Jack.Core.Configuration;
using Jack.Core.IO;
using Jack.Core.Windows.Services;

using Jack.Logger;

namespace Jack.Service
{
    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        #region Methods
        /// <summary>
        /// Main, Entry Point to Application
        /// </summary>
        /// <param name="args">Program Arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            TraceContext.ApplicationName = Common.DisplayName;
            TraceContext.Listeners.Add(
                new RollingFileListener
                {
                    OutputDirectory = FileHelper.BinDirectory
                }
            );
            TraceContext.SeverityFilter = new SourceSwitch("Jack.Service")
            {
                Level = SourceLevels.Verbose
            };

            using (var log = new TraceContext())
            {
                short port = AppConfig.Port;
                log.Debug("starting Jack;args={0},port={1}"
                    , args
                    , port);

                using (JackService service = new JackService(port))
                {
                    ServiceBase.Run(new ServiceBase[] { service });
                }
                log.Debug("ending Jack");
            }
        }
        #endregion
    }
}