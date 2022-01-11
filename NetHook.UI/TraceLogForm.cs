using NetHook.Cores.Handlers.Trace;
using NetHook.Cores.Inject;
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

        public TraceLogForm(ThreadInfo threadInfo)
        {
            InitializeComponent();
            ResizeFormHelper.Instance.AddResizeControl(treeView_TraceLog);
            ResizeFormHelper.Instance.AddFixControl(button_Ok);
            ResizeFormHelper.Instance.AddFixControl(button_Cancel);

            _threadInfo = threadInfo;
            _treeViewSearchHelper = new TreeViewSearchHelper(treeView_TraceLog);

            InitTree();
        }

        private void InitTree()
        {
            if (_threadInfo != null)
                treeView_TraceLog.Nodes.AddRange(_threadInfo.Frames.Select(x => ConvertRootNode(x)).ToArray());
        }

        private TreeNode ConvertRootNode(TraceFrameInfo traceFrameInfo)
        {
            TreeNodeHelper result = new TreeNodeHelper(_treeViewSearchHelper.HideRootNodes, treeView_TraceLog.Nodes, GetText(traceFrameInfo));

            result.AddRange(traceFrameInfo.ChildFrames.Select(ConvertNode).ToArray());

            return result;
        }

        private TreeNodeHelper ConvertNode(TraceFrameInfo traceFrameInfo)
        {
            TreeNodeHelper result = new TreeNodeHelper(GetText(traceFrameInfo));

            result.AddRange(traceFrameInfo.ChildFrames.Select(ConvertNode).ToArray());

            return result;
        }

        private string GetText(TraceFrameInfo frame)
        {
            return $"{frame.Signature} {(frame.IsRunning ? "" : frame.Elapsed.ToString())}";
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TraceLogForm_SizeChanged(object sender, EventArgs e)
        {
            ResizeFormHelper.Instance.ResizeСhangesForm(this);
        }
    }
}
