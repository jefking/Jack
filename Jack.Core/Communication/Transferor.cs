using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jack.Core.Communication;
using Jack.Core.IO.Storage;
using Jack.Core.Metric;

using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Transferor
    /// </summary>
    public abstract class Transferor : MarshalByRefObject
        , ILatency
        , IUnique<Guid>
        , IDisposable
    {
        #region Members
        /// <summary>
        /// Unique Identifier
        /// </summary>
        private readonly Guid m_identifier;
        /// <summary>
        /// Disposed
        /// </summary>
        protected bool m_disposed;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Transferor()
            :base()
        {
            using (var log = new TraceContext())
            {
                this.m_identifier = Guid.NewGuid();

                log.Debug("m_identifier={0}"
                    , this.m_identifier);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Is Connected
        /// </summary>
        /// <remarks>
        /// Used as a basic test for connectivity; This is throw an exception if the client isn't connected.
        /// </remarks>
        /// <returns>True</returns>
        public bool IsConnected()
        {
            using (var log = new TraceContext())
            {
                log.Debug("Is Connected Fired On: {0}"
                    , this.GetType());

                return true;
            }
        }

        #region ILatency Members
        /// <summary>
        /// Determine Latency
        /// </summary>
        /// <remarks>
        /// TimeSpan.Max denotes unknown latency
        /// -currently latency isn't being determined
        /// </remarks>
        /// <param name="type">Latency Type</param>
        /// <returns>Time Span</returns>
        public abstract TimeSpan Determine(LatencyType type);
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            using (var log = new TraceContext())
            {
                this.Dispose(true);

                GC.SuppressFinalize(this);
            }
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            using (var log = new TraceContext())
            {
                log.Debug("disposing={0}"
                    , disposing);
                if (!(this.m_disposed))
                {
                    if (disposing)
                    {
                        this.m_disposed = true;
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Properties
        #region IUnique Members
        /// <summary>
        /// Used to Uniquely Identify Proxy Objects
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return this.m_identifier;
            }
        }
        #endregion
        #endregion
    }
}
