using NetHook.Cores.Extensions;
using NetHook.Cores.Helpers;
using NetHook.Cores.Inject;
using NetHook.Cores.NetSocket;
using NetHook.UI.Extensions;
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
    public partial class FilterForm : Form
    {
        private readonly LoggerServer _server;
        private readonly Dictionary<TreeNodeHelper, object> _nodes = new Dictionary<TreeNodeHelper, object>();
        private bool _processing_AfterCheck;

        public List<MethodModelInfo> ResultValue { get; private set; }

        public readonly TreeViewSearchHelper _treeViewSearchHelper;

        public readonly TreeNodeRootHelper _treeViewRootNode;

        public FilterForm(LoggerServer loggerServer)
        {
            InitializeComponent();
            _server = loggerServer;
            _treeViewRootNode = new TreeNodeRootHelper(treeView_assemblies);
            _treeViewSearchHelper = new TreeViewSearchHelper(_treeViewRootNode, toolStripTextBox_searchValue);
            UpdateTree();
            ResizeFormHelper.Instance.AddResizeControl(treeView_assemblies);
            ResizeFormHelper.Instance.AddFixControl(button_Ok);
            ResizeFormHelper.Instance.AddFixControl(button_Cancel);
            pictureBox_Load_Processing.Visible = true;
        }

        private void InitTreeView(AssembleModelInfo[] assemblers)
        {
            treeView_assemblies.BeginUpdate();
            treeView_assemblies.Nodes.Clear();
            _nodes.Clear();

            using (treeView_assemblies.CreateUpdateContext())
            {
                foreach (var assemble in assemblers)
                    AddNode(assemble);
            }
            treeView_assemblies.EndUpdate();

            pictureBox_Load_Processing.Visible = false;
            toolStripTextBox_searchValue_Click(null, null);
        }

        private void toolStripButton_Update_Click(object sender, EventArgs e)
        {
            UpdateTree();
        }

        private void UpdateTree()
        {
            pictureBox_Load_Processing.Visible = true;

            ThreadHelper.RunLogic(() =>
            {
                AssembleModelInfo[] assemblers = _server.GetAssembles();
                this.Invoke(() => InitTreeView(assemblers));
            }, "UpdateTree");
        }

        private TreeNodeHelper AddNode(AssembleModelInfo assembleModel)
        {
            TreeNodeHelper node = _treeViewRootNode.CreateNode(assembleModel.Name);

            _nodes[node] = assembleModel;

            foreach (var types in assembleModel.Types.GroupBy(x => x.Namespace))
                AddNode(node, types.Key, types.ToArray());

            return node;
        }

        private TreeNodeHelper AddNode(TreeNodeHelper parent, string nameSpace, TypeModelInfo[] types)
        {
            TreeNodeHelper node = parent.CreateNode(nameSpace);
            _nodes[node] = nameSpace;

            foreach (var type in types)
                AddNode(node, type);

            return node;
        }

        private TreeNodeHelper AddNode(TreeNodeHelper parent, TypeModelInfo typeModel)
        {
            TreeNodeHelper node = parent.CreateNode(typeModel.Name);

            _nodes[node] = typeModel;

            foreach (var method in typeModel.Methods)
                AddNode(node, method, typeModel);

            return node;
        }

        private TreeNodeHelper AddNode(TreeNodeHelper parent, MethodModelInfo methodlInfo, TypeModelInfo typeModel)
        {
            TreeNodeHelper node = parent.CreateNode(methodlInfo.Signature);
            methodlInfo.TypeName = typeModel.FullName;
            _nodes[node] = methodlInfo;
            return node;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            ResultValue = null;
            this.Close();
        }

        private void toolStripTextBox_searchValue_Click(object sender, EventArgs e)
        {
            var value = toolStripTextBox_searchValue.Text;
            _treeViewSearchHelper.Search(value);
        }

        private void toolStripOnSearch_Click(object sender, EventArgs e) =>
            toolStripTextBox_searchValue_Click(sender, e);

        private void button_Ok_Click(object sender, EventArgs e)
        {
            ResultValue = new List<MethodModelInfo>();

            foreach (var keyValue in _nodes)
            {
                if (keyValue.Key.Checked)
                {
                    if (keyValue.Value is MethodModelInfo method)
                    {
                        ResultValue.Add(method);
                    }
                }
            }

            this.Close();
        }

    }
}
