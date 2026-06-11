using System;
using System.ServiceProcess;

namespace SedentaryReminder.Scheduler
{
    /// <summary>
    /// 调度服务入口
    /// 支持两种运行模式：
    /// 1. 服务模式：作为 Windows Service 安装运行
    /// 2. 控制台模式：直接运行用于调试（传入 --console 参数）
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--console")
            {
                // 控制台调试模式
                RunAsConsole();
            }
            else
            {
                // Windows Service 模式
                var servicesToRun = new ServiceBase[]
                {
                    new SchedulerService()
                };
                ServiceBase.Run(servicesToRun);
            }
        }

        private static void RunAsConsole()
        {
            Console.WriteLine("=== 久坐提醒调度服务（控制台调试模式） ===");
            Console.WriteLine("按 Ctrl+C 退出");

            var config = new ScheduleConfig();
            var launcher = new ClientLauncher();

            try
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    Console.WriteLine("\n正在停止...");
                    launcher.StopClient(config.GetClientExePath());
                    e.Cancel = true;
                };

                DateTime lastCheck = DateTime.Now;

                while (true)
                {
                    DateTime now = DateTime.Now;
                    Console.Clear();
                    Console.WriteLine($"=== 久坐提醒调度服务（控制台调试模式） ===");
                    Console.WriteLine($"当前时间: {now:yyyy-MM-dd HH:mm:ss} 星期{GetDayName(now.DayOfWeek)}");
                    Console.WriteLine();

                    var periods = config.GetWorkPeriods();
                    Console.WriteLine("工作时间段:");
                    foreach (var p in periods)
                    {
                        Console.WriteLine($"  {p.StartHour:D2}:{p.StartMinute:D2} - {p.EndHour:D2}:{p.EndMinute:D2}");
                    }
                    Console.WriteLine();

                    bool isWorkPeriod = config.IsWithinWorkPeriod(now);
                    Console.WriteLine($"当前是否工作时间: {(isWorkPeriod ? "是" : "否")}");
                    Console.WriteLine($"是否工作日: {(config.IsWorkDay(now.DayOfWeek) ? "是" : "否（周末）")}");
                    Console.WriteLine();

                    if (config.ShouldTriggerStart(now, lastCheck))
                    {
                        Console.WriteLine("[触发] 启动客户端...");
                        string exePath = config.GetClientExePath();
                        if (!string.IsNullOrEmpty(exePath))
                        {
                            launcher.LaunchClient(exePath);
                        }
                    }
                    else if (config.ShouldTriggerStop(now, lastCheck))
                    {
                        Console.WriteLine("[触发] 停止客户端...");
                        string exePath = config.GetClientExePath();
                        if (!string.IsNullOrEmpty(exePath))
                        {
                            launcher.StopClient(exePath);
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine("下次检查: 60秒后...");

                    lastCheck = now;
                    System.Threading.Thread.Sleep(60000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
            finally
            {
                launcher.Dispose();
            }
        }

        private static string GetDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "一";
                case DayOfWeek.Tuesday: return "二";
                case DayOfWeek.Wednesday: return "三";
                case DayOfWeek.Thursday: return "四";
                case DayOfWeek.Friday: return "五";
                case DayOfWeek.Saturday: return "六";
                case DayOfWeek.Sunday: return "日";
                default: return "";
            }
        }
    }
}