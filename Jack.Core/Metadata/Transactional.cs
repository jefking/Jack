using System;

namespace Jack.Core.Metadata
{
    [Serializable]
    public class BlockTransfer : MetaData
    {
        #region Properties
        Guid SrcPeerID;
        Guid DestPeerID;
        DateTime Start;
        DateTime End;
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// need?  maybe just bool was full/partial
        /// </remarks>
        Guid BlockID;
        #endregion
    }
    [Serializable]
    public class Dedup : MetaData
    {
        #region Properties
        Guid FileID; 
        long Version;
        TimeSpan DurationInCleartext;
        long TotalBlockCount;
        long DuplicatedBlockCount;
        #endregion
    }
}