using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Reminder
{
    /// <summary>
    /// 主题模式：浅色 / 深色 / 跟随系统
    /// </summary>
    public enum ThemeMode
    {
        Light,
        Dark,
        Auto
    }

    /// <summary>
    /// 集中管理 UI 主题：颜色、字体、圆角半径，以及通用绘制辅助。
    /// 所有窗体应从此处取色/取字体，避免硬编码导致主题不一致。
    /// 支持浅色/深色/跟随系统三套调色板；颜色属性按当前激活调色板返回。
    /// </summary>
    public static class ThemeManager
    {
        // ---------- 主题模式 ----------
        public static ThemeMode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode == value) return;
                _mode = value;
                SaveMode(value);
                ThemeChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static ThemeMode _mode = ThemeMode.Light;

        /// <summary>主题切换事件（含跟随系统时的系统主题变化）</summary>
        public static event EventHandler ThemeChanged;

        // ---------- 调色板 ----------
        private sealed class Palette
        {
            public Color Primary;
            public Color PrimaryDark;
            public Color OnPrimary;
            public Color Warning;
            public Color TextPrimary;
            public Color TextSecondary;
            public Color Surface;
            public Color Background;
            public Color PrimaryLight;
            public Color ControlFill;
            public Color ControlText;
            public Color Divider;
        }

        private static readonly Palette Light = new Palette
        {
            Primary = Color.FromArgb(76, 175, 80),
            PrimaryDark = Color.FromArgb(39, 174, 96),
            OnPrimary = Color.White,
            Warning = Color.FromArgb(231, 76, 60),
            TextPrimary = Color.FromArgb(65, 65, 65),
            TextSecondary = Color.FromArgb(117, 117, 117),
            Surface = Color.White,
            Background = Color.FromArgb(248, 249, 250),
            PrimaryLight = Color.FromArgb(220, 235, 250),
            ControlFill = Color.FromArgb(230, 230, 230),
            ControlText = Color.FromArgb(117, 117, 117),
            Divider = Color.FromArgb(225, 225, 225),
        };

        private static readonly Palette Dark = new Palette
        {
            Primary = Color.FromArgb(76, 175, 80),
            PrimaryDark = Color.FromArgb(39, 174, 96),
            OnPrimary = Color.White,
            Warning = Color.FromArgb(231, 76, 60),
            TextPrimary = Color.FromArgb(232, 232, 232),
            TextSecondary = Color.FromArgb(160, 160, 160),
            Surface = Color.FromArgb(45, 45, 48),
            Background = Color.FromArgb(30, 30, 32),
            PrimaryLight = Color.FromArgb(200, 225, 210),
            ControlFill = Color.FromArgb(60, 60, 65),
            ControlText = Color.FromArgb(210, 210, 210),
            Divider = Color.FromArgb(70, 70, 75),
        };

        private static Palette Active
        {
            get
            {
                bool dark = _mode == ThemeMode.Dark || (_mode == ThemeMode.Auto && SystemUsesDarkTheme());
                return dark ? Dark : Light;
            }
        }

        /// <summary>当前是否为深色调色板</summary>
        public static bool IsDark => ReferenceEquals(Active, Dark);

        // ---------- 颜色属性（按当前激活调色板返回） ----------
        public static Color Primary => Active.Primary;
        public static Color PrimaryDark => Active.PrimaryDark;
        public static Color OnPrimary => Active.OnPrimary;
        public static Color Warning => Active.Warning;
        public static Color TextPrimary => Active.TextPrimary;
        public static Color TextSecondary => Active.TextSecondary;
        public static Color Surface => Active.Surface;
        public static Color Background => Active.Background;
        public static Color PrimaryLight => Active.PrimaryLight;
        public static Color ControlFill => Active.ControlFill;
        public static Color ControlText => Active.ControlText;
        public static Color Divider => Active.Divider;

        // ---------- 度量 ----------
        /// <summary>统一圆角半径（像素，96 DPI 基准，随 DPI 缩放由调用方处理）</summary>
        public const int CornerRadius = 12;

        // ---------- 字体 ----------
        private const string CnFont = "微软雅黑";
        private const string NumFont = "Segoe UI Semibold";

        /// <summary>常规字体（中文）</summary>
        public static Font Regular(float size)
        {
            return new Font(CnFont, size, FontStyle.Regular, GraphicsUnit.Point, 134);
        }

        /// <summary>加粗字体（中文，标题）</summary>
        public static Font Bold(float size)
        {
            return new Font(CnFont, size, FontStyle.Bold, GraphicsUnit.Point, 134);
        }

        /// <summary>数字字体（西文 Semibold，用于大号倒计时显示）</summary>
        public static Font Number(float size)
        {
            return new Font(NumFont, size, FontStyle.Bold, GraphicsUnit.Point, 0);
        }

        /// <summary>
        /// 创建圆角矩形 Region。按当前设备像素尺寸计算，应在 Resize / DpiChanged 时重算。
        /// </summary>
        public static Region CreateRoundedRegion(int width, int height, int radius)
        {
            if (width <= 0 || height <= 0)
            {
                return new Region(new Rectangle(0, 0, Math.Max(width, 0), Math.Max(height, 0)));
            }

            int r = Math.Min(radius, Math.Min(width, height) / 2);
            if (r <= 0)
            {
                return new Region(new Rectangle(0, 0, width, height));
            }

            using (var path = new GraphicsPath())
            {
                path.AddArc(0, 0, r * 2, r * 2, 180, 90);
                path.AddArc(width - r * 2, 0, r * 2, r * 2, 270, 90);
                path.AddArc(width - r * 2, height - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(0, height - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                return new Region(path);
            }
        }

        // ---------- 系统深色检测（跟随系统模式使用） ----------
        private static bool SystemUsesDarkTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("AppsUseLightTheme");
                        if (val != null && (int)val == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // 读取失败则按浅色处理
            }

            return false;
        }

        // ---------- 持久化 ----------
        private static void SaveMode(ThemeMode mode)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string v = mode.ToString();
                if (config.AppSettings.Settings["ThemeMode"] != null)
                {
                    config.AppSettings.Settings["ThemeMode"].Value = v;
                }
                else
                {
                    config.AppSettings.Settings.Add("ThemeMode", v);
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch
            {
                // 无写入权限时静默忽略
            }
        }

        /// <summary>从 app.config 读取已保存的主题模式（无配置则默认浅色）</summary>
        public static void LoadMode()
        {
            try
            {
                var v = ConfigurationManager.AppSettings["ThemeMode"];
                if (!string.IsNullOrEmpty(v) && Enum.TryParse<ThemeMode>(v, out var m))
                {
                    _mode = m;
                }
            }
            catch
            {
                // 读取失败保持默认
            }
        }

        // ---------- 订阅系统主题变化（仅 Auto 模式触发重绘） ----------
        static ThemeManager()
        {
            try
            {
                SystemEvents.UserPreferenceChanged += (s, e) =>
                {
                    if (_mode == ThemeMode.Auto && e.Category == UserPreferenceCategory.General)
                    {
                        ThemeChanged?.Invoke(null, EventArgs.Empty);
                    }
                };
            }
            catch
            {
                // 某些环境下 SystemEvents 不可用，忽略
            }
        }
    }
}
