using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    public class WebLayout : LayoutBase, IDiagramLayout
    {
        private const double INITIAL_DISTANCE = 0.3;
        private readonly double _sizeFactor = 110;

        private IEnumerable<IDiagramNode> _disconnected = new List<IDiagramNode>();

        /// <summary>
        /// Get layout Width (called by node)
        /// </summary>
        /// <param name="width">initial width</param>
        /// <returns>Width</returns>
        public override Double GetWidth(Double width)
        {
            if (this.AppliesTo.Nodes.Any())
            {
                return this.MaxX + _rightmargin;
            }
            return width;
        }

        /// <summary>
        /// Get layout Height (called by node)
        /// </summary>
        /// <param name="height">initial height</param>
        /// <returns>Height</returns>
        public override Double GetHeight(Double height)
        {
            if (this.AppliesTo.Nodes.Any())
            {
                return this.MaxY + _topMargin;
            }
            return height;
        }

        /// <summary>
        /// Arranges Diagram Group
        /// </summary>
        public override void Arrange()
        {
            foreach (DiagramNode node in AppliesTo.Nodes.Cast<DiagramNode>())
            {
                node.Layout.Arrange();
            }

            TreeNode tree = SetInitialPosition();

            Debug.WriteLine("+++ START arranging");

            DateTime startTime = DateTime.Now;

            Arrange(tree, false);
            Arrange(tree, true);

            Debug.WriteLine("+++ END arranging - Time: {0} seconds", (DateTime.Now - startTime).TotalSeconds);

            foreach (IDiagramNode node in _disconnected)
            {
                node.X = 0;
                node.Y = 0;
            }

            Debug.WriteLine("+++ START CenterLayout");
            CenterLayout();
            Debug.WriteLine("+++ END CenterLayout");

            double y = this.MaxY;
            double x = this.MaxX;
            foreach (IDiagramNode node in _disconnected)
            {
                node.X = x + 50;
                node.Y = y - node.Height - 50;
                y = node.Y;
            }
        }

        private void Arrange(TreeNode tree, bool extended)
        {
            WebLayout layout = AppliesTo.Layout as WebLayout;

            layout.ArrangeNode(tree.Node, extended, tree.SubRadius);
            foreach (TreeNode child in tree.Children)
            {
                Arrange(child, extended);
            }
        }

        /// <summary>
        /// Executes Node level arranging steps
        /// </summary>
        private void ArrangeNode(IDiagramNode node, bool extended, double sizeFactor)
        {
            Vector resultVector = Vector.Null;
            Vector previousMove = Vector.Null;
            Vector previousPosition = Vector.Null;
            IContainer container = node.Container;
            int i = 0;
            Vector attractionVector = Vector.Null;
            Vector repulsionVector = Vector.Null;
            Vector repulsionVector2 = Vector.Null;

            if (container != null)
                do
                {
                    IEnumerable<DiagramNode> connectedNodes = node.Edges.Select(e => e.To == node ? (DiagramNode)e.From : (DiagramNode)e.To);

                    IEnumerable<Vector> attractionVectors = node.Edges.Where(e => e.From == node)
                            .Select(e => AsAttraction(new Vector(node.X, node.Y, e.To.X, e.To.Y), sizeFactor))
                        .Union(node.Edges.Where(e => e.To == node)
                            .Select(e => AsAttraction(new Vector(node.X, node.Y, e.From.X, e.From.Y), sizeFactor)));

                    if (attractionVectors != null && attractionVectors.Count() > 0)
                    {
                        attractionVector = Vector.Sum(attractionVectors);
                    }

                    IEnumerable<Vector> repulsionVectors = node.Edges.Where(e => e.From == node)
                            .Select(e => AsRepulsion(new Vector(node.X, node.Y, e.To.X, e.To.Y), sizeFactor))
                        .Union(node.Edges.Where(e => e.To == node)
                            .Select(e => AsRepulsion(new Vector(node.X, node.Y, e.From.X, e.From.Y), sizeFactor)));

                    if (repulsionVectors != null && repulsionVectors.Count() > 0)
                    {
                        repulsionVector = Vector.Sum(repulsionVectors);
                    }

                    IEnumerable<Vector> repulsionVectors2 = null;

                    if (extended)
                    {
                        repulsionVectors2 = container.Nodes.Where(n => !connectedNodes.Contains(n) && n != node)
                            .Select(n => AsRepulsion2(new Vector(node.X, node.Y, n.X, n.Y), sizeFactor));
                        if (repulsionVectors2 != null && repulsionVectors2.Count() > 0)
                            repulsionVector2 = Vector.Sum(repulsionVectors2);
                    }

                    resultVector = repulsionVector + attractionVector + repulsionVector2;

                    Vector result = Vector.Null;

                    if ((Int32)resultVector.Magnitude == (int)previousMove.Magnitude)
                        break;
                    if (i > 0 && Convert.ToInt32(previousMove.Invert().Direction) == Convert.ToInt32(resultVector.Direction))
                    {
                        Debug.WriteLine("*** OVERSHOOT Magnitude: {0}", resultVector.Magnitude);
                        resultVector = new Vector(previousMove.Magnitude / 2, previousMove.Direction, true);
                        result = previousPosition + resultVector;
                    }
                    else
                    {
                        result = new Vector(node.X, node.Y) + new Vector(Math.Min(_sizeFactor / 50, resultVector.Magnitude), resultVector.Direction, true);
                    }
                    previousMove = resultVector;

                    node.X = result.X;
                    node.Y = result.Y;

                    i++;
                } while (resultVector.Magnitude > _sizeFactor / 256 && i < 1000);
            if (i > 1)
                Debug.WriteLine("*** Node: {0} - itterations: {1}", node.Name, i);
        }

        public WebLayout(IContainer container)
        {
            _appliesTo = container;
        }

        private Double _avgShapeSize { get { return this.AppliesTo.Nodes.Average(node => node.Size) * 2; } }

        private void MakeAvailable()
        {
            foreach (DiagramNode node in AppliesTo.Nodes.Cast<DiagramNode>())
            {
                node.Pending = true;
            }
        }

        /// <summary>
        /// Set Shapes initial positions before arranging them
        /// </summary>
        private TreeNode SetInitialPosition()
        {
            Debug.WriteLine("+++ START Reorder");
            TreeNode tree = Reorder();
            Debug.WriteLine("+++ END Reorder");

            if (tree != null)
            {
                tree.Node.X = 0;
                tree.Node.Y = 0;
                Debug.WriteLine("+++ START SetTreePosition");
                SetTreePosition(tree, 2 * Math.PI, 0, 0);
                Debug.WriteLine("+++ END SetTreePosition");
            }
            return tree;
        }

        private void SetTreePosition(TreeNode node, double alpha, double radius, double direction)
        {
            int n = node.Children.Count();
            if (n > 1)
            {
                if (node.Level > 0)
                {
                    double leftExtension = Math.Min(alpha * node.LeftFree, Math.PI / 3);
                    double rightExtension = Math.Min(alpha * node.RightFree, Math.PI / 3);
                    if (leftExtension > 0 || rightExtension > 0)
                    {
                        direction = direction - leftExtension + rightExtension;
                        alpha = Math.Min(alpha + leftExtension + rightExtension, Math.PI);
                    }
                }
                node.SubRadius = Math.Max(n * _avgShapeSize * INITIAL_DISTANCE / alpha, _initial_Length);

                double d = Math.Sin(alpha / 2) * radius;
                double gama = Math.Abs(Math.PI / 2 - alpha / 2);
                double gama2 = d > node.SubRadius ? 0 : Math.Acos(d / node.SubRadius);
                double beta = node.Level == 0 ? Math.PI * 2 : Math.Min((Math.PI - gama - gama2) * 2, Math.PI);
                int i = 0;
                double subbeta = beta / n;
                double startDirection = direction - (beta / 2) + (subbeta / 2);
                foreach (TreeNode subNode in node.Children)
                {
                    direction = startDirection + (subbeta * i);
                    Double magnitude = node.SubRadius;
                    if (subNode.Children.Count > 10)
                    {
                        subbeta = 2 * Math.PI / 3;
                        magnitude = Math.Max(subNode.Children.Count * _avgShapeSize * INITIAL_DISTANCE / subbeta, _initial_Length);
                    }
                    else if (subNode.Count > 1)
                        magnitude = node.SubRadius + _initial_Length;
                    Vector move = new Vector(magnitude, direction, true);
                    subNode.Node.X = node.Node.X + move.X;
                    subNode.Node.Y = node.Node.Y + move.Y;

                    SetTreePosition(subNode, subbeta, magnitude, direction);
                    i++;
                }
            }
            else if (n == 1)
            {
                foreach (TreeNode subNode in node.Children)
                {
                    node.SubRadius = _initial_Length;
                    Vector move = new Vector(node.SubRadius, direction, true);
                    subNode.Node.X = node.Node.X + move.X;
                    subNode.Node.Y = node.Node.Y + move.Y;

                    SetTreePosition(subNode, alpha, node.SubRadius, direction);
                }
            }
            else
                node.SubRadius = radius == 0 ? _sizeFactor : radius;
        }

        private TreeNode Reorder()
        {
            // Create cached copy of nodes
            List<IDiagramNode> available = this.AppliesTo.Nodes.OrderByDescending(nd => nd.Edges.Count()).Select(n => (IDiagramNode)n).ToList();

            if (available.Count > 1) // There is more than 1 node
            {
                _disconnected = available.Where(nd => nd.Edges.Count() == 0).Select(n => (IDiagramNode)n).ToList();
                available.RemoveAll(nd => nd.Edges.Count() == 0);
            }

            if (available.Any())
            {
                IDiagramNode first = available.First();
                available.Remove(first);
                TreeNode tNode = new TreeNode(first);

                ReadTree(tNode, available);

                return ReorderTree(tNode);
            }
            else if (_disconnected.Any())
            {
                IDiagramNode first = _disconnected.First();
                TreeNode tNode = new TreeNode(first);

                return tNode;
            }
            return null;
        }

        private TreeNode ReorderTree(TreeNode originalTree)
        {
            TreeNode resultTree = new TreeNode(originalTree.Node);

            TreeNode[] ballanced = BalanceTree(originalTree.Children).ToArray();
            int n = ballanced.Count() / 2;
            List<TreeNode> reordered = new List<TreeNode>();
            for (int i = n; i < ballanced.Count(); i++)
                reordered.Add(ballanced[i]);
            for (int i = 0; i < n; i++)
                reordered.Add(ballanced[i]);

            foreach (TreeNode subNode in reordered)
                {
                resultTree.AddChild(ReorderTree(subNode));
            }

            return resultTree;
        }

        private IEnumerable<TreeNode> BalanceTree(IEnumerable<TreeNode> elements)
        {
            if (elements != null && elements.Count() != 0)
            {

                TreeNode[] nodes = elements.ToArray();
                int n = nodes.Count();

                for (int i = 0; i < n; i++)
                {
                    if (i % 2 == 0)
                        yield return nodes[i / 2];
                    else
                        yield return nodes[n - i / 2 - 1];
                }
            }
        }

        private void ReadTree(TreeNode tNode, List<IDiagramNode> available)
        {
            foreach (IDiagramEdge edge in tNode.Node.Edges.OrderByDescending(e => e.From.Edges.Count() + e.To.Edges.Count()))
            {
                IDiagramNode connectedNode = edge.From == tNode.Node ? edge.To : edge.From;
                if (available.Contains(connectedNode))
                {
                    available.Remove(connectedNode);
                    tNode.AddChild(connectedNode);
                }
            }
            foreach (TreeNode added in tNode.Children)
                {
                ReadTree(added, available);
            }
        }


        /// <summary>
        /// Formula to calculate "attraction force" between nodes
        /// </summary>
        private Vector AsAttraction(Vector relation, double factor)
        {
            Double direction = relation.Direction;
            Double distance = relation.Magnitude;
            Double force = Math.Pow(distance, 3) / Math.Pow(factor, 3);
            return new Vector(force, direction, true);
        }

        /// <summary>
        /// Formula to calculate "repulsion force" between nodes
        /// </summary>
        private Vector AsRepulsion(Vector relation, double factor)
        {
            relation = relation.Invert();
            Double direction = relation.Direction;
            Double distance = Math.Max(1, relation.Magnitude);
            Double force = Math.Pow(factor, 3) / Math.Pow(distance, 3);
            return new Vector(force, direction, true);
        }

        /// <summary>
        /// Second Formula to calculate "repulsion force" between nodes
        /// </summary>
        private Vector AsRepulsion2(Vector relation, double factor)
        {
            relation = relation.Invert();
            Double direction = relation.Direction;
            Double distance = relation.Magnitude;//Math.Max(1, relation.Magnitude);

            Double force = distance < _sizeFactor ? _sizeFactor - distance : 0;
            return new Vector(force, direction, true);
        }

        /// <summary>
        /// Third Formula to calculate "repulsion force" between nodes
        /// </summary>
        private Vector AsRepulsion3(Vector relation, double factor)
        {
            relation = relation.Invert();
            Double direction = relation.Rotate90().Direction;
            Double distance = Math.Max(1, relation.Magnitude);

            Double force = _avgShapeSize;
            return new Vector(force, direction, true);
        }

        /// <summary>
        /// Center Layout
        /// </summary>
        private void CenterLayout()
        {
            if (this.AppliesTo.Nodes.Any())
            {
                Vector moveVector = new Vector(this.MinX, this.MinY);
                Vector margin = new Vector(_leftMargin, _bottomMargin);
                moveVector -= margin;

                foreach (IDiagramNode node in this.AppliesTo.Nodes)
                {
                    node.X -= moveVector.X;
                    node.Y -= moveVector.Y;
                }
            }
        }

        private void DumpVectors(string header, IEnumerable<Vector> vectors)
        {
            if (vectors != null)
            {
                Debug.WriteLine("{0} ({1})", header, vectors.Count());
                foreach (Vector vec in vectors)
                    {
                    Debug.WriteLine("\t* " + vec.ToString());
                }
            }
        }
    }
}
