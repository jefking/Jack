using System;

using Jack.Core.IO;

namespace Jack.Core.Metadata
{
    /// <summary>
    /// New Version
    /// </summary>
    [Serializable]
    public class NewVersion : MetaData
    {
        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public NewVersion()
            :base()
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// File Id
        /// </summary>
        public Guid FileId;
        /// <summary>
        /// Version Number
        /// </summary>
        public long VersionNumber;
        /// <summary>
        /// Blocks
        /// </summary>
        public Sequence<Guid> Blocks;
        #endregion
    }
}