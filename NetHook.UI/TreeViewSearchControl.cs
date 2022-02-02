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
        public TreeNodeRootHelper TreeView { get; }

        public ToolStripTextBox SearchTextBox { get; }

        public TreeViewSearchHelper(TreeNodeRootHelper treeView, ToolStripTextBox toolStripTextBox_searchValue)
        {
            TreeView = treeView;
            SearchTextBox = toolStripTextBox_searchValue;

            toolStripTextBox_searchValue.TextChanged += (x, y) => Search(toolStripTextBox_searchValue.Text);
        }

        public void Search(string value)
        {

            TreeView.TreeView.BeginUpdate();
            using (TreeView.TreeView.CreateUpdateContext())
            {
                TreeView.TreeView.SelectedNode = null;
                CheckVisibleNodes(TreeView.AllNodes, value);
            }

            TreeView.TreeView.EndUpdate();
        }

        private bool CheckVisibleNodes(IEnumerable<TreeNodeHelper> nodes, string value)
        {
            bool any = false;

            foreach (TreeNodeHelper node in nodes.ToArray())
            {
                node.Collapse();
                node.Visible = CheckVisibleNodes(node.AllNodes, value) || node.Text.ContainsSearch(value);

                if (node.Visible)
                    any = true;
            }

            return any;
        }

    }
}
