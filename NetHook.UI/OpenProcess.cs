using NetHook.Cores.Extensions;
using NetHook.UI.Extensions;
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
    public partial class OpenProcess : Form
    {
        public OpenProcess()
        {
            InitializeComponent();

            button_OK.Enabled = false;
            dataGridView_ProcessList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView_ProcessList.MultiSelect = false;

            dataGridView_ProcessList.Columns.Add("PID", "PID");
            dataGridView_ProcessList.Columns.Add("Process Name", "Process Name");

            dataGridView_ProcessList.Rows.AddRange(Process.GetProcesses()
                .Select(x => (DataGridViewRow)new ProcessListViweInfo(x))
                .ToArray());

            dataGridView_ProcessList.AllowUserToResizeRows = false;
            dataGridView_ProcessList.AllowUserToResizeColumns = false;
            dataGridView_ProcessList.AllowUserToDeleteRows = false;
            dataGridView_ProcessList.AllowUserToAddRows = false;
            dataGridView_ProcessList.AllowDrop = false;
            dataGridView_ProcessList.ReadOnly = true;
            dataGridView_ProcessList.ScrollBars = ScrollBars.Both;

            dataGridView_ProcessList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            dataGridView_ProcessList.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_ProcessList.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.ActiveControl = toolStripTextBox_searchValue.TextBox;
        }

        public Process SelectedProcess { get; private set; }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button_cancell_Click(object sender, EventArgs e)
        {
            SelectedProcess = null;
            this.Close();
        }

        private class ProcessListViweInfo : DataGridViewRow
        {
            public Process Process { get; }
            public ProcessListViweInfo(Process process)
            {
                Process = process;

                this.Cells.Add(new DataGridViewTextBoxCell() { Value = Process.Id });
                this.Cells.Add(new DataGridViewTextBoxCell() { Value = Process.ProcessName });
            }


            private bool __init_DisplayInfo = false;
            private string _DisplayInfo;
            public string DisplayInfo
            {
                get
                {
                    if (!__init_DisplayInfo)
                    {
                        _DisplayInfo = $"{Process.Id.ToString().PadRight(7)} {Process.ProcessName}";
                        __init_DisplayInfo = true;
                    }
                    return _DisplayInfo;
                }
            }
        }

        private void dataGridView_ProcessList_SelectionChanged(object sender, EventArgs e)
        {

            SelectedProcess = (dataGridView_ProcessList.SelectedRows.Cast<ProcessListViweInfo>().FirstOrDefault())?.Process;
            button_OK.Enabled = SelectedProcess != null;
        }

        private void dataGridView_ProcessList_DoubleClick(object sender, EventArgs e)
        {
            SelectedProcess = (dataGridView_ProcessList.SelectedRows.Cast<ProcessListViweInfo>().FirstOrDefault())?.Process;
            if (SelectedProcess != null)
                this.Close();
        }

        private void toolStripTextBox_searchValue_Click(object sender, EventArgs e)
        {
            var value = toolStripTextBox_searchValue.Text;
            dataGridView_ProcessList.ClearSelection();

            using (dataGridView_ProcessList.CreateUpdateContext())
            {
                foreach (ProcessListViweInfo row in dataGridView_ProcessList.Rows)
                    row.Visible = row.DisplayInfo.ContainsSearch(value);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e) =>
            toolStripTextBox_searchValue_Click(sender, e);

        private void toolStripTextBox_searchValue_TextChanged(object sender, EventArgs e) =>
            toolStripTextBox_searchValue_Click(sender, e);

    }
}
