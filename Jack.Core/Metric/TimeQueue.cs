using System;
using System.Collections.Generic;

using Jack.Logger;

namespace Jack.Core.Metric
{
    /// <summary>
    /// Time Queue
    /// </summary>
    internal class TimeQueue
    {
        #region Members
        /// <summary>
        /// Maximum Tracked Time Spans
        /// </summary>
        private readonly byte m_max = 10;
        /// <summary>
        /// Time Span Queue
        /// </summary>
        private Queue<TimeSpan> m_time = new Queue<TimeSpan>(10);
        #endregion

        #region Constructors
        /// <summary>
        /// Time Queue Constructor
        /// </summary>
        /// <param name="max">Maximum Tracked Items</param>
        internal TimeQueue(byte max)
            : base()
        {
            using (var log = new TraceContext())
            {
                this.m_max = max;
                log.Debug("m_max={0}"
                    , this.m_max);
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        internal TimeQueue()
            : this(10)
        {
            using (var log = new TraceContext())
            {
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add Time
        /// </summary>
        /// <param name="startCall">Start of Call</param>
        internal void AddTime(DateTime startCall)
        {
            using (var log = new TraceContext())
            {
                TimeSpan time = DateTime.Now.Subtract(startCall);

                log.Debug("startCall={0},time={1}"
                    , startCall
                    , time);

                if (this.m_time.Count == this.m_max)
                {
                    this.m_time.Dequeue();
                }
                this.m_time.Enqueue(time);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Average Time Span
        /// </summary>
        internal TimeSpan Average
        {
            get
            {
                if (0 < this.m_time.Count)
                {
                    TimeSpan[] spans = new TimeSpan[this.m_time.Count];
                    this.m_time.CopyTo(spans
                        , 0);
                    double milliseconds = 0;
                    foreach (TimeSpan span in spans)
                    {
                        milliseconds += span.TotalMilliseconds;
                    }
                    return TimeSpan.FromMilliseconds(milliseconds / spans.Length);
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }
        #endregion
    }
}