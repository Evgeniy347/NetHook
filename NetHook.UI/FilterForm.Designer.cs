
namespace NetHook.UI
{
    partial class FilterForm
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
            this.treeView_assemblies = new System.Windows.Forms.TreeView();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_Ok = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_Update = new System.Windows.Forms.ToolStripButton();
            this.toolStripOnSearch = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBox_searchValue = new System.Windows.Forms.ToolStripTextBox();
            this.pictureBox_Load_Processing = new System.Windows.Forms.PictureBox();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Load_Processing)).BeginInit();
            this.SuspendLayout();
            // 
            // treeView_assemblies
            // 
            this.treeView_assemblies.CheckBoxes = true;
            this.treeView_assemblies.Location = new System.Drawing.Point(12, 28);
            this.treeView_assemblies.Name = "treeView_assemblies";
            this.treeView_assemblies.Size = new System.Drawing.Size(776, 381);
            this.treeView_assemblies.TabIndex = 0;
            this.treeView_assemblies.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView_assemblies_AfterCheck);
            this.treeView_assemblies.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_assemblies_BeforeCollapse);
            this.treeView_assemblies.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView_assemblies_AfterExpand);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(713, 415);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "Отмена";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_Ok
            // 
            this.button_Ok.Location = new System.Drawing.Point(632, 415);
            this.button_Ok.Name = "button_Ok";
            this.button_Ok.Size = new System.Drawing.Size(75, 23);
            this.button_Ok.TabIndex = 2;
            this.button_Ok.Text = "ОК";
            this.button_Ok.UseVisualStyleBackColor = true;
            this.button_Ok.Click += new System.EventHandler(this.button_Ok_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_Update,
            this.toolStripOnSearch,
            this.toolStripTextBox_searchValue});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_Update
            // 
            this.toolStripButton_Update.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Update.Image = global::NetHook.UI.Properties.Resources.arrow_clockwise;
            this.toolStripButton_Update.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Update.Name = "toolStripButton_Update";
            this.toolStripButton_Update.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Update.Text = "Update";
            this.toolStripButton_Update.Click += new System.EventHandler(this.toolStripButton_Update_Click);
            // 
            // toolStripOnSearch
            // 
            this.toolStripOnSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripOnSearch.Image = global::NetHook.UI.Properties.Resources.search;
            this.toolStripOnSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripOnSearch.Name = "toolStripOnSearch";
            this.toolStripOnSearch.Size = new System.Drawing.Size(23, 22);
            this.toolStripOnSearch.Text = "Search";
            this.toolStripOnSearch.Click += new System.EventHandler(this.toolStripOnSearch_Click);
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
            this.pictureBox_Load_Processing.Location = new System.Drawing.Point(763, 0);
            this.pictureBox_Load_Processing.Name = "pictureBox_Load_Processing";
            this.pictureBox_Load_Processing.Size = new System.Drawing.Size(25, 25);
            this.pictureBox_Load_Processing.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Load_Processing.TabIndex = 6;
            this.pictureBox_Load_Processing.TabStop = false;
            this.pictureBox_Load_Processing.Visible = false;
            // 
            // FilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pictureBox_Load_Processing);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.button_Ok);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.treeView_assemblies);
            this.Name = "FilterForm";
            this.Text = "FilterForm";
            this.SizeChanged += new System.EventHandler(this.FilterForm_SizeChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Load_Processing)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView_assemblies;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_Ok;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripOnSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_searchValue;
        private System.Windows.Forms.ToolStripButton toolStripButton_Update;
        private System.Windows.Forms.PictureBox pictureBox_Load_Processing;
    }
}