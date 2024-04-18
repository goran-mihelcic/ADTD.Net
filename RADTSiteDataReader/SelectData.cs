using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using Microsoft.ADTD.Net;

namespace Microsoft.ADTD.Data.RADT
{
    public class SelectData
    {
        public string fName { get; set; }
        public string dirName;

        internal bool readServers { get; set; }
        internal bool agregateSites { get; set; }
        internal bool expandSL { get; set; }

        internal dxDiagram diagram;
        internal IdxLayout layout;

        public int cnt = 0;
        public string myReplSelection;
        public string mySiteSelection;
        public bool siteOK = false;

        public SelectData()
        {
            this.diagram = new dxDiagram();
        }

        public bool getData()
        {
            try
            {
                dirName = fName.Substring(0, fName.LastIndexOf("\\") + 1);
                DirectoryInfo di = new DirectoryInfo(dirName);
                FileInfo[] files = di.GetFiles("GetSiteInfo*.xml");
                if (files.Length > 0)
                    mySiteSelection = files.OrderByDescending(f => f.LastWriteTime).First().FullName;
                else
                {
                    MessageBox.Show("No Site Info data available!\nPlease run first Site Configuration Test Case.");
                    return false;
                }

                //Missing data from repadmin test case
                files = di.GetFiles("CheckRepAdmin.*.xml");
                if (files.Length > 0)
                    myReplSelection = files.OrderByDescending(f => f.LastWriteTime).First().FullName;
                else
                {
                    MessageBox.Show("No Replication Info data available!\nPlease run first Replication Status Test Case.");
                    return false;
                }
                readSiteData();
                return true;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return false;
            }
        }

        private void readSiteData()
        {
            XmlDocument myXml = new XmlDocument();
            XmlDocument myReplXML = new XmlDocument();
            try
            {
                this.diagram.Edges.Clear();
                this.diagram.Nodes.Clear();
                string scheduleValue;
                Logger.log("Site testcase FileName selected: " + this.mySiteSelection);
                myXml.Load(this.mySiteSelection);
                myReplXML.LoadXml(this.getLowerXML(this.myReplSelection));
                Logger.log("Data collected at: " + myXml.SelectSingleNode("/WorkItem").Attributes["TimeEnded"].Value);
                XmlNodeList myXMLSites = myXml.SelectNodes("/WorkItem/Data/SiteInfo/Site");
                foreach (XmlNode node in myXMLSites)
                {
                    this.cnt++;
                    //this.Infowindow.Dispatcher.Invoke(this.delSetStatus, new object[] { "Reading diagram object " + this.cnt.ToString() });
                    dxNode newSite = new dxNode(this.cnt.ToString(), node.Attributes["Name"].Value, DiagramObjectNames.SiteObject);
                    newSite.addAttribute(AttributeNames.GroupCaching, node.Attributes["GroupCaching"].Value == "ENABLED");
                    newSite.addAttribute(AttributeNames.ISTG, node.Attributes["ISTG"].Value);
                    this.diagram.addNode(newSite);
                    XmlNodeList myXMLServers = myXml.SelectNodes("/WorkItem/Data/SiteInfo/Site[@Name =\"" + node.Attributes["Name"].Value + "\"]/Server");
                    if (readServers)
                    {
                        foreach (XmlNode servNd in myXMLServers)
                        {
                            this.cnt++;
                            //this.Infowindow.Dispatcher.Invoke(this.delSetStatus, new object[] { "Reading diagram object " + this.cnt.ToString() });
                            string machType = (servNd.OuterXml.IndexOf("MachineType") == -1) ? "UnKnown" : servNd.Attributes["MachineType"].Value;
                            bool isRODC = (servNd.OuterXml.IndexOf("IsRODC") != -1) && Convert.ToBoolean(servNd.Attributes["IsRODC"].Value);
                            dxNode newServer = new dxNode(this.cnt.ToString(), servNd.Attributes["Name"].Value, DiagramObjectNames.ServerObject);
                            newServer.addAttribute(AttributeNames.DomainFQDN, servNd.Attributes["ServerFQDN"].Value);
                            newServer.addAttribute(AttributeNames.ServerFQDN, servNd.Attributes["ConfigDNSHostname"].Value);
                            newServer.addAttribute(AttributeNames.MachineType, machType);
                            newServer.addAttribute(AttributeNames.GlobalCatalog, servNd.Attributes["GlobalCatalog"].Value == "ENABLED");
                            newServer.addAttribute(AttributeNames.RODC, isRODC);
                            newServer.addAttribute(AttributeNames.OS, servNd.Attributes["OS"].Value + " " + servNd.Attributes["SP"].Value);
                            newServer.addAttribute(AttributeNames.OSShort, Translator.osTranslate(servNd.Attributes["OS"].Value + " " + servNd.Attributes["SP"].Value));
                            newSite.addChild(newServer);
                            this.diagram.addNode(newServer);
                            bool authoritativeData = servNd.Attributes["DataCollectionMethod"].Value == "Authoritative";
                            XmlNodeList myXMLCO = myXml.SelectNodes("/WorkItem/Data/SiteInfo/Site[@Name =\"" + node.Attributes["Name"].Value + "\"]/Server[@Name =\"" + servNd.Attributes["Name"].Value + "\"]/ConnectionObject");
                            foreach (XmlNode CONode in myXMLCO)
                            {
                                this.cnt++;
                                //this.Infowindow.Dispatcher.Invoke(this.delSetStatus, new object[] { "Reading diagram object " + this.cnt.ToString() });
                                scheduleValue = (CONode.Attributes["Schedule"] == null) ? "" : CONode.Attributes["Schedule"].Value;
                                dxEdge newCO = new dxEdge(this.cnt.ToString(), CONode.Attributes["Name"].Value, DiagramObjectNames.ConnectionObject);
                                newCO.From = CONode.Attributes["From"].Value;
                                newCO.ToNode = newServer;
                                newCO.addAttribute(AttributeNames.TransportType, CONode.Attributes["TransportType"].Value);
                                newCO.addAttribute(AttributeNames.Schedule, scheduleValue);
                                newCO.addAttribute(AttributeNames.Authoritative, authoritativeData);
                                newServer.addEdge(newCO);
                                diagram.addEdge(newCO);
                                XmlNodeList myXMLNC = myXml.SelectNodes("/WorkItem/Data/SiteInfo/Site[@Name =\"" + node.Attributes["Name"].Value + "\"]/Server[@Name =\"" + servNd.Attributes["Name"].Value + "\"]/ConnectionObject[@Name = \"" + newCO.Name + "\"]/NamingContext");
                                foreach (XmlNode NCNode in myXMLNC)
                                {
                                    if (!newCO.childExists(NCNode.Attributes["Name"].Value))
                                    {
                                        this.cnt++;
                                        //this.Infowindow.Dispatcher.Invoke(this.delSetStatus, new object[] { "Reading diagram object " + this.cnt.ToString() });
                                        newCO.addChild(new dxNode(this.cnt.ToString(), NCNode.Attributes["Name"].Value, DiagramObjectNames.NamingContextObject));
                                    }
                                }
                                myXMLNC = myReplXML.SelectNodes("/workitem/data/collated/checkrepadmin/detail[@destinationdcname= \"" + servNd.Attributes["Name"].Value.ToLower() + "\"][@sourcedcname= \"" + newCO.From.ToLower() + "\"]");
                                foreach (XmlNode NCNode in myXMLNC)
                                {
                                    if (!newCO.childExists(NCNode.Attributes["namingcontext"].Value))
                                    {
                                        this.cnt++;
                                        //this.Infowindow.Dispatcher.Invoke(this.delSetStatus, new object[] { "Reading diagram object " + this.cnt.ToString() });
                                        newCO.addChild(new dxNode(this.cnt.ToString(), NCNode.Attributes["namingcontext"].Value, DiagramObjectNames.NamingContextObject));
                                    }
                                }
                            }
                        }
                    }
                }
                this.diagram.clearEdge("SiteLink");
                XmlNodeList myXMLSL = myXml.SelectNodes("/WorkItem/Data/SiteInfo/SiteLink");
                foreach (XmlNode SLnode in myXMLSL)
                {
                    myXMLSites = myXml.SelectNodes("/WorkItem/Data/SiteInfo/SiteLink[@Name =\"" + SLnode.Attributes["Name"].Value + "\"]/SiteInSiteLink");
                    if (expandSL && (myXMLSites.Count > 2))
                    {
                        for (int x = 0; x < myXMLSites.Count; x++)
                        {
                            XmlNode xNode = myXMLSites.Item(x);
                            for (int y = x + 1; y < myXMLSites.Count; y++)
                            {
                                this.cnt++;
                                scheduleValue = (SLnode.Attributes["Schedule"] == null) ? "" : SLnode.Attributes["Schedule"].Value;
                                dxEdge newSiteLink = new dxEdge(this.cnt.ToString(), SLnode.Attributes["Name"].Value, DiagramObjectNames.SiteLinkObject);
                                newSiteLink.addAttribute(AttributeNames.ReplicationInterval, SLnode.Attributes["ReplicationInterval"].Value);
                                newSiteLink.addAttribute(AttributeNames.Cost, SLnode.Attributes["Cost"].Value);
                                newSiteLink.addAttribute(AttributeNames.InterSiteChangeNotification, SLnode.Attributes["InterSiteChangeNotification"].Value.ToUpper() == "ENABLED");
                                newSiteLink.addAttribute(AttributeNames.Schedule, scheduleValue);
                                newSiteLink.FromNode = this.diagram.getNode("Site", myXMLSites[x].Attributes["Name"].Value);
                                newSiteLink.ToNode = this.diagram.getNode("Site", myXMLSites[y].Attributes["Name"].Value);
                                newSiteLink.FromNode.addEdge(newSiteLink);
                                newSiteLink.ToNode.addEdge(newSiteLink);
                                this.diagram.addEdge(newSiteLink);
                            }
                        }
                    }
                    else
                    {
                        this.cnt++;
                        //this.Infowindow.Dispatcher.Invoke(this.delSetStatus, new object[] { "Reading diagram object " + this.cnt.ToString() });
                        scheduleValue = (SLnode.Attributes["Schedule"] == null) ? "" : SLnode.Attributes["Schedule"].Value;
                        dxEdge newSiteLink = new dxEdge(this.cnt.ToString(), SLnode.Attributes["Name"].Value, DiagramObjectNames.SiteLinkObject);
                        newSiteLink.addAttribute(AttributeNames.ReplicationInterval, SLnode.Attributes["ReplicationInterval"].Value);
                        newSiteLink.addAttribute(AttributeNames.Cost, SLnode.Attributes["Cost"].Value);
                        newSiteLink.addAttribute(AttributeNames.InterSiteChangeNotification, SLnode.Attributes["InterSiteChangeNotification"].Value.ToUpper() == "ENABLED");
                        newSiteLink.addAttribute(AttributeNames.Schedule, scheduleValue);
                        if ((myXMLSites.Count > 2) || (myXMLSites.Count == 1))
                            addCP(myXMLSites, SLnode);
                        else
                        {
                            this.diagram.addEdge(newSiteLink);
                            newSiteLink.FromNode = this.diagram.getNode("Site", myXMLSites[0].Attributes["Name"].Value);
                            newSiteLink.ToNode = this.diagram.getNode("Site", myXMLSites[1].Attributes["Name"].Value);
                            newSiteLink.FromNode.addEdge(newSiteLink);
                            newSiteLink.ToNode.addEdge(newSiteLink);
                        }
                    }
                }
                diagram.resolveEdges(DiagramObjectNames.ConnectionObject);
                if (agregateSites) doAggregate();
                layout.doLayout(diagram);
            }
            catch (Exception exception)
            {
                Logger.logDebug(exception.ToString());
                System.Windows.MessageBox.Show(exception.Message);
            }
        }

        private string getLowerXML(string FName)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(FName);
                return document.OuterXml.ToLower();
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        private void addCP(XmlNodeList myXMLSites, XmlNode SLnode)
        {
            try
            {
                string scheduleValue;

                this.cnt++;
                //this.Infowindow.Dispatcher.Invoke(this.delSetStatus, new object[] { "Reading diagram object " + this.cnt.ToString() });
                dxNode cpSite = new dxNode(this.cnt.ToString(), "CP-" + this.cnt.ToString(), DiagramObjectNames.ConnectionPointObject);
                this.diagram.addNode(cpSite);
                foreach (XmlNode siteNode in myXMLSites)
                {
                    this.cnt++;
                    //this.Infowindow.Dispatcher.Invoke(this.delSetStatus, new object[] { "Reading diagram object " + this.cnt.ToString() });
                    scheduleValue = (SLnode.Attributes["SLSchedule"] == null) ? "" : SLnode.Attributes["SLSchedule"].Value;
                    dxEdge tmpSL = new dxEdge(this.cnt.ToString(), SLnode.Attributes["Name"].Value, DiagramObjectNames.SiteLinkObject);
                    tmpSL.addAttribute(AttributeNames.ReplicationInterval, SLnode.Attributes["ReplicationInterval"].Value);
                    tmpSL.addAttribute(AttributeNames.Cost, SLnode.Attributes["Cost"].Value);
                    tmpSL.addAttribute(AttributeNames.InterSiteChangeNotification, SLnode.Attributes["InterSiteChangeNotification"].Value.ToUpper() == "ENABLED");
                    tmpSL.addAttribute(AttributeNames.SLSchedule, scheduleValue);
                    tmpSL.addAttribute(AttributeNames.DisplayName, SLnode.Attributes["Name"].Value);
                    tmpSL.FromNode = this.diagram.getNode("Site", siteNode.Attributes["Name"].Value);
                    tmpSL.ToNode = cpSite;
                    tmpSL.FromNode.addEdge(tmpSL);
                    tmpSL.ToNode.addEdge(tmpSL);
                    this.diagram.addEdge(tmpSL);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        private void doAggregate()
        {
            try
            {
                IEnumerable<IGrouping<dxNode, dxNode>> emptySites = from site in this.diagram.Nodes[DiagramObjectNames.SiteObject].Values
                                                                    where site.Children.Count == 0 && site.Connected.Count == 1
                                                                    group site by site.Connected[0] into g
                                                                    select g;
                List<IGrouping<dxNode, dxNode>> groups4Aggregate = (from gr in emptySites
                                                                    where gr.Count() > 1
                                                                    select gr).ToList();
                foreach (IGrouping<dxNode, dxNode> gr in groups4Aggregate)
                {
                    foreach (dxNode site in gr)
                    {
                        dxEdge siteLink = diagram.Edges[DiagramObjectNames.SiteLinkObject].Where(sl =>
                            (sl.FromNode == site && sl.ToNode == gr.Key) ||
                            (sl.FromNode == gr && sl.ToNode == site)).First();
                        diagram.Edges[DiagramObjectNames.SiteLinkObject].Remove(siteLink);
                        diagram.Nodes[DiagramObjectNames.SiteObject].Remove(diagram.Nodes[DiagramObjectNames.SiteObject].Where(s => s.Value == site).First().Key);
                    }
                    this.cnt++;
                    dxNode newSite = new dxNode(this.cnt.ToString(), "Aggregate Sites-" + this.cnt.ToString(), DiagramObjectNames.SiteObject);
                    newSite.addAttribute(AttributeNames.GroupCaching, false);
                    newSite.addAttribute(AttributeNames.ISTG, "");
                    newSite.addAttribute(AttributeNames.AggregateCount, gr.Count().ToString());
                    this.diagram.addNode(newSite);

                    dxEdge tmpSL = new dxEdge(this.cnt.ToString(), "Aggregated SiteLink", DiagramObjectNames.SiteLinkObject);
                    tmpSL.addAttribute(AttributeNames.ReplicationInterval, "");
                    tmpSL.addAttribute(AttributeNames.Cost, "");
                    tmpSL.addAttribute(AttributeNames.InterSiteChangeNotification, false);
                    tmpSL.addAttribute(AttributeNames.SLSchedule, "");
                    tmpSL.addAttribute(AttributeNames.DisplayName, "Aggregated SiteLink");
                    tmpSL.FromNode = gr.Key;
                    tmpSL.ToNode = newSite;
                    tmpSL.FromNode.addEdge(tmpSL);
                    tmpSL.ToNode.addEdge(tmpSL);
                    this.diagram.addEdge(tmpSL);
                }
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }
    }
}
