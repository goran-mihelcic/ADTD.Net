using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using Microsoft.ADTD.Data;

namespace Microsoft.ADTD.Data.ldap
{
    public class SelectData
    {
        private dxData _data;

        public IData Data { get { return _data; } }

        internal bool readServers { get; set; }
        internal bool agregateSites { get; set; }
        internal bool expandSL { get; set; }

        public int cnt = 0;
        static private int cpNum = 0;
        public bool allSites = true;
        public scopeItem SiteSelection;
        public string myReplSelection;
        public string mySiteSelection;
        public bool siteOK = false;

        public SelectData(dxData data)
        {
            _data = data;
        }

        public bool getData()
        {
            try
            {
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
            try
            {
                this.Data.Clear();

                System.Text.StringBuilder objfilter;//= new System.Text.StringBuilder();

                List<scopeItem> sitList;
                if (this.allSites)
                    sitList = ldapReader.SiteList;
                else
                {
                    sitList = new List<scopeItem>();
                    sitList.Add(this.SiteSelection);
                }
                foreach (scopeItem siteResult in sitList)
                {
                    StringBuilder text = new StringBuilder();
                    StringBuilder comment = new StringBuilder();

                    this.cnt++;
                    string siteName = siteResult.Name;
                    typSiteOptions RemSiteOptions = (typSiteOptions)siteResult.Properties[ldapAttributes.SiteOptions];
                    dxShape newSite = new dxShape("Site", siteName, "Site");
                    newSite.AddAttribute(AttributeNames.SiteLocation, (siteResult.Properties[ldapAttributes.Location] ?? "").ToString());
                    newSite.AddAttribute(AttributeNames.gPLink, (siteResult.Properties[ldapAttributes.gPLink] ?? "").ToString());
                    newSite.AddAttribute(AttributeNames.GroupCaching, RemSiteOptions.Universal_Group_Membership_Caching);
                    newSite.AddAttribute(AttributeNames.ISTG, RemSiteOptions.ISTG);
                    text.AppendLine(String.Format("ISTG: {0}", RemSiteOptions.ISTG));
                    comment.AppendLine(String.Format("{0}\n{1}", siteName, text));

                    newSite.Text = text.ToString();
                    newSite.Comment = comment.ToString();

                    this.Data.AddShape(newSite);

                    // Read Site Servers
                    objfilter = null;
                    objfilter = new System.Text.StringBuilder();
                    objfilter.Append("(objectClass=nTDSDSA)");
                    //ldapSearch serverSearch = new ldapSearch(siteResult.GetDirectoryEntry());
                    ldapSearch serverSearch = new ldapSearch(siteResult.itemDE);
                    serverSearch.filter = objfilter.ToString();
                    serverSearch.PropertiesToLoad = "objectCategory,serverReference,options";
                    List<searchResult> serverResults = serverSearch.getAll();

                    foreach (searchResult serverResult in serverResults)
                    {
                        text = new StringBuilder();
                        comment = new StringBuilder();

                        text.AppendLine(siteName);

                        this.cnt++;
                        string serverRef = "";
                        bool rodc = false;
                        string objCat = (serverResult.Properties["objectCategory"] ?? "").ToString();
                        rodc = objCat.Contains("NTDS-DSA-RO");

                        ldapSearch srvSearch = new ldapSearch(ldapReader.getDirectoryEntry(ldapReader.ldapPrefix + ldapReader.GetParentComponent(serverResult.Path)))
                        {
                            scope = SearchScope.Base,
                            PropertiesToLoad = "serverReference",
                            filter = "(objectClass=*)"
                        };
                        searchResult srvResult = srvSearch.getOne();

                        serverRef = srvResult.Properties["serverReference"].ToString();
                        ldapSearch cmpSearch = new ldapSearch(ldapReader.getDirectoryEntry(ldapReader.ldapPrefix + serverRef))
                        {
                            scope = SearchScope.Base,
                            PropertiesToLoad = "name,dNSHostName,operatingSystem,operatingSystemServicePack",
                            filter = "(objectClass=*)"
                        };
                        searchResult cmpResult = cmpSearch.getOne();

                        bool isGC = (Convert.ToInt32(serverResult.Properties["options"]) & 1) == 1;
                        string master = "Domain Controller";
                        if (rodc)
                            master = "ReadOnlyDomain Controller.41";
                        else if (isGC)
                            master = "Global Catalog";

                        string serverName = cmpResult.Properties["name"].ToString().Replace("CN=", "").ToLower();//computerDe.Name.Replace("CN=", "").ToLower();
                        comment.AppendLine(serverName);
                        dxShape newServer = new dxShape("DC", serverName, master);
                        newServer.AddAttribute(AttributeNames.DomainFQDN,
                            cmpResult.Properties["dNSHostName"].ToString()
                            .ToLower().Replace(serverName.ToLower() + ".", ""));
                        newServer.AddAttribute(AttributeNames.ServerFQDN, cmpResult.Properties["dNSHostName"].ToString());
                        newServer.AddAttribute(AttributeNames.MachineType, "UnKnown");
                        newServer.AddAttribute(AttributeNames.GlobalCatalog, isGC);
                        newServer.AddAttribute(AttributeNames.RODC, rodc);
                        string os, sp;
                        os = (cmpResult.Properties["operatingSystem"] ?? "Unknown").ToString();
                        sp = (cmpResult.Properties["operatingSystemServicePack"] ?? "").ToString();
                        newServer.AddAttribute(AttributeNames.OS, os + " " + sp);
                        string osShort = OsTranslator.OsTranslate(os + " " + sp);
                        newServer.AddAttribute(AttributeNames.OSShort, osShort);
                        text.AppendLine(osShort);
                        //newSite.addChild(newServer);
                        this.Data.AddShape(newServer, newSite);
                        //computerDe = null;
                        //serverDe = null;
                    }

                    //objdeResult.Close();
                }
                //objResults = null;

                //Read Site Links
                string strPath = ldapReader.ldapPrefix + "CN=IP,CN=Inter-Site Transports,CN=Sites," + ldapReader.ConfigNC;
                objfilter = new System.Text.StringBuilder();
                objfilter.Append("(objectClass=SiteLink)");
                string propList = "name,adspath,SiteList,cost,replInterval,options,schedule" +
                    ((ldapReader.ExSchemaVersion >= 10394) ? ",msExchCost" : "");

                ldapSearch slSearch = new ldapSearch(ldapReader.getDirectoryEntry(strPath));
                slSearch.filter = objfilter.ToString();
                slSearch.PropertiesToLoad = propList;
                List<searchResult> slResults = slSearch.getAll();

                foreach (searchResult slResult in slResults)
                {
                    string[] sites = castRVPC(slResult.Properties["SiteList"]);
                    if (sites.Any(s => this.Data.GetNode("Site", s) != null))
                    {
                        this.cnt++;
                        string slName = slResult.Properties["name"].ToString();
                        if (!this.allSites) sites = new string[] { "CN=" + this.SiteSelection.Name };
                        //Logger.logDebug(slName);

                        StringBuilder text = new StringBuilder();
                        StringBuilder comment = new StringBuilder();
                        comment.AppendLine(slName);
                        string master = "IP Site Link";
                        comment.AppendLine(String.Format("Cost: {0}", slResult.Properties["cost"]));
                        comment.AppendLine(String.Format("Interval: {0}", slResult.Properties["replInterval"]));
                        comment.AppendLine(String.Format("Change Notification: {0}", ((Convert.ToInt32(slResult.Properties["options"]) & 1) == 1)));
                        comment.AppendLine(String.Format("Schedule: {0}", getSchedule((byte[])slResult.Properties["schedule"], false)));

                        if (sites.Count() == 2)
                        {
                            string fromNode = sites[0].Split(',')[0].Replace("CN=", "");
                            dxShape from = this.Data.GetNode("Site", fromNode);
                            string toNode = sites[1].Split(',')[0].Replace("CN=", "");
                            dxShape to = this.Data.GetNode("Site", toNode);
                            dxConnection newSL = new dxConnection("SL", slName, from, to, master);
                            newSL.Comment = comment.ToString();
                            newSL.Text = text.ToString();
                            this.Data.AddConnection(newSL);
                        }
                        else if (expandSL && sites.Count() > 2)
                        {
                            for (int x = 0; x < sites.Count(); x++)
                            {
                                string toNode = sites[x].ToString();
                                dxShape to = this.Data.GetNode("Site", toNode);
                                for (int y = x + 1; y < sites.Count(); y++)
                                {
                                    this.cnt++;
                                    string fromNode = sites[y].ToString();
                                    dxShape from = this.Data.GetNode("Site", fromNode);
                                    dxConnection newSL = new dxConnection("SL", slName, from, to, master);
                                    newSL.Comment = comment.ToString();
                                    newSL.Text = text.ToString();
                                    this.Data.AddConnection(newSL);
                                }
                            }
                        }
                        else
                        {
                            cpNum++;
                            dxShape cp = new dxShape("CP", cpNum.ToString(), "Site", Visio.Arrange.LayoutType.Node);
                            cp.Size(5, 5);
                            foreach (string fromNode in sites)
                            {
                                dxShape from = this.Data.GetNode("Site", fromNode);
                                dxConnection newSL = new dxConnection("SL", slName, from, cp, master);
                                newSL.Comment = comment.ToString();
                                newSL.Text = text.ToString();
                                this.Data.AddConnection(newSL);
                            }
                        }

                    }
                }

                foreach (scopeItem siteResult in sitList)
                {
                    // Read Site Servers
                    objfilter = null;
                    objfilter = new System.Text.StringBuilder();
                    objfilter.Append("(objectClass=nTDSDSA)");
                    //ldapSearch serverSearch = new ldapSearch(siteResult.GetDirectoryEntry());
                    ldapSearch serverSearch = new ldapSearch(siteResult.itemDE);
                    serverSearch.filter = objfilter.ToString();
                    serverSearch.PropertiesToLoad = "objectCategory,serverReference,options";
                    List<searchResult> serverResults = serverSearch.getAll();

                    foreach (searchResult serverResult in serverResults)
                    {
                        ldapSearch srvSearch = new ldapSearch(ldapReader.getDirectoryEntry(ldapReader.ldapPrefix + ldapReader.GetParentComponent(serverResult.Path)))
                        {
                            scope = SearchScope.Base,
                            PropertiesToLoad = "serverReference",
                            filter = "(objectClass=*)"
                        };
                        searchResult srvResult = srvSearch.getOne();

                        string serverRef = srvResult.Properties["serverReference"].ToString();
                        ldapSearch cmpSearch = new ldapSearch(ldapReader.getDirectoryEntry(ldapReader.ldapPrefix + serverRef))
                        {
                            scope = SearchScope.Base,
                            PropertiesToLoad = "name,dNSHostName",
                            filter = "(objectClass=*)"
                        };
                        searchResult cmpResult = cmpSearch.getOne();

                        string toServer = cmpResult.Properties["name"].ToString().Replace("CN=", "").ToLower();


                        objfilter = null;
                        objfilter = new System.Text.StringBuilder();
                        objfilter.Append("(objectClass=nTDSConnection)");
                        ldapSearch coSearch = new ldapSearch(serverResult.GetDirectoryEntry());
                        coSearch.filter = objfilter.ToString();
                        coSearch.PropertiesToLoad = "name,fromServer,options,schedule,transportType,ms-DS-ReplicatesNCReason";
                        List<searchResult> coResults = coSearch.getAll();
                        foreach (searchResult coResult in coResults)
                        {
                            string fromServer = ldapReader.serverNameFromNTDSSettings(coResult.Properties["fromServer"].ToString()).ToLower();
                            dxShape from = this.Data.GetNode("DC", fromServer);
                            dxShape to = this.Data.GetNode("DC", toServer);
                            string coName = coResult.Properties["name"].ToString();
                            StringBuilder comment = new StringBuilder();

                            HashSet<string> ncs = new HashSet<string>();
                            if (coResult.Properties["ms-DS-ReplicatesNCReason"] != null)
                            {
                                if ((coResult.Properties["ms-DS-ReplicatesNCReason"]).GetType() == typeof(string))
                                {
                                    string reason = (string)coResult.Properties["ms-DS-ReplicatesNCReason"];
                                    string nc = reason.Substring(reason.LastIndexOf(':') + 1);
                                    ncs.Add(nc);
                                }
                                else
                                    foreach (string reason in (ResultPropertyValueCollection)(coResult.Properties["ms-DS-ReplicatesNCReason"]))
                                    {
                                        string nc = reason.Substring(reason.LastIndexOf(':') + 1);
                                        ncs.Add(nc);
                                    }
                            }
                            string master = "Directory Replication IntraSit"; //****** Intra and Intersite repl...
                            comment.AppendLine(String.Join("\n", ncs));
                            comment.AppendLine(getSchedule((byte[])coResult.Properties["schedule"]));
                            dxConnection newCO = new dxConnection(coName, "CO", from, to, master, false);
                            newCO.Comment = comment.ToString();
                        }
                    }
                }

                if (agregateSites) doAggregate();
            }
            catch (Exception exception)
            {
                Logger.logDebug(exception.ToString());
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void doAggregate()
        {
            //try
            //{
            //    IEnumerable<IGrouping<dxNode, dxNode>> emptySites = from site in this.diagram.Nodes[DiagramObjectNames.SiteObject].Values
            //                                                        where site.Children.Count == 0 && site.Connected.Count == 1
            //                                                        group site by site.Connected[0] into g
            //                                                        select g;
            //    List<IGrouping<dxNode, dxNode>> groups4Aggregate = (from gr in emptySites
            //                                                        where gr.Count() > 1
            //                                                        select gr).ToList();
            //    foreach (IGrouping<dxNode, dxNode> gr in groups4Aggregate)
            //    {
            //        foreach (dxNode site in gr)
            //        {
            //            dxEdge siteLink = diagram.Edges[DiagramObjectNames.SiteLinkObject].Where(sl =>
            //                (sl.FromNode == site && sl.ToNode == gr.Key) ||
            //                (sl.FromNode == gr && sl.ToNode == site)).First();
            //            diagram.Edges[DiagramObjectNames.SiteLinkObject].Remove(siteLink);
            //            diagram.Nodes[DiagramObjectNames.SiteObject].Remove(diagram.Nodes[DiagramObjectNames.SiteObject].Where(s => s.Value == site).First().Key);
            //        }
            //        this.cnt++;
            //        dxNode newSite = new dxNode(this.cnt.ToString(), "Aggregate Sites-" + this.cnt.ToString(), DiagramObjectNames.SiteObject);
            //        newSite.addAttribute(AttributeNames.GroupCaching, false);
            //        newSite.addAttribute(AttributeNames.ISTG, "");
            //        newSite.addAttribute(AttributeNames.AggregateCount, gr.Count().ToString());
            //        this.diagram.addNode(newSite);

            //        dxEdge tmpSL = new dxEdge(this.cnt.ToString(), "Aggregated SiteLink", DiagramObjectNames.SiteLinkObject);
            //        tmpSL.addAttribute(AttributeNames.ReplicationInterval, "");
            //        tmpSL.addAttribute(AttributeNames.Cost, "");
            //        tmpSL.addAttribute(AttributeNames.InterSiteChangeNotification, false);
            //        tmpSL.addAttribute(AttributeNames.SLSchedule, "");
            //        tmpSL.addAttribute(AttributeNames.DisplayName, "Aggregated SiteLink");
            //        tmpSL.FromNode = gr.Key;
            //        tmpSL.ToNode = newSite;
            //        tmpSL.FromNode.addEdge(tmpSL);
            //        tmpSL.ToNode.addEdge(tmpSL);
            //        this.diagram.addEdge(tmpSL);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.logDebug(ex.ToString());
            //}
        }

        private string getTransportType(string trTapeDN)
        {
            string TransportType = trTapeDN.Split(',')[0];

            switch (TransportType)
            {
                case "cn=ip":
                    return "IP";
                case "cn=smtp":
                    return "SMTP";
                default:
                    return "RPC";
            }
        }

        private string[] castRVPC(object input)
        {
            if (input != null)
            {
                if (input.GetType() == typeof(ResultPropertyValueCollection))
                {
                    string list = "";
                    foreach (string i in (ResultPropertyValueCollection)input)
                    {
                        list = list + i.Substring(3, (i.IndexOf(',') - 3)) + "\t";
                    }
                    list = list.Remove(list.LastIndexOf('\t'));
                    return list.Split('\t');
                }
                else
                    return new string[1] { (string)input };
            }
            else return null;
        }

        private string getSchedule(byte[] Schedule, bool co = true)
        {
            UnicodeEncoding unicode = new UnicodeEncoding();

            const string strOnceEvery3Hours = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0 1 0 0";
            const string str24Hour4TimesPerHour = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F";
            const string str24Hour2TimesPerHour = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5";
            const string str24Hour1TimesPerHour = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1";
            const string str24Hour0TimesPerHour = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

            const string str24Hour4TimesPerHourRODC = "";
            const string str24Hour1TimesPerHourRODC = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1 F1";
            const string str24Hour2TimesPerHourRODC = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5 F5";
            const string str24Hour0TimesPerHourRODC = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F F";

            const string str24by7 = "BC 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 14 0 0 0 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";

            string strArray = "";

            if (Schedule != null)
            {
                foreach (byte b in Schedule)
                {
                    strArray = strArray + b.ToString("X") + " ";
                }
                strArray = strArray.Trim();
            }
            else strArray = "";

            string ScheduleOut = "custom";
            if (co)
            {
                if ((strArray == str24Hour0TimesPerHour) || (strArray == str24Hour0TimesPerHourRODC))
                    ScheduleOut = "none";

                if ((strArray == str24Hour1TimesPerHour) || (strArray == str24Hour1TimesPerHourRODC))
                    ScheduleOut = "once per hour";

                if ((strArray == str24Hour2TimesPerHour) || (strArray == str24Hour2TimesPerHourRODC))
                    ScheduleOut = "twice per hour";

                if ((strArray == str24Hour4TimesPerHour) || (strArray == str24Hour4TimesPerHourRODC))
                    ScheduleOut = "four times per hour";

                if (strArray == strOnceEvery3Hours)
                    ScheduleOut = "once every 3 hours";
            }
            else
                if ((strArray == str24by7) || (strArray == ""))
                ScheduleOut = "24 x 7";

            return ScheduleOut;
        }
    }
}
