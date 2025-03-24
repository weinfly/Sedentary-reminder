using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Reminder
{
    public partial class WorkFrm : Form
    {
        private int wrk_minutes;//工作时间(分)
        private int wrk_seconds;//工作时间(秒)
        private int wrk_m;
        private int rst_minutes;//休息时间(分)
        private bool input_flag;//是否选中锁定键盘
        private bool left_flag;//鼠标左键是否点击
        private Point mouseoff;
        private RestFrm[] restFrms;
        private static bool isRunning = false;

        public WorkFrm()
        {
            InitializeComponent();
        }
        private int GetConfigValue(string key, int defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return int.TryParse(value, out int result) ? result : defaultValue;
        }
        //定义一个构造函数，接受前一个窗体传来的参数
        public WorkFrm(int wrk_minutes, int rst_minutes)
        {
            InitializeComponent();

            // 获取当前时间
            DateTime now = DateTime.Now;

            // 根据配置文件的WorkTimeValue，计算下一个时间点  20250219，将第一次启动时的记时，改成根据配置文件，而不是固定的45
            // int workTimeValue = int.Parse(ConfigurationManager.AppSettings["WorkTimeValue"]);
            int workTimeValue = GetConfigValue("WorkTimeValue", 45);
            int restTimeValue = GetConfigValue("RestTimeValue", 15);

            DateTime nextBreakTime;
            if (now.Minute < workTimeValue)
            {
                nextBreakTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, workTimeValue, 0);
            }
            else
            {
                nextBreakTime = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, workTimeValue, 0);
            }

            // 计算时间差，精确到毫秒
            TimeSpan timeDiff = nextBreakTime - now;
            double totalMilliseconds = timeDiff.TotalMilliseconds;
            this.wrk_minutes = (int)(totalMilliseconds / 60000);
            this.wrk_seconds = (int)((totalMilliseconds % 60000) / 1000);
            this.rst_minutes = rst_minutes;
            this.wrk_m = wrk_minutes;
            this.input_flag = false;

            //int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - 160;
            //int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - 90;
            // 读取 WorkFormScreen 配置项，若不存在则默认显示在第一个屏幕
            int screenIndex = GetConfigValue("WorkFormScreen", 0);
            Screen[] screens = Screen.AllScreens;

            // 确保屏幕索引在有效范围内
            if (screenIndex < 0 || screenIndex >= screens.Length)
            {
                screenIndex = 0;
            }

            Screen selectedScreen = screens[screenIndex];
            // 读取配置文件中的偏移量
            int offsetX = GetConfigValue("WorkFormOffsetX", 160);
            int offsetY = GetConfigValue("WorkFormOffsetY", 90);
            // 计算窗体的位置   
            int x = selectedScreen.WorkingArea.Right - offsetX;
            int y = selectedScreen.WorkingArea.Bottom - offsetY;
            Point p = new Point(x, y);
            this.PointToScreen(p);
            this.Location = p;
            isRunning = true;
        }
        public WorkFrm(int wrk_minutes, int rst_minutes, bool input_flag)
        {
            InitializeComponent();
            this.wrk_minutes = wrk_minutes;
            this.rst_minutes = rst_minutes;
            this.wrk_m = wrk_minutes;
            this.input_flag = input_flag;
            bool hideWindow = ConfigurationManager.AppSettings["ShowTimerWindow"] == "1";

            int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - 160;
            int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - 90;
            Point p = new Point(x, y);
            this.PointToScreen(p);
            this.Location = p;
            isRunning = true;
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

        private void Form1_Load(object sender, EventArgs e)
        {
            if (wrk_seconds >= 10)
            {
                lblSecond.Text = wrk_seconds.ToString();
            }
            else
            {
                lblSecond.Text = "0" + wrk_seconds.ToString();
            }

            if (wrk_minutes >= 10)
            {
                lblMin.Text = wrk_minutes.ToString();
            }
            else
            {
                lblMin.Text = "0" + wrk_minutes.ToString();
            }

            this.Opacity = 0.8;

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                // 更新秒数
                if (wrk_seconds > 0)
                {
                    wrk_seconds--;
                }
                else
                {
                    // 秒数为0时，更新分钟数
                    wrk_minutes--;
                    wrk_seconds = 59;
                }

                // 更新显示
                lblSecond.Text = wrk_seconds.ToString("00");
                lblMin.Text = wrk_minutes.ToString("00");

                // 检查是否需要提醒
                Warn();

                // 检查是否结束
                if (wrk_minutes < 0)
                {
                    timerWrk.Stop();
                    this.Close();

                    // 在多显示器上显示休息提醒
                    restFrms = new RestFrm[Screen.AllScreens.Length];
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
                // 记录日志或处理异常
                Console.WriteLine($"Timer error: {ex.Message}");
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
                this.BackColor = Color.Red;
                lblWarn.ForeColor = Color.Yellow;
                lblWarn.Text = "该起来站\r\n站了，久\r\n坐对身体\r\n不好！";
                int x = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width) / 2 - this.Width / 2;
                int y = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height) / 2 - this.Height / 2;
                Point p = new Point(x, y);
                this.PointToScreen(p);
                this.Location = p;
            }

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
                mouseSet.Offset(-mouseoff.X, -mouseoff.Y);//这里注意下-的用意，offset
                Location = mouseSet;
            }
        }
        private void mouseUp()
        {
            if (left_flag)
            {
                left_flag = false;
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

        private void LblSecond_Click(object sender, EventArgs e)
        {
            // 空实现，仅用于满足事件绑定
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 空实现，仅用于满足事件绑定
        }




        private void button1_Click(object sender, EventArgs e)
        {
            // 推迟
            WorkFrm workFrm = new WorkFrm(wrk_minutes += 10, rst_minutes, input_flag);
            workFrm.Show();
            this.Close();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            this.timerWrk.Enabled = !this.timerWrk.Enabled;
            if (this.timerWrk.Enabled)
            {
                btnPause.Text = "暂停计时";
            }
            else
            {
                btnPause.Text = "恢复计时";

            }
        }
    }
}
