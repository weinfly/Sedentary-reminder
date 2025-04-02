using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using System.Threading;

namespace Reminder
{
    static class Program
    {
        private static Mutex mutex = new Mutex(true, "{SedentaryReminderUniqueIdentifier}");
        private static System.Timers.Timer startTimer;
        private static System.Timers.Timer stopTimer;
        private static System.Timers.Timer checkTimer;

        [STAThread]
        static void Main()
        {
            //Logger.Log($"配置文件路径: {AppDomain.CurrentDomain.SetupInformation.ConfigurationFile}");

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("程序运行中，见右下角系统托盘");
                return;
            }

            try
            {
                Configuration config = GetAppConfiguration();

                // 设置定时器以检查时间
                startTimer = new System.Timers.Timer(60000); // 1分钟间隔
                startTimer.Elapsed += (sender, e) => CheckAndStartWorkFrm(config);
                startTimer.Start();

                stopTimer = new System.Timers.Timer(60000); // 1分钟间隔
                stopTimer.Elapsed += (sender, e) => CheckAndStopWorkFrm(config);
                stopTimer.Start();

                checkTimer = new System.Timers.Timer(3600000);
                checkTimer.Elapsed += (sender, e) => CheckAndStopWorkFrm(config);
                checkTimer.Start();

                CheckAndStartWorkFrm(config);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                int workTimeValue = GetConfigValue(config, "WorkTimeValue", 45);
                int restTimeValue = GetConfigValue(config, "RestTimeValue", 15);
                MainFrm mainForm = new MainFrm();
                mainForm.SetWorkTimeValue(workTimeValue);
                mainForm.SetRestTimeValue(restTimeValue);
                Application.Run(mainForm);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private static Configuration GetAppConfiguration()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        private static int GetConfigValue(Configuration config, string key, int defaultValue)
        {
            var value = config.AppSettings.Settings[key]?.Value;
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        private static List<int> GetConfigValues(Configuration config, string key)
        {
            var values = config.AppSettings.Settings[key]?.Value;
            //Logger.Log($"正在读取配置项 {key}，值为: {values ?? "null"}");
            if (string.IsNullOrEmpty(values))
            {
                //Logger.Log($"配置项 {key} 为空，返回空列表");
                return new List<int>();
            }

            var parsedValues = values.Split(',')
                         .Select(v => int.TryParse(v, out int parsedValue) ? parsedValue : -1)
                         .Where(v => v >= 0)
                         .ToList();

            //Logger.Log($"配置项 {key} 解析结果为: {string.Join(", ", parsedValues)}");
            return parsedValues;
        }
        // 发现在自动启动的时候，日志记录了是存在workfrm窗口的，因此没有启动新的窗口，这里改成有存在也关闭。
        private static void CheckAndStartWorkFrm(Configuration config)
        {
            var startHours = GetConfigValues(config, "AutoStartHours");
            if (startHours.Count == 0)
            {
                startHours = new List<int> { 9, 13 }; // 默认值
            }
            if (startHours.Contains(DateTime.Now.Hour) && (DateTime.Now.Minute == 0 || DateTime.Now.Minute == 1))
            {
                Logger.Log($"尝试启动WorkFrm倒计时窗口，当前时间：{DateTime.Now}");

                var existingWorkForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is WorkFrm);

                var existingRestForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is RestFrm);

                // 如果WorkFrm窗口存在，先关闭
                if (existingWorkForm != null)
                {
                    Logger.Log("检测到WorkFrm窗口存在，正在关闭");
                    try
                    {
                        existingWorkForm.Invoke((Action)(() =>
                        {
                            existingWorkForm.Close();
                        }));
                        Logger.Log("WorkFrm窗口关闭成功");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"关闭WorkFrm窗口失败：{ex.Message}");
                        return; // 如果关闭失败，直接返回
                    }
                }

                // 如果RestFrm窗口存在，也关闭
                if (existingRestForm != null)
                {
                    Logger.Log("检测到RestFrm休息窗口存在，正在关闭");
                    try
                    {
                        existingRestForm.Invoke((Action)(() =>
                        {
                            existingRestForm.Close();
                        }));
                        Logger.Log("休息RestFrm窗口关闭成功");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"关闭RestFrm休息窗口失败：{ex.Message}");
                    }
                }

                // 启动新的WorkFrm窗口
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    int workTimeValue = GetConfigValue(config, "WorkTimeValue", 45);
                    int restTimeValue = GetConfigValue(config, "RestTimeValue", 15);

                    WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue);
                    workFrm.Show();
                    Logger.Log("WorkFrm倒计时窗口启动成功");
                }
                catch (Exception ex)
                {
                    Logger.Log($"WorkFrm倒计时窗口启动失败：{ex.Message}");
                }
            }
        }
        private static void CheckAndStartWorkFrm_old(Configuration config)
        {
            var startHours = GetConfigValues(config, "AutoStartHours");
            if (startHours.Count == 0)
            {
                //Logger.Log("警告：AutoStartHours配置为空，使用默认值[9,13]");
                startHours = new List<int> { 9, 13 }; // 默认值
            }
            //Logger.Log($"正在进行定期启动检查，startHours为{string.Join(", ", startHours)}，当前时间：{DateTime.Now}");
            if (startHours.Contains(DateTime.Now.Hour) && (DateTime.Now.Minute == 0 || DateTime.Now.Minute == 1))
            {
                Logger.Log($"尝试启动WorkFrm倒计时窗口，当前时间：{DateTime.Now}");

                var existingWorkForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is WorkFrm);

                var existingRestForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is RestFrm);

                if (existingWorkForm == null)
                {
                    if (existingRestForm != null)
                    {
                        Logger.Log("检测到RestFrm休息窗口存在，正在关闭");
                        try
                        {
                            existingRestForm.Invoke((Action)(() =>
                            {
                                existingRestForm.Close();
                            }));
                            Logger.Log("休息RestFrm窗口关闭成功");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"关闭RestFrm休息窗口失败：{ex.Message}");
                        }
                    }

                    try
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        int workTimeValue = GetConfigValue(config, "WorkTimeValue", 45);
                        int restTimeValue = GetConfigValue(config, "RestTimeValue", 15);

                        WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue);
                        workFrm.Show();
                        Logger.Log("WorkFrm倒计时窗口启动成功");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"WorkFrm倒计时窗口启动失败：{ex.Message}");
                    }
                }
                else
                {
                    Logger.Log("WorkFrm倒计时窗口已存在，无需再次启动");
                }
            }
        }


        private static void CheckAndStopWorkFrm(Configuration config)
        {
            var stopHours = GetConfigValues(config, "AutoStopHours");
            if (stopHours.Count == 0)
            {
                stopHours = new List<int> { 12, 18 }; // 默认值
            }
            if (stopHours.Contains(DateTime.Now.Hour))
            {
                Logger.Log($"尝试关闭WorkFrm倒计时窗口，当前时间：{DateTime.Now}");

                var forms = Application.OpenForms.Cast<Form>().ToArray();
                bool hasError = false;

                foreach (Form form in forms)
                {
                    if (form is WorkFrm)
                    {
                        try
                        {
                            form.Invoke((Action)(() => form.Close()));
                            Logger.Log("WorkFrm倒计时窗口关闭成功");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"WorkFrm倒计时窗口关闭失败：{ex.Message}");
                            hasError = true;
                        }
                    }
                }

                foreach (Form form in forms)
                {
                    if (form is RestFrm)
                    {
                        try
                        {
                            form.Invoke((Action)(() => form.Close()));
                            Logger.Log("RestFrm休息窗口关闭成功");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"RestFrm休息窗口关闭失败：{ex.Message}");
                            hasError = true;
                        }
                    }
                }

                if (!hasError)
                {
                    Logger.Log("所有窗口关闭完成");
                }
            }
        }
    }
}