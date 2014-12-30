using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Jack.Logger
{
    /// <summary>
    /// This wrapper exists mainly to inject the class and method names into the trace entries.
    /// 
    /// sample usage:
    /// 
    /// var classname = this;
    /// var methodName = "bar";
    /// using(var log = new TraceContext(this, methodname)
    /// {
    ///     log.Debug("ok,computer,x={0},y={1},z={2}",x,y,z);
    ///     log.Info("something wicked this way comes; m_identifier={0}",m_identifier);
    ///     log.Warn("free drive space &lt;1% !");
    ///     log.Error(exception);
    /// }
    /// </summary>
    public class TraceContext : IDisposable
    {
        #region Classes
        /// <summary>
        /// Uknown Type
        /// </summary>
        private struct UnknownType {}
        #endregion

        #region Members
        /// <summary>
        /// NULL for logging
        /// </summary>
        private const string c_null = "NULL";
        /// <summary>
        /// Listeners
        /// </summary>
        private readonly static List<TraceListener> s_listeners = new List<TraceListener>();
        /// <summary>
        /// Trace Source
        /// </summary>
        private TraceSource m_traceSource;
        /// <summary>
        /// Log Prefix
        /// </summary>
        private string m_logPrefix;
        #endregion

        #region Constructors
        /// <summary>
        /// Static Constructor
        /// </summary>
        static TraceContext()
        {
        }
        /// <summary>
        /// Trace Context
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="method">Method</param>
        public TraceContext()
            :base()
        {
            var stackTrace = new StackTrace();
            StackFrame frame = stackTrace.GetFrame(1);
            MethodBase method = frame.GetMethod();
            this.m_logPrefix = string.Format("thread={2},class={0},method={1},message="
                , method.DeclaringType
                , method
                , Thread.CurrentThread.ManagedThreadId);

            this.m_traceSource = new TraceSource(ApplicationName);
            this.m_traceSource.Listeners.AddRange(s_listeners.ToArray());
            this.m_traceSource.Switch = SeverityFilter;

            int eventID = 0;
            this.m_traceSource.TraceEvent(TraceEventType.Verbose
                , eventID
                , this.m_logPrefix + "Start Call");

            stackTrace = null;
            frame = null;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Null
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>String</returns>
        private static string Null(string str)
        {
            return str == null
                ? c_null
                : str;
        }
        /// <summary>
        /// Trace out with severity "Debug"
        /// </summary>
        /// <param name="message">If data are populated, this should be message plus a format string, ie: "something happened ID={0} user={1}"</param>
        /// <param name="data"></param>
        public void Debug(string message
            , params object[] data)
        {
            int eventID = 0;
            this.m_traceSource.TraceEvent(TraceEventType.Verbose
                , eventID
                , this.m_logPrefix + message
                , data);
        }

        /// <summary>
        /// Trace out with severity "Info"
        /// </summary>
        /// <param name="message">If data are populated, this should be message plus a format string, ie: "something happened ID={0} user={1}"</param>
        /// <param name="data"></param>
        public void Info(string message
            , params object[] data)
        {
            this.m_traceSource.TraceInformation(this.m_logPrefix + message
                , data);
        }

        /// <summary>
        /// Trace out with severity "Warning"
        /// </summary>
        /// <param name="message">If data are populated, this should be message plus a format string, ie: "something happened ID={0} user={1}"</param>
        /// <param name="data"></param>
        public void Warn(string message
            , params object[] data)
        {
            int eventID = 0;
            this.m_traceSource.TraceEvent(TraceEventType.Warning
                , eventID
                , this.m_logPrefix + message
                , data);
        }
        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="data">Data Parameters</param>
        public void Error(string message
            , params object[] data)
        {
            int eventID = 0;
            this.m_traceSource.TraceEvent(TraceEventType.Error
                , eventID
                , this.m_logPrefix + message
                , data);
        }
        /// <summary>
        /// Error
        /// </summary>
        /// <param name="ex">Exception to log</param>
        public void Error(Exception ex)
        {
            int eventID = 0;
            this.m_traceSource.TraceEvent(TraceEventType.Error
                , eventID
                , this.m_logPrefix + "Exception: {0}"
                , ex);
        }

        #region IDisposable Members
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            int eventID = 0;
            this.m_traceSource.TraceEvent(TraceEventType.Verbose
                , eventID
                , this.m_logPrefix + "End Call");

            if (null != this.m_traceSource)
            {
                this.m_traceSource.Flush();
                this.m_traceSource.Close();
                this.m_traceSource = null;
            }

            this.m_logPrefix = null;
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Common to all trace contexts.
        /// </summary>
        public static string ApplicationName { get; set; }
        /// <summary>
        /// Common to all trace contexts.
        /// Use this to set threshold below which log messages are discarded.
        /// </summary>
        public static SourceSwitch SeverityFilter { get; set; }
        /// <summary>
        /// Common to all trace contexts.
        /// </summary>
        public static IList<TraceListener> Listeners { get { return s_listeners; } }
        #endregion
    }
}