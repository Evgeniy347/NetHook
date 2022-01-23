
namespace NetHook.UI
{
    partial class OpenProcess
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
            this.button_cancell = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.dataGridView_ProcessList = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_Update = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBox_searchValue = new System.Windows.Forms.ToolStripTextBox();
            this.pictureBox_Load_Processing = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ProcessList)).BeginInit();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Load_Processing)).BeginInit();
            this.SuspendLayout();
            // 
            // button_cancell
            // 
            this.button_cancell.Location = new System.Drawing.Point(319, 425);
            this.button_cancell.Name = "button_cancell";
            this.button_cancell.Size = new System.Drawing.Size(75, 23);
            this.button_cancell.TabIndex = 1;
            this.button_cancell.Text = "Cancell";
            this.button_cancell.UseVisualStyleBackColor = true;
            this.button_cancell.Click += new System.EventHandler(this.button_cancell_Click);
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(238, 425);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // dataGridView_ProcessList
            // 
            this.dataGridView_ProcessList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_ProcessList.Location = new System.Drawing.Point(12, 33);
            this.dataGridView_ProcessList.Name = "dataGridView_ProcessList";
            this.dataGridView_ProcessList.Size = new System.Drawing.Size(382, 386);
            this.dataGridView_ProcessList.TabIndex = 3;
            this.dataGridView_ProcessList.SelectionChanged += new System.EventHandler(this.dataGridView_ProcessList_SelectionChanged);
            this.dataGridView_ProcessList.DoubleClick += new System.EventHandler(this.dataGridView_ProcessList_DoubleClick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_Update,
            this.toolStripButton1,
            this.toolStripTextBox_searchValue});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(401, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_Update
            // 
            this.toolStripButton_Update.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Update.Image = global::NetHook.UI.Properties.Resources.arrow_clockwise;
            this.toolStripButton_Update.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Update.Name = "toolStripButton_Update";
            this.toolStripButton_Update.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Update.Text = "toolStripButton_Update";
            this.toolStripButton_Update.Click += new System.EventHandler(this.toolStripButton_Update_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::NetHook.UI.Properties.Resources.search;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripOnSearch_Click);
            // 
            // toolStripTextBox_searchValue
            // 
            this.toolStripTextBox_searchValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolStripTextBox_searchValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox_searchValue.Name = "toolStripTextBox_searchValue";
            this.toolStripTextBox_searchValue.Size = new System.Drawing.Size(300, 25);
            this.toolStripTextBox_searchValue.TextChanged += new System.EventHandler(this.toolStripTextBox_searchValue_TextChanged);
            // 
            // pictureBox_Load_Processing
            // 
            this.pictureBox_Load_Processing.Image = global::NetHook.UI.Properties.Resources.spinner;
            this.pictureBox_Load_Processing.Location = new System.Drawing.Point(369, 0);
            this.pictureBox_Load_Processing.Name = "pictureBox_Load_Processing";
            this.pictureBox_Load_Processing.Size = new System.Drawing.Size(25, 25);
            this.pictureBox_Load_Processing.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Load_Processing.TabIndex = 7;
            this.pictureBox_Load_Processing.TabStop = false;
            this.pictureBox_Load_Processing.Visible = false;
            // 
            // OpenProcess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 450);
            this.Controls.Add(this.pictureBox_Load_Processing);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.dataGridView_ProcessList);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_cancell);
            this.Name = "OpenProcess";
            this.Text = "OpenProcess";
            this.SizeChanged += new System.EventHandler(this.OpenProcess_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ProcessList)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Load_Processing)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button_cancell;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.DataGridView dataGridView_ProcessList;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_searchValue;
        private System.Windows.Forms.ToolStripButton toolStripButton_Update;
        private System.Windows.Forms.PictureBox pictureBox_Load_Processing;
    }
}