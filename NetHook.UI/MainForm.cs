using NetHook.Core;
using NetHook.Cores.Inject;
using NetHook.Cores.Inject.AssemblyModel;
using NetHook.Cores.NetSocket;
using NetHook.UI.Enums;
using NetHook.UI.Extensions;
using NetHook.UI.Helpers;
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
        private LoggerServer _server;
        private ConcurrentStreamWriter _consoleStream = new ConcurrentStreamWriter();
        public Process CurrentProcess { get; private set; }
        public ServerLogStatus ServerStatus { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            _server = new LoggerServer();
            _server.StartServer();

            ResizeFormHelper.Instance.AddResizeControl(dataGridView_Threads);
            LoggerProxy.OnInjectDomainError += (x, y, z) =>
            {
                MessageBox.Show(y, z);
            };

            LoggerProxy.OnInjectProcessError += (y, z) =>
            {
                MessageBox.Show(y, z);
            };
            ServerStatus = ServerLogStatus.Run;
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
                    Console.WriteLine($"Form OpenProcess Closed");

                    if (form.SelectedProcess != null)
                        OpenProcess(form.SelectedProcess);
                };

                form.ShowDialog();
            }
        }

        private void OpenProcess(Process process)
        {
            Console.WriteLine($"OpenProcess {process.Id} {process.ProcessName}");
            CloseProcess();

            CurrentProcess = process;
            toolStripLabel_OpenProcess.Text = $"{process.Id} {process.ProcessName}";
            _remoteInjector = new RemoteInjector();
            _remoteInjector.InjectSocketerver(CurrentProcess, _server.Address);
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


        private void toolStripButton_Play_Click(object sender, EventArgs e)
        {
            ServerStatus = ServerLogStatus.Run;
        }

        private void dataGridView_Threads_DoubleClick(object sender, EventArgs e)
        {
            var row = dataGridView_Threads.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();

            if (row == null || !_rowsThread.TryGetValue(row, out ThreadInfo threadInfo))
                return;

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

            _server.StopTraceLog();

            using (FilterForm form = new FilterForm(_server))
            {
                form.FormClosed += (o, ev) =>
                {
                    if (form.ResultValue == null)
                        return;

                    _server.SetHook(form.ResultValue.ToArray());
                    _server.ReceveTraceLog = null;
                    _server.ReceveTraceLog = UpdateThreads;
                    _server.RunTraceLog();
                };

                form.ShowDialog();
            }
        }

        private readonly Dictionary<DataGridViewRow, ThreadInfo> _rowsThread = new Dictionary<DataGridViewRow, ThreadInfo>();

        private void UpdateThreads(ThreadInfo[] threads)
        {
            if (ServerStatus == ServerLogStatus.Pause)
                return;
            else if (ServerStatus == ServerLogStatus.OnlyRun)
                ServerStatus = ServerLogStatus.Pause;

            List<DataGridViewRow> rows = new List<DataGridViewRow>();
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
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = "" });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = thread.URL });

                    rows.Add(row);
                    _rowsThread[row] = thread;
                }
            };

            this.Invoke(() =>
            {
                this.dataGridView_Threads.Rows.Clear();
                this.dataGridView_Threads.Rows.AddRange(rows.ToArray());
            });
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string path = @"..\..\NetHook.TestInject\bin\Debug\NetHook.TestInject.exe";

            Process process = Process.Start(path);

            Thread.Sleep(1000);

            OpenProcess(process);

            toolStripButton_logConsole_Click(null, null);
        }

        private void toolStripButton_logConsole_Click(object sender, EventArgs e)
        {
            if (_consoleStream?.TextBox?.IsHandleCreated ?? false)
            {
                Form form = _consoleStream.TextBox.FindForm();

                if (form?.IsHandleCreated ?? false)
                {
                    form.Invoke(() => form.Activate());
                    return;
                }
            }

            Thread thread = new Thread(() =>
            {
                using (LogConsole form = new LogConsole(_consoleStream))
                {
                    form.ShowDialog();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void toolStripButton_Pause_Click(object sender, EventArgs e)
        {
            ServerStatus = ServerLogStatus.Pause;
        }

        private void toolStripButton_Refresh_Click(object sender, EventArgs e)
        {
            ServerStatus = ServerLogStatus.OnlyRun;
        }

        private void toolStripButton_Clear_Click(object sender, EventArgs e)
        {
            dataGridView_Threads.Rows.Clear();
        }
    }
}
