using NetHook.Cores.Extensions;
using NetHook.Cores.Inject;
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
    public partial class FilterForm : Form
    {
        public RemoteInjector RemoteInjector { get; }
        private readonly Dictionary<TreeNodeHelper, object> _nodes = new Dictionary<TreeNodeHelper, object>();

        public List<MethodModelInfo> ResultValue { get; private set; }

        public readonly TreeViewSearchHelper _treeViewSearchHelper;

        public FilterForm(RemoteInjector remoteInjector)
        {
            InitializeComponent();
            RemoteInjector = remoteInjector;
            _treeViewSearchHelper = new TreeViewSearchHelper(treeView_assemblies);

            InitTreeView();
            ResizeFormHelper.Instance.AddResizeControl(treeView_assemblies);
            ResizeFormHelper.Instance.AddFixControl(button_Ok);
            ResizeFormHelper.Instance.AddFixControl(button_Cancel);
        }

        private void InitTreeView()
        {
            var assemblers = RemoteInjector.LoggerServer.GetAssembles();

            treeView_assemblies.BeginUpdate();

            using (treeView_assemblies.CreateUpdateContext())
                treeView_assemblies.Nodes.AddRange(assemblers.Select(GetNode).ToArray());

            treeView_assemblies.EndUpdate();
        }

        private TreeNodeHelper GetNode(AssembleModelInfo assembleModel)
        {
            TreeNodeHelper node = new TreeNodeHelper(_treeViewSearchHelper.HideRootNodes, treeView_assemblies.Nodes, assembleModel.Name);
            _nodes[node] = assembleModel;

            node.AddRange(assembleModel.Types.GroupBy(x => x.Namespace)
                .Select(x => GetNode(x.Key, x.ToArray()))
                .ToArray());

            return node;
        }

        private TreeNodeHelper GetNode(string nameSpace, TypeModelInfo[] types)
        {
            TreeNodeHelper node = new TreeNodeHelper(nameSpace);
            _nodes[node] = nameSpace;

            node.AddRange(types.Select(x => GetNode(x)).ToArray());

            return node;
        }

        private TreeNodeHelper GetNode(TypeModelInfo typeModel)
        {
            TreeNodeHelper node = new TreeNodeHelper(typeModel.Name);
            _nodes[node] = typeModel;

            node.AddRange(typeModel.Methods.Select(x => GetNode(x)).ToArray());

            return node;
        }

        private TreeNodeHelper GetNode(MethodModelInfo methodlInfo)
        {
            TreeNodeHelper node = new TreeNodeHelper(methodlInfo.Signature);
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

        private void toolStripTextBox_searchValue_TextChanged(object sender, EventArgs e) =>
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

        private void FilterForm_SizeChanged(object sender, EventArgs e)
        {
            ResizeFormHelper.Instance.ResizeСhangesForm(this);
        }
    }
}
