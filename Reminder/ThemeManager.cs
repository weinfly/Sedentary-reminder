using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Reminder
{
    /// <summary>
    /// 集中管理 UI 主题：颜色、字体、圆角半径，以及通用绘制辅助。
    /// 所有窗体应从此处取色/取字体，避免硬编码导致主题不一致。
    /// </summary>
    public static class ThemeManager
    {
        // ---------- 颜色 ----------
        /// <summary>主色：绿色（标题栏、主按钮、强调）</summary>
        public static readonly Color Primary = Color.FromArgb(76, 175, 80);

        /// <summary>主色按下/hover 态（深一档绿），用于按钮交互态，而非独立主题色</summary>
        public static readonly Color PrimaryDark = Color.FromArgb(39, 174, 96);

        /// <summary>主色上的文字色（白色）</summary>
        public static readonly Color OnPrimary = Color.White;

        /// <summary>警告/提醒态（柔和红，倒计时最后阶段）</summary>
        public static readonly Color Warning = Color.FromArgb(231, 76, 60);

        /// <summary>主文本色</summary>
        public static readonly Color TextPrimary = Color.FromArgb(65, 65, 65);

        /// <summary>次要文本色</summary>
        public static readonly Color TextSecondary = Color.FromArgb(117, 117, 117);

        /// <summary>表面色（卡片/面板白）</summary>
        public static readonly Color Surface = Color.White;

        /// <summary>背景色（窗体底色，浅灰）</summary>
        public static readonly Color Background = Color.FromArgb(248, 249, 250);

        /// <summary>面板浅色强调文字（on primary 的浅绿白）</summary>
        public static readonly Color PrimaryLight = Color.FromArgb(220, 235, 250);

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
    }
}
