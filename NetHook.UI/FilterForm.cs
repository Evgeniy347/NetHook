using NetHook.Cores.Extensions;
using NetHook.Cores.Inject;
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
        private Dictionary<TreeNodeHelper, object> _nodes = new Dictionary<TreeNodeHelper, object>();
        private List<TreeNodeHelper> _rootNodes = new List<TreeNodeHelper>();

        public FilterForm(RemoteInjector remoteInjector)
        {
            InitializeComponent();
            RemoteInjector = remoteInjector;

            InitTreeView();
        }

        public class TreeNodeHelper : TreeNode
        {
            public TreeNodeHelper(List<TreeNodeHelper> hidesParent, TreeNodeCollection parentNodes, string text)
                : base(text)
            {
                _HideParentNodes = hidesParent;
                _ParentNodes = parentNodes;
            }

            public TreeNodeHelper(string text)
                : base(text)
            { }

            public List<TreeNodeHelper> HideNodes { get; } = new List<TreeNodeHelper>();

            public List<TreeNodeHelper> AllNodes => HideNodes.Union(this.Nodes.Cast<TreeNodeHelper>()).ToList();

            private List<TreeNodeHelper> _HideParentNodes;
            public List<TreeNodeHelper> HideParentNodes => _HideParentNodes ?? (_HideParentNodes = ((TreeNodeHelper)this.Parent).HideNodes);

            private TreeNodeCollection _ParentNodes;
            public TreeNodeCollection ParentNodes => _ParentNodes ?? (_ParentNodes = this.Parent.Nodes);

            private bool _Visible = true;
            public bool Visible
            {
                get => _Visible; set
                {
                    if (_Visible == value)
                        return;

                    if (value)
                        Show();
                    else
                        Hide();

                    _Visible = value;
                }
            }

            public void Hide()
            {
                HideParentNodes.Add(this);
                ParentNodes.Remove(this);
            }

            public void Show()
            {
                HideParentNodes.Remove(this);
                ParentNodes.Add(this);
            }
        }

        private void InitTreeView()
        {
            var assemblers = RemoteInjector.LoggerServer.GetAssembles();

            treeView_assemblies.BeginUpdate();

            treeView_assemblies.Nodes.AddRange(assemblers.Select(GetNode).ToArray());

            treeView_assemblies.EndUpdate();
        }

        private TreeNodeHelper GetNode(AssembleModelInfo assembleModel)
        {
            TreeNodeHelper node = new TreeNodeHelper(_rootNodes, treeView_assemblies.Nodes, assembleModel.Name);
            _nodes[node] = assembleModel;



            node.Nodes.AddRange(assembleModel.Types.GroupBy(x => x.Namespace)
                .Select(x => GetNode(x.Key, x.ToArray()))
                .ToArray());

            return node;
        }

        private TreeNodeHelper GetNode(string nameSpace, TypeModelInfo[] types)
        {
            TreeNodeHelper node = new TreeNodeHelper(nameSpace);
            _nodes[node] = nameSpace;

            node.Nodes.AddRange(types.Select(x => GetNode(x)).ToArray());

            return node;
        }

        private TreeNodeHelper GetNode(TypeModelInfo typeModel)
        {
            TreeNodeHelper node = new TreeNodeHelper(typeModel.Name);
            _nodes[node] = typeModel;

            node.Nodes.AddRange(typeModel.Methods.Select(x => GetNode(x)).ToArray());

            return node;
        }

        private TreeNodeHelper GetNode(TypeMethodlInfo methodlInfo)
        {
            TreeNodeHelper node = new TreeNodeHelper(methodlInfo.Signature);
            _nodes[node] = methodlInfo;

            return node;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkedListBox_process_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void FilterForm_Load(object sender, EventArgs e)
        {

        }

        private void treeView_assemblies_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void toolStripTextBox_searchValue_Click(object sender, EventArgs e)
        {
            var value = toolStripTextBox_searchValue.Text;

            treeView_assemblies.BeginUpdate();
            treeView_assemblies.SelectedNode = null;

            treeView_assemblies.Visible = false;
            CheckVisibleNodes(treeView_assemblies.Nodes.Cast<TreeNodeHelper>().Union(_rootNodes).ToList(), value);

            treeView_assemblies.Visible = true;

            treeView_assemblies.EndUpdate();
        }

        private bool CheckVisibleNodes(List<TreeNodeHelper> nodes, string value)
        {
            bool any = false;

            foreach (TreeNodeHelper node in nodes)
            {

                node.Visible = CheckVisibleNodes(node.AllNodes, value) || node.Text.ContainsSearch(value);

                if (node.Visible)
                    any = true;
            }

            return any;
        }

        private void toolStripOnSearch_Click(object sender, EventArgs e) =>
            toolStripTextBox_searchValue_Click(sender, e);

        private void toolStripTextBox_searchValue_TextChanged(object sender, EventArgs e) =>
            toolStripTextBox_searchValue_Click(sender, e);

    }
}
