using Jack.Logger;

namespace Jack.Core
{
    /// <summary>
    /// Event Arguments
    /// </summary>
    public struct EventArguments<data>
    {
        #region Members
        /// <summary>
        /// Data
        /// </summary>
        private readonly data m_data;
        #endregion

        #region Constructor
        /// <summary>
        /// Event Arguments Constructor
        /// </summary>
        /// <param name="data">Data</param>
        public EventArguments(data passedData)
        {
            using (var log = new TraceContext())
            {
                this.m_data = passedData;

                log.Debug("m_data"
                    , this.m_data);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Data
        /// </summary>
        public data Data
        {
            get
            {
                return this.m_data;
            }
        }
        /// <summary>
        /// Empty Operation Errored Event Arguments
        /// </summary>
        public static EventArguments<data> Empty
        {
            get
            {
                return new EventArguments<data>(default(data));
            }
        }
        #endregion
    }
}
