using System;
using System.Collections.Generic;

using Jack.Core.IO;
using Jack.Core.XML;

using Jack.Logger;

namespace Jack.Core.IO.Data
{
    /// <summary>
    /// Singleton repository for file manifest and transactional metadata
    /// </summary>
    public sealed class DataStore
    {
        #region Members
        /// <summary>
        /// Instance
        /// </summary>
        private static readonly DataStore s_instance = new DataStore();
        /// <summary>
        /// Manifest Xml
        /// </summary>
        private readonly ManifestXml m_manifests;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks>
        /// Private to enforce singleton pattern
        /// </remarks>
        private DataStore()
            :base()
        {
            using (var log = new TraceContext())
            {
                this.m_manifests = ManifestXml.Instance;

                log.Debug("m_manifests={0}"
                    , this.m_manifests);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get Manifest
        /// </summary>
        /// <param name="filename">File Name</param>
        /// <returns>File Manifest</returns>
        public FileManifest GetManifest(string unc)
        {
            using (var log = new TraceContext())
            {
                log.Debug("unc={0}"
                   , unc);

                if (string.IsNullOrEmpty(unc))
                {
                    log.Debug("no filename");
                    throw new ArgumentNullException("file name");
                }
                else
                {
                    return this.m_manifests.Get(unc);
                }
            }
        }
        /// <summary>
        /// Get Manifest
        /// </summary>
        /// <param name="filename">File Name</param>
        /// <returns>File Manifest</returns>
        public FileManifest GetManifest(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                   , identifier);

                if (Guid.Empty == identifier)
                {
                    log.Debug("no identifier");
                    throw new ArgumentNullException("identifier");
                }
                else
                {
                    return this.m_manifests.Get(identifier);
                }
            }
        }
        /// <summary>
        /// Put Manifest
        /// </summary>
        /// <param name="manifest">File Manifest</param>
        public void Put(FileManifest manifest)
        {
            using (var log = new TraceContext())
            {
                log.Info("manifest={0}"
                    , manifest);

                if (null == manifest)
                {
                    log.Debug("no manifest");
                    throw new NullReferenceException("Manifest");
                }
                else if (string.IsNullOrEmpty(manifest.UniversalNamingConvention))
                {
                    log.Debug("no filename");
                    throw new ArgumentNullException("filename");
                }
                else if (Guid.Empty == manifest.Identifier)
                {
                    log.Debug("no identifier");
                    throw new ArgumentNullException("identifier");
                }
                else if (this.m_manifests.HasFile(manifest))
                {
                    this.m_manifests.Replace(manifest);
                }
                else
                {
                    this.m_manifests.Store(manifest);
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Singleton accessor
        /// </summary>
        public static DataStore Instance { get { return s_instance; } }
        #endregion
    }
}