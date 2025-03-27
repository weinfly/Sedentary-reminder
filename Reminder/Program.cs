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
            if (string.IsNullOrEmpty(values))
                return new List<int>();

            return values.Split(',')
                         .Select(v => int.TryParse(v, out int result) ? result : -1)
                         .Where(v => v >= 0)
                         .ToList();
        }

        private static void CheckAndStartWorkFrm(Configuration config)
        {
            var startHours = GetConfigValues(config, "AutoStartHours");
            if (startHours.Contains(DateTime.Now.Hour) && DateTime.Now.Minute == 0)
            {
                var existingWorkForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is WorkFrm);

                var existingRestForm = Application.OpenForms.Cast<Form>()
                    .FirstOrDefault(form => form is RestFrm);

                if (existingWorkForm == null)
                {
                    if (existingRestForm != null)
                    {
                        existingRestForm.Invoke((Action)(() =>
                        {
                            existingRestForm.Close();
                        }));
                    }

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    int workTimeValue = GetConfigValue(config, "WorkTimeValue", 45);
                    int restTimeValue = GetConfigValue(config, "RestTimeValue", 15);

                    WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue);
                    workFrm.Show();
                }
                else
                {
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
                }
            }
        }

        private static void CheckAndStopWorkFrm(Configuration config)
        {
            var stopHours = GetConfigValues(config, "AutoStopHours");
            if (stopHours.Contains(DateTime.Now.Hour))
            {
                var forms = Application.OpenForms.Cast<Form>().ToArray();

                foreach (Form form in forms)
                {
                    if (form is WorkFrm)
                    {
                        form.Invoke((Action)(() => form.Close()));
                    }
                }

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