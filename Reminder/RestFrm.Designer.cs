namespace Reminder
{
    partial class RestFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                countdownTimer?.Stop();
                countdownTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblText = new System.Windows.Forms.Label();
            this.pnlCenter = new System.Windows.Forms.Panel();
            this.tableLayoutPanelCenter = new System.Windows.Forms.TableLayoutPanel();
            this.pnlIcon = new System.Windows.Forms.Panel();
            this.lblIcon = new System.Windows.Forms.Label();
            this.pnlTimer = new System.Windows.Forms.Panel();
            this.tableLayoutPanelTimer = new System.Windows.Forms.TableLayoutPanel();
            this.lblTimerTitle = new System.Windows.Forms.Label();
            this.pnlTimeDisplay = new System.Windows.Forms.Panel();
            this.lbl_minutes = new System.Windows.Forms.Label();
            this.labelColon = new System.Windows.Forms.Label();
            this.lbl_seconds = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblHint = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.pnlProgress = new System.Windows.Forms.Panel();
            this.tableLayoutPanelMain.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.pnlCenter.SuspendLayout();
            this.tableLayoutPanelCenter.SuspendLayout();
            this.pnlIcon.SuspendLayout();
            this.pnlTimer.SuspendLayout();
            this.tableLayoutPanelTimer.SuspendLayout();
            this.pnlTimeDisplay.SuspendLayout();
            this.pnlProgress.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.pnlTop, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.pnlCenter, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.pnlProgress, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.pnlBottom, 0, 3);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 4;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(800, 600);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.lblText);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(800, 120);
            this.pnlTop.TabIndex = 0;
            // 
            // lblText
            // 
            this.lblText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblText.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblText.ForeColor = System.Drawing.Color.White;
            this.lblText.Location = new System.Drawing.Point(0, 0);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(800, 120);
            this.lblText.TabIndex = 0;
            this.lblText.Text = "久坐对身体不好，站起来活动下吧！";
            this.lblText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlCenter
            // 
            this.pnlCenter.Controls.Add(this.tableLayoutPanelCenter);
            this.pnlCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCenter.Location = new System.Drawing.Point(0, 120);
            this.pnlCenter.Name = "pnlCenter";
            this.pnlCenter.Size = new System.Drawing.Size(800, 360);
            this.pnlCenter.TabIndex = 1;
            // 
            // tableLayoutPanelCenter
            // 
            this.tableLayoutPanelCenter.ColumnCount = 2;
            this.tableLayoutPanelCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelCenter.Controls.Add(this.pnlIcon, 0, 0);
            this.tableLayoutPanelCenter.Controls.Add(this.pnlTimer, 1, 0);
            this.tableLayoutPanelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelCenter.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelCenter.Name = "tableLayoutPanelCenter";
            this.tableLayoutPanelCenter.RowCount = 1;
            this.tableLayoutPanelCenter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelCenter.Size = new System.Drawing.Size(800, 360);
            this.tableLayoutPanelCenter.TabIndex = 0;
            // 
            // pnlIcon
            // 
            this.pnlIcon.Controls.Add(this.lblIcon);
            this.pnlIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlIcon.Location = new System.Drawing.Point(0, 0);
            this.pnlIcon.Name = "pnlIcon";
            this.pnlIcon.Size = new System.Drawing.Size(397, 360);
            this.pnlIcon.TabIndex = 0;
            // 
            // lblIcon
            // 
            this.lblIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIcon.Font = new System.Drawing.Font("微软雅黑", 80F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblIcon.ForeColor = System.Drawing.Color.White;
            this.lblIcon.Location = new System.Drawing.Point(0, 0);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(397, 360);
            this.lblIcon.TabIndex = 0;
            this.lblIcon.Text = "🏃";
            this.lblIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlTimer
            // 
            this.pnlTimer.Controls.Add(this.tableLayoutPanelTimer);
            this.pnlTimer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTimer.Location = new System.Drawing.Point(397, 0);
            this.pnlTimer.Name = "pnlTimer";
            this.pnlTimer.Size = new System.Drawing.Size(403, 360);
            this.pnlTimer.TabIndex = 1;
            // 
            // tableLayoutPanelTimer
            // 
            this.tableLayoutPanelTimer.ColumnCount = 1;
            this.tableLayoutPanelTimer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTimer.Controls.Add(this.lblTimerTitle, 0, 0);
            this.tableLayoutPanelTimer.Controls.Add(this.pnlTimeDisplay, 0, 1);
            this.tableLayoutPanelTimer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelTimer.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelTimer.Name = "tableLayoutPanelTimer";
            this.tableLayoutPanelTimer.RowCount = 2;
            this.tableLayoutPanelTimer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanelTimer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanelTimer.Size = new System.Drawing.Size(403, 360);
            this.tableLayoutPanelTimer.TabIndex = 0;
            // 
            // lblTimerTitle
            // 
            this.lblTimerTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTimerTitle.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTimerTitle.ForeColor = System.Drawing.Color.White;
            this.lblTimerTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTimerTitle.Name = "lblTimerTitle";
            this.lblTimerTitle.Size = new System.Drawing.Size(397, 108);
            this.lblTimerTitle.TabIndex = 0;
            this.lblTimerTitle.Text = "🔒 解锁倒计时";
            this.lblTimerTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlTimeDisplay
            // 
            this.pnlTimeDisplay.Controls.Add(this.lbl_minutes);
            this.pnlTimeDisplay.Controls.Add(this.labelColon);
            this.pnlTimeDisplay.Controls.Add(this.lbl_seconds);
            this.pnlTimeDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTimeDisplay.Location = new System.Drawing.Point(3, 111);
            this.pnlTimeDisplay.Name = "pnlTimeDisplay";
            this.pnlTimeDisplay.Size = new System.Drawing.Size(397, 100);
            this.pnlTimeDisplay.TabIndex = 1;
            // 
            // lbl_minutes
            // 
            this.lbl_minutes.Font = new System.Drawing.Font("Segoe UI Semibold", 44F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_minutes.ForeColor = System.Drawing.Color.White;
            this.lbl_minutes.Location = new System.Drawing.Point(10, 10);
            this.lbl_minutes.Name = "lbl_minutes";
            this.lbl_minutes.Size = new System.Drawing.Size(160, 80);
            this.lbl_minutes.TabIndex = 0;
            this.lbl_minutes.Text = "00";
            this.lbl_minutes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelColon
            // 
            this.labelColon.Font = new System.Drawing.Font("Segoe UI Semibold", 44F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelColon.ForeColor = System.Drawing.Color.White;
            this.labelColon.Location = new System.Drawing.Point(175, 10);
            this.labelColon.Name = "labelColon";
            this.labelColon.Size = new System.Drawing.Size(40, 80);
            this.labelColon.TabIndex = 1;
            this.labelColon.Text = ":";
            this.labelColon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_seconds
            // 
            this.lbl_seconds.Font = new System.Drawing.Font("Segoe UI Semibold", 44F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_seconds.ForeColor = System.Drawing.Color.White;
            this.lbl_seconds.Location = new System.Drawing.Point(215, 10);
            this.lbl_seconds.Name = "lbl_seconds";
            this.lbl_seconds.Size = new System.Drawing.Size(160, 80);
            this.lbl_seconds.TabIndex = 2;
            this.lbl_seconds.Text = "00";
            this.lbl_seconds.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlProgress
            // 
            this.pnlProgress.Controls.Add(this.progressBar);
            this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlProgress.Location = new System.Drawing.Point(100, 448);
            this.pnlProgress.Name = "pnlProgress";
            this.pnlProgress.Padding = new System.Windows.Forms.Padding(80, 8, 80, 8);
            this.pnlProgress.Size = new System.Drawing.Size(600, 30);
            this.pnlProgress.TabIndex = 3;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(80, 8);
            this.progressBar.Maximum = 100;
            this.progressBar.Minimum = 0;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(440, 14);
            this.progressBar.Step = 1;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 0;
            this.progressBar.Value = 100;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.lblHint);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottom.Location = new System.Drawing.Point(0, 478);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(800, 120);
            this.pnlBottom.TabIndex = 2;
            // 
            // lblHint
            // 
            this.lblHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHint.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblHint.ForeColor = System.Drawing.Color.White;
            this.lblHint.Location = new System.Drawing.Point(0, 0);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(800, 120);
            this.lblHint.TabIndex = 0;
            this.lblHint.Text = "💧 喝口水  🚶 走动一下  👀 远眺放松";
            this.lblHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RestFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RestFrm";
            this.Opacity = 0.92D;
            this.ShowInTaskbar = false;
            this.Text = "RestFrm";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.RestFrm_Load);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.pnlProgress.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlCenter.ResumeLayout(false);
            this.tableLayoutPanelCenter.ResumeLayout(false);
            this.pnlIcon.ResumeLayout(false);
            this.pnlTimer.ResumeLayout(false);
            this.tableLayoutPanelTimer.ResumeLayout(false);
            this.pnlTimeDisplay.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.Panel pnlCenter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCenter;
        private System.Windows.Forms.Panel pnlIcon;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.Panel pnlTimer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTimer;
        private System.Windows.Forms.Label lblTimerTitle;
        private System.Windows.Forms.Panel pnlTimeDisplay;
        private System.Windows.Forms.Label lbl_minutes;
        private System.Windows.Forms.Label labelColon;
        private System.Windows.Forms.Label lbl_seconds;
        private System.Windows.Forms.Panel pnlProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblHint;
    }
}