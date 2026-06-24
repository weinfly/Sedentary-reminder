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
        private bool colonVisible = true; // 冒号闪烁状态

        public RestFrm(int rst_minutes, int wrk_minutes, bool input_flag, bool main_screen, Point location)
        {
            InitializeComponent();
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
            this.Opacity = 0.85;

            UpdateTimeLabels();
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
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            // 闪烁冒号
            colonVisible = !colonVisible;
            labelColon.Visible = colonVisible;

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
            if (total_seconds <= 0) return;
            int remaining = rst_m * 60 + rst_s;
            int elapsed = total_seconds - remaining;
            int percent = (int)((double)elapsed / total_seconds * 100);
            progressBar.Value = Math.Min(100, Math.Max(0, percent));
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

                // 创建自定义消息框 - 现代化样式
                Form messageBoxForm = new Form
                {
                    Size = new Size(420, 220),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = "休息结束",
                    TopMost = true,
                    ControlBox = false,
                    BackColor = Color.FromArgb(76, 175, 80) // 绿色
                };

                // 创建计时器
                System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer
                {
                    Interval = 15000
                };

                Label messageLabel = new Label
                {
                    Text = "✅ 站立时间结束！\n\n请坐下继续工作吧～\n（窗口将在 15 秒后自动关闭）\n\n点击此处立即关闭",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("微软雅黑", 13F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                messageLabel.Click += (s, args) =>
                {
                    closeTimer.Stop();
                    messageBoxForm.Close();
                };
                messageLabel.Cursor = Cursors.Hand;

                messageBoxForm.Controls.Add(messageLabel);
                messageBoxForm.FormClosed += (s, args) => closeTimer.Dispose();

                closeTimer.Start();
                messageBoxForm.ShowDialog();

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