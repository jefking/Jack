using System.Collections.Generic;
using System.IO;
using System.Xml;

using Jack.Core.Reflection;

using Jack.Logger;

namespace Jack.Core.XML
{
    /// <summary>
    /// Base XML Document
    /// </summary>
    public abstract class XmlDocument<T> : XmlDocument
    {
        #region Members
        /// <summary>
        /// Object Interpreter
        /// </summary>
        private ObjectInterpreter<T> m_interpreter;
        /// <summary>
        /// Save Lock
        /// </summary>
        private readonly object m_diskLock = new object();
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        protected XmlDocument(bool autoCreateRoot)
            : base()
        {
            using (var log = new TraceContext())
            {
                log.Debug("Type {0}"
                    , typeof(T).FullName);
                this.m_interpreter = new ObjectInterpreter<T>();

                this.Load();

                if (null == this.DocumentElement
                    && autoCreateRoot)
                {
                    this.CreateEmptyXml();
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Load
        /// </summary>
        private void Load()
        {
            using (var log = new TraceContext())
            {
                lock (this.m_diskLock)
                {
                    if (File.Exists(this.FullPath))
                    {
                        base.Load(this.FullPath);
                    }
                }
            }
        }
        /// <summary>
        /// Creates an Empty Xml Document, Ready for Insertion
        /// is Stand Alone
        /// </summary>
        /// <returns>Root Element</returns>
        public XmlElement CreateEmptyXml()
        {
            using (var log = new TraceContext())
            {
                return this.CreateEmptyXml(true);
            }
        }
        /// <summary>
        /// Creates an Empty Xml Document, Ready for Insertion
        /// </summary>
        /// <param name="StandAlone">is Stand Alone</param>
        /// <returns>Root Element</returns>
        public XmlElement CreateEmptyXml(bool StandAlone)
        {
            using (var log = new TraceContext())
            {
                if (null == base.DocumentElement)
                {
                    XmlDeclaration xmlDeclaration = base.CreateXmlDeclaration("1.0"
                        , "utf-8"
                        , "yes");
                    XmlElement root = base.CreateElement(this.RootNodePath);
                    base.AppendChild(xmlDeclaration);
                    base.AppendChild(root);
                    return root;
                }
                else
                {
                    return base.DocumentElement;
                }
            }
        }
        /// <summary>
        /// Read All
        /// </summary>
        /// <returns>All</returns>
        public IList<T> ReadAll()
        {
            using (var log = new TraceContext())
            {
                IList<T> items = new List<T>();
                foreach (XmlNode node in this.DocumentElement.ChildNodes)
                {
                    items.Add(this.CreateInstance(node));
                }
                return items;
            }
        }
        /// <summary>
        /// Creates an Instance of an Object based on Node and Type
        /// </summary>
        /// <param name="node">Node (Data)</param>
        /// <returns>The Object Insstantiated</returns>
        protected T CreateInstance(XmlNode node)
        {
            using (var log = new TraceContext())
            {
                T obj = default(T);
                if (null != node)
                {
                    ObjectMeta<T> meta = this.m_interpreter.MetaData();
                    obj = (T)System.Activator.CreateInstance(typeof(T));
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        meta.SetValue(obj
                            , attribute.Name
                            , attribute.Value);
                    }
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        meta.SetValue(obj
                           , child.Name
                           , Utility.ConvertObjectFromHex(node.InnerText));
                    }
                }
                else
                {
                    throw new System.NullReferenceException("XML Node is null.");
                }
                return obj;
            }
        }
        /// <summary>
        /// Save Object
        /// </summary>
        /// <param name="obj">Object</param>
        public void Store(T obj)
        {
            using (var log = new TraceContext())
            {
                if (null != obj)
                {
                    ObjectMeta<T> meta = this.m_interpreter.MetaData();
                    XmlNode node = meta.Convert(obj
                        , this);

                    this.DocumentElement.AppendChild(node);

                    lock (this.m_diskLock)
                    {
                        this.Save(this.FullPath);
                    }

                    this.ObjectStored(obj);
                }
            }
        }
        /// <summary>
        /// Object Stored
        /// </summary>
        /// <param name="obj">Object</param>
        protected abstract void ObjectStored(T obj);
        #endregion

        #region Properties
        /// <summary>
        /// Root Node Path
        /// </summary>
        protected virtual string RootNodePath
        {
            get
            {
                return string.Format("{0}s"
                    , typeof(T).Name);
            }
        }
        /// <summary>
        /// Full Path
        /// </summary>
        protected abstract string FullPath
        {
            get;
        }
        #endregion
    }
}