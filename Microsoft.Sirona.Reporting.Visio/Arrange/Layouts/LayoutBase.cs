using System;
using System.Collections.Generic;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    /// <summary>
    /// Diagram Layout Object
    /// </summary>
    public class LayoutBase : IDiagramLayout
    {
        internal Double _leftMargin = 0;
        internal Double _bottomMargin = 0;
        internal Double _rightmargin = 0;
        internal Double _topMargin = 0;

        internal Double _attraction_Factor = 100;
        internal Double _repulsion_Factor = 90;
        internal Double _repulsion_Factor_nonconnected = 5;
        internal Double _initial_Length = 300;

        internal IContainer _appliesTo;

        private IDiagramNode _parentNode { get { return _appliesTo as IDiagramNode; } }

        private HierarchyMemberNode _asHierarchyNode { get { return this._appliesTo as HierarchyMemberNode; } }
        internal Double _parentLeftX
        {
            get
            {
                if (_parentNode == null)
                    return 0;
                else
                    return _parentNode.LeftX;
            }
        }

        internal Double _parentTopY
        {
            get
            {
                if (_parentNode == null)
                    return 0;
                else
                    return _parentNode.TopY;
            }
        }

        internal Double _parentRightX
        {
            get
            {
                if (_parentNode == null)
                    return 0;
                else
                    return _parentNode.RightX;
            }
        }

        internal Double _parentBottomY
        {
            get
            {
                if (_parentNode == null)
                    return 0;
                else
                    return _parentNode.BottomY;
            }
        }

        /// <summary>
        /// Container Layout applies to
        /// </summary>
        public IContainer AppliesTo { get { return _appliesTo; } }

        /// <summary>
        /// Position nodes in the layout
        /// </summary>
        public virtual void Arrange()
        {
            foreach (IDiagramNode node in AppliesTo.Nodes)
                node.Layout.Arrange();
        }

        /// <summary>
        /// Layout's left edge
        /// </summary>
        public virtual double MinX
        {
            get
            {
                if (_asHierarchyNode == null)
                    if (AppliesTo.Nodes.Count() == 0)
                        return _parentLeftX;
                    else
                        return _parentLeftX + AppliesTo.Nodes.Min(n => n.LeftX);
                else
                {
                    double minX_Nodes = AppliesTo.Nodes.Count() == 0 ? _parentLeftX : _parentLeftX + AppliesTo.Nodes.Min(n => n.LeftX);
                    double minX_Children = _asHierarchyNode.Children.Count() == 0 ? _parentLeftX : _parentLeftX + _asHierarchyNode.Children.Min(n => n.LeftX);
                    return Math.Min(minX_Nodes, minX_Children);
                }
            }
        }

        /// <summary>
        /// Layout's Bottom edge
        /// </summary>
        public virtual double MinY
        {
            get
            {
                if (_asHierarchyNode == null)
                    if (AppliesTo.Nodes.Count() == 0)
                        return _parentTopY;
                    else
                        return _parentTopY + AppliesTo.Nodes.Min(n => n.TopY);
                else
                {
                    double minX_Nodes = AppliesTo.Nodes.Count() == 0 ? _parentTopY : _parentTopY + AppliesTo.Nodes.Min(n => n.TopY);
                    double minX_Children = _asHierarchyNode.Children.Count() == 0 ? _parentTopY : _parentTopY + _asHierarchyNode.Children.Min(n => n.TopY);
                    return Math.Min(minX_Nodes, minX_Children);
                }
            }
        }

        /// <summary>
        /// Layout's Right edge
        /// </summary>
        public virtual double MaxX
        {
            get
            {
                if (_asHierarchyNode == null)
                    if (AppliesTo.Nodes.Count() == 0)
                        return _parentRightX;
                    else
                        return _parentRightX + AppliesTo.Nodes.Max(n => n.RightX);
                else
                {
                    double minX_Nodes = AppliesTo.Nodes.Count() == 0 ? _parentRightX : _parentRightX + AppliesTo.Nodes.Max(n => n.RightX);
                    double minX_Children = _asHierarchyNode.Children.Count() == 0 ? _parentRightX : _parentRightX + _asHierarchyNode.Children.Max(n => n.RightX);
                    return Math.Max(minX_Nodes, minX_Children);
                }
            }
        }

        /// <summary>
        /// Layout's Top edge
        /// </summary>
        public virtual double MaxY
        {
            get
            {
                if (_asHierarchyNode == null)
                    if (AppliesTo.Nodes.Count() == 0)
                        return _parentBottomY;
                    else
                        return _parentBottomY + AppliesTo.Nodes.Max(n => n.BottomY);
                else
                {
                    double minX_Nodes = AppliesTo.Nodes.Count() == 0 ? _parentBottomY : _parentBottomY + AppliesTo.Nodes.Max(n => n.BottomY);
                    double minX_Children = _asHierarchyNode.Children.Count() == 0 ? _parentBottomY : _parentBottomY + _asHierarchyNode.Children.Max(n => n.BottomY);
                    return Math.Max(minX_Nodes, minX_Children);
                }
            }
        }

        /// <summary>
        /// Get shape's X coordinate (called by node)
        /// </summary>
        /// <param name="x">input x</param>
        /// <returns>X coordinate</returns>
        public virtual double GetX(double x)
        {
            return x;
        }

        /// <summary>
        /// Get shape's Y Coordinate (called by node)
        /// </summary>
        /// <param name="y">input y</param>
        /// <returns>Y Coordinate</returns>
        public virtual double GetY(double y)
        {
            return y;
        }

        /// <summary>
        /// Get layout Width (called by node)
        /// </summary>
        /// <param name="width">initial width</param>
        /// <returns>Width</returns>
        public virtual double GetWidth(double width)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get layout Height (called by node)
        /// </summary>
        /// <param name="height">initial height</param>
        /// <returns>Height</returns>
        public virtual double GetHeight(double height)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get shape Width (called by node)
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public virtual double GetShapeWidth(double width)
        {
            return GetWidth(width);
        }

        /// <summary>
        /// Get shape Width (called by node)
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public virtual double GetShapeHeight(double height)
        {
            return GetHeight(height);
        }

        /// <summary>
        /// Set Layout parameters
        /// </summary>
        /// <param name="parameters">Parameters</param>
        public virtual void SetParameters(Dictionary<LayoutParameters, object> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters", "Parameters shouldn't be null");

            if (parameters.ContainsKey(LayoutParameters.LeftMargin))
                _leftMargin = Convert.ToDouble(parameters[LayoutParameters.LeftMargin]);
            if (parameters.ContainsKey(LayoutParameters.RightMargin))
                _rightmargin = Convert.ToDouble(parameters[LayoutParameters.RightMargin]);
            if (parameters.ContainsKey(LayoutParameters.TopMargin))
                _topMargin = Convert.ToDouble(parameters[LayoutParameters.TopMargin]);
            if (parameters.ContainsKey(LayoutParameters.BottomMargin))
                _bottomMargin = Convert.ToDouble(parameters[LayoutParameters.BottomMargin]);

            if (parameters.ContainsKey(LayoutParameters.AttractionFactor))
                _attraction_Factor = Convert.ToDouble(parameters[LayoutParameters.AttractionFactor]);
            if (parameters.ContainsKey(LayoutParameters.RepulsionFactor))
                _repulsion_Factor = Convert.ToDouble(parameters[LayoutParameters.RepulsionFactor]);
            if (parameters.ContainsKey(LayoutParameters.InitialLength))
                _initial_Length = Convert.ToDouble(parameters[LayoutParameters.InitialLength]);
        }

        /// <summary>
        /// Get Layout Parameters
        /// </summary>
        /// <returns>Parameters</returns>
        public virtual Dictionary<LayoutParameters, object> GetParameters()
        {
            Dictionary<LayoutParameters, object> parameters = new Dictionary<LayoutParameters, object>();

            parameters.Add(LayoutParameters.LeftMargin, _leftMargin);
            parameters.Add(LayoutParameters.RightMargin, _rightmargin);
            parameters.Add(LayoutParameters.TopMargin, _topMargin);
            parameters.Add(LayoutParameters.BottomMargin, _bottomMargin);

            parameters.Add(LayoutParameters.AttractionFactor, _attraction_Factor);
            parameters.Add(LayoutParameters.RepulsionFactor, _repulsion_Factor);
            parameters.Add(LayoutParameters.InitialLength, _initial_Length);


            return parameters;
        }
    }
}
