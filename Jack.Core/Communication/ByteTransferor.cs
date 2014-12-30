using System;

using Jack.Core.IO;
using Jack.Core.Metric;

using Jack.Logger;

namespace Jack.Core.Communication
{
    /// <summary>
    /// Byte Transferor
    /// </summary>
    [Communicator]
    public class ByteTransferor : Transferor
        , IGetBlock
    {
        #region Members
        /// <summary>
        /// Time Queue
        /// </summary>
        private readonly TimeQueue m_timeQueue;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks>
        /// Used for Remoting Instantiation
        /// </remarks>
        public ByteTransferor()
            : base()
        {
            using (var log = new TraceContext())
            {
                this.m_timeQueue = new TimeQueue();

                log.Debug("m_timeQueue={0}"
                    , this.m_timeQueue);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determine Latency
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override TimeSpan Determine(LatencyType type)
        {
            using (var log = new TraceContext())
            {
                log.Debug("type={0}"
                    , type);

                switch (type)
                {
                    case LatencyType.Storage:
                        return this.m_timeQueue.Average;
                    default:
                        return TimeSpan.Zero;
                }
            }
        }

        #region IGetBlock Members
        /// <summary>
        /// Get Block From Connected Host
        /// </summary>
        /// <param name="identifier">Block Identifier</param>
        /// <returns>Block Data</returns>
        public byte[] GetBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;

                log.Debug("identifier={0},startCall={1}"
                    , identifier
                    , startCall);

                if (Guid.Empty == identifier)
                {
                    throw new InvalidOperationException("Identifier is empty.");
                }
                else
                {
                    IGetBlock get = FileSystem.Instance;
                    byte[] bytes = get.GetBlock(identifier);

                    if (null != bytes)
                    {
                        this.m_timeQueue.AddTime(startCall);
                    }

                    return bytes;
                }
            }
        }
        #endregion
        #endregion
    }
}