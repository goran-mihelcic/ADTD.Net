using System;
using System.Collections.Generic;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    /// <summary>
    /// Matrix diagram Layout
    /// </summary>
    public class MatrixLayout : LayoutBase, IDiagramLayout
    {
        private ShapeMatrix _matrix;

        /// <summary>
        /// Place shapes on their positions
        /// </summary>
        public override void Arrange()
        {
            List<IDiagramNode> nodes = AppliesTo.Nodes.ToList();

            foreach (DiagramNode node in nodes)
                node.Layout.Arrange();

            _matrix.AddShapes(nodes);
            SetNodePositions();
        }

        public MatrixLayout(IContainer node)
        {
            _appliesTo = node;
            _matrix = new ShapeMatrix();
        }

        /// <summary>
        /// Get layout Width (called by node)
        /// </summary>
        /// <param name="width">initial width</param>
        /// <returns>Width</returns>
        public override Double GetWidth(Double width)
        {
            return _matrix.GetWidth(width);
        }

        /// <summary>
        /// Get layout Height (called by node)
        /// </summary>
        /// <param name="height">initial height</param>
        /// <returns>Height</returns>
        public override Double GetHeight(Double height)
        {
            return _matrix.GetHeight(height);
        }

        /// <summary>
        /// Set Layout parameters
        /// </summary>
        /// <param name="parameters">Parameters</param>
        public override void SetParameters(Dictionary<LayoutParameters, object> properties)
        {
            _matrix.SetParameters(properties);
        }

        /// <summary>
        /// Get Layout Parameters
        /// </summary>
        /// <returns>Parameters</returns>
        public override Dictionary<LayoutParameters, object> GetParameters()
        {
            return _matrix.GetParameters();
        }

        private void SetNodePositions()
        {
            foreach (Int32 rownum in _matrix.Rows.Keys)
            {
                foreach (Int32 column in _matrix.Rows[rownum].Keys)
                {
                    IDiagramNode node = _matrix.Rows[rownum][column];
                    node.X = _matrix.GetColumnLeft(column) + _matrix.GetColumnWidth(column) / 2;
                    node.Y = _matrix.GetRowTop(rownum) + _matrix.GetRowHeight(rownum) / 2;
                }
            }
        }
    }
}
