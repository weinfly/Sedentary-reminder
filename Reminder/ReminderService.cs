using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;
using System.Windows.Forms;

namespace Reminder
{
    public class ReminderService : ServiceBase
    {
        private Timer _timer;
        private DateTime _startTime;
        private DateTime _endTime;

        public ReminderService()
        {
            ServiceName = "SedentaryReminderService";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
            this.serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                // 从配置文件读取开始和结束时间，添加默认值
                _startTime = DateTime.TryParse(ConfigurationManager.AppSettings["StartTime"], out var start) 
                    ? start 
                    : DateTime.Parse("09:00");
                _endTime = DateTime.TryParse(ConfigurationManager.AppSettings["EndTime"], out var end)
                    ? end
                    : DateTime.Parse("18:00");

                // 使定时器间隔可配置，默认1分钟
                var interval = int.TryParse(ConfigurationManager.AppSettings["CheckInterval"], out var intervalValue)
                    ? intervalValue
                    : 60000;
                _timer = new Timer(interval);
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();
            }
            catch (Exception ex)
            {
                // 记录日志或处理异常
                EventLog.WriteEntry(ServiceName, $"Service start failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                if (now >= _startTime && now <= _endTime)
                {
                    // 使用线程启动窗体
                    new Thread(() => 
                    {
                        var form = new WorkFrm();
                        Application.Run(form);
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Timer elapsed error: {ex.Message}", EventLogEntryType.Error);
            }
        }
    }
}
