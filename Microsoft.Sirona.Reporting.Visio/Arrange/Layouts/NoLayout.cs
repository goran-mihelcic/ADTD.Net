using System;
using System.Collections.Generic;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    /// <summary>
    /// Layout for simple Nodes
    /// </summary>
    public class NoLayout : LayoutBase, IDiagramLayout
    {
        public NoLayout(IContainer appliesTo)
        {
            _appliesTo = appliesTo;
        }

        /// <summary>
        /// Get layout Width (called by node)
        /// </summary>
        /// <param name="width">initial width</param>
        /// <returns>Width</returns>
        public override Double GetWidth(Double width)
        {
            if (AppliesTo.Nodes.Count() > 0)
                return AppliesTo.Nodes.Max(n => n.RightX) - AppliesTo.Nodes.Min(n => n.LeftX);
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
            if (AppliesTo.Nodes.Count() > 0)
                return AppliesTo.Nodes.Max(n => n.BottomY) - AppliesTo.Nodes.Min(n => n.TopY);
            else
                return height;
        }

        /// <summary>
        /// Set Layout parameters - empty
        /// </summary>
        /// <param name="parameters">Parameters</param>
        public override void SetParameters(Dictionary<LayoutParameters, object> properties) { }
    }
}
