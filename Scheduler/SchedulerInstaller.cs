using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SedentaryReminder.Scheduler
{
    [RunInstaller(true)]
    public class SchedulerInstaller : Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;

        public SchedulerInstaller()
        {
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            // 使用 LocalSystem 账户，以便可以访问用户会话
            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.ServiceName = "SedentaryReminderScheduler";
            serviceInstaller.DisplayName = "久坐提醒调度服务";
            serviceInstaller.Description = "自动按照配置的工作时间段启动和停止久坐提醒倒计时。支持自定义工作时间段，周末自动跳过。";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}