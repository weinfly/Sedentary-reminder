using System;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Reminder
{
    public partial class RestFrm : Form
    {
        // 静态字段 - 使用锁保证线程安全
        private static bool messageShown = false;
        private static bool isMessageShowing = false;
        private static readonly object _lock = new object();

        // 实例字段
        private int rst_m;
        private int wrk_m;
        private bool input_flag;
        private int rst_s;
        private int total_seconds; // 总秒数，用于计算进度
        private System.Windows.Forms.Timer countdownTimer;
        private bool main_screen;
        private Point originalLocation;

        // 自定义进度条（替代原生 ProgressBar，颜色完全可控）
        private Panel pnlTrack;
        private Panel pnlFill;

        public RestFrm(int rst_minutes, int wrk_minutes, bool input_flag, bool main_screen, Point location)
        {
            InitializeComponent();
            ThemeManager.ThemeChanged += OnThemeChanged;
            this.Disposed += (s, e) => ThemeManager.ThemeChanged -= OnThemeChanged;
            this.rst_m = rst_minutes;
            this.rst_s = 0;
            this.wrk_m = wrk_minutes;
            this.total_seconds = rst_minutes * 60; // 记录总秒数
            this.input_flag = input_flag;
            this.main_screen = main_screen;
            this.originalLocation = location;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = location;
            // 绑定 FormClosing 事件处理程序
            this.FormClosing += new FormClosingEventHandler(RestFrm_FormClosing);
        }

        private void RestFrm_Load(object sender, EventArgs e)
        {
            SetupUI();
            InitializeTimer();
            this.Activate();
        }

        private void SetupUI()
        {
            ApplyTheme();

            // 根据是否锁定输入显示不同提示
            if (input_flag)
            {
                lblText.Text = $"⚠️ 久坐对身体不好！您已久坐{wrk_m}分钟了，\n键盘和鼠标被锁定，请站起来活动一下！";
                KeyboardBlocker.off();
            }
            else
            {
                lblText.Text = $"💪 久坐对身体不好！您已久坐{wrk_m}分钟了，\n站起来活动下吧，顺便喝口水！(Alt+F4 退出)";
            }

            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;

            // 自定义进度条：隐藏原生 ProgressBar，使用可控颜色的 Panel
            progressBar.Visible = false;
            pnlTrack = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(70, 255, 255, 255)
            };
            pnlFill = new Panel
            {
                Dock = DockStyle.Left,
                Width = pnlProgress.ClientSize.Width,
                BackColor = Color.White
            };
            pnlTrack.Controls.Add(pnlFill);
            pnlProgress.Controls.Add(pnlTrack);

            UpdateTimeLabels();
        }

        /// <summary>
        /// 从 ThemeManager 统一取色与字体，覆盖 Designer 中的硬编码
        /// </summary>
        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Primary;
            lblText.ForeColor = ThemeManager.OnPrimary;
            lblText.Font = ThemeManager.Bold(16);
            lblIcon.ForeColor = ThemeManager.OnPrimary;
            lblTimerTitle.ForeColor = ThemeManager.OnPrimary;
            lblTimerTitle.Font = ThemeManager.Regular(14);
            lbl_minutes.Font = ThemeManager.Number(44);
            lbl_minutes.ForeColor = ThemeManager.OnPrimary;
            labelColon.Font = ThemeManager.Number(44);
            labelColon.ForeColor = ThemeManager.OnPrimary;
            lbl_seconds.Font = ThemeManager.Number(44);
            lbl_seconds.ForeColor = ThemeManager.OnPrimary;
            lblHint.ForeColor = ThemeManager.OnPrimary;
            lblHint.Font = ThemeManager.Regular(11);
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (this.IsDisposed || this.Disposing) return;
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(ApplyTheme));
                return;
            }

            ApplyTheme();
        }

        private void InitializeTimer()
        {
            countdownTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // 1秒
            };
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            // 重置消息框显示标志
            lock (_lock)
            {
                messageShown = false;
            }

            FadeIn();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (rst_s > 0 || rst_m > 0)
            {
                if (rst_s == 0)
                {
                    rst_m--;
                    rst_s = 59;
                }
                else
                {
                    rst_s--;
                }
                UpdateTimeLabels();
                UpdateProgress();
            }
            else
            {
                // 检查消息框是否已经显示过
                lock (_lock)
                {
                    if (!messageShown)
                    {
                        messageShown = true;
                        StatsTracker.RecordRestSession(rst_m);
                        ShowRestEndMessage();
                    }
                    else
                    {
                        countdownTimer.Stop();
                        this.Close();
                    }
                }
            }
        }

        private void UpdateTimeLabels()
        {
            lbl_seconds.Text = rst_s.ToString("D2");
            lbl_minutes.Text = rst_m.ToString("D2");

            // 秒数跳动动画：短暂放大
            AnimateSecondsTick();
        }

        private void UpdateProgress()
        {
            if (total_seconds <= 0 || pnlTrack == null) return;
            int remaining = rst_m * 60 + rst_s;
            int elapsed = total_seconds - remaining;
            int percent = (int)((double)elapsed / total_seconds * 100);
            int w = (int)(pnlTrack.ClientSize.Width * Math.Min(100, Math.Max(0, percent)) / 100.0);
            pnlFill.Width = w;
        }

        private void AnimateSecondsTick()
        {
            // 短暂放大字体再恢复，产生跳动效果
            var originalFont = lbl_seconds.Font;
            var biggerFont = new Font(originalFont.FontFamily, originalFont.Size * 1.08f, originalFont.Style);
            lbl_seconds.Font = biggerFont;

            var animTimer = new System.Windows.Forms.Timer { Interval = 200 };
            animTimer.Tick += (s, args) =>
            {
                lbl_seconds.Font = originalFont;
                biggerFont.Dispose();
                animTimer.Stop();
                animTimer.Dispose();
            };
            animTimer.Start();
        }

        private void FadeIn()
        {
            this.Opacity = 0;
            var fadeTimer = new System.Windows.Forms.Timer { Interval = 20 };
            fadeTimer.Tick += (s, e) =>
            {
                this.Opacity += 0.04;
                if (this.Opacity >= 0.92)
                {
                    this.Opacity = 0.92;
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                }
            };
            fadeTimer.Start();
        }

        private void ShowRestEndMessage()
        {
            lock (_lock)
            {
                if (isMessageShowing)
                    return;
                isMessageShowing = true;
            }

            try
            {
                // 获取配置的多个停止时间，默认值为12,18
                var stopHours = GetConfigValue("AutoStopHours", "12,18")
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                int currentHour = DateTime.Now.Hour;

                // 检查当前时间是否等于任意一个停止时间
                if (stopHours.Any(h => currentHour == h))
                {
                    return;
                }

                // 使用统一的样式化对话框
                ThemeDialog.ShowInfo("休息结束",
                    "✅ 站立时间结束！\n\n请坐下继续工作吧～\n（窗口将在 15 秒后自动关闭）\n\n点击此处立即关闭", 15);

                // 显示工作窗口
                int workTimeValue = GetConfigValue("WorkTimeValue", 45);
                int restTimeValue = GetConfigValue("RestTimeValue", 15);
                WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue);
                workFrm.Show();

                // 关闭所有 RestFrm 实例
                var restForms = Application.OpenForms.Cast<Form>()
                    .Where(f => f is RestFrm)
                    .ToList();

                foreach (var openForm in restForms)
                {
                    openForm.Dispose();
                }
            }
            finally
            {
                lock (_lock)
                {
                    isMessageShowing = false;
                }
            }
        }

        private int GetConfigValue(string key, int defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        private string GetConfigValue(string key, string defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        private void RestFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (input_flag)
            {
                KeyboardBlocker.on();
            }

            // 检查是否是用户手动关闭休息遮罩窗体
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // 遍历所有打开的窗体，隐藏所有 RestFrm 实例
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm is RestFrm && openForm != this)
                    {
                        openForm.Hide();
                    }
                }
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                // 同步关闭所有 RestFrm 实例
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm is RestFrm)
                    {
                        openForm.Close();
                    }
                }
            }
        }

        private void lblText_Click(object sender, EventArgs e)
        {
            // 空实现，用于事件绑定
        }
    }
}
