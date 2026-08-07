using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Configuration;

namespace Reminder
{
    public partial class MainFrm : Form
    {
        WorkFrm wrkFrm;
        private ContextMenuStrip notifyIconContextMenu;
        private ToolStripMenuItem autoStartMenuItem;
        private ToolStripMenuItem exitMenuItem;
        private ToolStripMenuItem aboutMenuItem;
        private Timer statusTimer;

        public MainFrm()
        {
            InitializeComponent();

            // 初始化上下文菜单
            notifyIconContextMenu = new CheckedContextMenuStrip();
            notifyIcon1.ContextMenuStrip = notifyIconContextMenu;

            var mainFormMenuItem = new ToolStripMenuItem("主窗口");
            mainFormMenuItem.Click += MainMenuItemClick;
            notifyIconContextMenu.Items.Add(mainFormMenuItem);

            autoStartMenuItem = new ToolStripMenuItem("开机启动");
            autoStartMenuItem.CheckOnClick = false;
            autoStartMenuItem.Click += AutoStartMenuItem_Click;
            notifyIconContextMenu.Items.Add(autoStartMenuItem);

            aboutMenuItem = new ToolStripMenuItem("关于");
            aboutMenuItem.Click += 关于ToolStripMenuItem_Click;
            notifyIconContextMenu.Items.Add(aboutMenuItem);

            exitMenuItem = new ToolStripMenuItem("退出");
            exitMenuItem.Click += 退出ToolStripMenuItem_Click;
            notifyIconContextMenu.Items.Add(exitMenuItem);

            var statsMenuItem = new ToolStripMenuItem("📊 使用统计");
            statsMenuItem.Click += StatsMenuItem_Click;
            notifyIconContextMenu.Items.Add(statsMenuItem);

            // 锁定开关状态切换
            ckBoxInput.CheckedChanged += CkBoxInput_CheckedChanged;

            // 隐藏倒计时窗口开关
            ckBoxHide.CheckedChanged += CkBoxHide_CheckedChanged;

            // 托盘状态定时刷新
            statusTimer = new Timer { Interval = 1000 };
            statusTimer.Tick += (s, e) => UpdateTrayStatus();
            statusTimer.Start();

            // 主题变化（含跟随系统）时重绘主窗体
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            ThemeManager.LoadMode();
            ApplyTheme();
            UpdateLockToggle();

            // 初始化“隐藏倒计时窗口”开关，并应用主题色
            ckBoxHide.Checked = GetConfigInt("HideWorkForm", 1) == 1;
            UpdateHideToggle();

            // 初始化主题选择控件
            cmbTheme.Items.AddRange(new object[] { "☀ 浅色", "🌙 深色", "🖥 跟随系统" });
            cmbTheme.SelectedIndex = (int)ThemeManager.Mode;
            cmbTheme.SelectedIndexChanged += CmbTheme_SelectedIndexChanged;

            bool firstRun = !File.Exists(FirstRunFlagPath);
            if (firstRun)
            {
                // 首次运行：显示主窗口并给出引导提示
                this.ShowInTaskbar = true;
                this.Opacity = 1;
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.ShowBalloonTip(3000, "久坐提醒已启动",
                    "程序已最小化到系统托盘。右键托盘图标可打开设置或退出。", ToolTipIcon.Info);
                try { File.WriteAllText(FirstRunFlagPath, DateTime.Now.ToString()); }
                catch { /* 无权限时忽略，下次仍会引导 */ }
            }
            else
            {
                // 直接隐藏主窗口，不最小化到任务栏
                this.ShowInTaskbar = false;
                this.Hide();
            }

            bool autoStart = GetQuickFromFolder(systemStartPath, appAllPath).Count > 0;
            autoStartMenuItem.Checked = autoStart;
            UpdateTrayStatus();
        }

        private string FirstRunFlagPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firstrun.lock");

        public void SetWorkTimeValue(int value)
        {
            numWrkTime.Value = value;
        }

        public void SetRestTimeValue(int value)
        {
            numRstTime.Value = value;
        }

        private void First_Start(object sender, EventArgs e)
        {
            SetConfigValue("HideWorkForm", ckBoxHide.Checked ? 1 : 0);

            int wrkTime = (int)this.numWrkTime.Value;
            int rstTime = (int)this.numRstTime.Value;

            if (WorkFrm.IsRunning())
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is WorkFrm)
                    {
                        form.Close();
                        break;
                    }
                }
            }
            wrkFrm = new WorkFrm(wrkTime, rstTime);
            wrkFrm.Show();
            this.Visible = false;
        }

        private void Btn_start_Click(object sender, EventArgs e)
        {
            SetConfigValue("HideWorkForm", ckBoxHide.Checked ? 1 : 0);

            bool input_flag = this.ckBoxInput.Checked;

            int wrkTime = (int)this.numWrkTime.Value;
            int rstTime = (int)this.numRstTime.Value;

            if (WorkFrm.IsRunning())
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is WorkFrm)
                    {
                        form.Close();
                        break;
                    }
                }
            }
            wrkFrm = new WorkFrm(wrkTime, rstTime, input_flag);
            wrkFrm.Show();
            this.Visible = false;
            UpdateTrayStatus();
        }

        private void MainMenuItemClick(object sender, EventArgs e)
        {
            this.Opacity = 1;
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void 主窗体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Opacity = 1;
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消关闭窗口，仅隐藏主窗口
            e.Cancel = true;
            this.ShowInTaskbar = false;
            this.Opacity = 0;
            this.Hide();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            System.Environment.Exit(0);
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void CkBoxInput_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLockToggle();
        }

        private void UpdateLockToggle()
        {
            if (ckBoxInput.Checked)
            {
                ckBoxInput.BackColor = ThemeManager.Primary;
                ckBoxInput.ForeColor = ThemeManager.OnPrimary;
            }
            else
            {
                ckBoxInput.BackColor = ThemeManager.ControlFill;
                ckBoxInput.ForeColor = ThemeManager.ControlText;
            }
        }

        private void CkBoxHide_CheckedChanged(object sender, EventArgs e)
        {
            UpdateHideToggle();
            SetConfigValue("HideWorkForm", ckBoxHide.Checked ? 1 : 0);
            WorkFrm.Current?.SetHidden(ckBoxHide.Checked);
        }

        private void UpdateHideToggle()
        {
            if (ckBoxHide.Checked)
            {
                ckBoxHide.BackColor = ThemeManager.Primary;
                ckBoxHide.ForeColor = ThemeManager.OnPrimary;
            }
            else
            {
                ckBoxHide.BackColor = ThemeManager.ControlFill;
                ckBoxHide.ForeColor = ThemeManager.ControlText;
            }
        }

        private void UpdateTrayStatus()
        {
            if (WorkFrm.IsRunning() && WorkFrm.Current != null)
            {
                var remain = WorkFrm.Current.RemainingTime;
                notifyIcon1.Text = $"久坐提醒 · 下次休息 {remain.Hours:D2}:{remain.Minutes:D2}";
            }
            else
            {
                notifyIcon1.Text = "久坐提醒 · 未开始";
            }
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

        private void CmbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTheme.SelectedIndex < 0) return;
            ThemeManager.Mode = (ThemeMode)cmbTheme.SelectedIndex;
        }

        private void StatsMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new StatsFrm())
            {
                frm.ShowDialog(this);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Surface;
            this.ForeColor = ThemeManager.TextPrimary;

            pnlHeader.BackColor = ThemeManager.Primary;
            lblTitle.ForeColor = ThemeManager.OnPrimary;
            lblTitle.Font = ThemeManager.Bold(13);
            lblSubtitle.ForeColor = ThemeManager.PrimaryLight;
            lblSubtitle.Font = ThemeManager.Regular(8.5F);

            pnlBody.BackColor = ThemeManager.Background;

            lblTimerIcon.ForeColor = ThemeManager.Primary;
            lblTimerTitle.ForeColor = ThemeManager.TextPrimary;
            lblTimerTitle.Font = ThemeManager.Bold(10);

            lblWorkTime.ForeColor = ThemeManager.TextSecondary;
            lblRestTime.ForeColor = ThemeManager.TextSecondary;
            lblWorkUnit.ForeColor = ThemeManager.TextSecondary;
            lblRestUnit.ForeColor = ThemeManager.TextSecondary;

            lblOptionsIcon.ForeColor = ThemeManager.Primary;
            lblOptionsTitle.ForeColor = ThemeManager.TextPrimary;
            lblOptionsTitle.Font = ThemeManager.Bold(10);

            // 锁定开关：按钮外观 + 扁平风格，颜色完全可控
            ckBoxInput.Appearance = Appearance.Button;
            ckBoxInput.FlatStyle = FlatStyle.Flat;
            ckBoxInput.FlatAppearance.BorderSize = 0;
            ckBoxInput.ThreeState = false;
            ckBoxInput.UseVisualStyleBackColor = false;

            // 隐藏窗口开关：与锁定开关一致的按钮外观
            ckBoxHide.Appearance = Appearance.Button;
            ckBoxHide.FlatStyle = FlatStyle.Flat;
            ckBoxHide.FlatAppearance.BorderSize = 0;
            ckBoxHide.ThreeState = false;
            ckBoxHide.UseVisualStyleBackColor = false;

            // 主题选择下拉框与数值框随主题取色
            cmbTheme.BackColor = ThemeManager.Surface;
            cmbTheme.ForeColor = ThemeManager.TextPrimary;
            cmbTheme.FlatStyle = FlatStyle.Flat;
            numWrkTime.BackColor = ThemeManager.Surface;
            numWrkTime.ForeColor = ThemeManager.TextPrimary;
            numRstTime.BackColor = ThemeManager.Surface;
            numRstTime.ForeColor = ThemeManager.TextPrimary;

            StylePrimaryButton(btn_start);
        }

        private void StylePrimaryButton(Button btn)
        {
            btn.BackColor = ThemeManager.Primary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = ThemeManager.OnPrimary;
            btn.Font = ThemeManager.Bold(12);
            btn.FlatAppearance.MouseOverBackColor = ThemeManager.PrimaryDark;
            btn.FlatAppearance.MouseDownBackColor = ThemeManager.PrimaryDark;
        }

        // ---- 开机启动相关（保持原实现） ----
        private string QuickName = "Sedentary Reminder";

        private string systemStartPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Startup); } }

        private string appAllPath { get { return Process.GetCurrentProcess().MainModule.FileName; } }

        private void SetAutoStart(bool auto_run)
        {
            List<string> shortcurPaths = GetQuickFromFolder(systemStartPath, appAllPath);
            if (auto_run)
            {
                if (shortcurPaths.Count > 1)
                {
                    for (int i = 1; i < shortcurPaths.Count; i++)
                    {
                        DeleteFile(shortcurPaths[i]);
                    }
                }
                else if (shortcurPaths.Count == 0)
                {
                    bool res = CreateShortcut(systemStartPath, QuickName, appAllPath, "Sedentary Reminder");
                }
            }
            else
            {
                if (shortcurPaths.Count > 0)
                {
                    for (int i = 0; i < shortcurPaths.Count; i++)
                    {
                        DeleteFile(shortcurPaths[i]);
                    }
                }
            }
        }

        private bool CreateShortcut(string systemStartPath, string quickName, string appAllPath, string description = null)
        {
            try
            {
                if (!Directory.Exists(systemStartPath)) Directory.CreateDirectory(systemStartPath);
                string shortcutPath = Path.Combine(systemStartPath, string.Format("{0}.lnk", quickName));
                var shell = new IWshRuntimeLibrary.WshShell();
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = appAllPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(shortcutPath);
                shortcut.WindowStyle = 1;
                shortcut.Description = description;
                shortcut.IconLocation = "ICO2.ico";
                shortcut.Save();
                MessageBox.Show("设置开机启动成功", "info", MessageBoxButtons.OK);
                return true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void DeleteFile(string path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                System.IO.File.Delete(path);
                MessageBox.Show("取消开机启动成功", "info", MessageBoxButtons.OK);
            }
        }

        private List<string> GetQuickFromFolder(string systemStartPath, string appAllPath)
        {
            List<string> tmpStrs = new List<string>();
            String tmpStr = null;
            String[] files = Directory.GetFiles(systemStartPath, "*.lnk");
            for (int i = 0; i < files.Length; i++)
            {
                tmpStr = GetAppPathFromQuick(files[i]);
                if (tmpStr == appAllPath)
                {
                    tmpStrs.Add(files[i]);
                }
            }
            return tmpStrs;
        }

        private string GetAppPathFromQuick(string shortcutPath)
        {
            if (System.IO.File.Exists(shortcutPath))
            {
                var shell = new IWshRuntimeLibrary.WshShell();
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                return shortcut.TargetPath;
            }
            else
            {
                return null;
            }
        }

        private void AutoStartMenuItem_Click(object sender, EventArgs e)
        {
            bool autoStart = !autoStartMenuItem.Checked; // 取反当前状态
            SetAutoStart(autoStart); // 设置开机启动状态
            autoStartMenuItem.Checked = autoStart; // 更新菜单项勾选状态
        }

        /// <summary>
        /// 读取 appSettings 中的整型配置，失败返回默认值
        /// </summary>
        private int GetConfigInt(string key, int defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (!int.TryParse(value, out int result))
            {
                return defaultValue;
            }
            return result;
        }

        /// <summary>
        /// 写入并持久化 appSettings 配置；无写入权限时静默忽略
        /// </summary>
        private void SetConfigValue(string key, int value)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                {
                    config.AppSettings.Settings[key].Value = value.ToString();
                }
                else
                {
                    config.AppSettings.Settings.Add(key, value.ToString());
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置 {key} 失败: {ex.Message}");
            }
        }
    }
}
