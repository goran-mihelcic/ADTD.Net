using System;
using System.Collections.Generic;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    /// <summary>
    /// Represents Diagram
    /// </summary>
    public class Diagram : IDiagram, IContainer
    {
        private readonly List<IDiagramNode> _nodes;
        /// <summary>
        /// Diagram Node list
        /// </summary>
        public IEnumerable<IDiagramNode> Nodes { get { return _nodes; } }

        private readonly List<IDiagramNode> _allNodes;
        /// <summary>
        /// Diagram Node list
        /// </summary>
        public IEnumerable<IDiagramNode> AllNodes { get { return _allNodes; } }

        private readonly List<IDiagramEdge> _edges;
        /// <summary>
        /// Diagram Edges (Connections)
        /// </summary>
        public IEnumerable<IDiagramEdge> Edges => _edges;

        private Double _width;
        /// <summary>
        /// Diagram overall width with margins
        /// </summary>
        public Double Width { get { return Layout.GetWidth(_width); } set { _width = value; } }

        private Double _height;
        /// <summary>
        /// Diagram overall height with margins
        /// </summary>
        public Double Height { get { return Layout.GetHeight(_height); } set { _height = value; } }


        private readonly IDiagramLayout _layout;
        /// <summary>
        /// Basic layout of the diagram
        /// </summary>
        public IDiagramLayout Layout => _layout;


        /// <summary>
        /// Constructs diagram class
        /// </summary>
        /// <param name="layout">Diagram basic layout</param>
        public Diagram(LayoutType layout)
        {
            _nodes = new List<IDiagramNode>();
            _edges = new List<IDiagramEdge>();
            _allNodes = new List<IDiagramNode>();

            switch (layout)
            {
                case LayoutType.Web:
                    _layout = new WebLayout(this);
                    break;
                case LayoutType.Matrix:
                    _layout = new MatrixLayout(this);
                    break;
                case LayoutType.Stack:
                    _layout = new StackLayout(this);
                    break;
                default:
                    _layout = new NoLayout(this);
                    break;
            }
        }

        /// <summary>
        /// Start arrangement process
        /// </summary>
        public void Arrange()
        {
            this.Layout.Arrange();
        }

        /// <summary>
        /// Find node by name in the diagram
        /// </summary>
        /// <param name="name">Node name</param>
        /// <returns>Node found or null</returns>
        public IDiagramNode GetNodeByName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Node name we search shouldn't be empty");

            return _allNodes.FirstOrDefault(n => n.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Add IDiagramNode to the Nodes property of the diagram
        /// </summary>
        /// <param name="node">Node to add</param>
        public void AddNode(IDiagramNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "Node shouldn't be null");
            this._nodes.Add(node);
        }

        /// <summary>
        /// Create and Add Node to diagram hierarchy
        /// </summary>
        /// <param name="name">Node name to create and add</param>
        /// <param name="parent">Node's parent</param>
        /// <param name="layout">Node's layout</param>
        /// <returns>Node created and added to the diagram</returns>
        public IDiagramNode AddDiagramNode(string name, IDiagramNode parent, LayoutType layout)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Node name shouldn't be empty");

            // Check if parent is IDiagramNode or Diagram itself
            IContainer container;
            if (parent == null)
                container = this;
            else
                container = parent as IContainer;

            // Create new node and add it to the all nodes list
            DiagramNode newNode = new DiagramNode(name, container, layout, true);
            _allNodes.Add(newNode);

            // Add node to the hierarchy
            if (parent == null)
                _nodes.Add(newNode);

            return newNode;
        }

        /// <summary>
        /// Create and Add Node to diagram hierarchy and populate it's presentation properties
        /// </summary>
        /// <param name="name">Node name to create and add</param>
        /// <param name="parent">Node's parent</param>
        /// <param name="layout">Node's layout</param>
        /// <param name="presentationData">Node's presentation properties</param>
        /// <returns>Node created and added to the diagram</returns>
        public IDiagramNode AddDiagramNode(string name, IDiagramNode parent, LayoutType layout, ShapePresentation presentationData)
        {
            DiagramNode newNode = AddDiagramNode(name, parent, layout) as DiagramNode;
            newNode.Presentation = presentationData;
            return newNode;
        }

        /// <summary>
        /// Create and Add Node to diagram hierarchy, populate it's presentation properties and set node size
        /// </summary>
        /// <param name="name">Node name to create and add</param>
        /// <param name="parent">Node's parent</param>
        /// <param name="layout">Node's layout</param>
        /// <param name="presentationData">Node's presentation properties</param>
        /// <param name="width">Node width</param>
        /// <param name="height">Node Height</param>
        /// <returns>Node created and added to the diagram</returns>
        public IDiagramNode AddDiagramNode(string name, IDiagramNode parent, LayoutType layout, ShapePresentation presentationData, Double width, double height)
        {
            DiagramNode newNode = AddDiagramNode(name, parent, layout, presentationData) as DiagramNode;
            newNode.Width = width;
            newNode.Height = height;
            return newNode;
        }

        /// <summary>
        /// Add IDiagramNode to the AllNodes property of the diagram
        /// </summary>
        /// <param name="node">Node to add</param>
        public void AddDiagramNode(IDiagramNode node)
        {
            this._allNodes.Add(node);
        }

        /// <summary>
        /// Add Hierarchy to the Diagram
        /// </summary>
        /// <param name="name">Hierarchy name to create and add</param>
        /// <param name="parent">Hierarchy parent</param>
        /// <param name="layout">Hierarchy layout</param>
        /// <param name="width">Hierarchy width</param>
        /// <param name="height">Hierarchy height</param>
        /// <returns>Node representing hierarchy</returns>
        public IDiagramNode AddHierarchy(string name, IDiagramNode parent, HierarchyLayoutType layout, Double width, double height)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Node name shouldn't be empty");
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("parent", "Parent shouldn't be empty");

            IContainer container;
            if (parent == null)
                container = this;
            else
                container = parent as IContainer;

            DiagramNode newNode = new DiagramNode(name, container, layout);
            _allNodes.Add(newNode);

            // Add node to the hierarchy
            if (parent == null)
                _nodes.Add(newNode);

            newNode.Width = width;
            newNode.Height = height;

            return newNode;
        }

        /// <summary>
        /// Create new Edge and connect two nodes
        /// </summary>
        /// <param name="name">Edge name to create</param>
        /// <param name="fromNode">Connection from the node</param>
        /// <param name="toNode">Connection to node</param>
        /// <param name="twoWay">Two way connection</param>
        public DiagramEdge Connect(string name, IDiagramNode fromNode, IDiagramNode toNode, bool twoWay = true)
        {
            if (fromNode == null)
                throw new ArgumentNullException("fromNode", "From node shouldn't be null");
            if (toNode == null)
                throw new ArgumentNullException("toNode", "To node shouldn't be null");
            if (!this.AllNodes.Contains(fromNode))
                throw new ArgumentOutOfRangeException("fromNode", "To connect from node should be included into the diagram");
            if (!this.AllNodes.Contains(toNode))
                throw new ArgumentOutOfRangeException("toNode", "To connect TO node should be included into the diagram");

            DiagramEdge edge = new DiagramEdge(name, fromNode, toNode, true, twoWay);

            if (!(fromNode.Edges.Any(e => e.From == toNode) || fromNode.Edges.Any(e => e.To == toNode)))
            {
                toNode.AddEdge(edge);
                fromNode.AddEdge(edge);
                _edges.Add(edge);

                DiagramEdge hiddenEdge = GhostConnection(fromNode, toNode);
                if (hiddenEdge != null)
                {
                    hiddenEdge.From.AddEdge(hiddenEdge);
                    hiddenEdge.To.AddEdge(hiddenEdge);
                    _edges.Add(hiddenEdge);
                }
            }

            return edge;
        }

        /// <summary>
        /// Create invisible connection between two nodes
        /// This is used to give arranging algorithm knowledge of
        /// transitive connections
        /// </summary>
        /// <param name="fromNode">Connection from the node</param>
        /// <param name="toNode">Connection to node</param>
        /// <returns>Object representing invisible connection between two nodes</returns>
        private DiagramEdge GhostConnection(IDiagramNode fromNode, IDiagramNode toNode)
        {
            if (fromNode.Parent != toNode.Parent)
            {
                IDiagramNode common = CommonRoot(fromNode, toNode);

                if (common == fromNode || common == toNode)
                    return null;

                IDiagramNode n1 = GetFirstChildForCommonRoot(fromNode, common);
                IDiagramNode n2 = GetFirstChildForCommonRoot(toNode, common);

                if (!AreConnected(n1, n2))
                    return new DiagramEdge(Guid.NewGuid().ToString(), n1, n2, false, true);
            }

            return null;
        }

        /// <summary>
        /// Returns all (direct or transitive) parents of the node in the hierarchy
        /// </summary>
        /// <param name="node">Node to retrieve parents for</param>
        /// <returns>Set of parent nodes</returns>
        private static IEnumerable<IDiagramNode> GetParents(IDiagramNode node)
        {
            if (node.Parent != null)
            {
                yield return node;
                foreach (IDiagramNode pnode in GetParents(node.Parent))
                    yield return pnode;
            }
            else
                yield return node;
        }

        /// <summary>
        /// Returns true if two nodes are connected
        /// </summary>
        /// <param name="node1">First node</param>
        /// <param name="node2">second node</param>
        /// <returns>True if nodes are connected</returns>
        private bool AreConnected(IDiagramNode node1, IDiagramNode node2)
        {
            return this.Edges.Any(e => e.From == node1 && e.To == node2) || this.Edges.Any(e => e.From == node2 && e.To == node1);
        }

        /// <summary>
        /// Returns nearest common parent for two nodes
        /// </summary>
        /// <param name="node1">First node</param>
        /// <param name="node2">second node</param>
        /// <returns>Common parent node</returns>
        private static IDiagramNode CommonRoot(IDiagramNode node1, IDiagramNode node2)
        {
            IEnumerable<IDiagramNode> commonParents = GetParents(node1).Intersect(GetParents(node2));
            return commonParents.FirstOrDefault(n => !commonParents.Select(p => p.Parent).Contains(n));
        }

        /// <summary>
        /// Returns first downlevel node from the common parent on the path
        /// To the Node
        /// </summary>
        /// <param name="node">Node to test</param>
        /// <param name="root">Node's parent</param>
        /// <returns>Node representing first child</returns>
        private static IDiagramNode GetFirstChildForCommonRoot(IDiagramNode node, IDiagramNode root)
        {
            while (node.Parent != root && node.Parent != null)
                node = node.Parent;
            return node;
        }
    }
}
