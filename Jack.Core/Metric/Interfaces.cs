using System;

namespace Jack.Core.Metric
{
    #region Latency
    /// <summary>
    /// Latency Interface
    /// </summary>
    public interface ILatency
    {
        #region Methods
        /// <summary>
        /// Determine Latency
        /// </summary>
        /// <returns>Latency</returns>
        TimeSpan Determine(LatencyType type);
        #endregion
    }
    #endregion
}