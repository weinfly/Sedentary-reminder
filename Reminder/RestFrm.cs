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
                : $"久坐对身体不好！您已久坐{wrk_m}分钟了，站起来活动下，也别忘了喝水哦！Alt+F4 退出本界面。";

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

        private static bool messageShown = false;
        // 添加一个静态字段来跟踪消息框是否正在显示
        private static bool isMessageShowing = false;

        private void ShowRestEndMessage()
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
        /* private void ShowRestEndMessage()
         {
             // 检查消息框是否正在显示
             if (isMessageShowing)
             {
                 return;
             }

             // 设置消息框正在显示
             isMessageShowing = true;

             // 读取 app.config 中的 RestEndMessageScreen 设置项
             int restEndMessageScreen = GetConfigValue("RestEndMessageScreen", 0);

             // 获取所有显示器
             Screen[] screens = Screen.AllScreens;

             // 确定要显示消息框的显示器
             Screen targetScreen = restEndMessageScreen >= 0 && restEndMessageScreen < screens.Length
                 ? screens[restEndMessageScreen]
                 : Screen.PrimaryScreen;

             // 创建一个临时窗体，用于在指定的显示器上显示消息框
             Form messageForm = new Form
             {
                 StartPosition = FormStartPosition.Manual,
                 Location = targetScreen.Bounds.Location,
                 Size = new Size(300, 200),
                 FormBorderStyle = FormBorderStyle.None,
                 TopMost = true
             };

             try
             {
                 // 显示消息框
                 DialogResult result = MessageBox.Show(messageForm, "站立时间结束，请坐下继续搬砖吧！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
         }*/

        private int GetConfigValue(string key, int defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return int.TryParse(value, out int result) ? result : defaultValue;
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