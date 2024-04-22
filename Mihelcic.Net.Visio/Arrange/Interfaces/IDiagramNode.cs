using System;
using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Arrange
{
    public interface IDiagramNode
    {
        Guid Id { get; }
        string Name { get; }
        Double X { get; set; }
        Double Y { get; set; }
        Double Size { get; set; }
        Double Width { get; set; }
        Double Height { get; set; }
        Double ShapeWidth { get; }
        Double ShapeHeight { get; }
        Double RightX { get; }
        Double BottomY { get; }
        Double LeftX { get; }
        Double TopY { get; }
        IEnumerable<IDiagramNode> Nodes { get; }
        IEnumerable<IDiagramEdge> Edges { get; }
        IDiagramNode Parent { get; set; }
        IContainer Container { get; }
        IDiagramLayout Layout { get; set; }
        bool Pending { get; set; }
        Int32 Layer { get; }
        void AddNode(IDiagramNode node);
        void AddEdge(IDiagramEdge edge);
        Double Distance(IDiagramNode node);
    }
}
