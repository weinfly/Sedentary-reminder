using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Reminder
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.ServiceName = "SedentaryReminderService";
            serviceInstaller.DisplayName = "Sedentary Reminder Service";
            serviceInstaller.Description = "A service that reminds users to take breaks from sitting.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
