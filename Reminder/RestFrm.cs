using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Reminder
{
    public partial class RestFrm : Form
    {
        // 静态字段
        private static bool messageShown = false;
        private static bool isMessageShowing = false;

        // 实例字段
        private int rst_m;
        private int wrk_m;
        private bool input_flag;
        private int rst_s;
        private System.Windows.Forms.Timer countdownTimer;
        private bool main_screen;
        private Point originalLocation;

        public RestFrm(int rst_minutes, int wrk_minutes, bool input_flag, bool main_screen, Point location)
        {
            InitializeComponent();
            this.rst_m = rst_minutes;
            this.rst_s = 0;
            this.wrk_m = wrk_minutes;
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
            this.Activate(); // 确保窗体在加载时获得焦点
        }

        private void SetupUI()
        {
            lblText.Text = input_flag
                ? $"久坐对身体不好！您已久坐{wrk_m}分钟了，键盘和鼠标被锁定，禁止操作且无法退出，站起来活动下吧！"
                : $"久坐对身体不好！您已久坐{wrk_m}分钟了，站起来活动下吧，顺便喝口水，补充下水分！Alt+F4 退出本界面。";

            if (input_flag)
            {
                KeyboardBlocker.off();
            }

            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            this.Opacity = 0.75;

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
            messageShown = false;
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
                //System.Diagnostics.Debug.WriteLine($"倒计时中，rst_s的值为：{rst_s}");
            }
            else
            {
                // 检查消息框是否已经显示过
                if (!messageShown)
                {
                    // 标记消息框已经显示过
                    messageShown = true;

                    // 调用 ShowRestEndMessage 方法
                    ShowRestEndMessage();
                }
                else
                {
                    // 如果消息框已经显示过，停止计时器并关闭当前实例
                    countdownTimer.Stop();
                    this.Close();
                }
            }
        }

        private void UpdateTimeLabels()
        {
            lbl_seconds.Text = rst_s.ToString("D2");
            lbl_minutes.Text = rst_m.ToString("D2");
        }

        private void ShowRestEndMessage()
        {
            if (isMessageShowing)
                return;

            isMessageShowing = true;
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

                // 创建自定义消息框
                Form messageBoxForm = new Form
                {
                    Size = new Size(400, 200),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = "提示",
                    TopMost = true,
                    ControlBox = false
                };

                // 创建并配置计时器
                System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer
                {
                    Interval = 15000 // 15秒
                };

                // 添加计时器Tick事件处理
                closeTimer.Tick += (s, args) =>
                {
                    closeTimer.Stop();
                    messageBoxForm.Close();
                };

                Label messageLabel = new Label
                {
                    Text = "站立时间结束，请坐下继续搬砖吧！\n（本窗口将在15秒后自动关闭）\n\n点击此处可立即关闭",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("微软雅黑", 14)
                };
                messageLabel.Click += (s, args) =>
                {
                    closeTimer.Stop();
                    messageBoxForm.Close();
                };
                messageLabel.Cursor = Cursors.Hand; // 将光标改为手型，表示可点击

                messageBoxForm.Controls.Add(messageLabel);

                // 窗体关闭时释放资源
                messageBoxForm.FormClosed += (s, args) =>
                {
                    closeTimer.Dispose();
                };

                // 启动计时器并显示消息框
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
                    .ToList(); // 创建副本避免枚举时修改集合

                foreach (var openForm in restForms)
                {
                    openForm.Dispose();
                }
            }
            finally
            {
                isMessageShowing = false;
            }
        }

        private void ShowRestEndMessage_old()
        {
            if (!isMessageShowing)
            {
                isMessageShowing = true;
                try
                {
                    int stopHour = GetConfigValue("AutoStopHour", 18);
                    int currentHour = DateTime.Now.Hour;
                    if (currentHour >= stopHour)
                    {
                        // 如果当前时间已经超过停止时间，不显示消息框
                        return;
                    }
                    // 显示消息框
                    DialogResult result = MessageBox.Show("站立时间结束，请坐下继续搬砖吧！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (result == DialogResult.OK)
                    {
                        int workTimeValue = GetConfigValue("WorkTimeValue", 45);
                        int restTimeValue = GetConfigValue("RestTimeValue", 15);

                        WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue);
                        workFrm.Show();

                        // 关闭所有 RestFrm 实例
                        foreach (Form openForm in Application.OpenForms)
                        {
                            if (openForm is RestFrm)
                            {
                                openForm.Close();
                            }
                        }
                    }
                }
                finally
                {
                    // 设置消息框不再显示
                    isMessageShowing = false;
                }
            }
        }

        private int GetConfigValue(string key, int defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        // 添加新的方法来获取字符串类型的配置值
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
                e.Cancel = true; // 取消关闭操作
                this.Hide(); // 隐藏窗体
                //System.Diagnostics.Debug.WriteLine("RestFrm hidden");
            }
            //同步关闭：在 FormClosing 事件中，遍历 Application.OpenForms，关闭所有 RestFrm 实例。这确保了按一次 Alt+F4 可以关闭所有显示器上的遮罩窗体。
            else
            {
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm is RestFrm)
                    {
                        openForm.Close();
                    }
                }
            }
        }

        // 添加这个空方法来解决设计器文件中的错误
        private void lblText_Click(object sender, EventArgs e)
        {
        }
    }
}