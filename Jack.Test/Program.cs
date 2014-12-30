using System;
using System.Diagnostics;

using Jack.Logger;

namespace Jack.Test
{
    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        #region Methods
        /// <summary>
        /// Main, entry point for application
        /// </summary>
        /// <param name="args">Arguments</param>
        static void Main(string[] args)
        {
            RollingFileListener rfl = new RollingFileListener
            {
                OutputDirectory = Jack.Core.IO.FileHelper.BinDirectory
            };
            rfl.DeleteCurrent();
            TraceContext.ApplicationName = "Jack.Test";
            TraceContext.Listeners.Add(new ConsoleTraceListener());
            TraceContext.Listeners.Add(rfl);
            TraceContext.SeverityFilter = new SourceSwitch("switch")
            {
                Level = SourceLevels.All
            };
            using (var log = new TraceContext())
            {
                try
                {
                    Testor test = new Testor();
                    test.Run();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }
        #endregion
    }
}