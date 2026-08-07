using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

namespace Reminder
{
    public class ReminderService : ServiceBase
    {
        private System.Timers.Timer _timer;
        private DateTime _startTime;
        private DateTime _endTime;

        public ReminderService()
        {
            ServiceName = "SedentaryReminderService";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
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

                _timer = new System.Timers.Timer(interval);
                _timer.Elapsed += OnTimerElapsed;
                _timer.AutoReset = true;
                _timer.Start();

                EventLog.WriteEntry(ServiceName, "Service started successfully.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Service start failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            EventLog.WriteEntry(ServiceName, "Service stopped.", EventLogEntryType.Information);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;

                // 检查是否在工作时间内
                if (now.TimeOfDay >= _startTime.TimeOfDay && now.TimeOfDay <= _endTime.TimeOfDay)
                {
                    // 周末不提醒
                    if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                        return;

                    // 每分钟的第0秒触发，避免重复提醒
                    if (now.Second == 0)
                    {
                        // 通过事件日志记录提醒
                        EventLog.WriteEntry(ServiceName, $"Reminding user to take a break at {now:HH:mm:ss}", EventLogEntryType.Information);

                        // 尝试以交互式用户身份启动提醒窗口
                        TryLaunchReminderWindow();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Timer elapsed error: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void TryLaunchReminderWindow()
        {
            try
            {
                // 获取当前可执行文件路径
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                // 使用 schtasks 以当前登录用户的会话启动程序
                // 这样可以绕过 Session 0 隔离限制
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "schtasks",
                    Arguments = $"/create /tn \"SedentaryReminder_Popup\" /tr \"{exePath}\" /sc once /st {DateTime.Now.AddSeconds(1):HH:mm} /f",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(processStartInfo).WaitForExit();

                // 立即运行任务
                processStartInfo.Arguments = $"/run /tn \"SedentaryReminder_Popup\"";
                Process.Start(processStartInfo).WaitForExit();

                // 删除临时任务
                processStartInfo.Arguments = $"/delete /tn \"SedentaryReminder_Popup\" /f";
                Process.Start(processStartInfo).WaitForExit();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Failed to launch reminder window: {ex.Message}", EventLogEntryType.Warning);
            }
        }
    }
}