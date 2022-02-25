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
        private LoggerServer _server = new LoggerServer();
        private ConcurrentStreamWriter _consoleStream = new ConcurrentStreamWriter();
        private readonly ThreadsCollection _threadsCollection = new ThreadsCollection();
        public Process CurrentProcess { get; private set; }
        public ServerLogStatus ServerStatus { get; private set; }

        public MainForm()
        {
            InitializeComponent();
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
            var row = dataGridView_Threads.SelectedRows.Cast<DataGridViewRowThread>().FirstOrDefault();

            if (row?.ThreadInfo == null)
                return;

            Thread thread = new Thread(() =>
            {
                using (TraceLogForm form = new TraceLogForm(row.ThreadInfo))
                {
                    form.ShowDialog();
                }
            });

            thread.IsBackground = true;
            thread.Name = "TraceLogForm";
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

                    if (form.ResultValue.Count > 0)
                    {
                        _server.SetHook(form.ResultValue.ToArray());
                        _server.ReceveTraceLog = null;
                        _server.ReceveTraceLog = UpdateThreads;
                        _server.RunTraceLog();
                    }
                };

                form.ShowDialog();
            }
        }


        private class ThreadsCollection
        {
            private readonly Dictionary<int, DataGridViewRowThread> _threadIDs = new Dictionary<int, DataGridViewRowThread>();

            public void AddOrUpdate(ThreadInfo threadInfo)
            {
                if (!_threadIDs.TryGetValue(threadInfo.ThreadID, out DataGridViewRowThread row))
                    _threadIDs[threadInfo.ThreadID] = row = new DataGridViewRowThread();


                row.Update(threadInfo);
            }

            public void AddOrUpdateRange(params ThreadInfo[] threads)
            {
                foreach (var thread in threads)
                    AddOrUpdate(thread);
            }

            public DataGridViewRowThread[] ToRows()
            {
                return _threadIDs.Values.ToArray();
            }
        }

        private class DataGridViewRowThread : DataGridViewRow
        {
            public ThreadInfo ThreadInfo { get; private set; }

            public void Update(ThreadInfo thread)
            {
                if (ThreadInfo == null)
                    CreateData();
                ThreadInfo = thread;

                TimeSpan timeSpan = thread.Frames.Select(x => x.Elapsed).Aggregate((x, y) => x.Add(y));

                Cells[0].Value = thread.ThreadID;
                Cells[1].Value = thread.ThreadState;
                Cells[2].Value = thread.Frames.SelectRecursive(x => x.ChildFrames).Count();
                Cells[3].Value = thread.Frames.FirstOrDefault().DateCreate;
                Cells[4].Value = thread.Frames.FirstOrDefault().DateCreate.Add(timeSpan);
                Cells[5].Value = timeSpan;
                Cells[6].Value = "";
                Cells[7].Value = thread.URL;
            }

            private void CreateData()
            {
                for (int i = 0; i < 8; i++)
                    Cells.Add(new DataGridViewTextBoxCell());
            }
        }

        private void UpdateThreads(ThreadInfo[] threads)
        {
            if (ServerStatus == ServerLogStatus.Pause)
                return;
            else if (ServerStatus == ServerLogStatus.OnlyRun)
                ServerStatus = ServerLogStatus.Pause;

            _threadsCollection.AddOrUpdateRange(threads);

            this.Invoke(() =>
            {
                this.dataGridView_Threads.Rows.Clear();
                this.dataGridView_Threads.Rows.AddRange(_threadsCollection.ToRows());
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

            thread.Name = "LogConsoleThread";
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
