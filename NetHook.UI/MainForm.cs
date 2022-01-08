using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetHook.UI
{
    public partial class MainForm : Form
    {
        public Process CurrentProcess { get; private set; }
        int offsetGrideWidth = 0;
        int offsetGrideHeight = 0;
        public MainForm()
        {
            InitializeComponent();
            offsetGrideWidth = this.Width - this.dataGridView_Threads.Width;
            offsetGrideHeight = this.Height - this.dataGridView_Threads.Height;
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripButton_OpenProcess_Click(object sender, EventArgs e)
        {
            OpenProcess form = new OpenProcess();

            form.FormClosed += (o, ev) =>
            {
                if (form.SelectedProcess != null)
                    OpenProcess(form.SelectedProcess);
            };

            form.ShowDialog(this);
        }

        private void OpenProcess(Process process)
        {
            CloseProcess();

            CurrentProcess = process;
            toolStripLabel_OpenProcess.Text = $"{process.Id} {process.ProcessName}";
        }

        private void CloseProcess()
        {
            toolStripLabel_OpenProcess.Text = $"";
            CurrentProcess = null;
        }

        private void toolStripButton_Tool_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            this.dataGridView_Threads.Width = this.Width - offsetGrideWidth;
            this.dataGridView_Threads.Height = this.Height - offsetGrideHeight;
        }
    }
}
