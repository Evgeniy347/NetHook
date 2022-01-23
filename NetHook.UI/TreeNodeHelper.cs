using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NetHook.UI
{
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

        public List<TreeNodeHelper> CasheNodes { get; } = new List<TreeNodeHelper>();

        public List<TreeNodeHelper> HideNodes { get; } = new List<TreeNodeHelper>();

        private List<TreeNodeHelper> _HideParentNodes;
        public List<TreeNodeHelper> HideParentNodes => _HideParentNodes ?? (_HideParentNodes = (this.Parent).HideNodes);

        private TreeNodeCollection _ParentNodes;
        public TreeNodeCollection ParentNodes => _ParentNodes ?? (_ParentNodes = this.Parent.Nodes);

        public TreeNodeHelper[] AllNodes => HideNodes
            .Union(this.Nodes.Cast<TreeNodeHelper>())
            .Union(CasheNodes)
            .Where(x => x.Name != "Entity")
            .ToArray();

        public TreeNodeHelper Parent => (TreeNodeHelper)base.Parent;

        private bool _Visible = true;
        public bool Visible
        {
            get => _Visible; set
            {
                if (_Visible == value || Text == "Entity")
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
            if (Text == "Entity")
                return;

            HideParentNodes.Add(this);
            ParentNodes.Remove(this);
        }

        public void Show()
        {
            if (Text == "Entity")
                return;

            HideParentNodes.Remove(this);
            ParentNodes.Add(this);
        }

        public void AddRange(TreeNodeHelper[] nodes)
        {
            if (nodes.Length == 0)
                return;

            if (this.IsExpanded)
            {
                Nodes.AddRange(nodes);
            }
            else
            {
                CasheNodes.AddRange(nodes);
                foreach (TreeNodeHelper node in nodes)
                {
                    node._HideParentNodes = HideNodes;
                    node._ParentNodes = Nodes;
                }

                Nodes.Add(new TreeNodeHelper("Entity"));
            }
        }

        internal void BeforeCollapse()
        {
            Nodes.Clear();
            Nodes.Add(new TreeNodeHelper("Entity"));
        }

        internal void AfterExpand()
        {
            Nodes.Clear();
            Nodes.AddRange(CasheNodes.ToArray());
        }
    }
}
