
namespace NetHook.UI
{
    partial class LogConsole
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
            this.richTextBox_logConsole = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBox_logConsole
            // 
            this.richTextBox_logConsole.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBox_logConsole.Location = new System.Drawing.Point(12, 12);
            this.richTextBox_logConsole.Name = "richTextBox_logConsole";
            this.richTextBox_logConsole.ReadOnly = true;
            this.richTextBox_logConsole.Size = new System.Drawing.Size(776, 426);
            this.richTextBox_logConsole.TabIndex = 0;
            this.richTextBox_logConsole.Text = "";
            // 
            // LogConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.richTextBox_logConsole);
            this.Name = "LogConsole";
            this.Text = "LogConsole";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_logConsole;
    }
}