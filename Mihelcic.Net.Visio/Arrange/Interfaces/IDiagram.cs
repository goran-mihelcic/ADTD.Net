using System;
using System.Collections.Generic;
namespace Mihelcic.Net.Visio.Arrange
{
    public interface IDiagram
    {
        IEnumerable<IDiagramNode> Nodes { get; }
        IEnumerable<IDiagramEdge> Edges { get; }
        Double Width { get; set; }
        Double Height { get; set; }
        IEnumerable<IDiagramNode> AllNodes { get; }
        IDiagramLayout Layout { get; }
        void Arrange();
    }
}
