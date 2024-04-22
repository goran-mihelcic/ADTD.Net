using System;
using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Arrange
{
    /// <summary>
    /// Node that is hierarchy member
    /// </summary>
    public class HierarchyMemberNode : DiagramNode
    {
        private IDiagramNode _hierarchy;
        private HierarchyMemberNode _hParent;
        private List<HierarchyMemberNode> _children;

        /// <summary>
        /// Node that represents hierarchy
        /// </summary>
        public IDiagramNode Hierarchy { get { return _hierarchy; } }

        /// <summary>
        /// Hierarchy Layout
        /// </summary>
        public HierarchyLayout ArrangingLayout { get { return _hierarchy.Layout as HierarchyLayout; } }

        /// <summary>
        /// Hierarchy Parent node. Null if root
        /// </summary>
        public HierarchyMemberNode HParent { get { return _hParent; } }

        /// <summary>
        /// Node's children
        /// </summary>
        public List<HierarchyMemberNode> Children { get { return _children; } }

        /// <summary>
        /// Hierarchy Layout object
        /// </summary>
        public HierarchyLayoutType HLayout { get { return ArrangingLayout.GetLayout(this.HLayer); } }

        public int HLayer
        {
            get
            {
                if (this.HParent == null)
                    return 0;
                else
                    return this.HParent.HLayer + 1;
            }
        }

        public HierarchyMemberNode(string name, IContainer container, LayoutType layout, IDiagramNode hierarchy)
            : base(name, container, layout, false)
        {
            if (hierarchy == null)
                throw new ArgumentNullException("hierarchy", "You should supply Hierarchy Node");

            HierarchyLayout hierarchyLayout = hierarchy.Layout as HierarchyLayout;

            if (hierarchyLayout == null)
                throw new ArgumentException("Node provided isn't hierarchy root", "hierarchy");

            _hierarchy = hierarchy;
            _children = new List<HierarchyMemberNode>();
        }

        public HierarchyMemberNode(string name, IContainer container, LayoutType layout, HierarchyMemberNode parent)
            : this(name, container, layout, parent.Hierarchy)
        {
            _hParent = parent;
            parent.Children.Add(this);
        }
    }
}
