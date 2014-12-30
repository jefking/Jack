using System;
using System.Collections.Generic;

using Jack.Core.LDAP;
using Jack.Core.XML;

namespace Jack.Core.IO
{
    /// <summary>
    /// Version Manifest
    /// </summary>
    [Serializable]
    public class VersionManifest : IUnique<Guid>
        , IUnique<string>
    {
        #region Members
        /// <summary>
        /// Identifier
        /// </summary>
        private Guid m_identifier = Guid.Empty;
        /// <summary>
        /// Universal Naming Convention
        /// </summary>
        private string m_universalNamingConvention;
        #endregion

        #region Properties
        /// <summary>
        /// Full UNC path of file in storage system
        /// </summary>
        public string UniversalNamingConvention
        {
            get
            {
                return this.m_universalNamingConvention;
            }
            set
            {
                this.m_universalNamingConvention = value;
            }
        }
        /// <summary>
        /// File Length
        /// </summary>
        public long UnencryptedFileLength = long.MinValue;
        /// <summary>
        /// Created
        /// </summary>
        public DateTime Created = DateTime.Now;
        /// <summary>
        /// Blocks
        /// </summary>
        /// <remarks>
        /// Capacity should be set
        /// </remarks>
        public HashSet<Guid> Blocks = new HashSet<Guid>();
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
        #endregion

        #region IUnique<string> Members
        /// <summary>
        /// Identifier
        /// </summary>
        string IUnique<string>.Identifier
        {
            get
            {
                return this.m_universalNamingConvention;
            }
        }
        #endregion

        #region IUnique<Guid> Members
        /// <summary>
        /// Identifier
        /// </summary>
        Guid IUnique<Guid>.Identifier
        {
            get
            {
                return this.m_identifier;
            }
        }
        #endregion
    }
}