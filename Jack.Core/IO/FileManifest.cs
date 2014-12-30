using System;
using System.Collections.Generic;

using Jack.Core.XML;

namespace Jack.Core.IO
{
    /// <summary>
    /// File Manifest
    /// </summary>
    [Serializable]
    public class FileManifest : IUnique<Guid>
        , IUnique<string>
    {
        #region Members
        /// <summary>
        /// Identifier
        /// </summary>
        private Guid m_identifier = Guid.Empty;
        /// <summary>
        /// UNC
        /// </summary>
        private string m_universalNamingConvention = null;
        /// <summary>
        /// Versions
        /// </summary>
        private Stack<VersionManifest> m_versions = new Stack<VersionManifest>(1);
        #endregion

        #region Properties
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
        /// UNC
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
        /// Versions
        /// </summary>
        [XmlHex]
        public Stack<VersionManifest> Versions
        {
            get
            {
                return this.m_versions;
            }
            set
            {
                this.m_versions = value;
            }
        }

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
        #endregion
    }
}