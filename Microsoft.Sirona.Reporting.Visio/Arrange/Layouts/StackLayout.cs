using System;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    public class StackLayout :LayoutBase, IDiagramLayout
    {
        private Double _vSpace = 17.5;

        /// <summary>
        /// Place shapes on their positions
        /// </summary>
        public override void Arrange()
        {
            foreach (DiagramNode node in AppliesTo.Nodes)
                node.Layout.Arrange();

            int i = 0;
            foreach (IDiagramNode node in AppliesTo.Nodes)
                SetNodePosition(i++, node);
        }

        public StackLayout(IContainer node)
        {
            _appliesTo = node;
        }

        /// <summary>
        /// Get layout Width (called by node)
        /// </summary>
        /// <param name="width">initial width</param>
        /// <returns>Width</returns>
        public override Double GetWidth(Double width)
        {
            IDiagramNode mostRight = AppliesTo.Nodes.FirstOrDefault(n => n.X + n.Width == AppliesTo.Nodes.Max(m => m.X + m.Width));

            if (mostRight != null)
                return mostRight.X + mostRight.Width / 2 + _rightmargin;
            else
                return width;
        }

        /// <summary>
        /// Get layout Height (called by node)
        /// </summary>
        /// <param name="height">initial height</param>
        /// <returns>Height</returns>
        public override Double GetHeight(Double height)
        {
            IDiagramNode mostDown = AppliesTo.Nodes.FirstOrDefault(n => n.Y + n.Height == AppliesTo.Nodes.Max(m => m.Y + m.Height));

            if (mostDown != null)
                return mostDown.Y + mostDown.Height / 2 + _topMargin;
            else
                return height;
        }

        private void SetNodePosition(int i, IDiagramNode node)
        {
            node.X = Convert.ToInt32(_leftMargin) + node.Width / 2;

            Double prevY = 0;
            if (i > 0)
                prevY = node.Parent.Nodes.ToList()[i - 1].BottomY;

            node.Y = Convert.ToInt32(prevY + _vSpace + node.Height / 2);
        }
    }
}
