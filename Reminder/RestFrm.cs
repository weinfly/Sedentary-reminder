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
        // 分辨率自适应缩放因子：以 1920x1080 为基准 1.0，按目标屏幕尺寸收敛
        private float uiScale = 1f;

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
            Logger.Log($"[DBG] RestFrm constructed rst={rst_minutes} wrk_m={wrk_minutes} input_flag={input_flag}");
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
            // —— 分辨率自适应：先按目标屏幕计算统一缩放因子，再套用到所有字体与固定像素度量 ——
            // 这样在任何分辨率/DPI 下，文字、图标、倒计时、进度条保持一致的相对比例。
            Rectangle targetBounds = GetTargetScreenBounds();
            uiScale = Math.Max(0.7f, Math.Min(2.4f,
                Math.Min(targetBounds.Width / 1920f, targetBounds.Height / 1080f)));

            ApplyTheme();

            // 根据是否锁定输入显示不同提示
            if (input_flag)
            {
                lblText.Text = $"⚠️ 久坐对身体不好！您已久坐{wrk_m}分钟了，\n键盘和鼠标被锁定，请站起来活动一下！";
                lblTimerTitle.Text = "🔒 键盘已锁定 · 休息倒计时";
                lblIcon.Text = "🏃";
                lblHint.Text = "紧急解锁：按 Ctrl+Alt+Delete 调出安全界面，再切回即可恢复鼠标键盘。";
                // M-1：默认仅锁定前 2 分钟（120s），到点自动解锁；多显示器引用计数配对解锁（见 KeyboardBlocker）
                KeyboardBlocker.Block();
            }
            else
            {
                lblText.Text = $"💪 久坐对身体不好！您已久坐{wrk_m}分钟了，\n站起来活动下吧，顺便喝口水！(Alt+F4 退出)";
                lblTimerTitle.Text = "☕ 休息倒计时";
                lblIcon.Text = "☕";
                lblHint.Text = "（Alt+F4 可退出休息）";
            }

            this.TopMost = true;
            // 精确覆盖目标屏幕：不再用 WindowState=Maximized（多显示器/不同分辨率下
            // 最大化可能落到主屏或与其它屏遮罩重叠），改为显式设置到该屏幕的完整边界。
            this.WindowState = FormWindowState.Normal;
            this.Bounds = targetBounds;

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

            // 窗口尺寸变化时重新居中并缩放倒计时
            this.Resize += (s, e) => LayoutTimeDisplay();

            UpdateTimeLabels();
            LayoutTimeDisplay();
        }

        /// <summary>
        /// 从 ThemeManager 统一取色与字体，覆盖 Designer 中的硬编码
        /// </summary>
        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Primary;
            lblText.ForeColor = ThemeManager.OnPrimary;
            // 全部按 uiScale 同比缩放，避免“文字固定、倒计时暴涨”的比例失衡
            lblText.Font = ThemeManager.Bold(20f * uiScale);
            lblIcon.ForeColor = ThemeManager.OnPrimary;
            lblIcon.Font = ThemeManager.Regular(90f * uiScale);
            lblTimerTitle.ForeColor = ThemeManager.OnPrimary;
            lblTimerTitle.Font = ThemeManager.Regular(16f * uiScale);
            lblHint.ForeColor = ThemeManager.OnPrimary;
            lblHint.Font = ThemeManager.Regular(13f * uiScale);

            // 进度条行高与内边距随分辨率缩放：4K 下不再细如发丝，小屏也不再过粗
            if (tableLayoutPanelMain != null && tableLayoutPanelMain.RowStyles.Count > 2)
            {
                tableLayoutPanelMain.RowStyles[2].Height = Math.Max(18f, 30f * uiScale);
            }
            if (pnlProgress != null)
            {
                int padX = Math.Max(20, (int)(80f * uiScale));
                int padY = Math.Max(4, (int)(8f * uiScale));
                pnlProgress.Padding = new Padding(padX, padY, padX, padY);
            }

            LayoutTimeDisplay();
        }

        /// <summary>
        /// 居中并自适应缩放倒计时数字（MM : SS），避免弹跳、保证美观
        /// </summary>
        private void LayoutTimeDisplay()
        {
            if (pnlTimeDisplay == null) return;
            int w = pnlTimeDisplay.ClientSize.Width;
            int h = pnlTimeDisplay.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            // 字号随面板高度自适应，但必须同时受“宽度”和“分辨率”双重约束：
            //  - 宽度：两位数字(各约 1.85fs) + 冒号(约 0.6fs) ≈ 4.3fs，留 5% 边距，
            //    否则高分辨率下“02:45”的首末位会被裁切（只看到“2”“4”的原因）。
            //  - 分辨率：上限随 uiScale 收敛，避免数字相对其它文字过分膨胀。
            float fs = Math.Min(200f, h * 0.62f);
            float maxFsByWidth = w * 0.95f / 4.3f;
            fs = Math.Min(fs, maxFsByWidth);
            fs = Math.Min(fs, 170f * uiScale);
            fs = Math.Max(24f, fs);

            var font = ThemeManager.Number(fs);

            lbl_minutes.Font = font;
            labelColon.Font = font;
            lbl_seconds.Font = font;

            lbl_minutes.TextAlign = ContentAlignment.MiddleCenter;
            labelColon.TextAlign = ContentAlignment.MiddleCenter;
            lbl_seconds.TextAlign = ContentAlignment.MiddleCenter;

            int lineH = Math.Min((int)(fs * 1.3f), h);
            int colonW = (int)(fs * 0.6f);
            int numW = (int)(fs * 1.85f);
            int totalW = numW * 2 + colonW;
            int x = Math.Max(0, (w - totalW) / 2);
            int y = Math.Max(0, (h - lineH) / 2);

            lbl_minutes.SetBounds(x, y, numW, lineH);
            labelColon.SetBounds(x + numW, y, colonW, lineH);
            lbl_seconds.SetBounds(x + numW + colonW, y, numW, lineH);
        }

        /// <summary>
        /// 解析本窗体应覆盖的屏幕：优先用构造时传入的左上角点定位屏幕，失败回退主屏。
        /// 用于多显示器 / 不同分辨率场景，保证每块屏一个遮罩且尺寸与该屏精确一致。
        /// </summary>
        private Rectangle GetTargetScreenBounds()
        {
            try
            {
                Screen screen = Screen.FromPoint(originalLocation);
                if (screen != null)
                {
                    return screen.Bounds;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[WARN] RestFrm.GetTargetScreenBounds 失败，回退主屏: {ex.Message}");
            }
            return Screen.PrimaryScreen != null ? Screen.PrimaryScreen.Bounds : this.Bounds;
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
                return;
            }

            // 倒计时归零：立即停表，避免重复触发与重入
            if (countdownTimer != null && countdownTimer.Enabled)
            {
                countdownTimer.Stop();
            }

            bool shouldEnd;
            lock (_lock)
            {
                shouldEnd = !messageShown;
                if (shouldEnd) messageShown = true;
            }

            if (shouldEnd)
            {
                Logger.Log("[DBG] RestFrm countdown reached zero, ending rest session");
                ShowRestEndMessage();
            }
            else
            {
                // 其它实例（多显示器）已处理归零，本实例直接关闭
                Logger.Log("[DBG] RestFrm countdown zero handled by another instance, closing self");
                if (!this.IsDisposed) this.Close();
            }
        }

        private void UpdateTimeLabels()
        {
            lbl_seconds.Text = rst_s.ToString("D2");
            lbl_minutes.Text = rst_m.ToString("D2");
            // 不再做“弹跳”缩放动画，避免数字跳动、提升可读性
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
            bool entered;
            lock (_lock)
            {
                if (isMessageShowing)
                {
                    Logger.Log("[DBG] ShowRestEndMessage: isMessageShowing already true, early return");
                    return;
                }
                isMessageShowing = true;
                entered = true;
            }
            if (!entered) return;

            try
            {
                // 自动停止时段：到点不再开启下一轮工作倒计时
                var stopHours = GetConfigValue("AutoStopHours", "12,18")
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out int v) ? v : -1)
                    .Where(v => v >= 0)
                    .ToList();

                int currentHour = DateTime.Now.Hour;

                if (stopHours.Any(h => currentHour == h))
                {
                    Logger.Log($"[DBG] ShowRestEndMessage: stop hour ({currentHour}) reached, NOT creating new WorkFrm");
                    CloseAllRestForms();
                    return;
                }

                // 关键修复：先启动下一轮工作倒计时并关闭所有休息窗，再显示“休息结束”提示。
                // 这样即使提示窗口在模态期间被用户关闭/卡住，下一轮 WorkFrm 已经创建、
                // 所有 RestFrm 已经关闭，循环不会中断（第一轮休息后卡死的根因）。
                StartNextWorkForm();
                CloseAllRestForms();

                // 最后显示“休息结束”提示（此时所有 RestFrm 已关闭，即使用户在提示窗口按
                // Alt+F4 也只关闭提示本身，不会触发 RestFrm_FormClosing 隐藏关键窗体）。
                Logger.Log("[DBG] ShowRestEndMessage: showing rest-end dialog (modal)");
                ThemeDialog.ShowInfo("休息结束",
                    "✅ 站立时间结束！\n\n请坐下继续工作吧～\n（窗口将在 15 秒后自动关闭）\n\n点击此处立即关闭", 15);
            }
            catch (Exception ex)
            {
                // 任何异常都不应中断循环：兜底再尝试启动下一轮并关闭休息窗
                Logger.Log($"[ERR] ShowRestEndMessage 异常: {ex.Message}\n{ex.StackTrace}");
                try { StartNextWorkForm(); } catch { }
                try { CloseAllRestForms(); } catch { }
            }
            finally
            {
                lock (_lock)
                {
                    isMessageShowing = false;
                }
            }
        }

        /// <summary>
        /// 启动下一轮工作倒计时（防御性 try-catch，失败仅记录，不影响关闭流程）
        /// </summary>
        private void StartNextWorkForm()
        {
            try
            {
                int workTimeValue = GetConfigValue("WorkTimeValue", 45);
                int restTimeValue = GetConfigValue("RestTimeValue", 15);
                bool lockInput = GetConfigValue("LockInput", 0) == 1;
                WorkFrm workFrm = new WorkFrm(workTimeValue, restTimeValue, lockInput);
                workFrm.Show();
                Logger.Log($"[DBG] ShowRestEndMessage: next WorkFrm created (wrk={workTimeValue}, rst={restTimeValue}, lock={lockInput})");
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERR] ShowRestEndMessage 创建下一轮 WorkFrm 失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 关闭所有休息窗体：先关其它实例，再关自身（若自身仍存活），避免自销毁重入
        /// </summary>
        private void CloseAllRestForms()
        {
            try
            {
                var others = Application.OpenForms.Cast<Form>()
                    .Where(f => f is RestFrm && f != this)
                    .ToList();
                foreach (var f in others)
                {
                    if (!f.IsDisposed) f.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERR] 关闭其它 RestFrm 异常: {ex.Message}");
            }

            try
            {
                if (!this.IsDisposed && !this.Disposing) this.Close();
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERR] 关闭自身 RestFrm 异常: {ex.Message}");
            }
        }

        private int GetConfigValue(string key, int defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (!int.TryParse(value, out int result))
            {
                return defaultValue;
            }
            // m-3：工作时长/休息时长必须为正数，避免误配 0 导致“工作0秒→休息→再0秒”死循环闪烁
            if ((key == "WorkTimeValue" || key == "RestTimeValue") && result <= 0)
            {
                Logger.Log($"[WARN] RestFrm.GetConfigValue 无效 {key}={result}，使用默认 {defaultValue}");
                return defaultValue;
            }
            return result;
        }

        private string GetConfigValue(string key, string defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        private void RestFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.Log($"[DBG] RestFrm_FormClosing reason={e.CloseReason} input_flag={input_flag}");

            // 停止倒计时，避免窗体关闭后计时器仍在 Tick 已释放的控件
            if (countdownTimer != null)
            {
                countdownTimer.Stop();
                countdownTimer.Enabled = false;
            }

            if (input_flag)
            {
                KeyboardBlocker.Unblock();
            }

            // 用户手动关闭（Alt+F4 / 点击）：
            //  - 若正处于“休息结束→下一轮”流程中（isMessageShowing 为 true），不要隐藏，
            //    直接关闭本实例，避免隐藏关键窗体打断循环（第一轮休息后卡死的根因）。
            //  - 否则（休息进行中用户主动关闭）：隐藏并继续后台倒计时，保留原有便利行为。
            if (e.CloseReason == CloseReason.UserClosing && !isMessageShowing)
            {
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
            // 其它情况（程序化关闭 / 结束流程中的用户关闭）：直接关闭本实例
        }

        private void lblText_Click(object sender, EventArgs e)
        {
            // 空实现，用于事件绑定
        }
    }
}
