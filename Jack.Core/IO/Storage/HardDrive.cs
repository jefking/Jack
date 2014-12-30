using System;
using System.Runtime.InteropServices;

using Jack.Core.Metric;
using Jack.Logger;

namespace Jack.Core.IO.Storage
{
    /// <summary>
    /// Hard Drive
    /// </summary>
    public class HardDrive : IUnique<Guid>
        , ILatency
    {
        #region Import
        /// <summary>
        /// Get Drive Type
        /// </summary>
        /// <param name="rootPathName">Root Path Name</param>
        /// <returns>Type</returns>
        [DllImport("Kernel32.dll"
            , CharSet=CharSet.Auto
            , SetLastError=true)]
        private static extern uint GetDriveType(string rootPathName);
        /// <summary>
        /// Get Disk Free Space EX
        /// </summary>
        /// <param name="rootPathName">Root Path Name</param>
        /// <param name="freeBytesAvailableToCaller">Free Bytes Available To Caller</param>
        /// <param name="totalNumberOfBytes">Total Number Of Bytes</param>
        /// <param name="totalNumberOfFreeBytes">Total Number Of Free Bytes</param>
        /// <returns></returns>
        [DllImport("Kernel32.dll"
                , EntryPoint = "GetDiskFreeSpaceExA")]
        private static extern long GetDiskFreeSpaceExA(string rootPathName
            , out long freeBytesAvailableToCaller
            , out long totalNumberOfBytes
            , out long totalNumberOfFreeBytes);
        #endregion

        #region Members
        /// <summary>
        /// Drive
        /// </summary>
        private readonly string m_drive;
        /// <summary>
        /// Unique Identifier
        /// </summary>
        private readonly Guid m_identifier;
        /// <summary>
        /// Type
        /// </summary>
        private DriveType m_type;
        /// <summary>
        /// Space
        /// </summary>
        private HardDriveSpace m_space;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="drive">Drive</param>
        private HardDrive(string drive)
            :base()
        {
            using (var log = new TraceContext())
            {
                this.m_drive = drive;
                this.m_identifier = Guid.NewGuid();

                log.Debug("m_drive={0},m_identifier={1}"
                    , this.m_drive
                    , this.m_identifier);

                this.Load();
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
                uint type = HardDrive.GetDriveType(this.m_drive);
                this.m_type = (DriveType)type;

                long freeBytesAvailableToCaller;
                long totalNumberOfBytes;
                long totalNumberOfFreeBytes;

                this.m_space = new HardDriveSpace();

                this.m_space.Result = HardDrive.GetDiskFreeSpaceExA(this.m_drive
                    , out freeBytesAvailableToCaller
                    , out totalNumberOfBytes
                    , out totalNumberOfFreeBytes);

                this.m_space.FreeBytesAvailableToCaller = freeBytesAvailableToCaller;
                this.m_space.TotalNumberOfBytes = totalNumberOfBytes;
                this.m_space.TotalNumberOfFreeBytes = totalNumberOfFreeBytes;

                log.Debug("m_type={0},m_space={1}"
                    , this.m_type
                    , this.m_space);
            }
        }
        /// <summary>
        /// Get Drives
        /// </summary>
        /// <returns>Hard Drives</returns>
        public static HardDrive[] GetDrives()
        {
            using (var log = new TraceContext())
            {
                string[] drives = Environment.GetLogicalDrives();
                HardDrive[] hds = new HardDrive[drives.Length];
                for (int i = 0; i < drives.Length; i++)
                {
                    hds[i] = new HardDrive(drives[i]);
                }
                return hds;
            }
        }

        #region ILatency Members
        /// <summary>
        /// Determine Latency
        /// </summary>
        /// <param name="type">Latency Type</param>
        /// <returns>Time Span</returns>
        public TimeSpan Determine(LatencyType type)
        {
            using (var log = new TraceContext())
            {
                log.Debug("type={0}"
                    , type);

                switch (type)
                {
                    case LatencyType.Storage:
                        return TimeSpan.Zero;
                    default:
                        log.Warn("Invalid");
                        return TimeSpan.Zero;
                }
            }
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Type
        /// </summary>
        public DriveType Type
        {
            get
            {
                return this.m_type;
            }
        }
        /// <summary>
        /// Drive
        /// </summary>
        public string Drive
        {
            get
            {
                return this.m_drive;
            }
        }
        /// <summary>
        /// Space
        /// </summary>
        public HardDriveSpace Space
        {
            get
            {
                return this.m_space;
            }
        }

        #region IUnique<Guid> Members
        /// <summary>
        /// Identifier
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return this.m_identifier;
            }
        }
        #endregion
        #endregion
    }
}