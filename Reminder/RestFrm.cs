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
        }

        private void SetupUI()
        {
            lblText.Text = input_flag
                ? $"久坐对身体不好！您已久坐{wrk_m}分钟了，键盘和鼠标被锁定，禁止操作且无法退出，站起来活动下吧！"
                : $"久坐对身体不好！您已久坐{wrk_m}分钟了，站起来活动下！Alt+F4 退出本界面。";

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
                //System.Diagnostics.Debug.WriteLine($"倒计时结束，rst_s的值为：{rst_s}");;
                countdownTimer.Stop();
                ShowRestEndMessage();
            }
        }

        private void UpdateTimeLabels()
        {
            lbl_seconds.Text = rst_s.ToString("D2");
            lbl_minutes.Text = rst_m.ToString("D2");
        }

        private void ShowRestEndMessage()
        {
            DialogResult result = MessageBox.Show("站立时间结束，请坐下继续搬砖吧！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                int workTimeValue = GetConfigValue("WorkTimeValue", 45);
                int restTimeValue = GetConfigValue("RestTimeValue", 15);

                WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue, input_flag);
                workFrm.Show();

                this.Close(); // 关闭当前的 RestFrm 窗体
            }
        }

        private int GetConfigValue(string key, int defaultValue)
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configPath
            };

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            if (config.AppSettings.Settings.AllKeys.Contains(key))
            {
                string valueString = config.AppSettings.Settings[key].Value;
                if (int.TryParse(valueString, out int value))
                {
                    return value;
                }
            }

            return defaultValue;
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
                e.Cancel = true; // 取消关闭操作
                this.Hide(); // 隐藏窗体
                //System.Diagnostics.Debug.WriteLine("RestFrm hidden");
            }
        }

        // 添加这个空方法来解决设计器文件中的错误
        private void lblText_Click(object sender, EventArgs e)
        {
        }
    }
}