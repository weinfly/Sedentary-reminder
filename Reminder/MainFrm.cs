using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace Reminder
{
    public partial class MainFrm : Form
    {
        WorkFrm wrkFrm;
        private ContextMenuStrip notifyIconContextMenu;
        private ToolStripMenuItem autoStartMenuItem;
        private ToolStripMenuItem exitMenuItem;
        private ToolStripMenuItem aboutMenuItem;

        public MainFrm()
        {
            InitializeComponent();

            // 初始化上下文菜单
            //notifyIconContextMenu = new ContextMenuStrip();
            //notifyIcon1.ContextMenuStrip = notifyIconContextMenu;

            // 使用新的 CheckedContextMenuStrip 实例
            notifyIconContextMenu = new CheckedContextMenuStrip();
            notifyIcon1.ContextMenuStrip = notifyIconContextMenu;

            // 添加"主窗口"菜单项
            var mainFormMenuItem = new ToolStripMenuItem("主窗口");
            mainFormMenuItem.Click += MainMenuItemClick;
            notifyIconContextMenu.Items.Add(mainFormMenuItem);

            // 添加"开机启动"菜单项
            autoStartMenuItem = new ToolStripMenuItem("开机启动");
            autoStartMenuItem.CheckOnClick = false;
            autoStartMenuItem.Click += AutoStartMenuItem_Click;
            notifyIconContextMenu.Items.Add(autoStartMenuItem);

            // 添加"关于"菜单项
            aboutMenuItem = new ToolStripMenuItem("关于");
            aboutMenuItem.Click += 关于ToolStripMenuItem_Click;
            notifyIconContextMenu.Items.Add(aboutMenuItem);

            // 添加"退出"菜单项
            exitMenuItem = new ToolStripMenuItem("退出");
            exitMenuItem.Click += 退出ToolStripMenuItem_Click;
            notifyIconContextMenu.Items.Add(exitMenuItem);
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            //最小化主窗口
            //这里有问题，只能最小化，visible和hide都无法隐藏，暂时无法解决
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            //this.Visible = false;
            this.Hide();

            // 在窗体加载时调用 Btn_start_Click 方法
            First_Start(null, EventArgs.Empty); 

            bool autoStart = GetQuickFromFolder(systemStartPath, appAllPath).Count > 0;
            autoStartMenuItem.Checked = autoStart;
        }

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

            int wrkTime = (int)this.numWrkTime.Value;
            int rstTime = (int)this.numRstTime.Value;

            if (WorkFrm.IsRunning())
            {
                // 如果有 WorkFrm 在运行,先关闭它
                foreach (Form form in Application.OpenForms)
                {
                    if (form is WorkFrm)
                    {
                        form.Close();
                        break;
                    }
                }
            }
            // 创建并显示新的 WorkFrm
            wrkFrm = new WorkFrm(wrkTime, rstTime);
            wrkFrm.Show();
            this.Visible = false;
        }
        private void Btn_start_Click(object sender, EventArgs e)
        {
            bool input_flag;

            if (this.ckBoxInput.Checked)
            {
                input_flag = true;
            }
            else
            {
                input_flag = false;
            }

            int wrkTime = (int)this.numWrkTime.Value;
            int rstTime = (int)this.numRstTime.Value;

            if (WorkFrm.IsRunning())
            {
                // 如果有 WorkFrm 在运行,先关闭它
                foreach (Form form in Application.OpenForms)
                {
                    if (form is WorkFrm)
                    {
                        form.Close();
                        break;
                    }
                }
            }
            // 创建并显示新的 WorkFrm
            wrkFrm = new WorkFrm(wrkTime, rstTime, input_flag);
            wrkFrm.Show();
            //MainFrm.Visible = false;
            this.Visible = false;
        }
        private void MainMenuItemClick(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            if (wrkFrm != null)
            {
                wrkFrm.Close();
            }
        }
        private void 主窗体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            if (wrkFrm != null)
            {
                wrkFrm.Close();
            }
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消关闭窗口
            e.Cancel = true;
            //最小化主窗口
            this.WindowState = FormWindowState.Minimized;
            //this.Visible = false;
            //不在系统任务栏显示主窗口图标
            this.ShowInTaskbar = false;
            this.Hide();
            
            // 关闭所有 WorkFrm 窗口
            foreach (Form form in Application.OpenForms)
            {
                if (form is WorkFrm)
                {
                    form.Close();
                }
            }
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

        private string QuickName = "Sedentary Reminder";

        private string systemStartPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Startup); } }

        private string appAllPath { get { return Process.GetCurrentProcess().MainModule.FileName; } }

        private void SetAutoStart(bool auto_run)
        {
            // get the path set of this software
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
                WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
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
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
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
    }
}