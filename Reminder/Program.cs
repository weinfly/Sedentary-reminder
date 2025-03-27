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

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("程序运行中，见右下角系统托盘");
                return;
            }

            try
            {
                // 获取配置对象
                Configuration config = GetAppConfiguration();

                int autoStartHour = GetConfigValue(config, "AutoStartHour", 9);
                int autoStopHour = GetConfigValue(config, "AutoStopHour", 18);

                // 设置定时器以检查时间
                startTimer = new System.Timers.Timer(60000); // 1分钟间隔
                startTimer.Elapsed += (sender, e) => CheckAndStartWorkFrm(config, autoStartHour);
                startTimer.Start();

                stopTimer = new System.Timers.Timer(60000); // 1分钟间隔
                stopTimer.Elapsed += (sender, e) => CheckAndStopWorkFrm(config, autoStopHour);
                stopTimer.Start();

                // 每小时检查一次
                checkTimer = new System.Timers.Timer(3600000);
                checkTimer.Elapsed += (sender, e) => CheckAndStopWorkFrm(config, autoStopHour);
                checkTimer.Start();
                // 添加立即检查
                CheckAndStartWorkFrm(config, autoStartHour);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                int workTimeValue = GetConfigValue(config, "WorkTimeValue", 45);
                int restTimeValue = GetConfigValue(config, "RestTimeValue", 15);
                MainFrm mainForm = new MainFrm();
                mainForm.SetWorkTimeValue(workTimeValue);
                mainForm.SetRestTimeValue(restTimeValue);
                Application.Run(mainForm);
                // 直接创建并运行 WorkFrm
                //WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue);
                //Application.Run(workFrm);
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

        private static void CheckAndStartWorkFrm(Configuration config, int startHour)
        {
            int currentHour = DateTime.Now.Hour;
            // if (currentHour == startHour)
            // 添加日期检查，确保是当天的第一次启动
            if (currentHour == startHour && DateTime.Now.Minute == 0)
            {
                // 分别检查WorkFrm和RestFrm
                var existingWorkForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is WorkFrm);

                var existingRestForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is RestFrm);
                // 检查是否已经存在MainFrm
                var existingMainForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is MainFrm);

                // 这里因为已经只改成了整点检查，因此只判断workFrm即可
                // if (existingWorkForm == null && existingRestForm == null)
                if (existingWorkForm == null)
                {
                    // 如果存在RestFrm实例，则关闭它
                    if (existingRestForm != null)
                    {
                        existingRestForm.Invoke((Action)(() =>
                        {
                            existingRestForm.Close();
                        }));
                    }
                    // 如果没有WorkFrm实例，则通过MainFrm启动
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    int workTimeValue = GetConfigValue(config, "WorkTimeValue", 45);
                    int restTimeValue = GetConfigValue(config, "RestTimeValue", 15);

                    // 这里是定时启动的，因此不需要判断mainfrm的存在与否的，直接启动workfrm即可
                    WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue);
                    workFrm.Show();
                    /* // 如果MainFrm不存在，则创建它
                    if (existingMainForm == null)
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        MainFrm mainForm = new MainFrm();
                        mainForm.SetWorkTimeValue(workTimeValue);
                        mainForm.SetRestTimeValue(restTimeValue);
                        Application.Run(mainForm);
                    }
                    else
                    {
                        // 如果MainFrm存在，直接启动WorkFrm
                        WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue);
                        workFrm.Show();
                    }*/
                }
                else
                {
                    // 记录存在的窗口类型
                    string logMessage = "已存在窗口：";
                    if (existingWorkForm != null)
                    {
                        logMessage += "WorkFrm ";
                    }
                    if (existingRestForm != null)
                    {
                        logMessage += "RestFrm，已执行关闭";
                    }
                    Logger.Log(logMessage);
                    // 如果已经有WorkFrm实例，只需激活它
                    //existingWorkForm.Invoke((Action)(() =>
                    //{
                    //    existingWorkForm.WindowState = FormWindowState.Normal;
                    //    existingWorkForm.Activate();
                    //}));
                }
            }
        }

        private static void CheckAndStopWorkFrm(Configuration config, int stopHour)
        {
            int currentHour = DateTime.Now.Hour;
            if (currentHour == stopHour)
            {
                // 使用ToArray()创建副本以避免集合被修改
                var forms = Application.OpenForms.Cast<Form>().ToArray();

                // 关闭所有 WorkFrm 窗口
                foreach (Form form in forms)
                {
                    if (form is WorkFrm)
                    {
                        form.Invoke((Action)(() => form.Close()));
                    }
                }

                // 关闭所有 RestFrm 窗口
                foreach (Form form in forms)
                {
                    if (form is RestFrm)
                    {
                        form.Invoke((Action)(() => form.Close()));
                    }
                }
            }
        }
    }
}