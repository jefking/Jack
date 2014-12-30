using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using Jack.Logger;

namespace Jack.Core.IO
{
    /// <summary>
    /// Block
    /// </summary>
    [Serializable]
    public class Block : IBlock
    {
        #region Properties
        /// <summary>
        /// Identifier
        /// </summary>
        public Guid m_identifier = Guid.NewGuid();

        #region IBlock Members
        /// <summary>
        /// Data
        /// </summary>
        /// <remarks>
        /// size = Constants.BlockSize
        /// </remarks>
        public byte[] m_data = Cloneable.Block.Clone() as byte[];
        /// <summary>
        /// Hash
        /// </summary>
        /// <remarks>
        /// size = Constants.HashSize
        /// </remarks>
        public byte[] m_hash = Cloneable.Hash.Clone() as byte[];
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Compute Hash
        /// </summary>
        /// <param name="algorithm">Algorithm</param>
        internal byte[] ComputeHash(HashAlgorithm algorithm)
        {
            using (var log = new TraceContext())
            {
                log.Debug("algorithm={0}"
                    , algorithm);

                return this.m_hash = algorithm.ComputeHash(this.m_data
                    , 0
                    , this.m_data.Length);
            }
        }
        /// <summary>
        /// Pad Bytes
        /// </summary>
        /// <remarks>
        /// Creates a full block;
        /// when the payload is smaller then block size
        /// </remarks>
        /// <param name="payload">Payload</param>
        /// <param name="size">Size</param>
        /// <returns>Padded Load</returns>
        private static byte[] PadOutBytes(byte[] payload
            , byte[] fullSize)
        {
            using (var log = new TraceContext())
            {
                if (null == payload)
                {
                    log.Warn("payload=NULL");
                    return null;
                }
                else if (null == fullSize)
                {
                    log.Warn("fullSize=NULL");
                    return null;
                }
                else if (0 == fullSize.LongLength)
                {
                    throw new ApplicationException("Full size array to small (Length=0).");
                }
                else if (fullSize.LongLength < payload.LongLength)
                {
                    throw new ApplicationException("Full size array smaller than payload.");
                }
                else
                {
                    Array.Copy(payload
                        , fullSize
                        , payload.LongLength);

                    return fullSize;
                }
            }
        }
        /// <summary>
        /// Create Blocks
        /// </summary>
        /// <remarks>
        /// Creates a set of blocks from a single payload
        /// -thread safe
        /// -optimized for performance
        /// -returns NULL on error cases
        /// </remarks>
        /// <param name="payload">Payload</param>
        /// <returns>Blocks</returns>
        public static IEnumerable<IBlock> CreateBlocks(byte[] payload)
        {
            using (var log = new TraceContext())
            {
                if (null == payload)
                {
                    log.Warn("payload=NULL");
                    return null;
                }
                else
                {
                    long length = payload.LongLength;
                    if (length == 0)
                    {
                        log.Warn("payload.Length=0");
                        return null;
                    }
                    else
                    {
                        Block block = null;
                        int capacity = length % Constants.BlockSize == 0
                            ? Convert.ToInt32((length / Constants.BlockSize))
                            : Convert.ToInt32((length / Constants.BlockSize)) + 1;
                        IList<IBlock> blocks = new List<IBlock>(capacity);//Capacity set to proper length
                        if (length > Constants.BlockSize)
                        {
                            long i = 0;
                            for (; i + Constants.BlockSize < length; i += Constants.BlockSize)
                            {
                                block = new Block();
                                Array.Copy(payload
                                    , i
                                    , block.Data
                                    , 0
                                    , Constants.BlockSize);
                                blocks.Add(block);
                            }
                            if (i < length)//Last Remaining incomplete block
                            {
                                block = new Block();
                                Array.Copy(payload
                                    , i
                                    , block.Data
                                    , 0
                                    , Constants.BlockSize);
                                blocks.Add(block);
                            }
                        }
                        else
                        {
                            blocks = new List<IBlock>(1);
                            block = new Block();
                            block.Data = (payload.LongLength < Constants.BlockSize)
                                ? Block.PadOutBytes(payload
                                        , block.Data)
                                : payload;
                            blocks.Add(block);
                        }
                        return blocks;
                    }
                }
            }
        }
        #endregion

        #region IBlock Members
        /// <summary>
        /// Identifier
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return this.m_identifier;
            }
            set
            {
                this.m_identifier = value;
            }
        }
        /// <summary>
        /// Data
        /// </summary>
        /// <remarks>
        /// size = Constants.BlockSize
        /// </remarks>
        public byte[] Data
        {
            get
            {
                return this.m_data;
            }
            set
            {
                this.m_data = value;
            }
        }
        /// <summary>
        /// Hash
        /// </summary>
        public byte[] Hash
        {
            get
            {
                return this.m_hash;
            }
            set
            {
                this.m_hash = value;
            }
        }
        #endregion
    }
}