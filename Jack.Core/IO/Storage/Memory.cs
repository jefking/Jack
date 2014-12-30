using System;
using System.Collections.Generic;

using Jack.Core.Metric;
using Jack.Logger;

namespace Jack.Core.IO.Storage
{
    /// <summary>
    /// Memory, filer which stored data in memory
    /// </summary>
    public class Memory : IFiler
        , ILatency
    {
        #region Members
        /// <summary>
        /// Maximum number of items in store
        /// </summary>
        /// <remarks>
        /// This should be dynamic
        /// </remarks>
        private const ushort s_upperbound = 500;
        /// <summary>
        /// Memory
        /// </summary>
        private IDictionary<Guid, byte[]> m_memory;
        /// <summary>
        /// Store Identifier
        /// </summary>
        private readonly Guid m_storeIdentifier;
        /// <summary>
        /// IO Operation Duration
        /// </summary>
        private TimeQueue m_memoryOperationDurations;
        /// <summary>
        /// Disposed
        /// </summary>
        private bool m_disposed;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Memory()
            : base()
        {
            using (var log = new TraceContext())
            {
                this.m_storeIdentifier = Guid.NewGuid();

                this.m_memory = new Dictionary<Guid, byte[]>(s_upperbound);

                this.m_memoryOperationDurations = new TimeQueue();

                log.Debug("m_storeIdentifier={0},m_memory={1},m_memoryOperationDurations={2},s_upperbound={3}"
                    , this.m_storeIdentifier
                    , this.m_memory
                    , this.m_memoryOperationDurations
                    , s_upperbound);
            }
        }
        #endregion

        #region Methods
        #region ILatency Members
        /// <summary>
        /// Determine Latency
        /// </summary>
        /// <param name="type">Latency Type</param>
        /// <returns>Time Span</returns>
        public TimeSpan Determine(LatencyType type)
        {
            using (var log = new TraceContext())
            {
                log.Info("type={0}"
                    , type);

                switch (type)
                {
                    case LatencyType.Storage:
                        return this.m_memoryOperationDurations.Average;
                    default:
                        log.Warn("Invalid");
                        return TimeSpan.Zero;
                }
            }
        }
        #endregion

        #region IFiler Members
        /// <summary>
        /// Get Block from hard disk
        /// </summary>
        /// <remarks>
        /// returns null if it doesn't have the data
        /// </remarks>
        /// <param name="identifier">Identifier</param>
        /// <returns>Payload</returns>
        public byte[] GetBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;

                log.Debug("identifier={0},startCall={1}"
                    , identifier
                    , startCall);

                byte[] block = (this.m_memory.ContainsKey(identifier))
                    ? this.m_memory[identifier]
                    : null;

                this.m_memoryOperationDurations.AddTime(startCall);

                return block;
            }
        }
        /// <summary>
        /// Save Block
        /// </summary>
        /// <remarks>
        /// If it contains the Identifier it doesn't update store.
        /// </remarks>
        /// <param name="identifier">Identifier</param>
        /// <param name="block">Block</param>
        public void SaveBlock(Guid identifier
            , byte[] block)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;

                log.Debug("identifier={0},startCall={1}"
                    , identifier
                    , startCall);

                if (s_upperbound == this.m_memory.Count)
                {
                    log.Warn("Not storing block, memory full.");
                }
                else if (this.m_memory.ContainsKey(identifier))
                {
                    log.Warn("Block already exists in store.");
                }
                else
                {
                    this.m_memory.Add(identifier
                        , block);

                    this.m_memoryOperationDurations.AddTime(startCall);
                }
            }
        }
        /// <summary>
        /// Delete Block
        /// </summary>
        /// <remarks>
        /// If it doesn't have the identifier, no action occurs
        /// </remarks>
        /// <param name="identifier">Identifier</param>
        public void DeleteBlock(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                DateTime startCall = DateTime.Now;

                log.Debug("identifier={0},startCall={1}"
                    , identifier
                    , startCall);

                if (this.m_memory.ContainsKey(identifier))
                {
                    this.m_memory.Remove(identifier);

                    this.m_memoryOperationDurations.AddTime(startCall);
                }
                else
                {
                    log.Warn("Block doesn't exist in store.");
                }
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!(this.m_disposed))
            {
                if (disposing)
                {
                    this.m_memory = null;
                    this.m_memoryOperationDurations = null;

                    this.m_disposed = true;
                }
            }
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Client Id
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return this.m_storeIdentifier;
            }
        }
        /// <summary>
        /// Stored Identifiers
        /// </summary>
        public IEnumerable<Guid> Identifiers
        {
            get
            {
                return this.m_memory.Keys;
            }
        }
        #endregion
    }
}