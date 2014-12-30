using System;
using System.Reflection;

namespace Jack.Core.Reflection
{
    /// <summary>
    /// Assembly
    /// </summary>
    public static class Assembly
    {
        #region Properties
        /// <summary>
        /// Version
        /// </summary>
        public static Version Version
        {
            get
            {
                return Assembly.Version;
            }
        }
        /// <summary>
        /// Built On
        /// </summary>
        public static DateTime BuiltOn
        {
            get
            {
                DateTime build = new DateTime(2000, 1, 1);
                Version version = Assembly.Version;
                build = build.AddDays(version.Build);
                build = build.AddSeconds(version.Minor * 2);

                if (TimeZone.IsDaylightSavingTime(System.DateTime.Now
                    , TimeZone.CurrentTimeZone.GetDaylightChanges(System.DateTime.Now.Year)))
                {
                    build = build.AddHours(1);
                }

                return build;
            }
        }
        #endregion
    }
}