using System;
using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Arrange
{
    public class DiagramNode : IDiagramNode, IContainer
    {
        private Guid _id;
        private string _name;
        private Double _x = 0;
        private Double _y = 0;
        private List<IDiagramEdge> _edges;
        private List<IDiagramNode> _nodes;
        private Double _size;
        private Double _width;
        private Double _height;
        private IDiagramNode _parent;
        private ShapePresentation _presentation;
        private IDiagramLayout _layout;
        private IContainer _container;
        private bool _pending;

        /// <summary>
        /// Id connecting DiagramNode with NodeData
        /// </summary>
        public Guid Id { get { return _id; } set { _id = value; } }

        /// <summary>
        /// Diagram Node Name
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Node X coordinate
        /// </summary>
        public Double X
        {
            get { return _container.Layout.GetX(_x); }
            set
            {
                if (Math.Abs(_x - value) > 1)
                {
                    _x = value;
                }
            }
        }

        /// <summary>
        /// Node Y coordinate
        /// </summary>
        public Double Y
        {
            get { return _container.Layout.GetY(_y); }
            set
            {
                if (Math.Abs(_y - value) > 1)
                {
                    _y = value;
                }
            }
        }

        /// <summary>
        /// Right X coordinate of the shape
        /// </summary>
        public virtual Double RightX { get { return this.X + this.ShapeWidth / 2; } }

        /// <summary>
        /// Bottom Y coordinate of the shape
        /// </summary>
        public virtual Double BottomY { get { return this.Y + this.ShapeHeight / 2; } }

        /// <summary>
        /// Left X coordinate of the shape
        /// </summary>
        public virtual Double LeftX { get { return this.X - this.ShapeWidth / 2; } }

        /// <summary>
        /// Top Y coordinate of the shape
        /// </summary>
        public virtual Double TopY { get { return this.Y - this.ShapeHeight / 2; } }

        /// <summary>
        /// Shape size from center
        /// </summary>
        public Double Size { get { return Math.Max(Width, Height); } set { _size = value; } }

        /// <summary>
        /// Shape Width (in total)
        /// </summary>
        public virtual Double Width { get { return _layout.GetWidth(_width); } set { _width = value; } }

        /// <summary>
        /// Shape Hight (in total)
        /// </summary>
        public virtual Double Height { get { return _layout.GetHeight(_height); } set { _height = value; } }

        /// <summary>
        /// Shape Width (only this shape)
        /// </summary>
        public Double ShapeWidth { get { return Layout.GetShapeWidth(_width); } }

        /// <summary>
        /// Shape Height (only this shape)
        /// </summary>
        public Double ShapeHeight { get { return Layout.GetShapeHeight(_height); } }

        /// <summary>
        /// Shapes contained in this shape
        /// </summary>
        public IEnumerable<IDiagramNode> Nodes { get { return _nodes; } }

        /// <summary>
        /// Edges connecting the shape
        /// </summary>
        public IEnumerable<IDiagramEdge> Edges { get { return _edges; } }

        /// <summary>
        /// Parent node where shape is placed
        /// </summary>
        public IDiagramNode Parent { get { return _parent; } set { _parent = value; } }

        /// <summary>
        /// Shapes presentation data
        /// </summary>
        public ShapePresentation Presentation { get { return _presentation; } set { _presentation = value; } }

        /// <summary>
        /// Shapes internal layout
        /// </summary>
        public IDiagramLayout Layout { get { return _layout; } set { _layout = value; } }

        /// <summary>
        /// Container where shape was placed
        /// </summary>
        public IContainer Container { get { return _container; } }

        /// <summary>
        /// Is shape is pending arranging
        /// </summary>
        public bool Pending { get { return _pending; } set { _pending = value; } }

        /// <summary>
        /// Shape's layer in hierarchy
        /// </summary>
        public Int32 Layer
        {
            get
            {
                if (this.Parent == null)
                    return 0;
                else
                    return this.Parent.Layer + 1;
            }
        }

        /// <summary>
        /// Constructs new Diagram Node
        /// </summary>
        /// <param name="name">Node Name</param>
        /// <param name="container">Node Container (parent)</param>
        /// <param name="addToParent">Add node to his parent</param>
        private DiagramNode(string name, IContainer container, bool addToParent)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Name shouldn't be empty");

            if (container == null)
                throw new ArgumentNullException("container", "Container shouldn't be null");

            _id = Guid.NewGuid();
            _name = name;
            _container = container;
            _pending = false;
            
            IDiagramNode parent = container as IDiagramNode;

            if (addToParent)
                if (parent != null)
                    parent.AddNode(this);
                else
                    _parent = null;

            _nodes = new List<IDiagramNode>();
            _edges = new List<IDiagramEdge>();
        }

        /// <summary>
        /// Constructs new Diagram Node
        /// </summary>
        /// <param name="name">Node Name</param>
        /// <param name="container">Node Container (parent)</param>
        /// <param name="layout">Layout to apply</param>
        /// <param name="addToParent">Add node to his parent</param>
        public DiagramNode(string name, IContainer container, LayoutType layout, bool addToParent)
            : this(name, container, addToParent)
        {
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
        /// Constructs new Diagram Node
        /// </summary>
        /// <param name="name">Node Name</param>
        /// <param name="container">Node Container (parent)</param>
        /// <param name="hLayout">Layout to apply</param>
        public DiagramNode(string name, IContainer container, HierarchyLayoutType hLayout)
            : this(name, container, true)
        {
            _layout = new HierarchyLayout(this, hLayout);
        }

        /// <summary>
        /// Add node inside this node
        /// </summary>
        /// <param name="node">Node to add</param>
        public void AddNode(IDiagramNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "Node shouldn't be null");

            _nodes.Add(node);
            node.Parent = this;
        }

        /// <summary>
        /// Add edge to the list connecting edges
        /// </summary>
        /// <param name="edge">Edge to add</param>
        public void AddEdge(IDiagramEdge edge)
        {
            if (edge.From == this || edge.To == this || !edge.Visible)
                _edges.Add(edge);
            else
                throw new ArgumentException("Edge should connect node where it is added", "edge");
        }

        /// <summary>
        /// Distance from the other node
        /// </summary>
        /// <param name="node">Node NameU</param>
        /// <returns>Distance</returns>
        public Double Distance(IDiagramNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "Node shuld not be null");

            return (new Vector(this.X, this.Y, node.X, node.Y)).Magnitude;
        }
    }
}
