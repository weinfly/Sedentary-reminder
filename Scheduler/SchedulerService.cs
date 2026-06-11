using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;

namespace SedentaryReminder.Scheduler
{
    /// <summary>
    /// 久坐提醒调度服务
    /// 负责按照配置的工作时间段自动启动/停止倒计时客户端
    /// </summary>
    public class SchedulerService : ServiceBase
    {
        private Timer _checkTimer;
        private ScheduleConfig _config;
        private ClientLauncher _launcher;
        private DateTime _lastCheckTime;
        private bool _clientStarted;

        public SchedulerService()
        {
            ServiceName = "SedentaryReminderScheduler";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _config = new ScheduleConfig();
                _launcher = new ClientLauncher();
                _lastCheckTime = DateTime.Now;
                _clientStarted = false;

                // 每分钟检查一次
                _checkTimer = new Timer(60000);
                _checkTimer.Elapsed += OnTimerElapsed;
                _checkTimer.Start();

                Log("服务已启动");

                // 启动时立即检查一次
                CheckSchedule();
            }
            catch (Exception ex)
            {
                Log($"服务启动失败: {ex.Message}");
                EventLog.WriteEntry(this.ServiceName, $"Service start failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _checkTimer?.Stop();
                _checkTimer?.Dispose();

                // 停止时关闭客户端
                string clientExe = _config?.GetClientExePath();
                if (!string.IsNullOrEmpty(clientExe))
                {
                    _launcher?.StopClient(clientExe);
                    Log("已停止客户端程序");
                }

                _launcher?.Dispose();
                Log("服务已停止");
            }
            catch (Exception ex)
            {
                Log($"服务停止异常: {ex.Message}");
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                CheckSchedule();
            }
            catch (Exception ex)
            {
                Log($"定时检查异常: {ex.Message}");
                EventLog.WriteEntry(this.ServiceName, $"Timer error: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void CheckSchedule()
        {
            try
            {
                DateTime now = DateTime.Now;
                Log($"检查调度 - 时间:{now:HH:mm:ss}, 星期:{now.DayOfWeek}");

                bool isWorkPeriod = _config.IsWithinWorkPeriod(now);

                if (_config.ShouldTriggerStart(now, _lastCheckTime))
                {
                    // 进入工作时间段，启动客户端
                    StartClient();
                }
                else if (_config.ShouldTriggerStop(now, _lastCheckTime))
                {
                    // 离开工作时间段，停止客户端
                    StopClient();
                }
                else if (!isWorkPeriod && _clientStarted)
                {
                    // 如果不在工作时间且客户端仍在运行，也停止
                    StopClient();
                }

                _lastCheckTime = now;
            }
            catch (Exception ex)
            {
                Log($"调度检查异常: {ex.Message}");
            }
        }

        private void StartClient()
        {
            try
            {
                string clientExe = _config.GetClientExePath();
                if (string.IsNullOrEmpty(clientExe))
                {
                    Log("未找到客户端程序路径，跳过启动");
                    return;
                }

                // 检查是否已在运行
                if (_launcher.IsClientRunning(clientExe))
                {
                    Log("客户端已在运行中");
                    _clientStarted = true;
                    return;
                }

                Log($"启动客户端: {clientExe}");
                bool result = _launcher.LaunchClient(clientExe);

                if (result)
                {
                    _clientStarted = true;
                    Log("客户端启动成功");
                }
                else
                {
                    Log("客户端启动失败");
                }
            }
            catch (Exception ex)
            {
                Log($"启动客户端异常: {ex.Message}");
            }
        }

        private void StopClient()
        {
            try
            {
                string clientExe = _config.GetClientExePath();
                if (string.IsNullOrEmpty(clientExe))
                {
                    return;
                }

                Log("停止客户端程序");
                bool result = _launcher.StopClient(clientExe);

                if (result)
                {
                    _clientStarted = false;
                    Log("客户端已停止");
                }
            }
            catch (Exception ex)
            {
                Log($"停止客户端异常: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scheduler.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch
            {
                // 忽略日志写入错误
            }
        }
    }
}