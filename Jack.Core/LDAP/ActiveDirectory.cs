using System.Collections.Generic;
using System.DirectoryServices;
using System.Net;

using Jack.Logger;

namespace Jack.Core.LDAP
{
    /// <summary>
    /// Object for Querying Active Directory
    /// </summary>
    internal class ActiveDirectory
    {
        #region Variables
        /// <summary>
        /// Constant Variables
        /// </summary>
        private const string
            c_LDAP = "LDAP://",
            c_objectComputer = "(objectClass=computer)",
            c_user = "user",
            c_objectUser = "(objectClass=user)";
        #endregion

        #region Methods
        /// <summary>
        /// Gets a Listing of Computers on A Network
        /// </summary>
        /// <returns>The listing of Computers (name as System.String)</returns>
        internal static IList<Computer> Computers()
        {
            using (var log = new TraceContext())
            {
                IList<Computer> computers = new List<Computer>();
                computers.Add(new Computer(ActiveDirectory.MachineName));
                try
                {
                    using (DirectoryEntry entry = new DirectoryEntry(string.Format("{0}{1}"
                        , c_LDAP
                        , ActiveDirectory.DomainName)))
                    {
                        using (DirectorySearcher mySearcher = new DirectorySearcher(entry))
                        {
                            mySearcher.Filter = c_objectComputer;
                            foreach (SearchResult resEnt in mySearcher.FindAll())
                            {
                                computers.Add(new Computer(resEnt.GetDirectoryEntry().Properties["cn"].Value as string));
                            }
                        }
                    }
                }
                catch
                {
                }
                return computers;
            }
        }
        /// <summary>
        /// Users
        /// </summary>
        /// <returns>List of users</returns>
        public static IList<IUser> Users()
        {
            using (var log = new TraceContext())
            {
                IList<IUser> users = new List<IUser>();
                try
                {
                    using (DirectoryEntry entry = new DirectoryEntry(string.Format("{0}{1}"
                        , c_LDAP
                        , ActiveDirectory.DomainName)))
                    {
                        using (DirectorySearcher
                                   mySearcher = new DirectorySearcher(entry))
                        {
                            mySearcher.Filter = c_objectUser;
                            DirectoryEntry de;
                            foreach (SearchResult resEnt in mySearcher.FindAll())
                            {
                                de = resEnt.GetDirectoryEntry();
                                if (c_user == de.SchemaClassName)
                                {

                                }
                            }
                        }
                    }
                }
                catch { }
                return users;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Domain Name
        /// </summary>
        internal static string DomainName
        {
            get
            {
                string fqdn = ActiveDirectory.FullyQualifiedDomainName;
                return fqdn.ToLower().Replace(string.Format("{0}."
                    , MachineName)
                   , string.Empty);
            }
        }
        /// <summary>
        /// Fully Qualified Domain Name (FQDN)
        /// </summary>
        internal static string FullyQualifiedDomainName
        {
            get
            {
                return Dns.GetHostEntry(ActiveDirectory.MachineName).HostName;
            }
        }
        /// <summary>
        /// Machine Name
        /// </summary>
        internal static string MachineName
        {
            get
            {
                return System.Environment.MachineName;
            }
        }
        #endregion
    }
}
