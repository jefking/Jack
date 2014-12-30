using System;

using Jack.Core.IO;

namespace Jack.Test
{
    /// <summary>
    /// Constants
    /// </summary>
    internal struct Constants
    {
        /// <summary>
        /// Server Port
        /// </summary>
        internal const short ServerPort = 3456;
    }
    /// <summary>
    /// Testing Helper
    /// </summary>
    internal static class Helper
    {
        #region Methods
        /// <summary>
        /// Random Block
        /// </summary>
        /// <returns>Block Data</returns>
        public static byte[] RandomBlock()
        {
            Random random = new Random();
            byte[] block = Cloneable.Block.Clone() as byte[];
            random.NextBytes(block);
            return block;
        }
        #endregion
    }
}