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

            UpdateRows();
            ResizeFormHelper.Instance.AddResizeControl(dataGridView_ProcessList);
            ResizeFormHelper.Instance.AddFixControl(button_OK);
            ResizeFormHelper.Instance.AddFixControl(button_cancell);
        }

        private void UpdateRows()
        {
            DataGridViewRow[] rows = Process.GetProcesses()
                .Select(x => (DataGridViewRow)new ProcessListViweInfo(x))
                .ToArray();

            this.Invoke(() => UpdateRows(rows));
        }

        private void UpdateRows(DataGridViewRow[] rows)
        {
            dataGridView_ProcessList.Rows.Clear();

            dataGridView_ProcessList.Rows.AddRange(rows);

            dataGridView_ProcessList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
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
            Load += (o, e) => dataGridView_ProcessList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            pictureBox_Load_Processing.Visible = false;
            toolStripTextBox_searchValue_Click(null, null);
        }

        private void toolStripButton_Update_Click(object sender, EventArgs e)
        {
            pictureBox_Load_Processing.Visible = true;
            UpdateRows();
        }

        private Process _selectedProcess;

        public Process SelectedProcess { get; private set; }

        private void button_OK_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"OK Click {_selectedProcess}");

            SelectedProcess = _selectedProcess;
            this.Close();
        }

        private void button_cancell_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"Click Click {_selectedProcess}");

            _selectedProcess = null;
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
                        _DisplayInfo = $"{Process.Id,-7} {Process.ProcessName}";
                        __init_DisplayInfo = true;
                    }
                    return _DisplayInfo;
                }
            }
        }

        private void dataGridView_ProcessList_SelectionChanged(object sender, EventArgs e)
        {

            _selectedProcess = (dataGridView_ProcessList.SelectedRows.Cast<ProcessListViweInfo>().FirstOrDefault())?.Process;
            button_OK.Enabled = _selectedProcess != null;
        }

        private void dataGridView_ProcessList_DoubleClick(object sender, EventArgs e)
        {
            _selectedProcess = (dataGridView_ProcessList.SelectedRows.Cast<ProcessListViweInfo>().FirstOrDefault())?.Process;
            if (_selectedProcess != null)
            {
                SelectedProcess = _selectedProcess;
                this.Close();
            }
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

        private void toolStripOnSearch_Click(object sender, EventArgs e) =>
            toolStripTextBox_searchValue_Click(sender, e);

        private void toolStripTextBox_searchValue_TextChanged(object sender, EventArgs e) =>
            toolStripTextBox_searchValue_Click(sender, e);

    }
}
