using Jack.Logger;

namespace Jack.Core.Windows.Services
{
    /// <summary>
    /// Service
    /// </summary>
    public abstract class Service : System.ServiceProcess.ServiceBase
    {
        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Service()
            : base()
        {
            using (var log = new TraceContext()) { }
        }
        /// <summary>
        /// Properly Constructs Service
        /// </summary>
        /// <param name="ServiceName">Service Name</param>
        public Service(string serviceName)
            : base()
        {
            using (var log = new TraceContext())
            {
                log.Debug("serviceName={0}"
                    , serviceName);
                base.ServiceName = serviceName;
            }
        }
        #endregion
    }
}