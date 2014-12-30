using System.Diagnostics;
using System.ComponentModel;
using System.Configuration.Install;

using Jack.Core.IO;
using Jack.Core.Windows.Services;

using Jack.Logger;

namespace Jack.Service
{
    /// <summary>
    /// Service Installer
    /// </summary>
    [RunInstaller(true)]
    public partial class ServiceInstaller : Installer
    {
        #region Constructors
        /// <summary>
        /// Service Installer
        /// </summary>
        /// <remarks>
        /// Initialize logger for installation.
        /// </remarks>
        static ServiceInstaller()
        {
            ServiceInstaller.InitLogger();
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ServiceInstaller()
            :base()
        {
            using (var log = new TraceContext())
            {
                this.InitializeComponent();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize Logger
        /// </summary>
        private static void InitLogger()
        {
            TraceContext.ApplicationName = Common.DisplayName;
            TraceContext.Listeners.Add(
                new RollingFileListener
                {
                    LogFilenamePattern = "{0:yyyy-MMM-dd}.log",
                    LogEntryPattern = "{0:HH:mm:ss.ss} {1}",
                    AutoFlush = true,
                    OutputDirectory = FileHelper.ApplicationDataDirectory
                }
            );
            TraceContext.SeverityFilter = new SourceSwitch("Jack.Service")
            {
                Level = SourceLevels.Information
            };
        }
        #endregion
    }
}