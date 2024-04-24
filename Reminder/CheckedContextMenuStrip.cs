using System;
using System.Drawing;
using System.Windows.Forms;

public class CheckedContextMenuStrip : ContextMenuStrip
{
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        foreach (ToolStripMenuItem item in this.Items)
        {
            if (item.Checked)
            {
                item.ForeColor = SystemColors.HighlightText;
            }
            else
            {
                item.ForeColor = SystemColors.MenuText;
            }
        }
    }
}