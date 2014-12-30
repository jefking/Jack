using Jack.Logger;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Jack.Core
{
    /// <summary>
    /// Utility Methods
    /// </summary>
    public static class Utility
    {
        #region Variables
        /// <summary>
        /// Hex Format
        /// </summary>
        private const string
            c_hexFormat = "{0:X2}";
        #endregion

        #region Methods
        /// <summary>
        /// Compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is Equal</returns>
        public static bool Compare(byte[] a
            , byte[] b)
        {
            using (var log = new TraceContext())
            {
                if (b == null
                    && a == null)
                {
                    return true;
                }
                else if (b == null
                    || a == null)
                {
                    return false;
                }
                else if (b.LongLength != a.LongLength)
                {
                    return false;
                }

                for (long i = 0; i < b.LongLength; i++)
                {
                    if (a[i] != b[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Serializes an Object
        /// </summary>
        /// <param name="obj">object to Serialize</param>
        /// <returns>bytes</returns>
        internal static byte[] Serialize(object obj)
        {
            using (var log = new TraceContext())
            {
                if (null != obj)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bf.Serialize(ms
                            , obj);
                        return ms.ToArray();
                    }
                }
                else
                {
                    throw new System.NullReferenceException("Object is Null.");
                }
            }
        }
        /// <summary>
        /// Deserialized Bytes
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>object</returns>
        internal static T Deserialize<T>(byte[] bytes)
        {
            using (var log = new TraceContext())
            {
                if (null != bytes
                    && 0 < bytes.LongLength)
                {
                    using (MemoryStream ms = new System.IO.MemoryStream())
                    {
                        ms.Write(bytes
                            , 0
                            , bytes.Length);
                        ms.Position = 0;
                        BinaryFormatter bf = new BinaryFormatter();
                        return (T)bf.Deserialize(ms);
                    }
                }
                else
                {
                    throw new System.NullReferenceException("Bytes are not Set.");
                }
            }
        }
        /// <summary>
        /// Converts Bytes to String
        /// </summary>
        /// <param name="bytes">Bytes to Convert</param>
        /// <param name="Format">Format To Use</param>
        /// <returns>String in Hexidecimal Format</returns>
        private static string ConvertToHex(byte[] bytes)
        {
            using (var log = new TraceContext())
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte inByte in bytes)
                {
                    sb.Append(string.Format(c_hexFormat
                        , inByte));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// Converts Object to Hex
        /// </summary>
        /// <param name="obj">Object to Convert</param>
        /// <returns>Hex</returns>
        public static string ConvertToHex(object obj)
        {
            using (var log = new TraceContext())
            {
                return ConvertToHex(Serialize(obj));
            }
        }
        /// <summary>
        /// Converts Hex to an Object
        /// </summary>
        /// <param name="hex">Hex to Convert</param>
        /// <returns>Object</returns>
        public static object ConvertObjectFromHex(string hex)
        {
            using (var log = new TraceContext())
            {
                return Deserialize<object>(ConvertFromHex(hex));
            }
        }
        /// <summary>
        /// Converts String to Byte[]
        /// </summary>
        /// <param name="Hex"></param>
        /// <returns></returns>
        public static byte[] ConvertFromHex(string hex)
        {
            using (var log = new TraceContext())
            {
                if (string.IsNullOrEmpty(hex))
                {
                    throw new System.ApplicationException("Hexidecimal string is invalid, null || empty.");
                }

                int length = hex.Length;
                if (0 == (length % 2))
                {
                    StringBuilder hexidecimal = new StringBuilder(hex);
                    char[][] bytes = new char[length / 2][];
                    char[] bits = hexidecimal.ToString().ToCharArray();
                    for (uint i = 0; i < length / 2; i++)
                    {
                        bytes[i] = new char[] { bits[i * 2], bits[(i * 2) + 1] };
                    }
                    return ConvertFromHex(bytes);
                }
                else
                {
                    throw new System.ApplicationException("Hexidecimal string is invalid (mod 2 is not equal to 0).");
                }
            }
        }
        /// <summary>
        /// Converts Char[][] To Byte[]
        /// </summary>
        /// <param name="ByteStrings"></param>
        /// <returns></returns>
        private static byte[] ConvertFromHex(char[][] ByteChars)
        {
            using (var log = new TraceContext())
            {
                byte[] bytes = new byte[ByteChars.LongLength];
                for (long i = 0; i < ByteChars.LongLength; i++)
                {
                    bytes[i] = byte.Parse(ByteChars[i][0].ToString() + ByteChars[i][1]
                        , NumberStyles.AllowHexSpecifier);
                }
                return bytes;
            }
        }
        #endregion
    }
}