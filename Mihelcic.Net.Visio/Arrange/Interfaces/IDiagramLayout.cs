using System;
using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Arrange
{
    public interface IDiagramLayout
    {
        IContainer AppliesTo { get; }
        void Arrange();
        Double MinX { get; }
        Double MinY { get; }
        Double MaxX { get; }
        Double MaxY { get; }
        Double GetX(Double x);
        Double GetY(Double y);
        Double GetWidth(Double width);
        Double GetHeight(Double height);
        Double GetShapeWidth(Double width);
        Double GetShapeHeight(Double height);
        void SetParameters(Dictionary<LayoutParameters, object> properties);
        Dictionary<LayoutParameters, object> GetParameters();
    }
}
