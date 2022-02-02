using NetHook.Cores.Handlers.Trace;
using NetHook.Cores.Inject;
using NetHook.Cores.Inject.AssemblyModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetHook.UI
{
    public partial class TraceLogForm : Form
    {
        private readonly ThreadInfo _threadInfo;
        private readonly TreeViewSearchHelper _treeViewSearchHelper;
        private readonly TreeNodeRootHelper _treeViewRoot;

        public TraceLogForm(ThreadInfo threadInfo)
        {
            InitializeComponent();
            ResizeFormHelper.Instance.AddResizeControl(treeView_TraceLog);
            ResizeFormHelper.Instance.AddFixControl(button_Ok);
            ResizeFormHelper.Instance.AddFixControl(button_Cancel);

            _threadInfo = threadInfo;
            _treeViewRoot = new TreeNodeRootHelper(treeView_TraceLog);
            _treeViewSearchHelper = new TreeViewSearchHelper(_treeViewRoot, toolStripTextBox_searchValue);

            InitTree();
        }

        private void InitTree()
        {
            if (_threadInfo != null)
                treeView_TraceLog.Nodes.AddRange(_threadInfo.Frames.Select(x => AddRootNode(x)).ToArray());
        }

        private TreeNode AddRootNode(TraceFrameInfo traceFrameInfo)
        {
            TreeNodeHelper result = _treeViewRoot.CreateNode(GetText(traceFrameInfo));

            foreach (var frame in traceFrameInfo.ChildFrames)
                AddNode(result, frame);

            return result;
        }


        private TreeNodeHelper AddNode(TreeNodeHelper parent, TraceFrameInfo traceFrameInfo)
        {
            TreeNodeHelper result = parent.CreateNode(GetText(traceFrameInfo));

            foreach (var frame in traceFrameInfo.ChildFrames)
                AddNode(result, frame);

            return result;
        }

        private string GetText(TraceFrameInfo frame)
        {
            if (frame.IsRunning)
            {
                return $"{frame.Signature} Start:{frame.DateCreate:HH:mm:ss.fff}";
            }
            else
            {
                return $"{frame.Signature} Start:{frame.DateCreate:HH:mm:ss.fff} End:{frame.DateCreate.Add(frame.Elapsed):HH:mm:ss.fff} Elapsed:{frame.Elapsed}";
            }
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
