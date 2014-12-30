namespace Jack.Core.IO
{
    #region Cloneable
    /// <summary>
    /// Cloneable Items for quick memory allocation
    /// </summary>
    public struct Cloneable
    {
        #region Properties
        /// <summary>
        /// Empty Guid
        /// </summary>
        internal static readonly byte[] Guid = new byte[Constants.GuidLength];
        /// <summary>
        /// Empty Block
        /// </summary>
        public static readonly byte[] Block = new byte[Constants.BlockSize];
        /// <summary>
        /// Empty Hash
        /// </summary>
        public static readonly byte[] Hash = new byte[Constants.HashSize];
        /// <summary>
        /// Empty Storage Block
        /// </summary>
        internal static readonly byte[] StorageBlock = new byte[Constants.StorageBlockSize];
        #endregion
    }
    #endregion

    #region Constants
    /// <summary>
    /// Constants
    /// </summary>
    public struct Constants
    {
        #region Members
        /// <summary>
        /// Block Size in bytes
        /// </summary>
        /// <remarks>
        /// This has been tested to ensure that all algorithms consume this variable to ensure that we are able to change block size
        /// as we gain metrics about the system we will be able to tune this properly.
        /// </remarks>
        public const short BlockSize = 1024;
        /// <summary>
        /// Guid Length
        /// </summary>
        internal const byte GuidLength = 16;
        /// <summary>
        /// Storage Block Size
        /// </summary>
        internal const short StorageBlockSize = Constants.GuidLength + Constants.BlockSize;
        /// <summary>
        /// Max File Size
        /// </summary>
        internal const uint MaxFileSize = 1024 * 1024 * 10;//bytes * kilobytes * 10 MB
        /// <summary>
        /// Hash Size in bytes
        /// </summary>
        internal const byte HashSize = 16; //128-bit
        #endregion
    }
    #endregion
}