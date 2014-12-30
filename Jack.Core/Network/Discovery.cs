using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

using Jack.Core.LDAP;

using Jack.Logger;

namespace Jack.Core.Network
{
    /// <summary>
    /// Discovers Network information
    /// </summary>
    internal class Discovery
    {
        #region Structs
        /// <summary>
        /// Computer Struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ComputerStruct
        {
            #region Properties
            /// <summary>
            /// Platform Id
            /// </summary>
            internal int PlatformId;
            /// <summary>
            /// Name
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string Name;
            #endregion
        }
        #endregion

        #region Dll Imports
        /// <summary>
        /// Netapi32 : NetServerEnum method import
        /// </summary>
        /// <param name="ServerNane"></param>
        /// <param name="dwLevel"></param>
        /// <param name="pBuf"></param>
        /// <param name="dwPrefMaxLen"></param>
        /// <param name="dwEntriesRead"></param>
        /// <param name="dwTotalEntries"></param>
        /// <param name="dwServerType"></param>
        /// <param name="domain"></param>
        /// <param name="dwResumeHandle"></param>
        /// <returns></returns>
        [DllImport("Netapi32"
                , CharSet = CharSet.Auto
                , SetLastError = true)
            , SuppressUnmanagedCodeSecurityAttribute]
        private static extern int NetServerEnum(string ServerNane//must be null
            , int dwLevel
            , ref IntPtr pBuf
            , int dwPrefMaxLen
            , out int dwEntriesRead
            , out int dwTotalEntries
            , int dwServerType
            , string domain//null for login domain
            , out int dwResumeHandle);
        /// <summary>
        /// Netapi32 : NetApiBufferFree method import
        /// </summary>
        /// <param name="pBuf">Ptr</param>
        /// <returns></returns>
        [DllImport("Netapi32"
            , SetLastError = true)
            , SuppressUnmanagedCodeSecurityAttribute]
        private static extern int NetApiBufferFree(IntPtr pBuf);
        #endregion

        #region Public Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Discovery()
            :base()
        {
            using (var log = new TraceContext()) { }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Networked Computers
        /// </summary>
        /// <returns>Computers</returns>
        public IList<Computer> NetworkedComputers()
        {
            using (var log = new TraceContext())
            {
                IList<Computer> networkComputers = new List<Computer>();
                const int maxPreferredLength = -1;
                int workstationType = 1;
                int serverType = 2;
                IntPtr buffer = IntPtr.Zero;
                IntPtr tmpBuffer = IntPtr.Zero;
                int entriesRead = 0;
                int totalEntries = 0;
                int resHandle = 0;
                int sizeofINFO = Marshal.SizeOf(typeof(ComputerStruct));

                try
                {
                    int ret = Discovery.NetServerEnum(null
                        , 100
                        , ref buffer
                        , maxPreferredLength
                        , out entriesRead
                        , out totalEntries
                        , workstationType
                            | serverType
                        , null
                        , out resHandle);

                    if (0 == ret)
                    {
                        for (int i = 0; i < totalEntries; i++)
                        {
                            tmpBuffer = new IntPtr((int)buffer +
                                       (i * sizeofINFO));

                            ComputerStruct svrInfo = (ComputerStruct)Marshal.PtrToStructure(tmpBuffer
                                , typeof(ComputerStruct));

                            networkComputers.Add(new Computer(svrInfo.Name));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Problem with acessing network computers in {0}: {1}"
                        , this.GetType()
                        , ex.Message));
                }
                finally
                {
                    Discovery.NetApiBufferFree(buffer);
                }
                return networkComputers;
            }
        }
        #endregion
    }
}