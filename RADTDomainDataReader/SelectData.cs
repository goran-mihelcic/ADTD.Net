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

        internal dxDiagram diagram;
        internal IdxLayout layout;

        public string myDomainSelection;
        public string myServerSelection;
        public string myServerDetailsSelection = "";
        public int cnt = 0;

        public SelectData()
        {
            try
            {
                this.diagram = new dxDiagram();
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public bool getData()
        {
            try
            {
                dirName = fName.Substring(0, fName.LastIndexOf("\\") + 1);
                DirectoryInfo di = new DirectoryInfo(dirName);
                FileInfo[] files = di.GetFiles("GetFsmoInfo*.xml");
                if (files.Length > 0)
                    myDomainSelection = files.OrderByDescending(f => f.LastWriteTime).First().FullName;
                else
                {
                    MessageBox.Show("No Domain Info data available!\nPlease run first Forest/Domain Info Test Case.");
                    return false;
                }
                files = di.GetFiles("ActiveDirectoryContext.ctx");
                if (files.Length > 0)
                    myServerSelection = files.OrderByDescending(f => f.LastWriteTime).First().FullName;
                else
                {
                    MessageBox.Show("No OS Info data available!\nPlease run first Discovery in RADT.");
                    return false;
                }
                files = di.GetFiles("GetOsInfo*.xml");
                if (files.Length > 0)
                    myServerDetailsSelection = files.OrderByDescending(f => f.LastWriteTime).First().FullName;
                readDomainData();
                return true;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return false;
            }
        }

        private void readDomainData()
        {
            XmlDocument myXml = new XmlDocument();
            XmlDocument myReplXML = new XmlDocument();

            try
            {
                this.diagram.Edges.Clear();
                this.diagram.Nodes.Clear();
                Logger.log("FSMO testcase FileName selected: " + this.myDomainSelection);
                //this.diagram.clearNode(DiagramObjectNames.DomainObject);
                myXml.Load(this.myDomainSelection);
                myReplXML.LoadXml(this.getLowerXML(this.myDomainSelection));
                Logger.log("Data collected at: " + myXml.SelectSingleNode("/WorkItem").Attributes["TimeEnded"].Value);
                XmlNodeList myXMLForest = myXml.SelectNodes("/WorkItem/Data/FSMOInfo/Forest");

                foreach (XmlNode node in myXMLForest)
                {
                    cnt++;
                    dxNode myForest = new dxNode(cnt.ToString(), node.Attributes["RootDomain"].Value, DiagramObjectNames.ForestObject);
                    myForest.addAttribute(AttributeNames.SchemaMaster, node.Attributes["Schema"].Value);
                    myForest.addAttribute(AttributeNames.DomainNamingMaster, node.Attributes["DomainNamingMaster"].Value);
                    myForest.addAttribute(AttributeNames.ForestFunctionalLevel, node.Attributes["ForestFunctionalLevel"].Value);
                    myForest.addAttribute(AttributeNames.SchemaVersion, node.Attributes["SchemaVersion"].Value);
                    this.diagram.addNode(myForest);


                    XmlNodeList myXMLDomains = myXml.SelectNodes("/WorkItem/Data/FSMOInfo/Domain");
                    foreach (XmlNode domainNd in myXMLDomains)
                    {
                        cnt++;

                        dxNode newDomain = new dxNode(cnt.ToString(), domainNd.Attributes["Name"].Value, DiagramObjectNames.DomainObject);
                        newDomain.addAttribute(AttributeNames.PDC, domainNd.Attributes["PDC"].Value);
                        newDomain.addAttribute(AttributeNames.RIDMaster, domainNd.Attributes["RID"].Value);
                        newDomain.addAttribute(AttributeNames.InfrastructureMaster, domainNd.Attributes["Infrastructure"].Value);
                        newDomain.addAttribute(AttributeNames.DomainFunctionalLevel, domainNd.Attributes["DomainFunctionalLevel"].Value);
                        newDomain.addAttribute(AttributeNames.InternalDomain, true);
                        this.diagram.addNode(newDomain);
                        newDomain.HierarchyUp = myForest;
                    }
                }


                //Read Realm Data
                XmlNodeList myXMLRealms = myXml.SelectNodes("/WorkItem/Data/FSMOInfo/Trust");
                foreach (XmlNode realmNd in myXMLRealms)
                {
                    dxNode newRealm;
                    string trustType = realmNd.Attributes["TrustType"].Value;
                    bool externalTrust = false;
                    if (trustType.ToLower() == "downlevel")
                    {
                        if (this.diagram.Nodes.ContainsKey(DiagramObjectNames.RealmObject) && this.diagram.Nodes[DiagramObjectNames.RealmObject].ContainsKey(realmNd.Attributes["TrustPartner"].Value.ToLower()))
                            newRealm = this.diagram.getNode(DiagramObjectNames.RealmObject, realmNd.Attributes["TrustPartner"].Value.ToLower());
                        else
                        {
                            cnt++;
                            newRealm = new dxNode(cnt.ToString(), realmNd.Attributes["TrustPartner"].Value.ToLower(), DiagramObjectNames.RealmObject);
                            newRealm.addAttribute(AttributeNames.InternalDomain, false);
                            newRealm.Width = 82.5; newRealm.Height = 47.5;
                            this.diagram.addNode(newRealm);
                        }
                        externalTrust = true;
                    }
                    else
                    {
                        if (this.diagram.Nodes[DiagramObjectNames.DomainObject].ContainsKey(realmNd.Attributes["TrustPartner"].Value.ToLower()))
                            newRealm = this.diagram.getNode(DiagramObjectNames.DomainObject, realmNd.Attributes["TrustPartner"].Value.ToLower());
                        else
                        {
                            cnt++;
                            newRealm = new dxNode(cnt.ToString(), realmNd.Attributes["TrustPartner"].Value.ToLower(), DiagramObjectNames.DomainObject);
                            if (newRealm.ID == "161") Debugger.Break();

                            newRealm.addAttribute(AttributeNames.InternalDomain, false);
                            this.diagram.addNode(newRealm);
                            externalTrust = true;
                        }
                    }

                    cnt++;
                    dxEdge newTrust = new dxEdge(cnt.ToString(),
                        realmNd.Attributes["Domain"].Value + " -> " + realmNd.Attributes["TrustPartner"].Value.ToLower(),
                        trustType.ToLower() == "downlevel" ? DiagramObjectNames.DownlevelTrustObject : DiagramObjectNames.Windows2000TrustObject);
                    newTrust.ToNode = newRealm;
                    newTrust.FromNode = this.diagram.getNode(DiagramObjectNames.DomainObject, realmNd.Attributes["Domain"].Value);
                    newTrust.addAttribute(AttributeNames.TrustType, trustType);
                    newTrust.addAttribute(AttributeNames.TrustAttributes, realmNd.Attributes["TrustAttributes"].Value);
                    newTrust.addAttribute(AttributeNames.TrustDirection, realmNd.Attributes["TrustDirection"].Value);
                    this.diagram.addEdge(newTrust);
                    newTrust.FromNode.addUniqueEdge(newTrust);
                    if (!externalTrust) newTrust.ToNode.addUniqueEdge(newTrust);
                }

                if (readServers)
                {
                    // Read Server Data
                    myXml.LoadXml(this.getLowerXML(this.myServerSelection));
                    XmlNodeList myXMLServers = myXml.SelectNodes("/domaindata/dc");
                    XmlDocument myXml1 = new XmlDocument();
                    XmlNode myXMLServerDetails = null;
                    if (myServerDetailsSelection != "")
                        myXml1.LoadXml(this.getLowerXML(myServerDetailsSelection));

                    foreach (XmlNode servNd in myXMLServers)
                    {
                        this.cnt++;
                        bool isRODC = (servNd.OuterXml.IndexOf("isrodc") != -1) && Convert.ToBoolean(servNd.Attributes["isrodc"].Value);
                        dxNode newServer = new dxNode(this.cnt.ToString(), servNd.Attributes["name"].Value, DiagramObjectNames.ServerObject);
                        newServer.addAttribute(AttributeNames.ExtendedServerAttributes, false);
                        string fqdn;
                        if (servNd.Attributes["domainfqdn"] == null)
                        {
                            fqdn = servNd.Attributes["fqdn"].Value;
                            fqdn = fqdn.Substring(fqdn.IndexOf('.') + 1);
                        }
                        else fqdn = servNd.Attributes["domainfqdn"].Value;
                        newServer.addAttribute(AttributeNames.Domain, fqdn);
                        newServer.addAttribute(AttributeNames.ServerFQDN, servNd.Attributes["fqdn"].Value);
                        newServer.addAttribute(AttributeNames.Site, servNd.Attributes["sitename"].Value);
                        newServer.addAttribute(AttributeNames.GlobalCatalog, Convert.ToBoolean(servNd.Attributes["isgc"].Value));
                        newServer.addAttribute(AttributeNames.RODC, isRODC);
                        newServer.addAttribute(AttributeNames.OS, servNd.Attributes["os"].Value + " " + servNd.Attributes["ossp"].Value);
                        newServer.addAttribute(AttributeNames.OSShort, Translator.osTranslate(servNd.Attributes["os"].Value + " " + servNd.Attributes["ossp"].Value));
                        dxNode myDomain = diagram.getNode(DiagramObjectNames.DomainObject, fqdn);
                        if (myXml1.InnerXml != "" && Convert.ToInt32(myXml1.SelectSingleNode("/workitem").Attributes["engineversion"].Value.Substring(0, 1)) > 1)
                        {
                            myXMLServerDetails = myXml1.SelectSingleNode("/workitem/data/collated/serverinfo/server[@name=\"" + servNd.Attributes["name"].Value + "\"]");
                            newServer.addAttribute(AttributeNames.ExtendedServerAttributes, true);
                            if (myXMLServerDetails.Attributes["machinetype"] != null)
                                newServer.addAttribute(AttributeNames.MachineType, myXMLServerDetails.Attributes["machinetype"].Value);
                            newServer.addAttribute(AttributeNames.OSVersion, Translator.osTranslate(myXMLServerDetails.Attributes["osversion"].Value));
                        }

                        myDomain.addChild(newServer);
                        this.diagram.addNode(newServer);
                    }
                }
                //dxNode forest = diagram.Nodes[DiagramObjectNames.ForestObject].Values.First();
                //dxNode root = diagram.Nodes[DiagramObjectNames.DomainObject][forest.Name];
                //root.addAttribute(AttributeNames.ForestFunctionalLevel, forest.getAttribute(AttributeNames.ForestFunctionalLevel));
                //root.addAttribute(AttributeNames.ForestFunctionalLevel, forest.getAttribute(AttributeNames.ForestFunctionalLevel));
                layout.LayoutData.Add("Root", diagram.Nodes[DiagramObjectNames.ForestObject].Values.First().Name);
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
    }
}
