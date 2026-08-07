using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Reminder
{
    /// <summary>
    /// 使用统计面板：展示今日与累计的专注/休息次数与时长。
    /// 控件在代码中构建，主题取色统一来自 ThemeManager。
    /// </summary>
    public class StatsFrm : Form
    {
        private static readonly string[] MetricNames = { "专注次数", "专注时长", "休息次数", "休息时长" };

        private readonly Label[] _todayVal = new Label[4];
        private readonly Label[] _totalVal = new Label[4];
        private Panel _pnlHeader;
        private Panel _pnlBody;
        private Panel _pnlToday;
        private Panel _pnlTotal;
        private Button _btnClose;

        public StatsFrm()
        {
            BuildUI();
            RefreshValues();
            ApplyTheme();
        }

        private void BuildUI()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(380, 480);
            this.Text = "使用统计";

            _pnlHeader = new Panel { Dock = DockStyle.Top, Height = 54 };
            _pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15) };

            var lblTitle = new Label
            {
                Text = "📊 使用统计",
                Dock = DockStyle.Fill,
                Font = ThemeManager.Bold(14),
                ForeColor = ThemeManager.OnPrimary,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _pnlHeader.Controls.Add(lblTitle);

            _pnlToday = MakeCard("今日", _todayVal);
            _pnlToday.Dock = DockStyle.Top;
            _pnlTotal = MakeCard("累计", _totalVal);
            _pnlTotal.Dock = DockStyle.Top;

            var spacer = new Panel { Dock = DockStyle.Top, Height = 12 };

            _pnlBody.Controls.Add(_pnlTotal);
            _pnlBody.Controls.Add(spacer);
            _pnlBody.Controls.Add(_pnlToday);

            _btnClose = new Button
            {
                Dock = DockStyle.Fill,
                Text = "关闭",
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            _btnClose.Click += (s, e) => this.Close();
            pnlFooter.Controls.Add(_btnClose);

            this.Controls.Add(_pnlBody);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(_pnlHeader);
        }

        private Panel MakeCard(string title, Label[] vals)
        {
            var card = new Panel { Height = 168 };

            var titleLbl = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 26,
                Font = ThemeManager.Bold(11),
                Padding = new Padding(4, 0, 0, 0)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(4)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            for (int i = 0; i < 4; i++)
            {
                var cell = new Panel { Margin = new Padding(4) };
                var nameLbl = new Label
                {
                    Text = MetricNames[i],
                    Dock = DockStyle.Top,
                    Height = 20,
                    Font = ThemeManager.Regular(9),
                    Padding = new Padding(2, 0, 0, 0)
                };
                var valLbl = new Label
                {
                    Text = "-",
                    Dock = DockStyle.Fill,
                    Font = ThemeManager.Bold(16),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(2, 0, 0, 0),
                    Tag = "val"
                };
                cell.Controls.Add(valLbl);
                cell.Controls.Add(nameLbl);
                vals[i] = valLbl;
                grid.Controls.Add(cell, i % 2, i / 2);
            }

            card.Controls.Add(grid);
            card.Controls.Add(titleLbl);
            return card;
        }

        private void RefreshValues()
        {
            var t = StatsTracker.GetToday();
            _todayVal[0].Text = t.FocusSessions.ToString();
            _todayVal[1].Text = FormatMinutes(t.FocusMinutes);
            _todayVal[2].Text = t.RestSessions.ToString();
            _todayVal[3].Text = FormatMinutes(t.RestMinutes);

            var a = StatsTracker.GetTotal();
            _totalVal[0].Text = a.FocusSessions.ToString();
            _totalVal[1].Text = FormatMinutes(a.FocusMinutes);
            _totalVal[2].Text = a.RestSessions.ToString();
            _totalVal[3].Text = FormatMinutes(a.RestMinutes);
        }

        private static string FormatMinutes(int m)
        {
            if (m < 60) return m + " 分钟";
            int h = m / 60;
            int mm = m % 60;
            return mm == 0 ? $"{h} 小时" : $"{h} 小时 {mm} 分";
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            _pnlHeader.BackColor = ThemeManager.Primary;
            _pnlBody.BackColor = ThemeManager.Background;

            _pnlToday.BackColor = ThemeManager.Surface;
            _pnlTotal.BackColor = ThemeManager.Surface;

            foreach (var card in new[] { _pnlToday, _pnlTotal })
            {
                var title = card.Controls.OfType<Label>().FirstOrDefault();
                if (title != null) title.ForeColor = ThemeManager.TextPrimary;

                foreach (var grid in card.Controls.OfType<TableLayoutPanel>())
                {
                    foreach (var cell in grid.Controls.OfType<Panel>())
                    {
                        foreach (var lbl in cell.Controls.OfType<Label>())
                        {
                            lbl.ForeColor = (string)lbl.Tag == "val"
                                ? ThemeManager.TextPrimary
                                : ThemeManager.TextSecondary;
                        }
                    }
                }
            }

            _btnClose.BackColor = ThemeManager.Primary;
            _btnClose.ForeColor = ThemeManager.OnPrimary;
            _btnClose.Font = ThemeManager.Bold(12);
            _btnClose.FlatAppearance.MouseOverBackColor = ThemeManager.PrimaryDark;
            _btnClose.FlatAppearance.MouseDownBackColor = ThemeManager.PrimaryDark;
        }
    }
}
