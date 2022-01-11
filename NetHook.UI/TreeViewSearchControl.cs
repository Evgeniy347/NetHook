using NetHook.Cores.Extensions;
using NetHook.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetHook.UI
{
    public class TreeViewSearchHelper
    {
        public TreeView TreeView { get; }

        public List<TreeNodeHelper> HideRootNodes { get; } = new List<TreeNodeHelper>();

        public TreeViewSearchHelper(TreeView treeView)
        {
            TreeView = treeView;
        }

        public void Search(string value)
        {

            TreeView.BeginUpdate();
            using (TreeView.CreateUpdateContext())
            {
                TreeView.SelectedNode = null;

                HideRootNodes.AddRange(TreeView.Nodes.Cast<TreeNodeHelper>());
                TreeView.Nodes.Clear();
                HideRootNodes.ForEach(x => x.Visible = false);

                CheckVisibleNodes(HideRootNodes, value);
            }

            TreeView.EndUpdate();
        }

        private bool CheckVisibleNodes(IEnumerable<TreeNodeHelper> nodes, string value)
        {
            bool any = false;

            foreach (TreeNodeHelper node in nodes.ToArray())
            {
                node.Visible = CheckVisibleNodes(node.AllNodes, value) || node.Text.ContainsSearch(value);

                if (node.Visible)
                    any = true;
            }

            return any;
        }

    }
}
