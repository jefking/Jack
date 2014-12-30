using System;
using System.IO;
using System.Diagnostics;

namespace Jack.Logger
{
    /// <summary>
    /// Writes a log to disk.
    /// Timestamps log entries & starts a new log file each day.
    /// </summary>
    public class RollingFileListener : TraceListener
    {
        #region Members
        /// <summary>
        /// Output Directory
        /// </summary>
        private string m_outputDirectory;
        /// <summary>
        /// Mutex used to prevent multiple threads accessing file on disk
        /// </summary>
        private static readonly object s_logFileMutex = new object();
        /// <summary>
        /// First Time File is opened
        /// </summary>
        private static volatile bool s_firstOpen = true;
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public RollingFileListener()
            :base()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Build Path
        /// </summary>
        /// <returns>Path</returns>
        private string BuildPath()
        {
            return Path.Combine(this.m_outputDirectory
                , string.Format(LogFilenamePattern
                    , DateTime.Now));
        }
        /// <summary>
        /// This gets a timestamp because the tracing system 
        /// calls to write the prefix to the log entry.
        /// </summary>
        /// <param name="message">Message to log</param>
        public override void Write(string message)
        {
            lock (s_logFileMutex)//Locking is bad; we should have a background thread logging messeages; non-blocking
            {
                using (TextWriter writer = new StreamWriter(File.Open(this.BuildPath()
                    , FileMode.Append
                    , FileAccess.Write
                    , FileShare.Write)))
                {
                    if (s_firstOpen)
                    {
                        const string c_logBreak = "-------------------------------------------------------------------------------";
                        writer.WriteLine(c_logBreak);
                        s_firstOpen = false;
                    }

                    writer.Write(string.Format(LogEntryPattern
                        , DateTime.Now
                        , message));
                }
            }
        }
        /// <summary>
        /// No timestamp because the line is already prefixed with one.
        /// </summary>
        /// <param name="message">Message to log</param>
        public override void WriteLine(string message)
        {
            lock (s_logFileMutex)//Locking is bad; we should have a background thread logging messeages; non-blocking
            {
                using (TextWriter writer = new StreamWriter(File.Open(this.BuildPath()
                    , FileMode.Append
                    , FileAccess.Write
                    , FileShare.Write)))
                {
                    if (s_firstOpen)
                    {
                        const string c_logBreak = "-------------------------------------------------------------------------------";
                        writer.WriteLine(c_logBreak);
                        s_firstOpen = false;
                    }

                    writer.WriteLine(message);
                }
            }
        }
        /// <summary>
        /// Deletes the Current Log File
        /// </summary>
        public void DeleteCurrent()
        {
            string file = this.BuildPath();
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Includes timestamp
        /// </summary>
        public string LogFilenamePattern = "{0:yyyy-MMM-dd}.log";
        /// <summary>
        /// Includes timestamp
        /// </summary>
        public string LogEntryPattern = "{0:HH:mm:ss.fff} {1}";
        /// <summary>
        /// Auto flush log buffer to disk?
        /// </summary>
        public bool AutoFlush = true;
        /// <summary>
        /// Where log files live
        /// </summary>
        /// <remarks>
        /// Creates the target directory, if it does not already exist.
        /// </remarks>
        public string OutputDirectory
        {
            set
            {
                string dir = value;
                if (string.IsNullOrEmpty(dir))
                {
                    //Dir = X?, logging should be safe at all times.
                    throw new ArgumentNullException("OutputDirectory");
                }
                else
                {
                    if (!(Directory.Exists(dir)))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    this.m_outputDirectory = dir;
                }
            }
        }
        #endregion
    }
}