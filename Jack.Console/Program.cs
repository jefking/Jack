using System;
using System.Diagnostics;
using System.Collections.Generic;

using Jack.Core.Configuration;
using Jack.Core.IO;
using Jack.Core.IO.Storage;
using Jack.Logger;

namespace Jack.Console
{
    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        #region Methods
        /// <summary>
        /// Initialize Logger
        /// </summary>
        private static void InitLogger()
        {
            Jack.Logger.TraceContext.ApplicationName = "Jack";
            Jack.Logger.TraceContext.Listeners.Add(new ConsoleTraceListener());  // trace -> stdout = Good Shit
            Jack.Logger.TraceContext.Listeners.Add(
                new RollingFileListener
                {
                    OutputDirectory = FileHelper.BinDirectory
                }
            );
            Jack.Logger.TraceContext.SeverityFilter = new SourceSwitch("switch")
            {
                Level = SourceLevels.All
            };
        }
        /// <summary>
        /// Main, Entry Point to Application
        /// </summary>
        /// <param name="args">Program Arguments</param>
        public static void Main(string[] args)
        {
            InitLogger();

            using (var log = new TraceContext())
            {
                try
                {
                    byte[] bytes = new byte[] { 12, 12, 3, 1, 123, 254, 112, 123 };
                    var key = System.Console.ReadKey();
                    System.Console.WriteLine();
                    switch (key.KeyChar)
                    {
                        case 's':
                            log.Info("You pressed 's'!");

                            log.Info("Starting server...");
                            using (Jack.Core.ILifetime server = new Jack.Core.Windows.Services.JackService(AppConfig.Port))
                            {
                                server.Initialize();
                                server.Load();
                                System.Console.ReadLine();
                                server.Unload();
                            }
                            System.Console.ReadLine();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    System.Console.ReadLine();
                }
            }
        }
        #endregion
    }
}