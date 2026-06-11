namespace Reminder
{
    partial class WorkFrm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timerWrk = new System.Windows.Forms.Timer(this.components);
            this.pnlMain = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblWarn = new System.Windows.Forms.Label();
            this.pnlCenter = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblMin = new System.Windows.Forms.Label();
            this.labelColon = new System.Windows.Forms.Label();
            this.lblSecond = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnDelay = new System.Windows.Forms.Button();
            this.lblProgress = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.pnlCenter.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerWrk
            // 
            this.timerWrk.Enabled = true;
            this.timerWrk.Interval = 1000;
            this.timerWrk.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.Controls.Add(this.tableLayoutPanel1);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(200, 140);
            this.pnlMain.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pnlTop, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnlCenter, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.pnlBottom, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(200, 140);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.lblWarn);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTop.Location = new System.Drawing.Point(3, 3);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(194, 28);
            this.pnlTop.TabIndex = 0;
            // 
            // lblWarn
            // 
            this.lblWarn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWarn.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblWarn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(144)))), ((int)(((byte)(217)))));
            this.lblWarn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblWarn.Location = new System.Drawing.Point(0, 0);
            this.lblWarn.Name = "lblWarn";
            this.lblWarn.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.lblWarn.Size = new System.Drawing.Size(194, 28);
            this.lblWarn.TabIndex = 0;
            this.lblWarn.Text = " ⏰ 注意坐姿";
            this.lblWarn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblWarn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseDown);
            this.lblWarn.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseMove);
            this.lblWarn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseUp);
            // 
            // pnlCenter
            // 
            this.pnlCenter.Controls.Add(this.tableLayoutPanel2);
            this.pnlCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCenter.Location = new System.Drawing.Point(3, 37);
            this.pnlCenter.Name = "pnlCenter";
            this.pnlCenter.Size = new System.Drawing.Size(194, 65);
            this.pnlCenter.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.Controls.Add(this.lblMin, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelColon, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblSecond, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(194, 65);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // lblMin
            // 
            this.lblMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMin.Font = new System.Drawing.Font("Segoe UI Semibold", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.lblMin.Location = new System.Drawing.Point(3, 0);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(74, 65);
            this.lblMin.TabIndex = 0;
            this.lblMin.Text = "00";
            this.lblMin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblMin.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseDown);
            this.lblMin.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseMove);
            this.lblMin.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseUp);
            // 
            // labelColon
            // 
            this.labelColon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelColon.Font = new System.Drawing.Font("Segoe UI Semibold", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelColon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.labelColon.Location = new System.Drawing.Point(77, 0);
            this.labelColon.Name = "labelColon";
            this.labelColon.Size = new System.Drawing.Size(35, 65);
            this.labelColon.TabIndex = 1;
            this.labelColon.Text = ":";
            this.labelColon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelColon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseDown);
            this.labelColon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseMove);
            this.labelColon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseUp);
            // 
            // lblSecond
            // 
            this.lblSecond.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSecond.Font = new System.Drawing.Font("Segoe UI Semibold", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSecond.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.lblSecond.Location = new System.Drawing.Point(115, 0);
            this.lblSecond.Name = "lblSecond";
            this.lblSecond.Size = new System.Drawing.Size(76, 65);
            this.lblSecond.TabIndex = 2;
            this.lblSecond.Text = "00";
            this.lblSecond.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSecond.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseDown);
            this.lblSecond.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseMove);
            this.lblSecond.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseUp);
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnPause);
            this.pnlBottom.Controls.Add(this.btnDelay);
            this.pnlBottom.Controls.Add(this.lblProgress);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottom.Location = new System.Drawing.Point(3, 108);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(194, 29);
            this.pnlBottom.TabIndex = 2;
            // 
            // btnPause
            // 
            this.btnPause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPause.BackColor = System.Drawing.Color.White;
            this.btnPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPause.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(144)))), ((int)(((byte)(217)))));
            this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPause.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnPause.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(144)))), ((int)(((byte)(217)))));
            this.btnPause.Location = new System.Drawing.Point(95, 3);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(48, 23);
            this.btnPause.TabIndex = 1;
            this.btnPause.Text = "暂停";
            this.btnPause.UseVisualStyleBackColor = false;
            this.btnPause.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // btnDelay
            // 
            this.btnDelay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelay.BackColor = System.Drawing.Color.White;
            this.btnDelay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelay.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(144)))), ((int)(((byte)(217)))));
            this.btnDelay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelay.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDelay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(144)))), ((int)(((byte)(217)))));
            this.btnDelay.Location = new System.Drawing.Point(149, 3);
            this.btnDelay.Name = "btnDelay";
            this.btnDelay.Size = new System.Drawing.Size(42, 23);
            this.btnDelay.TabIndex = 0;
            this.btnDelay.Text = "+10分";
            this.btnDelay.UseVisualStyleBackColor = false;
            this.btnDelay.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgress.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(110)))), ((int)(((byte)(114)))));
            this.lblProgress.Location = new System.Drawing.Point(3, 3);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(89, 23);
            this.lblProgress.TabIndex = 2;
            this.lblProgress.Text = "工作中...";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // WorkFrm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(200, 140);
            this.Controls.Add(this.pnlMain);
            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkFrm";
            this.Opacity = 0.92D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "WorkFrm";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFrm_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorkFrm_MouseUp);
            this.pnlMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlCenter.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timerWrk;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblWarn;
        private System.Windows.Forms.Panel pnlCenter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lblMin;
        private System.Windows.Forms.Label labelColon;
        private System.Windows.Forms.Label lblSecond;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnDelay;
        private System.Windows.Forms.Label lblProgress;
    }
}