using System;
using System.Collections.Generic;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    public class HierarchyLayout : LayoutBase, IDiagramLayout
    {
        internal Double _vSpace = 15;
        internal Double _hspace = 10;

        private HierarchyLayoutType _layoutType;
        private HierarchyMemberNode _root;
        private ShapeMatrix _matrix;

        /// <summary>
        /// Type of hierarchy layout
        /// </summary>
        public HierarchyLayoutType LayoutType { get { return _layoutType; } set { _layoutType = value; } }

        /// <summary>
        /// Hierarchy Root
        /// </summary>
        public HierarchyMemberNode Root { get { return _root; } set { _root = value; } }

        /// <summary>
        /// Most Left X coordinate of the layout
        /// </summary>
        public override double MinX { get { return Root.LeftX; } }

        /// <summary>
        /// Bottom of the layout
        /// </summary>
        public override double MinY { get { return Root.TopY; } }

        /// <summary>
        /// Most Right X coordinate of the layout
        /// </summary>
        public override double MaxX { get { return Root.RightX; } }

        /// <summary>
        /// Top of the Layout
        /// </summary>
        public override double MaxY { get { return Root.BottomY; } }

        /// <summary>
        /// Arrange node in the layout
        /// </summary>
        public override void Arrange()
        {
            List<IDiagramNode> nodes = Root.Nodes.ToList();

            foreach (DiagramNode node in nodes)
                node.Layout.Arrange();

            bool horizontalLayout =
                LayoutType == HierarchyLayoutType.HorizontalCenter ||
                LayoutType == HierarchyLayoutType.HorizontalLeft ;//||
                //LayoutType == HierarchyLayoutType.Alternate_HorizontalLeft_VerticalTop ||
                //LayoutType == HierarchyLayoutType.Alternate_HorizontalLeft_VerticalCenter;

            bool centralLayout =
                //LayoutType == HierarchyLayoutType.Alternate_HorizontalCenter_VerticalCenter ||
                //LayoutType == HierarchyLayoutType.Alternate_HorizontalCenter_VerticalTop ||
                //LayoutType == HierarchyLayoutType.Alternate_VerticalCenter_HorizontalCenter ||
                //LayoutType == HierarchyLayoutType.Alternate_VerticalCenter_HorizontalLeft ||
                LayoutType == HierarchyLayoutType.HorizontalCenter ||
                LayoutType == HierarchyLayoutType.VerticalCenter;

            _matrix.AddHierarchy(this.Root, horizontalLayout);

            if (centralLayout)
                _matrix.CenterHierarchy(this.Root, true);
            _matrix.RemoveEmptySpace();
            _matrix.SetAspect();

            SetNodePositions(this.Root);
        }

        /// <summary>
        /// Construct Layout 
        /// </summary>
        /// <param name="node">Root node</param>
        /// <param name="layoutType">Hierarchy layout Type</param>
        public HierarchyLayout(IContainer node, HierarchyLayoutType layoutType)
        {
            _appliesTo = node;
            _layoutType = layoutType;
            _matrix = new ShapeMatrix(true);
        }

        /// <summary>
        /// Returns layout Width
        /// </summary>
        /// <param name="width">initial width</param>
        /// <returns>Layout Width</returns>
        public override Double GetWidth(Double width)
        {
            return _matrix.GetWidth(width);
        }

        /// <summary>
        /// Return layout Height
        /// </summary>
        /// <param name="height">Initial Height</param>
        /// <returns>Layout Height</returns>
        public override Double GetHeight(Double height)
        {
            return _matrix.GetHeight(height);
        }

        /// <summary>
        /// Set Layout parameters
        /// </summary>
        /// <param name="properties">Parameters</param>
        public override void SetParameters(Dictionary<LayoutParameters, object> properties)
        {
            _matrix.SetParameters(properties);
        }

        /// <summary>
        /// Get Layout Parameters
        /// </summary>
        /// <returns>Layout Parameters</returns>
        public override Dictionary<LayoutParameters, object> GetParameters()
        {
            return _matrix.GetParameters();
        }

        private void SetNodePositions(HierarchyMemberNode node)
        {
            Int32 col = _matrix.GetHierarchyNodeColumn(node);
            Int32 row = _matrix.GetHierarchyNodeRow(node);

            if (node.HParent == null)
            {
                node.X = _matrix.GetColumnLeft(col) +_matrix.GetColumnWidth(col) / 2;
                node.Y = _matrix.GetRowTop(row) +_matrix.GetRowHeight(row) / 2;
            }
            else
            {
                HierarchyMemberNode parent = node.HParent;
                Int32 parentCol = _matrix.GetHierarchyNodeColumn(parent);
                Int32 parentRow = _matrix.GetHierarchyNodeRow(parent);

                node.X = _matrix.GetColumnLeft(col) - _matrix.GetColumnLeft(parentCol) + _matrix.GetColumnWidth(col) / 2;
                node.Y = _matrix.GetRowTop(row) - _matrix.GetRowTop(parentRow) + _matrix.GetRowHeight(row) / 2;
            }

            foreach (HierarchyMemberNode child in node.Children)
                SetNodePositions(child);
        }

        /// <summary>
        /// Returns Layer Layout for combined layouts
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        internal HierarchyLayoutType GetLayout(int layer)
        {
            HierarchyLayoutType layout = this.LayoutType;
            bool even = IsEven(layer);

            switch (layout)
            {
                //case HierarchyLayoutType.Alternate_HorizontalCenter_VerticalCenter:
                //    return even ? HierarchyLayoutType.HorizontalCenter : HierarchyLayoutType.VerticalCenter;
                //case HierarchyLayoutType.Alternate_HorizontalCenter_VerticalTop:
                //    return even ? HierarchyLayoutType.HorizontalCenter : HierarchyLayoutType.VerticalTop;
                //case HierarchyLayoutType.Alternate_HorizontalLeft_VerticalCenter:
                //    return even ? HierarchyLayoutType.HorizontalLeft : HierarchyLayoutType.VerticalCenter;
                //case HierarchyLayoutType.Alternate_HorizontalLeft_VerticalTop:
                //    return even ? HierarchyLayoutType.HorizontalLeft : HierarchyLayoutType.VerticalTop;
                //case HierarchyLayoutType.Alternate_VerticalCenter_HorizontalCenter:
                //    return even ? HierarchyLayoutType.VerticalCenter : HierarchyLayoutType.HorizontalCenter;
                //case HierarchyLayoutType.Alternate_VerticalCenter_HorizontalLeft:
                //    return even ? HierarchyLayoutType.VerticalCenter : HierarchyLayoutType.HorizontalLeft;
                //case HierarchyLayoutType.Alternate_VerticalTop_HorizontalCenter:
                //    return even ? HierarchyLayoutType.VerticalTop : HierarchyLayoutType.HorizontalCenter;
                //case HierarchyLayoutType.Alternate_VerticalTop_HorizontalLeft:
                //    return even ? HierarchyLayoutType.VerticalTop : HierarchyLayoutType.HorizontalLeft;
                default:
                    return layout;
            }
        }

        private bool IsEven(int value)
        {
            return value % 2 == 0;
        }

    }
}
