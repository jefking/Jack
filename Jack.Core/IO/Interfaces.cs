using System;
using System.Collections.Generic;

using Jack.Core.Threading;

namespace Jack.Core.IO
{
    /// <summary>
    /// For classes that have hashes
    /// </summary>
    internal interface IHashable
    {
        #region Methods
        /// <summary>
        /// Validate Hash, against Self
        /// </summary>
        /// <param name="hash">Hash</param>
        /// <returns>Is Valid</returns>
        bool Validate(byte[] hash);
        #endregion

        #region Properties
        /// <summary>
        /// Hash
        /// </summary>
        /// <returns>Hash</returns>
        byte[] Hash
        {
            get;
        }
        #endregion
    }
    /// <summary>
    /// Get Block Interface
    /// </summary>
    public interface IGetBlock : IUnique<Guid>
    {
        #region Methods
        /// <summary>
        /// Retrieves the block of data associated with the identifier
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <returns>Block of Data</returns>
        byte[] GetBlock(System.Guid identifier);
        #endregion
    }
    /// <summary>
    /// File Interface for the Blocks of Data
    /// </summary>
    public interface IFiler : IGetBlock
        , IAggregateUnique<Guid>
        , IDisposable
    {
        #region Methods
        /// <summary>
        /// Saves a block of data, and identifier for later retrieval
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="block">Block of Data</param>
        void SaveBlock(Guid identifier
            , byte[] block);
        /// <summary>
        /// Deletes a block of data associated with the identifier
        /// </summary>
        /// <param name="identifier">Identifier</param>
        void DeleteBlock(Guid identifier);
        #endregion
    }
    /// <summary>
    /// ILoad
    /// </summary>
    internal interface ILoad
    {
        #region Properties
        /// <summary>
        /// Load State
        /// </summary>
        LoadingState LoadState
        {
            get;
        }
        #endregion
    }
    /// <summary>
    /// Block Interface
    /// </summary>
    public interface IBlock : IUnique<Guid>
    {
        #region Properties
        /// <summary>
        /// Block
        /// </summary>
        byte[] Data
        {
            get;
        }
        #endregion
    }
    /// <summary>
    /// File Interface
    /// </summary>
    public interface IFile : IAggregateUnique<Guid>
    {
        #region Properties
        /// <summary>
        /// Hash
        /// </summary>
        byte[] Hash
        {
            get;
        }
        /// <summary>
        /// Data Blocks
        /// </summary>
        IBlock[] DataBlocks
        {
            get;
        }
        #endregion
    }
    /// <summary>
    /// Operations
    /// </summary>
    public interface IDataOperations
    {
        #region Methods
        /// <summary>
        /// Store
        /// </summary>
        /// <param name="unc">UNC Name</param>
        void Store(string unc
            , byte[] payload);
        /// <summary>
        /// Retrieve
        /// </summary>
        /// <param name="unc">UNC Name</param>
        /// <returns>File</returns>
        IFile Retrieve(string unc);
        #endregion
    }
}