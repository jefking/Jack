using System;
using System.Diagnostics;
using System.Windows.Forms;

using Jack.Core.IO;

using Jack.Logger;

namespace Jack.Administration
{
    /// <summary>
    /// Program
    /// </summary>
    public static class Program
    {
        #region Methods
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Program Arguments</param>
        [STAThread]
        static void Main(string[] args)
        {
            TraceContext.ApplicationName = "Jack.Administration";
            TraceContext.Listeners.Add(
                new RollingFileListener
                {
                    OutputDirectory = FileHelper.BinDirectory
                }
            );
            TraceContext.SeverityFilter = new SourceSwitch("Jack.Service")
            {
                Level = SourceLevels.Information
            };

            using (var log = new TraceContext())
            {
                log.Debug("application starting;args={0}"
                    , args);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Administration());
                log.Debug("application exiting");
            }
        }
        #endregion
    }
}