using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using Jack.Core.XML;

using Jack.Logger;

namespace Jack.Core.Reflection
{
    /// <summary>
    /// Object Meta
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    internal class ObjectMeta<T>
    {
        #region Methods
        /// <summary>
        /// Properties
        /// </summary>
        private List<PropertyInfo> m_properties;
        /// <summary>
        /// Type
        /// </summary>
        private Type m_type;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        internal ObjectMeta()
            : base()
        {
            using (var log = new TraceContext())
            {
                log.Debug("Type {0}"
                    , typeof(T).FullName);

                this.m_type = typeof(T);
                this.m_properties = new List<PropertyInfo>();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add Property
        /// </summary>
        /// <param name="prop">Property Info</param>
        internal void AddProperty(PropertyInfo prop)
        {
            using (var log = new TraceContext())
            {
                if (!(this.m_properties.Contains(prop)))
                {
                    this.m_properties.Add(prop);
                }
            }
        }
        /// <summary>
        /// Get Value
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="propName">Property Name</param>
        /// <returns>Object</returns>
        internal object GetValue(object obj
                                , string propName)
        {
            using (var log = new TraceContext())
            {
                foreach (PropertyInfo prop in this.m_properties)
                {
                    if (prop.CanRead
                        && propName == prop.Name)
                    {
                        return prop.GetValue(obj
                            , null);
                    }
                }
                throw new InvalidOperationException(string.Format("Can't Read: '{0}', on '{1}'."
                                                                    , propName
                                                                    , obj.GetType()));
            }
        }
        /// <summary>
        /// Set Value
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="prop">Property Name</param>
        /// <param name="value">Value</param>
        internal void SetValue(object obj
            , string prop
            , object value)
        {
            using (var log = new TraceContext())
            {
                log.Info(prop);
                if (null != obj)
                {
                    object v;
                    foreach (PropertyInfo propInfo in this.Write)
                    {
                        if (prop == propInfo.Name)
                        {
                            try
                            {
                                v = (propInfo.PropertyType == typeof(Guid))
                                        ? new Guid(value as string)
                                        : value;

                                propInfo.SetValue(obj
                                    , System.Convert.ChangeType(v
                                        , propInfo.PropertyType)
                                    , null);
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }
                        }
                    }
                }
                else
                {
                    log.Warn("obj is null");
                }
            }
        }
        /// <summary>
        /// Custom Attributes
        /// </summary>
        /// <typeparam name="cA"></typeparam>
        /// <returns></returns>
        internal IList<cA> CustomAttributes<cA>()
        {
            using (var log = new TraceContext())
            {
                log.Debug("type={0}"
                    , typeof(cA).FullName);

                object[] objects = this.m_type.GetCustomAttributes(typeof(cA)
                    , true);
                IList<cA> attributes = new List<cA>();
                foreach (cA obj in objects)
                {
                    attributes.Add(obj);
                }
                return attributes;
            }
        }
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="doc">Xml Document</param>
        /// <returns>Xml Node</returns>
        internal XmlNode Convert(object obj
            , XmlDocument doc)
        {
            using (var log = new TraceContext())
            {
                XmlNode child;
                XmlElement node = doc.CreateElement(this.m_type.Name);

                object objPtr;
                object[] ignoreAttributes;
                object[] hex;
                foreach (PropertyInfo prop in this.Read)
                {
                    ignoreAttributes = prop.GetCustomAttributes(typeof(XmlIgnoreAttribute)
                        , true);
                    hex = prop.GetCustomAttributes(typeof(XmlHexAttribute)
                        , true);
                    objPtr = prop.GetValue(obj
                            , null);
                    if (null != hex
                        && 0 < hex.Length)
                    {
                        child = doc.CreateElement(prop.Name);
                        child.InnerText = Utility.ConvertToHex(objPtr);
                        node.AppendChild(child);
                    }
                    else if (null == ignoreAttributes
                        || 0 == ignoreAttributes.Length)
                    {
                        node.SetAttribute(prop.Name
                            , System.Convert.ToString(objPtr));
                    }
                }

                return node;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Read Properties
        /// </summary>
        internal IEnumerable<PropertyInfo> Read
        {
            get
            {
                IDictionary<string, PropertyInfo> readProperties = new Dictionary<string, PropertyInfo>();
                foreach (PropertyInfo prop in this.m_properties)
                {
                    if (prop.CanRead
                        && !(readProperties.ContainsKey(prop.Name)))
                    {
                        readProperties.Add(prop.Name
                            , prop);
                    }
                }
                return readProperties.Values;
            }
        }
        /// <summary>
        /// Write Properties
        /// </summary>
        internal IList<PropertyInfo> Write
        {
            get
            {
                IList<PropertyInfo> writeProperties = new List<PropertyInfo>(this.m_properties.Count);
                foreach (PropertyInfo prop in this.m_properties)
                {
                    if (prop.CanWrite)
                    {
                        writeProperties.Add(prop);
                    }
                }
                return writeProperties;
            }
        }
        #endregion
    }
}