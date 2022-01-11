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

        public List<TreeNodeHelper> HideNodes { get; } = new List<TreeNodeHelper>();

        private List<TreeNodeHelper> _HideParentNodes;
        public List<TreeNodeHelper> HideParentNodes => _HideParentNodes ?? (_HideParentNodes = ((TreeNodeHelper)this.Parent).HideNodes);

        private TreeNodeCollection _ParentNodes;
        public TreeNodeCollection ParentNodes => _ParentNodes ?? (_ParentNodes = this.Parent.Nodes);

        public TreeNodeHelper[] AllNodes => HideNodes.Union(this.Nodes.Cast<TreeNodeHelper>()).ToArray();

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

        public void AddRange(TreeNodeHelper[] nodes)
        {
            Nodes.AddRange(nodes);
        }
    }
}
