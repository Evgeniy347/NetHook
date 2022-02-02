using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NetHook.UI
{
    public class TreeNodeRootHelper : TreeNodeHelper
    {
        private readonly TreeView _treeView;

        public TreeView TreeView => _treeView;

        public TreeNodeRootHelper(TreeView treeView)
            : base(null, null)
        {
            _treeView = treeView;
            treeView.BeforeCollapse += BeforeCollapse;
            treeView.AfterExpand += AfterExpand;
            treeView.AfterCheck += AfterCheck;

            this.Collapse();
        }


        private void BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            TreeNodeHelper node = e.Node as TreeNodeHelper;
            if (node != null)
                node.BeforeCollapse();
        }

        private void AfterExpand(object sender, TreeViewEventArgs e)
        {
            TreeNodeHelper node = e.Node as TreeNodeHelper;
            if (node != null)
                node.AfterExpand();
        }

        private bool _processing_AfterCheck;

        private void AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_processing_AfterCheck)
                return;

            try
            {
                _processing_AfterCheck = true;
                TreeNodeHelper treeNode = e.Node as TreeNodeHelper;

                if (treeNode != null)
                {
                    if (!treeNode.Checked)
                        ChangeParent(treeNode, false);

                    ChangeChilds(treeNode, treeNode.Checked);
                }

            }
            finally
            {
                _processing_AfterCheck = false;
            }
        }

        private void ChangeParent(TreeNodeHelper treeNode, bool value)
        {
            TreeNodeHelper parent = treeNode.Parent;
            if (parent != null)
            {
                parent.Checked = value;
                ChangeParent(parent, value);
            }
        }

        private void ChangeChilds(TreeNodeHelper treeNode, bool value)
        {
            foreach (var node in treeNode.AllNodes)
            {
                node.Checked = value;
                ChangeChilds(node, value);
            }
        }

        protected override void Add(TreeNodeHelper node)
        {
            CasheNodes.Add(node);
            _treeView.Nodes.Add(node);
        }

        protected override void ChangeVisible(TreeNodeHelper treeNodeHelper)
        {
            base.ChangeVisible(treeNodeHelper);
            if (treeNodeHelper.Visible)
                _treeView.Nodes.Add(treeNodeHelper);
            else
                _treeView.Nodes.Remove(treeNodeHelper);
        }
    }

    public class TreeNodeHelper : TreeNode
    {
        protected TreeNodeHelper(TreeNodeHelper parent, string text)
             : base(text)
        {
            Parent = parent;
        }

        protected List<TreeNodeHelper> CasheNodes { get; } = new List<TreeNodeHelper>();

        protected List<TreeNodeHelper> HideNodes { get; } = new List<TreeNodeHelper>();

        public TreeNodeHelper[] AllNodes => CasheNodes
            .Union(HideNodes)
            .Where(x => x.Text != "Entity")
            .ToArray();

        public TreeNodeHelper Parent { get; }

        private bool _Visible = true;
        public bool Visible
        {
            get => _Visible; set
            {
                if (_Visible != value)
                {
                    _Visible = value;
                    ChangeVisible();
                }
            }
        }

        protected virtual void ChangeVisible()
        {
            Parent.ChangeVisible(this);
        }

        protected virtual void ChangeVisible(TreeNodeHelper treeNodeHelper)
        {
            if (treeNodeHelper.Visible)
            {
                if (HideNodes.Remove(treeNodeHelper))
                    CasheNodes.Add(treeNodeHelper);
                if (this.IsExpanded)
                    Nodes.Add(treeNodeHelper);
            }
            else
            {
                if (CasheNodes.Remove(treeNodeHelper))
                    HideNodes.Add(treeNodeHelper);
                if (this.IsExpanded)
                    Nodes.Remove(treeNodeHelper);
            }
        }

        public TreeNodeHelper CreateNode(string text)
        {
            TreeNodeHelper result = new TreeNodeHelper(this, text);
            Add(result);
            return result;
        }

        protected virtual void Add(TreeNodeHelper node)
        {
            CasheNodes.Add(node);
            if (this.IsExpanded)
                Nodes.Add(node);
            else
                BeforeCollapse();
        }

        internal void BeforeCollapse()
        {
            Nodes.Clear();
            Nodes.Add(new TreeNodeHelper(this, "Entity") { Name = "Entity" });
        }

        internal void AfterExpand()
        {
            Nodes.Clear();
            Nodes.AddRange(CasheNodes.ToArray());
        }
    }
}
