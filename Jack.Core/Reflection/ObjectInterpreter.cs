using System;
using System.Collections.Generic;
using System.Reflection;

using Jack.Logger;

namespace Jack.Core.Reflection
{
    /// <summary>
    /// Object Interpreter
    /// </summary>
    internal class ObjectInterpreter<T>
    {
        #region Members
        /// <summary>
        /// Binding Flags
        /// </summary>
        private const BindingFlags c_bindingFlags = BindingFlags.Public
            | BindingFlags.Instance
            | BindingFlags.ExactBinding;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        internal ObjectInterpreter()
            : base()
        {
            using (var log = new TraceContext())
            {
                log.Debug("Type {0}"
                    , typeof(T).FullName);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Properties
        /// </summary>
        /// <returns>Property Info's</returns>
        internal IEnumerable<PropertyInfo> Properties()
        {
            using (var log = new TraceContext())
            {
                return this.Properties(Operation.Read);
            }
        }
        /// <summary>
        /// Properties
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="operation">Operation</param>
        /// <returns>Property Info's</returns>
        internal IEnumerable<PropertyInfo> Properties(Operation operation)
        {
            using (var log = new TraceContext())
            {
                log.Debug("operation={0}"
                    , operation);

                ObjectMeta<T> objectMeta = this.MetaData();
                switch (operation)
                {
                    case Operation.Read:
                        return objectMeta.Read;
                    case Operation.Write:
                        return objectMeta.Write;
                    default:
                        throw new InvalidFilterCriteriaException("Invalid Operation.");
                }
            }
        }
        /// <summary>
        /// Meta Data
        /// </summary>
        /// <returns>Object Meta</returns>
        internal ObjectMeta<T> MetaData()
        {
            using (var log = new TraceContext())
            {
                Type type = typeof(T);
                ObjectMeta<T> objectMeta = new ObjectMeta<T>();
                while (null != type
                    && !(type.IsPrimitive)
                    && !(type.IsInterface)
                    && !(type.IsAbstract))
                {
                    foreach (PropertyInfo prop in type.GetProperties(c_bindingFlags))
                    {
                        if (null != prop) // Add Ignore Attribute
                        {
                            objectMeta.AddProperty(prop);
                        }
                    }

                    type = type.BaseType;
                }
                return objectMeta;
            }
        }
        #endregion
    }
}