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
        // 添加静态引用以防止窗口被垃圾回收
        private static WorkFrm activeWorkFrm;
        private static RestFrm activeRestFrm;

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

                checkTimer = new System.Timers.Timer(3600000); // 1小时间隔
                checkTimer.Elapsed += (sender, e) => CheckTimerStatus(config);
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
        //20250409，启动后增加窗口是否存在判断
        private static void CheckAndStartWorkFrm(Configuration config)
        {
            var startHours = GetConfigValues(config, "AutoStartHours");
            if (startHours.Count == 0)
            {
                startHours = new List<int> { 9, 13 }; // 默认值
            }
            if (startHours.Contains(DateTime.Now.Hour) && (DateTime.Now.Minute == 0))
            {
                // 关闭所有 WorkFrm 和RestFrm窗口
                foreach (Form form in Application.OpenForms)
                {
                    if (form is WorkFrm || form is RestFrm)
                    {
                        form.Dispose();
                        Logger.Log($"已关闭：{form}--{DateTime.Now}");
                    }
                }

                // 启动新的WorkFrm窗口
                try
                {
                    Logger.Log($"尝试启动WorkFrm倒计时窗口，当前时间：{DateTime.Now}");
                    int workTimeValue = GetConfigValue(config, "WorkTimeValue", 45);
                    int restTimeValue = GetConfigValue(config, "RestTimeValue", 15);

                    // 创建新窗口并保存引用
                    activeWorkFrm = new WorkFrm(workTimeValue, restTimeValue);
                    // 设置窗口始终在最前面
                    activeWorkFrm.TopMost = true;
                    // 显示窗口
                    activeWorkFrm.Show();
                    // 激活窗口，确保它获得焦点
                    activeWorkFrm.Activate();

                    // 检查窗口是否成功启动
                    int retryCount = 0;
                    const int maxRetries = 3;
                    bool windowExists = false;

                    while (!windowExists && retryCount < maxRetries)
                    {
                        // 检查窗口是否存在于OpenForms集合中
                        windowExists = Application.OpenForms.Cast<Form>().Any(f => f is WorkFrm);

                        if (!windowExists)
                        {
                            Logger.Log($"WorkFrm窗口未成功启动，尝试重新启动 (尝试 {retryCount + 1}/{maxRetries})");
                            retryCount++;

                            // 清理可能的无效引用
                            if (activeWorkFrm != null)
                            {
                                try { activeWorkFrm.Dispose(); } catch { }
                                activeWorkFrm = null;
                            }

                            // 重新创建窗口
                            if (retryCount < maxRetries)
                            {
                                activeWorkFrm = new WorkFrm(workTimeValue, restTimeValue);
                                activeWorkFrm.TopMost = true;
                                activeWorkFrm.Show();
                                activeWorkFrm.Activate();

                                // 短暂等待窗口初始化
                                System.Threading.Thread.Sleep(1000);
                            }
                        }
                    }

                    if (windowExists)
                    {
                        Logger.Log("WorkFrm倒计时窗口启动成功");

                        // 添加窗口关闭事件处理，记录窗口关闭情况
                        activeWorkFrm.FormClosed += (sender, e) =>
                        {
                            Logger.Log($"WorkFrm窗口被关闭，关闭时间：{DateTime.Now}");
                            activeWorkFrm = null;

                            // 检查是否有RestFrm窗口被打开，如果有则保存引用
                            foreach (Form form in Application.OpenForms)
                            {
                                if (form is RestFrm)
                                {
                                    activeRestFrm = (RestFrm)form;
                                    activeRestFrm.FormClosed += (s, args) =>
                                    {
                                        Logger.Log($"RestFrm窗口被关闭，关闭时间：{DateTime.Now}");
                                        activeRestFrm = null;
                                    };
                                    break;
                                }
                            }
                        };
                    }
                    else
                    {
                        Logger.Log($"WorkFrm倒计时窗口启动失败，已尝试 {maxRetries} 次");
                    }
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
                startHours = new List<int> { 9, 13 }; // 默认值
            }
            if (startHours.Contains(DateTime.Now.Hour) && (DateTime.Now.Minute == 0))
            {
                // 关闭所有 WorkFrm 和RestFrm窗口
                foreach (Form form in Application.OpenForms)
                {
                    if (form is WorkFrm || form is RestFrm)
                    {
                        form.Dispose();
                        Logger.Log($"已关闭：{form}--{DateTime.Now}");
                    }
                }

                // 启动新的WorkFrm窗口
                try
                {
                    Logger.Log($"尝试启动WorkFrm倒计时窗口，当前时间：{DateTime.Now}");
                    int workTimeValue = GetConfigValue(config, "WorkTimeValue", 45);
                    int restTimeValue = GetConfigValue(config, "RestTimeValue", 15);

                    // 创建新窗口并保存引用
                    activeWorkFrm = new WorkFrm(workTimeValue, restTimeValue);
                    // 设置窗口始终在最前面
                    activeWorkFrm.TopMost = true;
                    // 显示窗口
                    activeWorkFrm.Show();
                    // 激活窗口，确保它获得焦点
                    activeWorkFrm.Activate();
                    Logger.Log("WorkFrm倒计时窗口启动成功");

                    // 添加窗口关闭事件处理，记录窗口关闭情况
                    activeWorkFrm.FormClosed += (sender, e) =>
                    {
                        Logger.Log($"WorkFrm窗口被关闭，关闭时间：{DateTime.Now}");
                        activeWorkFrm = null;

                        // 检查是否有RestFrm窗口被打开，如果有则保存引用
                        foreach (Form form in Application.OpenForms)
                        {
                            if (form is RestFrm)
                            {
                                activeRestFrm = (RestFrm)form;
                                activeRestFrm.FormClosed += (s, args) =>
                                {
                                    Logger.Log($"RestFrm窗口被关闭，关闭时间：{DateTime.Now}");
                                    activeRestFrm = null;
                                };
                                break;
                            }
                        }
                    };
                }
                catch (Exception ex)
                {
                    Logger.Log($"WorkFrm倒计时窗口启动失败：{ex.Message}");
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
                Logger.Log($"尝试关闭WorkFrm倒计时和Restfrm窗口，当前时间：{DateTime.Now}");
                var forms = Application.OpenForms.Cast<Form>().ToArray();
                bool hasError = false;

                foreach (Form form in forms)
                {
                    if (form is WorkFrm || form is RestFrm)
                    {
                        try
                        {
                            form.Invoke((Action)(() => form.Dispose()));
                            Logger.Log($"时间：{DateTime.Now}，{form}窗口已关闭");
                            // 清除引用
                            if (form == activeWorkFrm)
                                activeWorkFrm = null;
                            else if (form == activeRestFrm)
                                activeRestFrm = null;
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"时间：{DateTime.Now}，{form}窗口关闭失败:,异常信息：{ex.Message}{DateTime.Now}");
                            hasError = true;
                        }
                    }
                }

                if (!hasError)
                {
                    Logger.Log("所有窗口关闭完成");
                }
                // 清除可能的引用
                activeWorkFrm = null;
                activeRestFrm = null;
            }
        }

        private static void CheckTimerStatus(Configuration config)
        {
            // 检查当前是否在工作时间
            var startHours = GetConfigValues(config, "AutoStartHours");
            var stopHours = GetConfigValues(config, "AutoStopHours");

            if (startHours.Count == 0)
                startHours = new List<int> { 9, 13 };

            if (stopHours.Count == 0)
                stopHours = new List<int> { 12, 18 };

            int currentHour = DateTime.Now.Hour;

            // 如果当前时间是停止时间，关闭窗口
            if (stopHours.Contains(currentHour))
            {
                CheckAndStopWorkFrm(config);
            }
            // 如果当前时间是开始时间，且没有活动窗口，启动窗口
            else if (startHours.Contains(currentHour) && activeWorkFrm == null && activeRestFrm == null)
            {
                CheckAndStartWorkFrm(config);
            }

            // 检查定时器是否正常运行
            if (!startTimer.Enabled)
            {
                Logger.Log("重新启动startTimer");
                startTimer.Start();
            }

            if (!stopTimer.Enabled)
            {
                Logger.Log("重新启动stopTimer");
                stopTimer.Start();
            }
        }
    }
}