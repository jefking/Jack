using System;
using System.Collections.Generic;

namespace Jack.Core.IO
{
    /// <summary>
    /// File
    /// </summary>
    [Serializable]
    internal class File : IFile
    {
        #region Members
        /// <summary>
        /// Identifier
        /// </summary>
        private Guid m_identifier = Guid.Empty;
        /// <summary>
        /// Blocks
        /// </summary>
        private List<IBlock> m_blocks = new List<IBlock>();
        #endregion
        
        #region Methods
        /// <summary>
        /// Add Block
        /// </summary>
        /// <param name="block">Block</param>
        internal void AddBlock(IBlock block)
        {
            this.m_blocks.Add(block);
        }
        /// <summary>
        /// Trim Last Block
        /// </summary>
        /// <param name="length">Length</param>
        internal void TrimLastBlock(long length)
        {
            Block block = this.m_blocks[this.m_blocks.Count - 1] as Block;
            byte[] output = new byte[length];
            Array.Copy(block.Data
                , 0
                , output
                , 0
                , length);
            block.Data = output;
        }
        #endregion

        #region IFile Members
        /// <summary>
        /// Identifiers
        /// </summary>
        public IEnumerable<Guid> Identifiers
        {
            get
            {
                IList<Guid> guids = new List<Guid>(this.m_blocks.Count);
                foreach (IBlock block in this.m_blocks)
                {
                    guids.Add(block.Identifier);
                }
                return guids;
            }
        }
        /// <summary>
        /// Hash
        /// </summary>
        public byte[] Hash
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// Data Blocks
        /// </summary>
        public IBlock[] DataBlocks
        {
            get
            {
                return this.m_blocks.ToArray();
            }
            set
            {
                this.m_blocks.Clear();
                this.m_blocks = new List<IBlock>();
                this.m_blocks.AddRange(value);
            }
        }
        #endregion
    }
}
