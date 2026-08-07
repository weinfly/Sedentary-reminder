using System;
using System.Drawing;
using System.Windows.Forms;

namespace Reminder
{
    /// <summary>
    /// 统一的样式化信息对话框，替代手写 Form 与 MessageBox。
    /// 视觉风格由 ThemeManager 集中控制，支持自动关闭。
    /// </summary>
    public static class ThemeDialog
    {
        /// <summary>
        /// 显示信息对话框；autoCloseSeconds &gt; 0 时按指定秒数自动关闭。
        /// </summary>
        public static void ShowInfo(string title, string message, int autoCloseSeconds = 0)
        {
            using (var dlg = new Form())
            {
                dlg.Size = new Size(420, 220);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.Text = title;
                dlg.TopMost = true;
                dlg.ControlBox = false;
                dlg.BackColor = ThemeManager.Primary;
                dlg.ForeColor = ThemeManager.OnPrimary;

                var label = new Label
                {
                    Text = message,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = ThemeManager.Regular(13),
                    ForeColor = ThemeManager.OnPrimary,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                label.Click += (s, e) => dlg.Close();
                dlg.Controls.Add(label);

                if (autoCloseSeconds > 0)
                {
                    var closeTimer = new Timer { Interval = autoCloseSeconds * 1000 };
                    closeTimer.Tick += (s, e) =>
                    {
                        closeTimer.Stop();
                        closeTimer.Dispose();
                        dlg.Close();
                    };
                    closeTimer.Start();
                }

                dlg.ShowDialog();
            }
        }
    }
}
