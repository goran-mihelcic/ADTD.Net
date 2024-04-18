namespace Microsoft.ADTD.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using Microsoft.ADTD.Net;

    public class dxDiagram
    {
        private const double PAGE_BORDER = 20;
        //Properties
        private double _minX = 9999999;
        private double _minY = 9999999;
        private double _maxX = -9999999;
        private double _maxY = -9999999;

        public double MinX { get { return _minX; } }
        public double MinY { get { return _minY; } }
        public double MaxX { get { return _maxX; } }
        public double MaxY { get { return _maxY; } }

        public double PageWidth { get { return _maxX - _minX + (2 * PAGE_BORDER); } }
        public double PageHeight { get { return _maxY - _minY + (2 * PAGE_BORDER); } }

        private Dictionary<string, List<dxEdge>> _edges = new Dictionary<string, List<dxEdge>>();
        private Dictionary<string, Dictionary<string, dxNode>> _nodes = new Dictionary<string, Dictionary<string, dxNode>>();


        public Dictionary<string, List<dxEdge>> Edges
        {
            get
            {
                return this._edges;
            }
        }

        public Dictionary<string, Dictionary<string, dxNode>> Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        //Methods

        public void addEdge(dxEdge edge)
        {
            try
            {
                if (this._edges.ContainsKey(edge.Type))
                {
                    this._edges[edge.Type].Add(edge);
                }
                else
                {
                    List<dxEdge> list = new List<dxEdge> {
                    edge
                };
                    this._edges.Add(edge.Type, list);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public void addNode(dxNode node)
        {
            try
            {
                if (this._nodes.ContainsKey(node.Type))
                {
                    if (this._nodes[node.Type].ContainsKey(node.Name))
                    {
                        Logger.logDebug("AddNode - Type: " + node.Type + " Name: " + node.Name);
                        throw new ArgumentException();
                    }
                    this._nodes[node.Type].Add(node.Name, node);
                }
                else
                {
                    Dictionary<string, dxNode> dictionary = new Dictionary<string, dxNode>();
                    dictionary.Add(node.Name, node);
                    this._nodes.Add(node.Type, dictionary);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public void addUniqueNode(dxNode node)
        {
            try
            {
                if (this._nodes.ContainsKey(node.Type))
                {
                    if (!this._nodes[node.Type].ContainsKey(node.Name))
                        this._nodes[node.Type].Add(node.Name, node);
                }
                else
                {
                    Dictionary<string, dxNode> dictionary = new Dictionary<string, dxNode>();
                    dictionary.Add(node.Name, node);
                    this._nodes.Add(node.Type, dictionary);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public void clearEdge(string section)
        {
            try
            {
                if (this._edges.ContainsKey(section))
                {
                    this._edges.Remove(section);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public void clearNode(string section)
        {
            try
            {
                if (this._nodes.ContainsKey(section))
                {
                    this._nodes.Remove(section);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public dxNode getNode(string section, string name)
        {
            try
            {
                if (this._nodes.ContainsKey(section) && this._nodes[section].ContainsKey(name.ToLower()))
                {
                    return this._nodes[section][name.ToLower()];
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        public List<dxEdge> xEdges(string edgeType)
        {
            try
            {
                if (this._edges.ContainsKey(edgeType))
                {
                    return this._edges[edgeType];
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        public Dictionary<string, dxNode> xNodes(string NodeType)
        {
            try
            {
                if (this._nodes.ContainsKey(NodeType))
                {
                    return this._nodes[NodeType];
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        public dxNode findNodeByName(string name)
        {
            try
            {
                var nodes = from n in _nodes
                            from m in n.Value
                            where m.Key == name
                            select m;
                if (nodes.Count() == 1) return nodes.First().Value;
                return null;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        public void resolveEdges(string edgeType)
        {

            if (this.xEdges(edgeType) != null)
            {
                try
                {
                    IEnumerable<dxEdge> toResolve = from e in this.xEdges(edgeType)
                                                    where e.FromNode == null
                                                    select e;
                    foreach (dxEdge e in toResolve)
                    {
                        e.FromNode = this.findNodeByName(e.From);
                    }

                    // Clean unresolved - Error in data
                    this.Edges[edgeType].RemoveAll(e => e.FromNode == null);
                }
                catch (Exception ex)
                {
                    Logger.logDebug(ex.ToString());
                }
            }
        }

        public void resolvePageSize(string[] nodeTypes)
        {
            try
            {
                foreach (string nodeType in nodeTypes)
                    if (_nodes.ContainsKey(nodeType))
                    {
                        _minX = Math.Min((_nodes[nodeType].Values).Min(n => n.X - (n.Width / 2)), _minX);
                        _maxX = Math.Max((_nodes[nodeType].Values).Max(n => n.X + (n.Width / 2)), _maxX);
                        _minY = Math.Min((_nodes[nodeType].Values).Min(n => n.Y - (n.Height / 2)), _minY);
                        _maxY = Math.Max((_nodes[nodeType].Values).Max(n => n.Y + (n.Height / 2)), _maxY);
                    }
                reposition(PAGE_BORDER - _minX, PAGE_BORDER - _minY);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public void reposition(double dX, double dY)
        {
            try
            {
                foreach (Dictionary<string, dxNode> nodeType in _nodes.Values)
                {
                    foreach (dxNode node in nodeType.Values)
                    {
                        node.X = node.X + dX;
                        node.Y = node.Y + dY;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }
    }
}
