using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Arrange
{
    public interface IContainer
    {
        IEnumerable<IDiagramNode> Nodes { get; }
        IDiagramLayout Layout { get; }
    }
}
