
namespace NetHook.UI
{
    partial class MainForm
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_OpenProcess = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Tool = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Filter = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Play = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Pause = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Clear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Search = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel_OpenProcess = new System.Windows.Forms.ToolStripLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.dataGridView_Threads = new System.Windows.Forms.DataGridView();
            this.ThreadID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StartTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EndTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Elapsed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HttpUserLogin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.URL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Threads)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_OpenProcess,
            this.toolStripButton_Tool,
            this.toolStripSeparator1,
            this.toolStripButton_Filter,
            this.toolStripButton_Refresh,
            this.toolStripButton_Play,
            this.toolStripButton_Pause,
            this.toolStripButton_Clear,
            this.toolStripSeparator2,
            this.toolStripButton_Search,
            this.toolStripTextBox1,
            this.toolStripSeparator3,
            this.toolStripLabel_OpenProcess,
            this.toolStripProgressBar1,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_OpenProcess
            // 
            this.toolStripButton_OpenProcess.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_OpenProcess.Image = global::NetHook.UI.Properties.Resources.window;
            this.toolStripButton_OpenProcess.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_OpenProcess.Name = "toolStripButton_OpenProcess";
            this.toolStripButton_OpenProcess.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_OpenProcess.Text = "Open Process";
            this.toolStripButton_OpenProcess.Click += new System.EventHandler(this.toolStripButton_OpenProcess_Click);
            // 
            // toolStripButton_Tool
            // 
            this.toolStripButton_Tool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Tool.Image = global::NetHook.UI.Properties.Resources.tool;
            this.toolStripButton_Tool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Tool.Name = "toolStripButton_Tool";
            this.toolStripButton_Tool.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Tool.Text = "Tool";
            this.toolStripButton_Tool.Click += new System.EventHandler(this.toolStripButton_Tool_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_Filter
            // 
            this.toolStripButton_Filter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Filter.Image = global::NetHook.UI.Properties.Resources.filter;
            this.toolStripButton_Filter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Filter.Name = "toolStripButton_Filter";
            this.toolStripButton_Filter.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Filter.Text = "Filter";
            this.toolStripButton_Filter.Click += new System.EventHandler(this.toolStripButton_Filter_Click);
            // 
            // toolStripButton_Refresh
            // 
            this.toolStripButton_Refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Refresh.Image = global::NetHook.UI.Properties.Resources.arrow_clockwise;
            this.toolStripButton_Refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Refresh.Name = "toolStripButton_Refresh";
            this.toolStripButton_Refresh.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Refresh.Text = "toolStripButton_Refresh";
            this.toolStripButton_Refresh.ToolTipText = "Refresh";
            // 
            // toolStripButton_Play
            // 
            this.toolStripButton_Play.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Play.Image = global::NetHook.UI.Properties.Resources.play_fill;
            this.toolStripButton_Play.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Play.Name = "toolStripButton_Play";
            this.toolStripButton_Play.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Play.Text = "toolStripButton_Play";
            this.toolStripButton_Play.ToolTipText = "Play";
            this.toolStripButton_Play.Click += new System.EventHandler(this.toolStripButton_Play_Click);
            // 
            // toolStripButton_Pause
            // 
            this.toolStripButton_Pause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Pause.Image = global::NetHook.UI.Properties.Resources.pause_fill;
            this.toolStripButton_Pause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Pause.Name = "toolStripButton_Pause";
            this.toolStripButton_Pause.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Pause.Text = "toolStripButton_Pause";
            this.toolStripButton_Pause.ToolTipText = "Pause";
            // 
            // toolStripButton_Clear
            // 
            this.toolStripButton_Clear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Clear.Image = global::NetHook.UI.Properties.Resources.delete;
            this.toolStripButton_Clear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Clear.Name = "toolStripButton_Clear";
            this.toolStripButton_Clear.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Clear.Text = "toolStripButton_Clear";
            this.toolStripButton_Clear.ToolTipText = "Clear";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_Search
            // 
            this.toolStripButton_Search.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Search.Image = global::NetHook.UI.Properties.Resources.search;
            this.toolStripButton_Search.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Search.Name = "toolStripButton_Search";
            this.toolStripButton_Search.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Search.Text = "toolStripButton_Search";
            this.toolStripButton_Search.ToolTipText = "Search";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(100, 25);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel_OpenProcess
            // 
            this.toolStripLabel_OpenProcess.Name = "toolStripLabel_OpenProcess";
            this.toolStripLabel_OpenProcess.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 22);
            // 
            // dataGridView_Threads
            // 
            this.dataGridView_Threads.AllowUserToAddRows = false;
            this.dataGridView_Threads.AllowUserToDeleteRows = false;
            this.dataGridView_Threads.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Threads.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ThreadID,
            this.Status,
            this.StartTime,
            this.EndTime,
            this.Elapsed,
            this.HttpUserLogin,
            this.URL});
            this.dataGridView_Threads.Location = new System.Drawing.Point(12, 28);
            this.dataGridView_Threads.Name = "dataGridView_Threads";
            this.dataGridView_Threads.ReadOnly = true;
            this.dataGridView_Threads.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_Threads.Size = new System.Drawing.Size(776, 403);
            this.dataGridView_Threads.TabIndex = 4;
            // 
            // ThreadID
            // 
            this.ThreadID.HeaderText = "ThreadID";
            this.ThreadID.Name = "ThreadID";
            this.ThreadID.ReadOnly = true;
            // 
            // Status
            // 
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            // 
            // StartTime
            // 
            this.StartTime.HeaderText = "StartTime";
            this.StartTime.Name = "StartTime";
            this.StartTime.ReadOnly = true;
            // 
            // EndTime
            // 
            this.EndTime.HeaderText = "EndTime";
            this.EndTime.Name = "EndTime";
            this.EndTime.ReadOnly = true;
            // 
            // Elapsed
            // 
            this.Elapsed.HeaderText = "Elapsed";
            this.Elapsed.Name = "Elapsed";
            this.Elapsed.ReadOnly = true;
            // 
            // HttpUserLogin
            // 
            this.HttpUserLogin.HeaderText = "HttpUserLogin";
            this.HttpUserLogin.Name = "HttpUserLogin";
            this.HttpUserLogin.ReadOnly = true;
            // 
            // URL
            // 
            this.URL.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.URL.HeaderText = "URL";
            this.URL.Name = "URL";
            this.URL.ReadOnly = true;
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 443);
            this.Controls.Add(this.dataGridView_Threads);
            this.Controls.Add(this.toolStrip1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Threads)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_OpenProcess;
        private System.Windows.Forms.ToolStripButton toolStripButton_Tool;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Refresh;
        private System.Windows.Forms.ToolStripButton toolStripButton_Play;
        private System.Windows.Forms.ToolStripButton toolStripButton_Pause;
        private System.Windows.Forms.ToolStripButton toolStripButton_Clear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Search;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_OpenProcess;
        private System.Windows.Forms.ToolStripButton toolStripButton_Filter;
        private System.Windows.Forms.DataGridView dataGridView_Threads;
        private System.Windows.Forms.DataGridViewTextBoxColumn ThreadID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn StartTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn EndTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Elapsed;
        private System.Windows.Forms.DataGridViewTextBoxColumn HttpUserLogin;
        private System.Windows.Forms.DataGridViewTextBoxColumn URL;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
    }
}

