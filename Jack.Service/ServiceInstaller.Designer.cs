using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

using Jack.Core.Windows.Services;

using Jack.Logger;

namespace Jack.Service
{
    /// <summary>
    /// Service Installer
    /// </summary>
    partial class ServiceInstaller : Installer
    {
        #region Members
        /// <summary>
        /// Service Process Installer
        /// </summary>
        private ServiceProcessInstaller m_serviceProcessInstaller;
        /// <summary>
        /// Service Installer
        /// </summary>
        private System.ServiceProcess.ServiceInstaller m_serviceInstaller;
        #endregion

        #region Methods
        /// <summary>
        /// Actions to Install the Service
        /// </summary>
        /// <param name="stateServer">State Server</param>
        public override void Install(IDictionary stateServer)
        {
            using (var log = new TraceContext())
            {
                base.Install(stateServer);

                new Jack.Core.XML.ManifestXml();
            }
        }
        /// <summary>
        /// Actions To Roll Back a Failed Installation
        /// </summary>
        /// <param name="stateServer">State Server</param>
        public override void Rollback(IDictionary stateServer)
        {
            using (var log = new TraceContext())
            {
                base.Rollback(stateServer);
            }
        }
        /// <summary>
        /// Actions to Uninstall the Service
        /// </summary>
        /// <param name="stateServer">State Server</param>
        public override void Uninstall(IDictionary stateServer)
        {
            using (var log = new TraceContext())
            {
                base.Uninstall(stateServer);
            }
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            using (var log = new TraceContext())
            {
                this.m_serviceProcessInstaller = new ServiceProcessInstaller();
                this.m_serviceInstaller = new System.ServiceProcess.ServiceInstaller();

                this.m_serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
                this.m_serviceProcessInstaller.Username = null;
                this.m_serviceProcessInstaller.Password = null;

                this.m_serviceInstaller.DisplayName = Common.DisplayName;
                this.m_serviceInstaller.Description = "Jack works with other Jack's to save your data; he is friendly.";
                this.m_serviceInstaller.ServiceName = Common.ServiceName;
                this.m_serviceInstaller.StartType = ServiceStartMode.Automatic;

                this.Installers.AddRange(new System.Configuration.Install.Installer[]
                    {
                        this.m_serviceInstaller
                        , this.m_serviceProcessInstaller
                    }
                );
            }
        }
        #endregion
        #endregion
    }
}