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
        private const int DEFAULT_WORK_TIME = 45;
        private const int DEFAULT_REST_TIME = 15;
        private static readonly List<int> DEFAULT_START_HOURS = new List<int> { 9, 13 };
        private static readonly List<int> DEFAULT_STOP_HOURS = new List<int> { 12, 18 };
        private const int TIMER_INTERVAL_MINUTE = 60000;
        private const int TIMER_INTERVAL_HOUR = 3600000;
        private const int MAX_RETRIES = 3;

        private static readonly Mutex mutex = new Mutex(true, "{SedentaryReminderUniqueIdentifier}");
        private static readonly object _lock = new object();
        private static System.Timers.Timer startTimer;
        private static System.Timers.Timer stopTimer;
        private static System.Timers.Timer checkTimer;
        private static WorkFrm activeWorkFrm;
        private static RestFrm activeRestFrm;

        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) =>
            {
                Logger.Log($"未处理异常: {e.Exception.Message}");
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Logger.Log($"未处理域异常: {(e.ExceptionObject as Exception)?.Message}");
            };

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("程序运行中，见右下角系统托盘");
                return;
            }

            try
            {
                InitializeApplication();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private static void InitializeApplication()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Configuration config = GetAppConfiguration();
            var mainForm = CreateMainForm(config);

            InitializeTimers(config);
            ShowInitialWorkForm(config);

            Application.Run(mainForm);
        }

        private static void InitializeTimers(Configuration config)
        {
            startTimer = CreateTimer(TIMER_INTERVAL_MINUTE, () => CheckAndStartWorkFrm(config));
            stopTimer = CreateTimer(TIMER_INTERVAL_MINUTE, () => CheckAndStopWorkFrm(config));
            checkTimer = CreateTimer(TIMER_INTERVAL_HOUR, () => CheckTimerStatus(config));
        }

        private static System.Timers.Timer CreateTimer(int interval, Action action)
        {
            var timer = new System.Timers.Timer(interval);
            timer.Elapsed += (sender, e) => action();
            timer.Start();
            return timer;
        }

        private static void ShowInitialWorkForm(Configuration config)
        {
            CheckAndStartWorkFrm(config);
        }

        private static MainFrm CreateMainForm(Configuration config)
        {
            var mainForm = new MainFrm();
            mainForm.SetWorkTimeValue(GetConfigValue(config, "WorkTimeValue", DEFAULT_WORK_TIME));
            mainForm.SetRestTimeValue(GetConfigValue(config, "RestTimeValue", DEFAULT_REST_TIME));
            return mainForm;
        }

        private static Configuration GetAppConfiguration()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        private static int GetConfigValue(Configuration config, string key, int defaultValue)
        {
            var value = config.AppSettings.Settings[key]?.Value;
            if (!int.TryParse(value, out int result) || result <= 0)
            {
                Logger.Log($"无效配置值 {key}={value}，使用默认值 {defaultValue}");
                return defaultValue;
            }
            return result;
        }

        private static List<int> GetConfigValues(Configuration config, string key)
        {
            var values = config.AppSettings.Settings[key]?.Value;
            if (string.IsNullOrEmpty(values))
            {
                return new List<int>();
            }

            return values.Split(',')
                .Select(v => int.TryParse(v, out int parsedValue) ? parsedValue : -1)
                .Where(v => v >= 0)
                .ToList();
        }

        private static void CheckAndStartWorkFrm(Configuration config)
        {
            var startHours = GetConfigValues(config, "AutoStartHours");
            if (startHours.Count == 0) startHours = DEFAULT_START_HOURS;

            if (ShouldStartWorkForm(startHours))
            {
                CleanupExistingForms();
                StartNewWorkForm(config);
            }
        }

        private static bool ShouldStartWorkForm(List<int> startHours)
        {
            return startHours.Contains(DateTime.Now.Hour) && DateTime.Now.Minute == 0;
        }

        private static void CleanupExistingForms()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is WorkFrm || form is RestFrm)
                {
                    form.Dispose();
                    Logger.Log($"已关闭：{form}--{DateTime.Now}");
                }
            }
        }

        private static void StartNewWorkForm(Configuration config)
        {
            try
            {
                Logger.Log($"尝试启动WorkFrm倒计时窗口，当前时间：{DateTime.Now}");

                var workTime = GetConfigValue(config, "WorkTimeValue", DEFAULT_WORK_TIME);
                var restTime = GetConfigValue(config, "RestTimeValue", DEFAULT_REST_TIME);

                if (TryCreateWorkForm(workTime, restTime))
                {
                    Logger.Log("WorkFrm倒计时窗口启动成功");
                    SetupFormClosedHandlers();
                }
                else
                {
                    Logger.Log($"WorkFrm倒计时窗口启动失败，已尝试 {MAX_RETRIES} 次");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"WorkFrm倒计时窗口启动失败：{ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private static bool TryCreateWorkForm(int workTime, int restTime)
        {
            int retryCount = 0;
            bool windowExists = false;

            while (!windowExists && retryCount < MAX_RETRIES)
            {
                lock (_lock)
                {
                    activeWorkFrm = CreateAndShowWorkForm(workTime, restTime);
                }

                Thread.Sleep(500);
                windowExists = Application.OpenForms.Cast<Form>().Any(f => f is WorkFrm);

                if (!windowExists)
                {
                    Logger.Log($"WorkFrm窗口未成功启动，尝试重新启动 (尝试 {++retryCount}/{MAX_RETRIES})");
                    CleanupWorkForm();
                    Thread.Sleep(1000);
                }
            }

            return windowExists;
        }

        private static WorkFrm CreateAndShowWorkForm(int workTime, int restTime)
        {
            WorkFrm form = null;
            if (Application.OpenForms.Count > 0)
            {
                var mainForm = Application.OpenForms[0];
                mainForm.Invoke((Action)(() =>
                {
                    form = new WorkFrm(workTime, restTime);
                    form.TopMost = true;
                    form.Show();
                    form.Activate();
                }));
            }
            else
            {
                var thread = new Thread(() =>
                {
                    form = new WorkFrm(workTime, restTime);
                    form.TopMost = true;
                    Application.Run(form);
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            return form;
        }

        private static void CleanupWorkForm()
        {
            lock (_lock)
            {
                try { activeWorkFrm?.Dispose(); } catch { }
                activeWorkFrm = null;
            }
        }

        private static void SetupFormClosedHandlers()
        {
            lock (_lock)
            {
                if (activeWorkFrm != null)
                {
                    activeWorkFrm.FormClosed += (sender, e) =>
                    {
                        Logger.Log($"WorkFrm窗口被关闭，关闭时间：{DateTime.Now}");
                        activeWorkFrm = null;
                        FindAndTrackRestForm();
                    };
                }
            }
        }

        private static void FindAndTrackRestForm()
        {
            lock (_lock)
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is RestFrm restForm)
                    {
                        activeRestFrm = restForm;
                        activeRestFrm.FormClosed += (s, args) =>
                        {
                            Logger.Log($"RestFrm窗口被关闭，关闭时间：{DateTime.Now}");
                            activeRestFrm = null;
                        };
                        break;
                    }
                }
            }
        }

        private static void CheckAndStopWorkFrm(Configuration config)
        {
            var stopHours = GetConfigValues(config, "AutoStopHours");
            if (stopHours.Count == 0) stopHours = DEFAULT_STOP_HOURS;

            if (stopHours.Contains(DateTime.Now.Hour))
            {
                Logger.Log($"尝试关闭WorkFrm倒计时和Restfrm窗口，当前时间：{DateTime.Now}");
                CloseAllWorkForms();
            }
        }

        private static void CloseAllWorkForms()
        {
            bool hasError = false;
            var forms = Application.OpenForms.Cast<Form>().ToArray();

            foreach (Form form in forms)
            {
                if (form is WorkFrm || form is RestFrm)
                {
                    try
                    {
                        form.Invoke((Action)(() => form.Dispose()));
                        Logger.Log($"时间：{DateTime.Now}，{form}窗口已关闭");
                        UpdateActiveFormReferences(form);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"时间：{DateTime.Now}，{form}窗口关闭失败:,异常信息：{ex.Message}");
                        hasError = true;
                    }
                }
            }

            if (!hasError) Logger.Log("所有窗口关闭完成");
            ClearActiveFormReferences();
        }

        private static void UpdateActiveFormReferences(Form form)
        {
            lock (_lock)
            {
                if (form == activeWorkFrm) activeWorkFrm = null;
                else if (form == activeRestFrm) activeRestFrm = null;
            }
        }

        private static void ClearActiveFormReferences()
        {
            lock (_lock)
            {
                activeWorkFrm = null;
                activeRestFrm = null;
            }
        }

        private static void CheckTimerStatus(Configuration config)
        {
            var startHours = GetConfigValues(config, "AutoStartHours");
            var stopHours = GetConfigValues(config, "AutoStopHours");

            if (startHours.Count == 0) startHours = DEFAULT_START_HOURS;
            if (stopHours.Count == 0) stopHours = DEFAULT_STOP_HOURS;

            int currentHour = DateTime.Now.Hour;

            if (stopHours.Contains(currentHour))
            {
                CheckAndStopWorkFrm(config);
            }
            else if (startHours.Contains(currentHour) && activeWorkFrm == null && activeRestFrm == null)
            {
                CheckAndStartWorkFrm(config);
            }

            RestartTimerIfStopped(startTimer, "startTimer");
            RestartTimerIfStopped(stopTimer, "stopTimer");
        }

        private static void RestartTimerIfStopped(System.Timers.Timer timer, string timerName)
        {
            if (!timer.Enabled)
            {
                Logger.Log($"重新启动{timerName}");
                timer.Start();
            }
        }
    }
}