using System;
using System.Collections.Generic;

using Jack.Logger;

namespace Jack.Core.Crypto
{
    /// <summary>
    /// For symmetric encryption of block data.
    /// </summary>
    public sealed class Cipher
    {
        #region Members
        /// <summary>
        /// Keystore Mutxe
        /// </summary>
        private static readonly object m_keystoreMutex = new object();
        /// <summary>
        /// Key Store
        /// </summary>
        private static Dictionary<Guid, byte[]> m_keystore = new Dictionary<Guid, byte[]>();
        #endregion

        #region Methods
        /// <summary>
        /// Get Key
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <returns></returns>
        public static byte[] GetKey(Guid identifier)
        {
            using (var log = new TraceContext())
            {
                log.Debug("identifier={0}"
                    , identifier);

                lock (m_keystoreMutex)
                {
                    if (m_keystore.ContainsKey(identifier))
                    {
                        return m_keystore[identifier];
                    }
                }

                //TODO: fetch from somewhere...add to keystore
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="keyID"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] plaintext
            , Guid keyID)
        {
            using (var log = new TraceContext())
            {
                //TODO:
                return plaintext;
            }
        }
        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="keyID"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] ciphertext
            , Guid keyID)
        {
            using (var log = new TraceContext())
            {
                //TODO:
                return ciphertext;
            }
        }
        #endregion
    }
}