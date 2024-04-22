using System;

namespace Mihelcic.Net.Visio.Arrange
{
    public interface IDiagramEdge
    {
        Guid Id { get; }
        string Name { get; }
        IDiagramNode From { get; }
        IDiagramNode To { get; }
        bool TwoWay { get; }
        bool Visible { get; }
    }
}
