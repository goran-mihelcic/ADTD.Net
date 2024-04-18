using System;
using System.Collections.Generic;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    /// <summary>
    /// Represents Diagram node in the tree structure
    /// </summary>
    public class TreeNode
    {
        private IDiagramNode _node;
        private TreeNode _parent;
        private List<TreeNode> _children;
        private Double _subRadius;

        /// <summary>
        /// Diagram node stored in the tree
        /// </summary>
        public IDiagramNode Node { get { return _node; } }

        /// <summary>
        /// Parent node
        /// </summary>
        public TreeNode Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Node children
        /// </summary>
        public IList<TreeNode> Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Level in hierarchy
        /// </summary>
        public int Level
        {
            get
            {
                return this.Parent == null ? 0 : this.Parent.Level + 1;
            }
        }

        /// <summary>
        /// Return Left Node in Tree
        /// </summary>
        public TreeNode Left
        {
            get
            {
                if (this.Parent == null || this.Parent.Children.Count == 1)
                    return null;

                int n = this.Parent.Children.IndexOf(this);
                if (n == 0)
                    return this.Parent.Children[this.Parent.Children.Count - 1];
                else
                    return this.Parent.Children[n - 1];
            }
        }

        /// <summary>
        /// Return Right Node in Tree
        /// </summary>
        public TreeNode Right
        {
            get
            {
                if (this.Parent == null || this.Parent.Children.Count == 1)
                    return null;

                int n = this.Parent.Children.IndexOf(this);
                if (n == this.Parent.Children.Count - 1)
                    return this.Parent.Children[0];
                else
                    return this.Parent.Children[n + 1];
            }
        }

        /// <summary>
        /// Return how many Left Nodes in Tree are withut subnode
        /// </summary>
        public int LeftFree
        {
            get
            {
                if (this.Parent == null || this.Parent.Children.Count == 1 || this.Children.Count < 2)
                    return 0;

                int result = 0;
                TreeNode next = this.Left;

                for (int i = 0; i < this.Parent.Children.Count / 2; i++)
                {
                    if (next.Children.Count == 0)
                        result++;
                    else if (next.Children.Count == 1)
                    {
                        result++;
                        break;
                    }
                    else
                        break;
                    next = next.Left;
                }

                return result;
            }
        }

        /// <summary>
        /// Return how many Right Nodes in Tree are withut subnode
        /// </summary>
        public int RightFree
        {
            get
            {
                if (this.Parent == null || this.Parent.Children.Count == 1 || this.Children.Count < 2)
                    return 0;

                int result = 0;
                TreeNode next = this.Right;

                for (int i = 0; i < this.Parent.Children.Count / 2; i++)
                {
                    if (next.Children.Count == 0)
                        result++;
                    else if (next.Children.Count == 1)
                    {
                        result++;
                        break;
                    }
                    else
                        break;
                    next = next.Right;
                }

                return result;
            }
        }

        /// <summary>
        /// Return SubRadius
        /// </summary>
        public Double SubRadius { get { return _subRadius; } set { _subRadius = value; } }

        public int Count
        {
            get
            {
                    return this.Children.Sum(c => c.Count) +1;
            }
        }

        /// <summary>
        /// Construct Tree Node
        /// </summary>
        /// <param name="node">Root Node of the new Tree</param>
        public TreeNode(IDiagramNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "Node shouldn't be null");

            _children = new List<TreeNode>();
            _node = node;
        }

        /// <summary>
        /// Add child node
        /// </summary>
        /// <param name="node">Child node to add</param>
        /// <returns>Resulting tree node</returns>
        public TreeNode AddChild(IDiagramNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "Node shouldn't be null");

            TreeNode child = new TreeNode(node);
            this._children.Add(child);
            child.SetParent(this);
            return child;
        }

        /// <summary>
        /// Add child node
        /// </summary>
        /// <param name="node">Child node to add</param>
        public void AddChild(TreeNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "Node shouldn't be null");

            this._children.Add(node);
            node.SetParent(this);
        }

        /// <summary>
        /// Set parent to the node
        /// </summary>
        /// <param name="parent">Parent node</param>
        private void SetParent(TreeNode parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Returns flat list of nodes in the hierarchy starting from this node
        /// </summary>
        public IEnumerable<TreeNode> Flat
        {
            get
            {
                yield return this;
                foreach (TreeNode node in this._children)
                    foreach (TreeNode subNode in node.Flat)
                    {
                        yield return subNode;
                    }
            }
        }

    }
}
