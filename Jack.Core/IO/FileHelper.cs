using System.IO;
using System.Reflection;

namespace Jack.Core.IO
{
    /// <summary>
    /// File Helper
    /// </summary>
    public static class FileHelper
    {
        #region Methods
        /// <summary>
        /// Ensure Folder Exists
        /// </summary>
        /// <param name="path">Path</param>
        internal static void EnsureFolderExists(string path)
        {
            if (!(Directory.Exists(path)))
            {
                Directory.CreateDirectory(path);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Bin Directory
        /// </summary>
        public static string BinDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
        /// <summary>
        /// Common Application Data Directory
        /// </summary>
        /// <remarks>
        /// Windows 7=C:\Users\User\AppData
        /// </remarks>
        public static string ApplicationDataDirectory
        {
            get
            {
                return Path.GetDirectoryName(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));
            }
        }
        #endregion
    }
}