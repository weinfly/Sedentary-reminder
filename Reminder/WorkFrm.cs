using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Reminder
{
    public partial class WorkFrm : Form
    {
        private int wrk_minutes;//工作时间(分)
        private int wrk_seconds;//工作时间(秒)
        private int wrk_m;//原始工作分钟数，用于计算进度
        private int rst_minutes;//休息时间(分)
        private bool input_flag;//是否选中锁定键盘
        private bool left_flag;//鼠标左键是否点击
        private Point mouseoff;
        private static bool isRunning = false;
        private static WorkFrm currentInstance;
        private bool hideWorkForm;//是否隐藏倒计时窗口

        public WorkFrm()
        {
            InitializeComponent();
            SetupWindow();
        }

        /// <summary>
        /// 从配置文件读取值，并验证合法性
        /// </summary>
        private int GetConfigValue(string key, int defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (!int.TryParse(value, out int result))
            {
                return defaultValue;
            }

            // 添加参数验证
            if (key == "WorkTimeValue" || key == "RestTimeValue")
            {
                if (result <= 0)
                {
                    LogException(new ArgumentException($"{key}值必须为正数"), "配置验证");
                    return defaultValue;
                }

                // 对于工作时间，设置最大限制（480分钟=8小时）
                if (key == "WorkTimeValue" && result > 480)
                {
                    LogException(new ArgumentException("工作时间不能超过8小时"), "配置验证");
                    return defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// 无参数构造函数 - 从配置文件读取并自动计算倒计时
        /// </summary>
        public WorkFrm(int wrk_minutes, int rst_minutes)
        {
            try
            {
                InitializeComponent();
                SetupWindow();

                // 获取当前时间
                DateTime now = DateTime.Now;

                // 根据配置文件的WorkTimeValue，计算下一个时间点
                int workTimeValue = GetConfigValue("WorkTimeValue", 45);
                int restTimeValue = GetConfigValue("RestTimeValue", 15);

                DateTime hourStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
                DateTime nextBreakTime = hourStart.AddMinutes(workTimeValue);

                if (nextBreakTime <= now)
                {
                    nextBreakTime = now.AddMinutes(workTimeValue);
                }

                TimeSpan timeDiff = nextBreakTime - now;
                double totalMilliseconds = timeDiff.TotalMilliseconds;
                this.wrk_minutes = (int)(totalMilliseconds / 60000);
                this.wrk_seconds = (int)((totalMilliseconds % 60000) / 1000);
                this.rst_minutes = restTimeValue;
                this.wrk_m = workTimeValue;
                this.input_flag = false;

                SetFormPosition();
                isRunning = true;
                currentInstance = this;
            }
            catch (Exception ex)
            {
                LogException(ex, "构造函数异常");
                throw;
            }
        }

        /// <summary>
        /// 带参数的构造函数 - 使用指定的工作时间
        /// </summary>
        public WorkFrm(int wrk_minutes, int rst_minutes, bool input_flag)
        {
            try
            {
                InitializeComponent();
                SetupWindow();
                this.wrk_minutes = wrk_minutes;
                this.rst_minutes = rst_minutes;
                this.wrk_m = wrk_minutes;
                this.input_flag = input_flag;

                SetFormPosition();
                isRunning = true;
                currentInstance = this;
            }
            catch (Exception ex)
            {
                LogException(ex, "构造函数异常");
                throw;
            }
        }

        /// <summary>
        /// 设置窗体在多显示器上的位置
        /// </summary>
        private void SetFormPosition()
        {
            if (LoadSavedPosition())
                return;

            int screenIndex = GetConfigValue("WorkFormScreen", 0);
            Screen[] screens = Screen.AllScreens;

            if (screenIndex < 0 || screenIndex >= screens.Length)
            {
                screenIndex = 0;
            }

            Screen selectedScreen = screens[screenIndex];
            int offsetX = GetConfigValue("WorkFormOffsetX", 160);
            int offsetY = GetConfigValue("WorkFormOffsetY", 90);

            int x = selectedScreen.WorkingArea.Right - offsetX;
            int y = selectedScreen.WorkingArea.Bottom - offsetY;
            this.Location = new Point(x, y);
        }

        private bool LoadSavedPosition()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window_position.txt");
                if (!File.Exists(settingsPath))
                    return false;

                string[] lines = File.ReadAllLines(settingsPath);
                if (lines.Length < 2)
                    return false;

                if (int.TryParse(lines[0], out int x) && int.TryParse(lines[1], out int y))
                {
                    foreach (Screen screen in Screen.AllScreens)
                    {
                        if (screen.WorkingArea.Contains(x, y))
                        {
                            this.Location = new Point(x, y);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "加载窗口位置失败");
            }
            return false;
        }

        private void SavePosition()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window_position.txt");
                File.WriteAllLines(settingsPath, new[]
                {
                    this.Location.X.ToString(),
                    this.Location.Y.ToString()
                });
            }
            catch (Exception ex)
            {
                LogException(ex, "保存窗口位置失败");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            isRunning = false;
        }

        public static bool IsRunning()
        {
            return isRunning;
        }

        /// <summary>
        /// 运行时动态切换倒计时窗口的隐藏状态（由设置界面调用）
        /// </summary>
        public void SetHidden(bool hidden)
        {
            hideWorkForm = hidden;
            if (hidden)
            {
                this.Opacity = 0;
            }
            else
            {
                FadeIn();
            }
        }

        /// <summary>
        /// 当前运行的实例，供托盘状态等跨窗体读取；无运行实例则为 null
        /// </summary>
        public static WorkFrm Current => currentInstance;

        /// <summary>
        /// 剩余工作时间，供托盘状态显示
        /// </summary>
        public TimeSpan RemainingTime => new TimeSpan(0, wrk_minutes, wrk_seconds);

        private void Form1_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            lblSecond.Text = wrk_seconds.ToString("D2");
            lblMin.Text = wrk_minutes.ToString("D2");

            ApplyRoundedCorners();

            hideWorkForm = GetConfigValue("HideWorkForm", 0) == 1;
            if (hideWorkForm)
            {
                this.Opacity = 0;
            }
            else
            {
                FadeIn();
            }
        }

        /// <summary>
        /// 设置圆角效果（按当前尺寸从 ThemeManager 计算，Resize/DpiChanged 时重算）
        /// </summary>
        private void ApplyRoundedCorners()
        {
            this.Region = ThemeManager.CreateRoundedRegion(this.Width, this.Height, ThemeManager.CornerRadius);
        }

        /// <summary>
        /// 尺寸变化（含 DPI 缩放）时重算圆角 Region，避免圆角错位
        /// </summary>
        private void WorkFrm_Resize(object sender, EventArgs e)
        {
            ApplyRoundedCorners();
        }

        /// <summary>
        /// DPI 变化时重算圆角与布局
        /// </summary>
        private void WorkFrm_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            ApplyRoundedCorners();
        }

        /// <summary>
        /// 统一窗口初始化：高 DPI 缩放模式 + 圆角重算事件订阅
        /// </summary>
        private void SetupWindow()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(6F, 12F);
            this.Resize += WorkFrm_Resize;
            this.DpiChanged += WorkFrm_DpiChanged;
        }

        /// <summary>
        /// 从 ThemeManager 统一取色与字体，覆盖 Designer 中的硬编码
        /// </summary>
        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Primary;

            lblWarn.ForeColor = ThemeManager.OnPrimary;
            lblWarn.Text = " ⏰ 注意坐姿 · 可拖动";

            lblMin.Font = ThemeManager.Number(28);
            lblMin.ForeColor = ThemeManager.OnPrimary;
            labelColon.Font = ThemeManager.Number(28);
            labelColon.ForeColor = ThemeManager.OnPrimary;
            lblSecond.Font = ThemeManager.Number(28);
            lblSecond.ForeColor = ThemeManager.OnPrimary;

            lblProgress.ForeColor = ThemeManager.OnPrimary;

            StyleActionButton(btnPause);
            StyleActionButton(btnDelay);
        }

        /// <summary>
        /// 统一操作按钮（暂停/推迟）的扁平风格与交互态
        /// </summary>
        private void StyleActionButton(Button btn)
        {
            btn.BackColor = ThemeManager.PrimaryDark;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = ThemeManager.OnPrimary;
            btn.FlatAppearance.BorderSize = 1;
            btn.ForeColor = ThemeManager.OnPrimary;
            btn.FlatAppearance.MouseOverBackColor = ThemeManager.Primary;
            btn.FlatAppearance.MouseDownBackColor = ThemeManager.PrimaryDark;
        }

        /// <summary>
        /// 窗体淡入动画（从透明渐显至 0.95）
        /// </summary>
        private void FadeIn()
        {
            this.Opacity = 0;
            var fadeTimer = new System.Windows.Forms.Timer { Interval = 20 };
            fadeTimer.Tick += (s, ev) =>
            {
                this.Opacity += 0.05;
                if (this.Opacity >= 0.95)
                {
                    this.Opacity = 0.95;
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                }
            };
            fadeTimer.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (wrk_seconds > 0)
                {
                    wrk_seconds--;
                }
                else
                {
                    wrk_minutes--;
                    wrk_seconds = 59;
                }

                lblSecond.Text = wrk_seconds.ToString("D2");
                lblMin.Text = wrk_minutes.ToString("D2");

                Warn();

                if (wrk_minutes < 0)
                {
                    timerWrk.Stop();
                    this.Close();

                    RestFrm[] restFrms = new RestFrm[Screen.AllScreens.Length];
                    for (int i = 0; i < Screen.AllScreens.Length; i++)
                    {
                        var screen = Screen.AllScreens[i];
                        restFrms[i] = new RestFrm(rst_minutes, wrk_m, input_flag, screen.Primary, screen.Bounds.Location);
                        restFrms[i].Show();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Timer error");
                timerWrk.Stop();
            }
        }

        /// <summary>
        /// 工作的最后15秒提醒
        /// </summary>
        private void Warn()
        {
            if (wrk_minutes == 0 && wrk_seconds <= 16)
            {
                btnDelay.Visible = false;
                btnPause.Visible = false;
                this.BackColor = ThemeManager.Warning;
                lblWarn.ForeColor = ThemeManager.OnPrimary;
                lblWarn.Text = "⚠️ 该起来了！";
                lblProgress.Text = "即将锁定...";
                lblProgress.ForeColor = ThemeManager.OnPrimary;
                lblMin.ForeColor = ThemeManager.OnPrimary;
                lblSecond.ForeColor = ThemeManager.OnPrimary;
                labelColon.ForeColor = ThemeManager.OnPrimary;

                int x = (Screen.PrimaryScreen.WorkingArea.Size.Width) / 2 - this.Width / 2;
                int y = (Screen.PrimaryScreen.WorkingArea.Size.Height) / 2 - this.Height / 2;
                this.Location = new Point(x, y);

                if (hideWorkForm && this.Opacity < 0.5)
                {
                    this.Opacity = 0.95;
                }
            }
        }

        /// <summary>
        /// 显示窗口时不抢占焦点
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /// <summary>
        /// 让程序不显示在alt+Tab视图窗体中
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_APPWINDOW = 0x40000;
                const int WS_EX_TOOLWINDOW = 0x80;
                CreateParams cp = base.CreateParams;
                cp.ExStyle &= (~WS_EX_APPWINDOW);    // 不显示在TaskBar
                cp.ExStyle |= WS_EX_TOOLWINDOW;      // 不显示在Alt+Tab
                return cp;
            }
        }

        private void mouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseoff = new Point(e.X, e.Y);
                left_flag = true;
            }
        }

        private void mouseMove()
        {
            if (left_flag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(-mouseoff.X, -mouseoff.Y);
                Location = mouseSet;
            }
        }

        private void mouseUp()
        {
            if (left_flag)
            {
                left_flag = false;
                SavePosition();
            }
        }

        private void WorkFrm_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown(e);
        }

        private void WorkFrm_MouseMove(object sender, MouseEventArgs e)
        {
            mouseMove();
        }

        private void WorkFrm_MouseUp(object sender, MouseEventArgs e)
        {
            mouseUp();
        }

        /// <summary>
        /// 推迟10分钟按钮 - 修复：固定增加10分钟
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            WorkFrm workFrm = new WorkFrm(wrk_minutes + 10, rst_minutes, input_flag);
            workFrm.Show();
            this.Close();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            this.timerWrk.Enabled = !this.timerWrk.Enabled;
            if (this.timerWrk.Enabled)
            {
                btnPause.Text = "暂停";
                lblProgress.Text = "工作中...";
            }
            else
            {
                btnPause.Text = "继续";
                lblProgress.Text = "已暂停";
            }
        }

        /// <summary>
        /// 统一的异常日志记录方法
        /// </summary>
        private void LogException(Exception ex, string context)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            string logMessage = $"[{DateTime.Now}] {context}: {ex.Message}\nStackTrace: {ex.StackTrace}\n\n";
            File.AppendAllText(logPath, logMessage);
            Console.WriteLine($"{context}: {ex.Message}");
        }

        private void LblSecond_Click(object sender, EventArgs e)
        {
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
