using System;
using System.Collections.Generic;

namespace Jack.Core
{
    /// <summary>
    /// IUnique, use to uniquely identify and object
    /// </summary>
    public interface IUnique<DataType>
    {
        #region Properties
        /// <summary>
        /// Object Identifier
        /// </summary>
        DataType Identifier
        {
            get;
        }
        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="DataType"></typeparam>
    public interface IAggregateUnique<DataType>
    {
        #region Properties
        /// <summary>
        /// Object Identifier
        /// </summary>
        IEnumerable<DataType> Identifiers
        {
            get;
        }
        #endregion
    }
    /// <summary>
    /// Initialize
    /// </summary>
    public interface IInitialize
    {
        #region Methods
        /// <summary>
        /// Initializes Lifetime Service
        /// </summary>
        void Initialize();
        #endregion
    }
    /// <summary>
    /// Standardize the life time of serving objects
    /// </summary>
    public interface ILifetime : IDisposable
        , IInitialize
    {
        #region Methods
        /// <summary>
        /// Loads Lifetime Service
        /// </summary>
        void Load();
        /// <summary>
        /// Unloads Lifetime Service
        /// </summary>
        void Unload();
        #endregion
    }
}