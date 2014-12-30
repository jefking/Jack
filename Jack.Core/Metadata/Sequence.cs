using System;

namespace Jack.Core.Metadata
{
    /// <summary>
    /// Sequence
    /// </summary>
    /// <typeparam name="T">Identifier</typeparam>
    [Serializable]
    public class Sequence<T> : IComparable<Sequence<T>>
    {
        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Sequence()
            : base()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Compare To
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Comparison</returns>
        public int CompareTo(Sequence<T> other)
        {
            return this.Order.CompareTo(other.Order);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Order
        /// </summary>
        public long Order = -1;
        /// <summary>
        /// Id
        /// </summary>
        public T Id;
        #endregion
    }
}