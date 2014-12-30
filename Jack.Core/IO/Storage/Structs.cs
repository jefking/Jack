namespace Jack.Core.IO.Storage
{
    /// <summary>
    /// Hard Drive Space
    /// </summary>
    public struct HardDriveSpace
    {
        #region Properties
        /// <summary>
        /// Free Bytes Available To Caller
        /// </summary>
        public long FreeBytesAvailableToCaller;
        /// <summary>
        /// Total Number Of Bytes
        /// </summary>
        public long TotalNumberOfBytes;
        /// <summary>
        /// Total Number Of Free Bytes
        /// </summary>
        public long TotalNumberOfFreeBytes;
        /// <summary>
        /// Result
        /// </summary>
        public long Result;
        #endregion
    }
}