using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetHook.UI.Controls
{
    public class TreeViewFast : TreeView
    {
        private readonly Dictionary<string, TreeNode> _treeNodes = new Dictionary<string, TreeNode>();

        /// <summary>
        /// Load the TreeView with items.
        /// </summary>
        /// <typeparam name="TNode">Item type</typeparam>
        /// <param name="items">Collection of items</param>
        /// <param name="getId">Function to parse Id value from item object</param>
        /// <param name="getParentId">Function to parse parentId value from item object</param>
        /// <param name="getDisplayName">Function to parse display name value from item object. This is used as node text.</param>
        public void LoadItems<TNode>(IEnumerable<TNode> items, Func<TNode, string> getParentId)
           where TNode : TreeNode
        {
            // Clear view and internal dictionary
            Nodes.Clear();
            _treeNodes.Clear();

            // Load internal dictionary with nodes
            foreach (var item in items)
                _treeNodes.Add(item.Name, item);

            // Create hierarchy and load into view
            foreach (var id in _treeNodes.Keys)
            {
                var node = GetNode(id);
                var obj = (TNode)node.Tag;
                var parentId = getParentId(obj);

                if (!string.IsNullOrEmpty(parentId))
                {
                    var parentNode = GetNode(parentId);
                    parentNode.Nodes.Add(node);
                }
                else
                {
                    Nodes.Add(node);
                }
            }
        }

        /// <summary>
        /// Get a handle to the object collection.
        /// This is convenient if you want to search the object collection.
        /// </summary>
        public IQueryable<T> GetItems<T>()
        {
            return _treeNodes.Values.Select(x => (T)x.Tag).AsQueryable();
        }

        /// <summary>
        /// Retrieve TreeNode by Id.
        /// Useful when you want to select a specific node.
        /// </summary>
        /// <param name="id">Item id</param>
        public TreeNode GetNode(string id)
        {
            return _treeNodes[id];
        }

        /// <summary>
        /// Retrieve item object by Id.
        /// Useful when you want to get hold of object for reading or further manipulating.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="id">Item id</param>
        /// <returns>Item object</returns>
        public T GetItem<T>(string id)
        {
            return (T)GetNode(id).Tag;
        }


        /// <summary>
        /// Get parent item.
        /// Will return NULL if item is at top level.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="id">Item id</param>
        /// <returns>Item object</returns>
        public T GetParent<T>(string id) where T : class
        {
            var parentNode = GetNode(id).Parent;
            return parentNode == null ? null : (T)Parent.Tag;
        }

        /// <summary>
        /// Retrieve descendants to specified item.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="id">Item id</param>
        /// <param name="deepLimit">Number of generations to traverse down. 1 means only direct children. Null means no limit.</param>
        /// <returns>List of item objects</returns>
        public List<T> GetDescendants<T>(string id, int? deepLimit = null)
        {
            var node = GetNode(id);
            var enumerator = node.Nodes.GetEnumerator();
            var items = new List<T>();

            if (deepLimit.HasValue && deepLimit.Value <= 0)
                return items;

            while (enumerator.MoveNext())
            {
                // Add child
                var childNode = (TreeNode)enumerator.Current;
                var childItem = (T)childNode.Tag;
                items.Add(childItem);

                // If requested add grandchildren recursively
                var childDeepLimit = deepLimit.HasValue ? deepLimit.Value - 1 : (int?)null;
                if (!deepLimit.HasValue || childDeepLimit > 0)
                {
                    var childId = childNode.Name;
                    var descendants = GetDescendants<T>(childId, childDeepLimit);
                    items.AddRange(descendants);
                }
            }
            return items;
        }
    }
}
