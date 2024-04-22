using System;

namespace Mihelcic.Net.Visio.Arrange
{
    public class DiagramEdge : IDiagramEdge
    {
        private Guid _id;
        private readonly string _name;
        private readonly IDiagramNode _from;
        private readonly IDiagramNode _to;
        private readonly bool _twoWay;
        private readonly bool _visible;

        /// <summary>
        /// Id connecting DiagramEdge with EdgeData
        /// </summary>
        public Guid Id => _id;

        /// <summary>
        /// Edge Name
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Diagram Node from which Edge starts
        /// </summary>
        public IDiagramNode From => _from;

        /// <summary>
        /// Diagram Node where Edge finishes
        /// </summary>
        public IDiagramNode To => _to;

        /// <summary>
        /// Is Edge TwoWay Edge
        /// </summary>
        public bool TwoWay => _twoWay;

        /// <summary>
        /// Is Edge visible
        /// </summary>
        public bool Visible => _visible;

        /// <summary>
        /// Edges presentation data
        /// </summary>
        public ShapePresentation Presentation { get; set; }


        /// <summary>
        /// Constructs One way Diagram Edge
        /// </summary>
        /// <param name="name">Edge Name</param>
        /// <param name="from">Node From</param>
        /// <param name="to">Node To</param>
        /// <param name="visible">Is line visible on the diagram</param>
        /// <param name="twoWay">Is connection in both directions</param>
        public DiagramEdge(string name, IDiagramNode from, IDiagramNode to, bool visible = true, bool twoWay = true)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Name shouldn't be null");
            _id = Guid.NewGuid();
            _name = name;
            _from = from ?? throw new ArgumentNullException("from", "From node shouldn't be null");
            _to = to ?? throw new ArgumentNullException("to", "To node shouldn't be null");
            _twoWay = twoWay;
            _visible = visible;
        }

        /// <summary>
        /// Check if two Edges are connecting same Nodes
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool Equals(IDiagramEdge edge)
        {
            return this._from == edge.From && this._to == edge.To;
        }
    }
}
