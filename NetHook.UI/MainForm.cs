using NetHook.Core;
using NetHook.Cores.Inject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NetHook.UI
{
    public partial class MainForm : Form
    {
        private RemoteInjector _remoteInjector;
        public Process CurrentProcess { get; private set; }

        public MainForm()
        {
            InitializeComponent();

            ResizeFormHelper.Instance.AddResizeControl(dataGridView_Threads);
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripButton_OpenProcess_Click(object sender, EventArgs e)
        {
            using (OpenProcess form = new OpenProcess())
            {
                form.FormClosed += (o, ev) =>
                {
                    if (form.SelectedProcess != null)
                        OpenProcess(form.SelectedProcess);
                };

                form.ShowDialog();
            }
        }

        private void OpenProcess(Process process)
        {
            CloseProcess();

            CurrentProcess = process;
            toolStripLabel_OpenProcess.Text = $"{process.Id} {process.ProcessName}";
            _remoteInjector = new RemoteInjector();
            _remoteInjector.Inject(CurrentProcess);
        }

        private void CloseProcess()
        {
            toolStripLabel_OpenProcess.Text = $"";
            CurrentProcess = null;
            _remoteInjector?.Dispose();
        }

        private void toolStripButton_Tool_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            ResizeFormHelper.Instance.ResizeСhangesForm(this);
        }

        private void toolStripButton_Play_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView_Threads_DoubleClick(object sender, EventArgs e)
        {
            var row = dataGridView_Threads.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();

            if (row == null || !_rowsThread.TryGetValue(row, out ThreadInfo threadInfo))
                return;

            //ThreadInfo threadInfo = null;
            Thread thread = new Thread(() =>
            {
                using (TraceLogForm form = new TraceLogForm(threadInfo))
                {
                    form.ShowDialog();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void toolStripButton_Filter_Click(object sender, EventArgs e)
        {
            if (CurrentProcess == null || _remoteInjector == null)
                return;

            LoggerInterface.WaitOnline();

            using (FilterForm form = new FilterForm(_remoteInjector))
            {
                form.FormClosed += (o, ev) =>
                {
                    if (form.ResultValue == null)
                        return;

                    _remoteInjector.LoggerServer.SetHook(form.ResultValue.ToArray());

                    _remoteInjector.LoggerServer.OnTraceLoad += UpdateThreads;
                };

                form.ShowDialog();
            }
        }

        private readonly Dictionary<DataGridViewRow, ThreadInfo> _rowsThread = new Dictionary<DataGridViewRow, ThreadInfo>();

        private void UpdateThreads(ThreadInfo[] threads)
        {
            this.Invoke((Action)(() =>
            {
                this.dataGridView_Threads.Rows.Clear();
                _rowsThread.Clear();

                foreach (var thread in threads)
                {
                    if (thread.Frames.Length > 0)
                    {
                        TimeSpan timeSpan = thread.Frames.Select(x => x.Elapsed).Aggregate((x, y) => x.Add(y));

                        DataGridViewRow row = new DataGridViewRow();
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = thread.ThreadID });
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = thread.ThreadState });
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = thread.Frames.SelectRecursive(x => x.ChildFrames).Count() });
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = thread.Frames.FirstOrDefault().DateCreate });
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = thread.Frames.FirstOrDefault().DateCreate.Add(timeSpan) });
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = timeSpan });

                        this.dataGridView_Threads.Rows.Add(row);
                        _rowsThread[row] = thread;
                    }
                }
            }));
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

            string path = @"..\..\NetHook.TestInject\bin\Debug\NetHook.TestInject.exe";

            Process process = Process.Start(path);

            Thread.Sleep(1000);

            OpenProcess(process);
        }

    }
}
