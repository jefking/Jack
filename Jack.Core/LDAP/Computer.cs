using Jack.Logger;

namespace Jack.Core.LDAP
{
    /// <summary>
    /// Computer
    /// </summary>
    internal class Computer
    {
        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="MachineName">Machine Name</param>
        public Computer(string machineName)
            :base()
        {
            using (var log = new TraceContext())
            {
                this.Name = machineName;

                log.Debug("Name={0}"
                    , this.Name);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name
        /// </summary>
        public readonly string Name;
        #endregion
    }
}