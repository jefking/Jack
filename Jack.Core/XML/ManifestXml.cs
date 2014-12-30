using System;
using System.Collections.Generic;
using System.Xml;

using Jack.Core.XML;
using Jack.Core.IO;

using Jack.Logger;

namespace Jack.Core.XML
{
    /// <summary>
    /// Manifest Storage Class
    /// </summary>
    public class ManifestXml : XmlDocument<FileManifest>
    {
        #region Members
        /// <summary>
        /// XML Storage
        /// </summary>
        private const string c_dataFile = "manifest.xml";
        /// <summary>
        /// Path
        /// </summary>
        private static readonly string s_fullPath;
        /// <summary>
        /// Manifest Instance
        /// </summary>
        private static ManifestXml s_instance;
        /// <summary>
        /// Manifests
        /// </summary>
        /// <remarks>
        /// Make Thread Safe?
        /// </remarks>
        private readonly IList<FileManifest> m_manifests;
        #endregion

        #region Constructors
        /// <summary>
        /// Static Constructor
        /// </summary>
        static ManifestXml()
        {
            using (var log = new TraceContext())
            {
                s_fullPath = string.Format("{0}\\{1}"
                    , FileHelper.BinDirectory
                    , c_dataFile);
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManifestXml()
            : base(true)
        {
            using (var log = new TraceContext())
            {
                this.m_manifests = base.ReadAll();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Has File
        /// </summary>
        /// <param name="manifest">manifest</param>
        /// <returns>Has File</returns>
        internal bool HasFile(FileManifest manifest)
        {
            using (var log = new TraceContext())
            {
                return null != manifest
                    && (this.HasFile(manifest.Identifier)
                    || this.HasFile(manifest.UniversalNamingConvention));
            }
        }
        /// <summary>
        /// Has File
        /// </summary>
        /// <param name="unc">UNC</param>
        /// <returns>Has File</returns>
        internal bool HasFile(string unc)
        {
            using (var log = new TraceContext())
            {
                log.Debug("UNC={0}"
                    , unc);
                if (string.IsNullOrEmpty(unc))
                {
                    return false;
                }
                else
                {
                    return null != this.Select(unc);
                }
            }
        }
        /// <summary>
        /// Has File
        /// </summary>
        /// <param name="unc"></param>
        /// <returns></returns>
        internal bool HasFile(Guid guid)
        {
            using (var log = new TraceContext())
            {
                log.Debug("Identifier={0}"
                    , guid);
                if (guid != Guid.Empty)
                {
                    return false;
                }
                else
                {
                    return null != this.Select(guid);
                }
            }
        }
        /// <summary>
        /// Get File Manifest
        /// </summary>
        /// <param name="manifest">File Manifest</param>
        /// <returns>File Manifest</returns>
        internal FileManifest Get(FileManifest manifest)
        {
            using (var log = new TraceContext())
            {
                FileManifest fileManifest = null;
                if (null != manifest)
                {
                    if (Guid.Empty != manifest.Identifier
                        && this.HasFile(manifest.Identifier))
                    {
                        fileManifest = this.Get(manifest.Identifier);
                    }
                    else if (!(string.IsNullOrEmpty(manifest.UniversalNamingConvention))
                        && this.HasFile(manifest.UniversalNamingConvention))
                    {
                        fileManifest = this.Get(manifest.UniversalNamingConvention);
                    }
                }
                return fileManifest;
            }
        }
        /// <summary>
        /// Get Manifest
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <returns></returns>
        internal FileManifest Get(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                    , identifier);
                if (this.HasFile(identifier))
                {
                    foreach (FileManifest fileManifest in this.m_manifests)
                    {
                        if (fileManifest.Identifier == identifier)
                        {
                            return fileManifest;
                        }
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Ge tManifest
        /// </summary>
        /// <param name="unc">UNC</param>
        /// <returns>File Manifest</returns>
        internal FileManifest Get(string unc)
        {
            using (var log = new TraceContext())
            {
                log.Debug("unc={0}"
                    , unc);
                if (this.HasFile(unc))
                {
                    foreach (FileManifest fileManifest in this.m_manifests)
                    {
                        if (fileManifest.UniversalNamingConvention == unc)
                        {
                            return fileManifest;
                        }
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Object Stored
        /// </summary>
        /// <param name="manifest">File Manifest</param>
        protected override void ObjectStored(FileManifest manifest)
        {
            using (var log = new TraceContext())
            {
                this.m_manifests.Add(manifest);
            }
        }
        /// <summary>
        /// Replace File Manifest
        /// </summary>
        /// <param name="manifest">File Manifest</param>
        public void Replace(FileManifest manifest)
        {
            using (var log = new TraceContext())
            {
                if (this.HasFile(manifest))
                {
                    base.DocumentElement.RemoveChild(this.Select(manifest.Identifier));

                    int index = 0;
                    foreach (FileManifest fileManifest in this.m_manifests)
                    {
                        if (fileManifest.Identifier == manifest.Identifier)
                        {
                            break;
                        }
                        index++;
                    }
                    this.m_manifests.RemoveAt(index);

                    base.Store(manifest);
                }
                else
                {
                    throw new InvalidOperationException("Manifest doesn't already exist.");
                }
            }
        }
        /// <summary>
        /// Select Node by UNC
        /// </summary>
        /// <param name="unc">unc</param>
        /// <returns>XML node</returns>
        private XmlNode Select(string unc)
        {
            using (var log = new TraceContext())
            {
                log.Debug("unc={0}"
                    , unc);
                return base.SelectSingleNode(string.Format("FileManifests/FileManifest[@UniversalNamingConvention='{0}']"
                               , unc));
            }
        }
        /// <summary>
        /// Select Node by Identifier
        /// </summary>
        /// <param name="unc">Identifier</param>
        /// <returns>XML node</returns>
        private XmlNode Select(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                    , identifier);
                return base.SelectSingleNode(string.Format("FileManifests/FileManifest[@Identifier='{0}']"
                               , identifier));
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Full Path
        /// </summary>
        protected override string FullPath
        {
            get
            {
                return s_fullPath;
            }
        }
        /// <summary>
        /// Mnifest Instance
        /// </summary>
        internal static ManifestXml Instance
        {
            get
            {
                s_instance = s_instance ?? new ManifestXml();
                return s_instance;
            }
        }
        #endregion
    }
}