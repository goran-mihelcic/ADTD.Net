using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ADTD.Data;
using Microsoft.ADTD;
using Microsoft.ADTD.Net;
using System.Diagnostics;

namespace Microsoft.ADTD.Layouts
{
    public class Layout : IdxLayout
    {

        private dxDiagram Diagram;

        public string diagramType { get; set; }

        public Dictionary<string, object> LayoutData { get; set; }

        private double _initialThetha = 2 * Math.PI;
        public double InitialThetha
        {
            get { return _initialThetha; }
            set { _initialThetha = value; }
        }

        private double _initialDirection = Math.PI / 2;//2 * Math.PI;
        public double InitialDirection
        {
            get { return _initialDirection; }
            set { _initialDirection = value; }
        }

        private List<dxNode> nodeObjects
        {
            get
            {
                try
                {
                    if (diagramType == "SiteDiagram")
                        return Diagram.Nodes["Site"].Values
                            .Union(Diagram.Nodes.ContainsKey("ConnectionPoint") ?
                            Diagram.Nodes["ConnectionPoint"].Values :
                            new Dictionary<string, dxNode>().Values).ToList();
                    if (diagramType == "DomainDiagram")
                        return Diagram.Nodes["Domain"].Values.ToList();
                    return (new Dictionary<string, dxNode>().Values).ToList();
                }
                catch (Exception ex)
                {
                    Logger.logDebug(ex.ToString());
                    return null;
                }
            }
        }

        public Layout()
        {
            try
            {
                diagramType = "SiteDiagram";
                LayoutData = new Dictionary<string, object>()
            {
                {"InitialThetha", 2 * Math.PI},
                {"InitialDirection", Math.PI}
            };
                InitialThetha = (double)LayoutData["InitialThetha"];
                InitialDirection = (double)LayoutData["InitialDirection"];
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public Layout(string type, Dictionary<string, object> data)
        {
            try
            {
                diagramType = type;
                LayoutData = data;
                if (LayoutData.ContainsKey("InitialThetha"))
                    InitialThetha = (double)LayoutData["InitialThetha"];
                if (LayoutData.ContainsKey("InitialDirection"))
                    InitialDirection = (double)LayoutData["InitialDirection"];
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public void doLayout(dxDiagram diagram)
        {
            try
            {
                Diagram = diagram;
                switch (diagramType)
                {
                    case "SiteDiagram":
                        enumGroups("Site", "ConnectionPoint");
                        break;
                    case "DomainDiagram":
                        if (LayoutData.ContainsKey("Root"))
                        {
                            dxNode root = diagram.Nodes["Domain"][(string)LayoutData["Root"]];
                            root.level = 0;
                            root.HierarchyUp = null;
                            enumLevels(root);
                            //correctLevels();
                            var levels = from g in nodeObjects
                                         group g by g.level into g
                                         select new { Level = g.Key, Nodes = g };
                            Distribute(root);
                            orderByAfinity(root);
                            setNodeSizes("Domain", "Dummy");
                            populateSimple();
                            populate(root);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private void enumGroups(string mainNodeType, string secondaryNodeType)
        {
            try
            {
                int grNo = 1;
                bool doLoop = true;
                do
                {
                    IEnumerable<dxNode> tmpsites = from S in nodeObjects
                                                   where S.Edges.Count() > 0 && S.moved == false
                                                   select S;
                    if (tmpsites.Count() != 0)
                    {
                        //writeStatus("Group No " & grNo)
                        ArrangeShapes(grNo, mainNodeType, secondaryNodeType);
                        grNo += 1;
                    }
                    else doLoop = false;
                } while (doLoop);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private void ArrangeShapes(int grId, string mainNodeType, string secondaryNodeType)
        {
            try
            {
                setNodeSizes(mainNodeType, secondaryNodeType);
                dxNode hub = moveHubToCenter();
                populateSimple();
                populate(hub);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private dxNode moveHubToCenter()
        {
            //***********************************************************
            // Selects Hub site - site with maximum number of Site Links
            // and move elected site to center of drawing - makes him
            // starting point...
            //***********************************************************

            try
            {
                var numLinks = from S in nodeObjects
                               where S.Edges.Count() > 0 && S.moved == false
                               orderby S.Edges.Count descending
                               select new { S.Edges.Count, S };

                //var maxLinks = numLinks.Max(n => n.Count);

                //IEnumerable<dxNode> Sites = from x in numLinks
                //                            where x.Count == maxLinks
                //                            select x.S;
                //dxNode electedSite = Sites.First();
                dxNode electedSite = numLinks.First().S;
                electedSite.level = 0;
                enumLevels(electedSite);
                var levels = from g in nodeObjects
                             group g by g.level into g
                             select new { Level = g.Key, Nodes = g };
                Distribute(electedSite);
                //documentIt(nodeObjects);
                orderByAfinity(electedSite);
                orderByAfinity2(electedSite);
                //orderByAfinity2(electedSite);
                //orderByAfinity2(electedSite);
                //orderByAfinity2(electedSite);
                return electedSite;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        private void enumLevels(dxNode node)
        {
            List<dxNode> nodesWithChildren = new List<dxNode>();

            try
            {
                int nextLevel = node.level + 1;
                foreach (dxEdge e in node.Edges)
                {
                    dxNode from = e.FromNode == node ? e.ToNode : e.FromNode;
                    //Debug.WriteLine("Node: " + from.Name + "\tLevel: " + from.level + "\tNextLevel:" + nextLevel);
                    if (from.level > node.level)
                    {
                        from.level = nextLevel;
                        from.HierarchyUp = node;
                        if (from.Edges.Count > 0)
                        {
                            nodesWithChildren.Add(from);
                        }
                    }
                }

                foreach (dxNode n in nodesWithChildren) enumLevels(n);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private void correctLevels()
        {
            try
            {
                int maxLevel = nodeObjects.Where(n => (bool)(n.getAttribute("InternalDomain")) == true).Max(m => m.level);
                foreach (dxNode realm in nodeObjects.Where(n => (bool)(n.getAttribute("InternalDomain")) == true))
                    realm.level = maxLevel + 1;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private void Distribute(dxNode node)
        {
            try
            {
                var connectedNodes = from e in node.Edges
                                     select new { edge = e, node = e.FromNode == node ? e.ToNode : e.FromNode };
                List<dxEdge> OrderEdges = (from n in connectedNodes
                                           orderby n.node.Edges.Count
                                           select n.edge).ToList<dxEdge>();
                List<dxEdge> tmp = new List<dxEdge>();
                int lowerN = 0;
                int upperN = OrderEdges.Count() - 1;
                bool fromTop = true;
                for (int i = 0; i < OrderEdges.Count(); i++)
                {
                    if (fromTop)
                    {
                        tmp.Add(OrderEdges[lowerN]);
                        lowerN += 1;
                    }
                    else
                    {
                        tmp.Add(OrderEdges[upperN]);
                        upperN -= 1;
                    }
                    fromTop = !fromTop;
                }
                node.Edges = tmp;

                foreach (var nextNode in connectedNodes)
                {
                    if (nextNode.node.level > node.level)
                        Distribute(nextNode.node);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        #region "Domain Diagram specific"

        /// <summary>
        /// Orders nodes by their affinity to each other
        /// </summary>
        /// <param name="node"></param>
        private void orderByAfinity(dxNode node)
        {
            //if (node.Name == "qrhq-dr") Debugger.Break();
            try
            {
                var connectedNodes = from e in node.Edges
                                     select new { edge = e, node = e.FromNode == node ? e.ToNode : e.FromNode };
                var OrderEdges = (from n in connectedNodes
                                  where n.node.level > node.level
                                  orderby n.node.Edges.Count descending
                                  select n).ToList();

                List<dxEdge> tmpSortedEdges = new List<dxEdge>();
                List<dxNode> tmpSortedNodes = new List<dxNode>();

                foreach (var nd in OrderEdges)
                {
                    if (nd.node != node.HierarchyUp && nd.node.ConnectionCount < 2)
                    {
                        if (tmpSortedNodes.Count == 0)
                        {
                            tmpSortedEdges.Add(nd.edge);
                            tmpSortedNodes.Add(nd.node);
                        }
                        else
                        {
                            int lIdx = 0;
                            int idx = 0;
                            int nearest = 99999;
                            dxNode nearestNode = null;
                            dxEdge nearestEdge = null;

                            foreach (dxEdge link in nd.node.Edges)
                            {
                                dxNode linkedNode = link.FromNode == nd.node ? link.ToNode : link.FromNode;
                                //Debug.WriteLine("Node = " + linkedNode.Name + ",  Link = " + link.Name + "LinkId: " + link.ID.ToString());
                                lIdx = tmpSortedNodes.IndexOf(linkedNode);
                                if (lIdx > -1)
                                {
                                    int rIdx = tmpSortedNodes.Count - lIdx;
                                    idx = lIdx < rIdx ? lIdx * -1 : rIdx;
                                    if (Math.Abs(idx) < Math.Abs(nearest))
                                    {
                                        nearest = idx;
                                        nearestNode = linkedNode;
                                        nearestEdge = link;
                                    }
                                }
                            }
                            if (nearest < 0)
                            {
                                tmpSortedEdges.Insert(0, nd.edge);
                                tmpSortedNodes.Insert(0, nd.node);
                            }
                            else
                            {
                                tmpSortedEdges.Add(nd.edge);
                                tmpSortedNodes.Add(nd.node);
                            }



                        }
                    }
                    orderByAfinity(nd.node);
                }
                node.Edges.RemoveAll(e => tmpSortedEdges.Exists(ed => ed == e));
                node.Edges.AddRange(tmpSortedEdges);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private void orderByAfinity2(dxNode node)
        {
            //if (node.Name == "qrhq-dr") Debugger.Break();

            try
            {
                var connectedNodes = from e in node.Edges
                                     select new { edge = e, node = e.FromNode == node ? e.ToNode : e.FromNode };
                var OrderEdges = (from n in connectedNodes
                                  where n.node.level > node.level
                                  orderby n.node.Edges.Count descending
                                  select n).ToList();

                List<dxEdge> tmpEdges = new List<dxEdge>();
                List<dxNode> tmpNodes = new List<dxNode>();
                int found = 0;
                int i = 0;

                foreach (var nd in OrderEdges)
                {
                    if (tmpEdges.Count == 0)
                    {
                        tmpEdges.Add(nd.edge);
                        tmpNodes.Add(nd.node);
                    }
                    else
                    {
                        do
                        {
                            bool? right = isRight(nd.node, tmpNodes[i]);
                            if (right == false)
                            {
                                found = i;
                            }
                            if (right == true)
                            {
                                found = i * (-1);
                            }
                            i++;
                        } while (i < tmpEdges.Count && found != 0);
                        if (found > 0)
                        {
                            if (found == tmpEdges.Count)
                            {
                                tmpEdges.Add(nd.edge);
                                tmpNodes.Add(nd.node);
                            }
                            else
                            {
                                tmpEdges.Insert(found + 1, nd.edge);
                                tmpNodes.Insert(found + 1, nd.node);
                            }
                        }
                        else if (found < 0)
                        {
                            tmpEdges.Insert(Math.Abs(found), nd.edge);
                            tmpNodes.Insert(Math.Abs(found), nd.node);
                        }
                        else
                        {
                            tmpEdges.Add(nd.edge);
                            tmpNodes.Add(nd.node);
                        }
                    }
                    orderByAfinity2(nd.node);
                }

                //if (tmpRightEdges.Count > 0 || tmpLeftEdges.Count > 0) Debugger.Break();
                node.Edges.RemoveAll(e => tmpEdges.Exists(ed => ed == e));
                node.Edges.InsertRange(0, tmpEdges);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        /// <summary>
        /// Returns true if connection is on the right othervise returns false
        /// </summary>
        /// <param name="N1"></param>
        /// <param name="N2"></param>
        /// <returns></returns>
        private bool? isRight(dxNode N1, dxNode N2)
        {
            try
            {
                if (N1.HierarchyUp == null || N2.HierarchyUp == null || N1.HierarchyUp == N2 || N2.HierarchyUp == N1)
                {
                    //Debug.WriteLine("Right - Returned!!!");
                    return null;
                }
                if (N1.level < N2.level) return isRight(N1, N2.HierarchyUp);
                else if (N2.level < N1.level) return isRight(N1.HierarchyUp, N2);
                else
                {
                    if (N1.HierarchyUp == N2.HierarchyUp)
                    {
                        int num = N1.HierarchyUp.ConnectionCount;
                        int left = ((N1.HierarchyUp.Connected.IndexOf(N2) - N1.HierarchyUp.Connected.IndexOf(N1)) - num) % num;
                        if (left < 0) left = left + num;
                        int right = ((N1.HierarchyUp.Connected.IndexOf(N1) - N1.HierarchyUp.Connected.IndexOf(N2)) + num) % num;
                        if (right < 0) right = right + num;
                        return left < right;
                    }
                    else return isRight(N1.HierarchyUp, N2.HierarchyUp);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Calculates connections weight for left (positive) or right side
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int wLeft(dxNode node)
        {
            try
            {
                foreach (dxNode nd in node.Connected)
                {
                    int result = 0;
                    if (node.HierarchyUp == nd || nd.HierarchyUp == node) return 0;
                    else
                    {
                        result += isRight(node, nd) == true ? -1 : 1;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }
        #endregion

        #region "Node size"
        private const double baseWidth = 82.5;
        private const double baseHeigth = 47.5;

        private void setNodeSizes(string mainNodeType, string secondaryNodeType)
        {
            foreach (dxNode nd in Diagram.Nodes[mainNodeType].Values)
            {
                double x = Math.Truncate(Math.Sqrt(nd.Children.Count)) + 1;
                double y = Math.Truncate(Math.Sqrt(nd.Children.Count + x));

                double shWidth = baseWidth + (Math.Max((x - 2), 0) * 37.5);
                double shHeigth = baseHeigth + Math.Max((y - 1), 0) * 27.5;

                nd.setSize(shWidth, shHeigth);
            }

            if (Diagram.Nodes.ContainsKey("ConnectionPoint"))
                foreach (dxNode nd in Diagram.Nodes["ConnectionPoint"].Values)
                    nd.setSize(4, 4);
        }

        #endregion

        #region "Node Positioning"
        int branchLimit = 20;
        double cf = 1;
        double minimumRadius = 150;
        double cAspect = 1.3;
        int startX = 0;
        int startY = 0;

        private void populateSimple()
        {
            try
            {
                foreach (dxNode node in (nodeObjects).OrderByDescending(n => n.level))
                {
                    //if (node.Name == "qrhq-dr") Debugger.Break();
                    node.setInternalAttribute("isBigHubCenter", GetIsBigHub(node));
                    node.setInternalAttribute("Width", GetWidth(node));
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private void populate(dxNode node)
        {
            try
            {
                //Debug.WriteLine(node.Name);
                node.setInternalAttribute("Theta", getTheta(node));
                node.setInternalAttribute("LeftCorrection", getLeftCorrection(node));
                node.setInternalAttribute("RightCorrection", getRightCorrection(node));
                node.setInternalAttribute("CorrectedTheta", getCorrectedTheta(node));
                node.setInternalAttribute("Beta", getBeta(node));
                node.setInternalAttribute("Radius", getRadius(node));
                node.setInternalAttribute("BranchRadius", getBranchRadius(node));
                node.setInternalAttribute("Direction", getDirection(node));
                this.setPosition(node);

                foreach (dxNode nd in node.Downlevel) populate(nd);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private bool GetIsBigHub(dxNode node)
        {
            try
            {
                if (node.HierarchyUp == null)
                    return false;
                else
                    return (node.ConnectionCount > this.branchLimit);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return false;
            }
        }

        private bool isBigHub(dxNode node)
        {
            try
            {
                if (node.existsIA("isBigHubCenter"))
                    return node.getBooleanIA("isBigHubCenter");
                else
                    return false;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return false;
            }
        }

        private int GetWidth(dxNode node)
        {
            try
            {
                int suma = 1;
                if (node.Downlevel.Count > 0)
                    if (isBigHub(node)) suma = 5;
                    else suma = (int)node.Downlevel.Sum(n => groupWidth(n));
                return suma;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private int groupWidth(dxNode node)
        {
            try
            {
                if (node.existsIA("Width"))
                    return node.getIntIA("Width");
                else
                    return 1;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getLeftCorrection(dxNode node)
        {
            try
            {
                double _leftCorrection = 0;
                if (node.HierarchyUp != null && node.HierarchyUp.Downlevel.Count > 1)
                {
                    int idx = node.HierarchyUp.ConnectedIndex(node);
                    int limit = node.HierarchyUp.Downlevel.Count - 1;

                    for (int i = 0; i < (limit / 2); i++)
                    {
                        idx -= 1;
                        if (idx < 0) idx = limit;
                        if (node.HierarchyUp.Edges.ElementAt(idx).getOtherSide(node.HierarchyUp).hasEdges) goto exit;
                        _leftCorrection += node.HierarchyUp.getDoubleIA("Beta");
                    }
                exit:
                    return Math.Min(_leftCorrection, (Math.PI - node.getDoubleIA("Theta")) / 2);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getRightCorrection(dxNode node)
        {
            try
            {
                double _rightCorrection = 0;
                if (node.HierarchyUp != null && node.HierarchyUp.Downlevel.Count > 1)
                {
                    int idx = node.HierarchyUp.Connected.IndexOf(node);
                    int limit = node.HierarchyUp.Downlevel.Count - 1;

                    for (int i = 0; i < (limit / 2); i++)
                    {
                        idx += 1;
                        if (idx > limit) idx = 0;
                        if (node.HierarchyUp.Edges.ElementAt(idx).getOtherSide(node.HierarchyUp).hasEdges) goto exit;
                        _rightCorrection += node.HierarchyUp.getDoubleIA("Beta");
                    }
                exit:
                    return Math.Min(_rightCorrection, (Math.PI - node.getDoubleIA("Theta")) / 2);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getTheta(dxNode node)
        {
            try
            {
                if (node.HierarchyUp == null) return (_initialThetha);
                else
                    if (isBigHub(node))
                    {
                        return (3 * Math.PI / 2);
                    }
                    else return (node.HierarchyUp.getDoubleIA("Beta") * groupWidth(node));
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getBeta(dxNode node)
        {
            try
            {
                if (node.ConnectionCount > 0)
                {
                    if (isBigHub(node))
                        return (node.getDoubleIA("Theta") / (node.ConnectionCount));
                    else
                        return (node.getDoubleIA("CorrectedTheta") / groupWidth(node));
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getCorrectedTheta(dxNode node)
        {
            try
            {
                if (node.HierarchyUp == null)
                    return node.getDoubleIA("Theta");
                else return Math.Min(node.getDoubleIA("Theta") + node.getDoubleIA("LeftCorrection") + node.getDoubleIA("RightCorrection"), Math.PI * 2);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getArc(dxNode node)
        {
            try
            {
                return node.Downlevel.Sum(n => n.Diagonale);
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getRadius(dxNode node)
        {
            try
            {
                return Math.Max(getArc(node) * this.cf / node.getDoubleIA("Theta"), Math.Max(minimumRadius, getMaxSubDiag(node) + node.Diagonale / 2));
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getMaxSubDiag(dxNode node)
        {
            try
            {
                if (node.Downlevel.Count > 0)
                    return node.Downlevel.Max(n => n.Diagonale) / 2;
                else return 0;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getAvarageShapeWidth(dxNode node)
        {
            try
            {
                int w = 0;
                if (node != null)
                {
                    if (node.ConnectionCount > 0)
                    {
                        double tmpWidth = node.Downlevel.Average(n => n.Diagonale);
                        w = (int)(Math.Max(50, tmpWidth));
                    }
                }
                return w;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getBranchRadius(dxNode node)
        {
            try
            {
                double dummy1 = 0;
                double dummy2 = 0;
                double r = node.getDoubleIA("Radius");
                double R0;
                if (node.HierarchyUp == null) R0 = getAvarageShapeWidth(node.HierarchyUp) + r;
                else
                    R0 = (node.HierarchyUp.getDoubleIA("Radius") + getAvarageShapeWidth(node.HierarchyUp) + r);
                double gama = 0, b = 0, c = 0;
                if (isBigHub(node))
                {
                    b = Math.Sqrt(Math.Pow(R0, 2) - Math.Pow(r, 2));

                    Triangle.SSS(r, b, R0, ref gama, ref dummy1, ref dummy2);
                    if (gama < (node.HierarchyUp.getDoubleIA("Beta") * groupWidth(node)) / 2)
                        r = R0;
                    else
                    {
                        Triangle.AAS(r + getAvarageShapeWidth(node),
                            (node.HierarchyUp.getDoubleIA("Beta") * groupWidth(node)) / 2,
                            Math.PI / 2, ref gama, ref b, ref c);
                        r = Math.Max(Math.Abs(b), R0);
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getLeftBorderDirection(dxNode node)
        {
            try
            {
                double dir = _initialDirection;

                if (node.HierarchyUp != null)
                {
                    if (isBigHub(node))
                        dir = node.getDoubleIA("Direction") - (node.getDoubleIA("Theta") / 2);
                    else
                        dir = node.getDoubleIA("Direction") - ((node.getDoubleIA("Theta") / 2) + node.getDoubleIA("LeftCorrection"));
                }
                return dir;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private double getDirection(dxNode node)
        {
            try
            {
                int idx;

                if (node.HierarchyUp == null)
                    return _initialDirection;
                else
                {
                    idx = node.HierarchyUp.ConnectedIndex(node);
                    if (node.HierarchyUp.ConnectionCount == 1)
                        return node.HierarchyUp.getDoubleIA("Direction");
                    else
                    {
                        if (idx == 0)
                            return getLeftBorderDirection(node.HierarchyUp) +
                                (isBigHub(node) ? Math.Asin(node.getDoubleIA("Radius") / node.getDoubleIA("BranchRadius")) : (node.getDoubleIA("Theta") / 2));
                        else
                            return (node.HierarchyUp.ConnectedAtIndex(idx - 1).getDoubleIA("Direction")) +
                            ((node.HierarchyUp.getDoubleIA("Beta") * groupWidth(node)) / 2) +
                            ((node.HierarchyUp.getDoubleIA("Beta") * groupWidth(node.HierarchyUp.ConnectedAtIndex(idx - 1))) / 2);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return 0;
            }
        }

        private void setPosition(dxNode node)
        {
            try
            {
                double ef, r;

                if (node.HierarchyUp == null)
                    node.setPosition(startX, startY);
                else
                {
                    r = node.HierarchyUp.getDoubleIA("Radius");
                    if (isBigHub(node))
                        r = node.getDoubleIA("BranchRadius");

                    ef = node.HierarchyUp.HierarchyUp == null ? this.cAspect : 1;
                    node.setPosition(
                        (node.HierarchyUp.X + (Math.Sin(node.getDoubleIA("Direction")) * r * ef)),
                    node.HierarchyUp.Y + (Math.Cos(node.getDoubleIA("Direction"))) * r);
                }
                node.moved = true;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        #endregion

        private void documentIt(List<dxNode> nodes)
        {
            try
            {
                string result = "";
                foreach (dxNode node in nodes)
                {
                    result = result + node.Name + "," + (node.HierarchyUp == null ? "" : node.HierarchyUp.Name) + "," + node.level.ToString() + ",";
                    foreach (dxEdge edge in node.Edges)
                    {
                        result = result + (edge.FromNode == node ? edge.ToNode.Name : edge.FromNode.Name) + ",";
                    }
                    result = result + "\n";
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }
    }
}
