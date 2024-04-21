namespace Mihelcic.Net.Visio.Data
{
    using Mihelcic.Net.Visio.Common;
    using Mihelcic.Net.Visio.Arrange;
    using Mihelcic.Net.Visio.Xml;
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Site topology data reader
    /// </summary>
    public class SiteDataReader : IDataReader
    {
        #region Private Fields

        static private int cpNum = 0;
        private readonly dxData _data;
        private bool _connected;
        private bool _readServers;
        private bool _agregateSites;
        private bool _expandSL;
        private bool _allSites;
        private ScopeItem _siteSelection;
        private LoginInfo _loginInfo;

        #endregion

        // Data
        // Connected
        #region Public Properties

        /// <summary>
        /// Collected Data
        /// </summary>
        public IData Data { get { return _data; } }

        /// <summary>
        /// Connection to data source status 
        /// </summary>
        public bool Connected { get { return true; } }

        #endregion

        /// <summary>
        /// Create SiteDataReader instance
        /// </summary>
        public SiteDataReader()
        {
            _data = new dxData();
            _connected = false;
        }

        // Read
        // Connect
        #region Public Methods

        /// <summary>
        /// Read Site Topology Data
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool? Read(Dictionary<string, object> parameters, ReportProgress statusCallback)
        {
            if (parameters == null)
                return null;
            try
            {
                if (parameters.ContainsKey(Mihelcic.Net.Visio.Data.ParameterNames.readServers))
                    _readServers = (bool)(parameters[Mihelcic.Net.Visio.Data.ParameterNames.readServers]);
                else
                    _readServers = true;
                if (parameters.ContainsKey(Mihelcic.Net.Visio.Data.ParameterNames.agregateSites))
                    _agregateSites = (bool)(parameters[ParameterNames.agregateSites]);
                else
                    _agregateSites = false;
                if (parameters.ContainsKey(Mihelcic.Net.Visio.Data.ParameterNames.expandSiteLinks))
                    _expandSL = (bool)(parameters[ParameterNames.expandSiteLinks]);
                else
                    _expandSL = false;
                if (parameters.ContainsKey(ParameterNames.LoginInfo))
                    _loginInfo = (LoginInfo)parameters[ParameterNames.LoginInfo];
                //if (parameters.ContainsKey(Mihelcic.Net.Visio.Data.ParameterNames.expandSiteLinks))
                //    _allSites = (bool)(parameters[ParameterNames.allSites]);
                //else
                _allSites = true;
                _siteSelection = null;
                bool? result = GetData(statusCallback);

                return true;
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                return null;
            }
        }

        public bool Connect(Dictionary<string, object> parameters, ReportProgress statusCallback)
        {
            if (parameters == null)
                return false;
            _connected = Mihelcic.Net.Visio.Data.LdapReader.VerifyAuthentication(true, true, true, (LoginInfo)parameters[ParameterNames.LoginInfo], statusCallback);
            return _connected;
        }

        #endregion

        #region Private Methods

        private bool GetData(ReportProgress statusCallback)
        {
            try
            {
                Logger.TraceVerbose(TrcStrings.StartSiteRdr);
                return ReadSiteData(statusCallback);
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                return false;
            }
        }

        private bool ReadSiteData(ReportProgress statusCallback)
        {
            bool result = false;
            try
            {
                this.Data.Clear();

                System.Text.StringBuilder objfilter;

                List<ScopeItem> siteList;
                if (_allSites)
                    siteList = LdapReader.SiteList;
                else
                {
                    siteList = new List<ScopeItem>();
                    if (_siteSelection != null)
                        siteList.Add(_siteSelection);
                }
                int i = 1;
                foreach (ScopeItem siteResult in siteList)
                {
                    statusCallback($"{Strings.StatusDrawingSites} ({i})");
                    String text = String.Empty;
                    StringBuilder comment = new StringBuilder();

                    string siteName = siteResult.Name;
                    TypSiteOptions RemSiteOptions = (TypSiteOptions)siteResult.Properties[LdapAttrNames.SiteOptions];
                    dxShape newSite = new dxShape(ShapeRsx.SitePfx, siteName, ShapeRsx.SiteShape, LayoutType.Matrix);
                    newSite.AddAttribute(AttributeNames.SiteLocation, (siteResult.Properties[LdapAttrNames.Location] ?? String.Empty).ToString());
                    newSite.AddAttribute(AttributeNames.gPLink, (siteResult.Properties[LdapAttrNames.gPLink] ?? String.Empty).ToString());
                    newSite.AddAttribute(AttributeNames.GroupCaching, RemSiteOptions.Universal_Group_Membership_Caching);
                    newSite.AddAttribute(AttributeNames.ISTG, RemSiteOptions.ISTG);
                    comment.AppendLine(siteName);
                    if (!String.IsNullOrWhiteSpace(RemSiteOptions.ISTG))
                    {
                        text = String.Format(Strings.IstgOutput, RemSiteOptions.ISTG);
                        comment.AppendLine(text);
                        newSite.Text = text.ToString();
                    }
                    newSite.Comment = comment.ToString();
                    newSite.Header = siteName;

                    newSite.HeaderStyle = TextStyle.Header1;

                    newSite.LayoutParams = new SiteLayoutParameters().GetLayoutParameters();

                    this.Data.AddShape(newSite);

                    // Read Site Servers
                    objfilter = null;
                    objfilter = new System.Text.StringBuilder();
                    objfilter.Append($"({LdapAttrNames.ObjectClass}={LdapStrings.NtDsDsaClass})");
                    LdapSearch serverSearch = new LdapSearch(siteResult.ItemDE)
                    {
                        filter = objfilter.ToString(),
                        PropertiesToLoad = $"{LdapAttrNames.ObjectCategory},{LdapAttrNames.options}"
                    };
                    List<SearchResult> serverResults = serverSearch.GetAll();

                    int j = 1;
                    foreach (SearchResult serverResult in serverResults)
                    {
                        statusCallback($"{Strings.StatusDrawingSites} ({i}) {Strings.StatusAddingServer} ({j})");
                        StringBuilder textSl = new StringBuilder();
                        comment = new StringBuilder();

                        comment.AppendLine(String.Format(Strings.SiteComment, siteName));

                        string serverRef = String.Empty;
                        bool rodc = false;
                        string objCat = (serverResult.Properties[LdapAttrNames.ObjectCategory] ?? String.Empty).ToString();
                        rodc = objCat.Contains(LdapStrings.NtDsDsaRoClass);

                        LdapSearch srvSearch = new LdapSearch(LdapReader.GetDirectoryEntry(LdapReader.LdapPrefixS + LdapReader.GetParentComponent(serverResult.Path), _loginInfo))
                        {
                            Scope = SearchScope.Base,
                            PropertiesToLoad = LdapAttrNames.ServerReference,
                            filter = $"({LdapAttrNames.ObjectClass}=*)"
                        };
                        SearchResult srvResult = srvSearch.GetOne();

                        serverRef = srvResult.Properties[LdapAttrNames.ServerReference].ToString();
                        LdapSearch cmpSearch = new LdapSearch(LdapReader.GetDirectoryEntry(LdapReader.LdapPrefixS + serverRef, _loginInfo))
                        {
                            Scope = SearchScope.Base,
                            PropertiesToLoad = $"{LdapAttrNames.Name},{LdapAttrNames.DNSHostName},{LdapAttrNames.OperatingSystem},{LdapAttrNames.OperatingSystemSP}",
                            filter = $"({LdapAttrNames.ObjectClass}=*)"
                        };
                        SearchResult cmpResult = cmpSearch.GetOne();

                        bool isGC = (Convert.ToInt32(serverResult.Properties[LdapAttrNames.options]) & 1) == 1;

                        if (isGC) comment.AppendLine(Strings.GCComment);
                        if (rodc) comment.AppendLine(Strings.RodcComment);

                        string master = ShapeRsx.DCShape;
                        if (rodc)
                            master = ShapeRsx.RODCShape;
                        else if (isGC)
                            master = ShapeRsx.GCShape;

                        string serverName = cmpResult.Properties[LdapAttrNames.Name].ToString().Replace(LdapStrings.CnComponent, String.Empty).ToLower();//computerDe.Name.Replace("CN=", String.Empty).ToLower();
                        comment.AppendLine(String.Format(Strings.NameComment, serverName));
                        dxShape newServer = new dxShape(ShapeRsx.DCPfx, serverName, master, LayoutType.Node);
                        newServer.AddAttribute(AttributeNames.DomainFQDN,
                            cmpResult.Properties[LdapAttrNames.DNSHostName].ToString()
                            .ToLower().Replace(serverName.ToLower() + ".", String.Empty));
                        newServer.AddAttribute(AttributeNames.ServerFQDN, cmpResult.Properties[LdapAttrNames.DNSHostName].ToString());
                        newServer.AddAttribute(AttributeNames.MachineType, Strings.UnKnown);
                        newServer.AddAttribute(AttributeNames.GlobalCatalog, isGC);
                        newServer.AddAttribute(AttributeNames.RODC, rodc);
                        string os, sp;
                        os = (cmpResult.Properties[LdapAttrNames.OperatingSystem] ?? Strings.UnKnown).ToString();
                        sp = (cmpResult.Properties[LdapAttrNames.OperatingSystemSP] ?? String.Empty).ToString();
                        newServer.AddAttribute(AttributeNames.OS, os + " " + sp);
                        string osShort = OsTranslator.OsTranslate(os + " " + sp);
                        newServer.AddAttribute(AttributeNames.OSShort, osShort);
                        newServer.AddAttribute(AttributeNames.Site, siteName);
                        newServer.Header = serverName;
                        newServer.ColorCodedAttribute = AttributeNames.DomainFQDN;
                        newServer.SubShapeToColor = ShapeRsx.ShapeToColor;
                        textSl.AppendLine(osShort);
                        comment.AppendLine(osShort);
                        newServer.Text = textSl.ToString();
                        newServer.Comment = comment.ToString();
                        newServer.HeaderStyle = TextStyle.SmallNormalBold;
                        newServer.TextStyle = TextStyle.SmallNormal;

                        //newSite.addChild(newServer);
                        newServer.LayoutParams = new dxLayoutParameters().GetLayoutParameters();
                        this.Data.AddShape(newServer, newSite);
                        j++;
                    }
                    i++;
                }

                //Read Site Links
                string strPath = $"{LdapReader.LdapPrefixS}{LdapStrings.IPTransportDn},{LdapReader.ConfigNC}";
                objfilter = new System.Text.StringBuilder();
                objfilter.Append($"({LdapAttrNames.ObjectClass}={LdapStrings.SiteLinkClass})");
                string propList = $"{LdapAttrNames.Name},{LdapAttrNames.AdsPath},{LdapAttrNames.SiteList},{LdapAttrNames.Cost},{LdapAttrNames.ReplInterval},{LdapAttrNames.options},{LdapAttrNames.Schedule}{((LdapReader.GetExSchemaVersion(_loginInfo) >= 10394) ? ",msExchCost" : String.Empty)}";

                LdapSearch slSearch = new LdapSearch(LdapReader.GetDirectoryEntry(strPath, _loginInfo))
                {
                    filter = objfilter.ToString(),
                    PropertiesToLoad = propList
                };
                List<SearchResult> slResults = slSearch.GetAll();

                i = 1;
                foreach (SearchResult slResult in slResults)
                {
                    statusCallback($"{Strings.StatusDrawingSiteLinks} ({i})");
                    string[] sites = CastRVPC(slResult.Properties[LdapAttrNames.SiteList]);
                    if (sites.Any(s => this.Data.GetNode(ShapeRsx.SitePfx, s) != null))
                    {
                        string slName = slResult.Properties[LdapAttrNames.Name].ToString();
                        if (!_allSites) sites = new string[] { $"{LdapStrings.CnComponent}{_siteSelection.Name}" };

                        StringBuilder text = new StringBuilder();
                        StringBuilder comment = new StringBuilder();
                        comment.AppendLine(slName);
                        string master = ShapeRsx.SiteLinkShape;
                        comment.AppendLine(String.Format(Strings.CostComment, slResult.Properties[LdapAttrNames.Cost]));
                        comment.AppendLine(String.Format(Strings.IntervalComment, slResult.Properties[LdapAttrNames.ReplInterval]));
                        comment.AppendLine(String.Format(Strings.ChangeNotifComment, ((Convert.ToInt32(slResult.Properties[LdapAttrNames.options]) & 1) == 1)));
                        comment.AppendLine(String.Format(Strings.ScheduleComent, GetSchedule((byte[])slResult.Properties[LdapAttrNames.Schedule], false)));

                        if (sites.Count() == 2)
                        {
                            string fromNode = sites[0].Split(',')[0].Replace(LdapStrings.CnComponent, String.Empty);
                            dxShape from = this.Data.GetNode(ShapeRsx.SitePfx, fromNode);
                            string toNode = sites[1].Split(',')[0].Replace(LdapStrings.CnComponent, String.Empty);
                            dxShape to = this.Data.GetNode(ShapeRsx.SitePfx, toNode);
                            dxConnection newSL = new dxConnection("SL", slName, from, to, master)
                            {
                                Comment = comment.ToString(),
                                Text = text.ToString(),
                                HeaderStyle = TextStyle.NormalItalic
                            };
                            this.Data.AddConnection(newSL);
                        }
                        else if (_expandSL && sites.Count() > 2)
                        {
                            for (int x = 0; x < sites.Count(); x++)
                            {
                                string toNode = sites[x].ToString();
                                dxShape to = this.Data.GetNode(ShapeRsx.SitePfx, toNode);
                                for (int y = x + 1; y < sites.Count(); y++)
                                {
                                    string fromNode = sites[y].ToString();
                                    dxShape from = this.Data.GetNode(ShapeRsx.SitePfx, fromNode);
                                    dxConnection newSL = new dxConnection(ShapeRsx.SiteLinkPfx, $"{slName}-{y}", from, to, master)
                                    {
                                        Comment = comment.ToString(),
                                        Text = text.ToString(),
                                        HeaderStyle = TextStyle.NormalItalic,
                                        LayoutParams = new dxLayoutParameters().GetLayoutParameters()
                                    };
                                    this.Data.AddConnection(newSL);
                                }
                            }
                        }
                        else
                        {
                            int slNum = 0;
                            cpNum++;
                            dxShape cp = new dxShape(ShapeRsx.ConnPointPfx, cpNum.ToString(), ShapeRsx.SiteShape, Visio.Arrange.LayoutType.Node);
                            cp.Size(5, 5);
                            cp.LayoutParams = new dxLayoutParameters().GetLayoutParameters();
                            this.Data.AddShape(cp);
                            foreach (string fromNode in sites)
                            {
                                slNum++;
                                dxShape from = this.Data.GetNode(ShapeRsx.SitePfx, fromNode);
                                dxConnection newSL = new dxConnection(ShapeRsx.SiteLinkPfx, $"{slName}-{slNum}", from, cp, master)
                                {
                                    Comment = comment.ToString(),
                                    Text = text.ToString(),
                                    LayoutParams = new dxLayoutParameters().GetLayoutParameters()
                                };
                                this.Data.AddConnection(newSL);
                            }
                        }

                    }
                    i++;
                }

                i = 1;
                foreach (ScopeItem siteResult in siteList)
                {
                    // Read Site Servers
                    objfilter = null;
                    objfilter = new System.Text.StringBuilder();
                    objfilter.Append($"({LdapAttrNames.ObjectClass}={LdapStrings.NtDsDsaClass})");
                    LdapSearch serverSearch = new LdapSearch(siteResult.ItemDE)
                    {
                        filter = objfilter.ToString(),
                        PropertiesToLoad = $"{LdapAttrNames.ObjectCategory},{LdapAttrNames.ServerReference},{LdapAttrNames.options}"
                    };
                    List<SearchResult> serverResults = serverSearch.GetAll();

                    foreach (SearchResult serverResult in serverResults)
                    {
                        LdapSearch srvSearch = new LdapSearch(LdapReader.GetDirectoryEntry($"{LdapReader.LdapPrefixS}{LdapReader.GetParentComponent(serverResult.Path)}", _loginInfo))
                        {
                            Scope = SearchScope.Base,
                            PropertiesToLoad = LdapAttrNames.ServerReference,
                            filter = $"({LdapAttrNames.ObjectClass}=*)"
                        };
                        SearchResult srvResult = srvSearch.GetOne();

                        string serverRef = srvResult.Properties[LdapAttrNames.ServerReference].ToString();
                        LdapSearch cmpSearch = new LdapSearch(LdapReader.GetDirectoryEntry(LdapReader.LdapPrefixS + serverRef, _loginInfo))
                        {
                            Scope = SearchScope.Base,
                            PropertiesToLoad = $"{LdapAttrNames.Name},{LdapAttrNames.DNSHostName}",
                            filter = $"({LdapAttrNames.ObjectClass}=*)"
                        };
                        SearchResult cmpResult = cmpSearch.GetOne();

                        string toServer = cmpResult.Properties[LdapAttrNames.Name].ToString().Replace(LdapStrings.CnComponent, String.Empty).ToLower();


                        objfilter = null;
                        objfilter = new System.Text.StringBuilder();
                        objfilter.Append($"({LdapAttrNames.ObjectClass}={LdapStrings.nTDSConnectionClass})");
                        LdapSearch coSearch = new LdapSearch(serverResult.GetDirectoryEntry())
                        {
                            filter = objfilter.ToString(),
                            PropertiesToLoad = $"{LdapAttrNames.Name},{LdapAttrNames.FromServer},{LdapAttrNames.options},{LdapAttrNames.Schedule},{LdapAttrNames.TransportType},{LdapAttrNames.MsDSReplicatesNCReason}"
                        };
                        List<SearchResult> coResults = coSearch.GetAll();
                        foreach (SearchResult coResult in coResults)
                        {
                            statusCallback($"{Strings.StatusDrawingConnectionObjects} ({i})");
                            string fromServer = LdapReader.ServerNameFromNTDSSettings(coResult.Properties[LdapAttrNames.FromServer].ToString()).ToLower();
                            dxShape from = this.Data.GetNode(ShapeRsx.DCPfx, fromServer);
                            dxShape to = this.Data.GetNode(ShapeRsx.DCPfx, toServer);
                            string coName = coResult.Properties[LdapAttrNames.Name].ToString();
                            StringBuilder comment = new StringBuilder();

                            HashSet<string> ncs = new HashSet<string>();
                            if (coResult.Properties[LdapAttrNames.MsDSReplicatesNCReason] != null)
                            {
                                if ((coResult.Properties[LdapAttrNames.MsDSReplicatesNCReason]).GetType() == typeof(string))
                                {
                                    string reason = (string)coResult.Properties[LdapAttrNames.MsDSReplicatesNCReason];
                                    string nc = reason.Substring(reason.LastIndexOf(':') + 1);
                                    ncs.Add(nc);
                                }
                                else
                                    foreach (string reason in (ResultPropertyValueCollection)(coResult.Properties[LdapAttrNames.MsDSReplicatesNCReason]))
                                    {
                                        string nc = reason.Substring(reason.LastIndexOf(':') + 1);
                                        ncs.Add(nc);
                                    }
                            }

                            string master = ShapeRsx.IntraConnShape;
                            if (from.Attributes[AttributeNames.Site] != to.Attributes[AttributeNames.Site])
                                master = ShapeRsx.InterConnShape;
                            comment.AppendLine(String.Join("\n", ncs));
                            comment.AppendLine(GetSchedule((byte[])coResult.Properties[LdapAttrNames.Schedule]));
                            dxConnection newCO = new dxConnection(coName, from, to, master, false)
                            {
                                Comment = comment.ToString(),
                                LayoutParams = new dxLayoutParameters().GetLayoutParameters()
                            };
                            this.Data.AddConnection(newCO);
                            i++;
                        }
                    }
                }

                if (_agregateSites) DoAggregate();
                result = true;
            }
            catch (Exception exception)
            {
                Logger.TraceException(exception.ToString());
            }
            return result;
        }

        private string[] CastRVPC(object input)
        {
            if (input != null)
            {
                if (input.GetType() == typeof(ResultPropertyValueCollection))
                {
                    string list = String.Empty;
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

        private string GetSchedule(byte[] Schedule, bool co = true)
        {
            _ = new UnicodeEncoding();

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

            string strArray = String.Empty;

            if (Schedule != null)
            {
                foreach (byte b in Schedule)
                {
                    strArray = strArray + b.ToString("X") + " ";
                }
                strArray = strArray.Trim();
            }
            else strArray = String.Empty;

            string ScheduleOut = Strings.ScheduleCustom;
            if (co)
            {
                if ((strArray == str24Hour0TimesPerHour) || (strArray == str24Hour0TimesPerHourRODC))
                    ScheduleOut = Strings.ScheduleNone;

                if ((strArray == str24Hour1TimesPerHour) || (strArray == str24Hour1TimesPerHourRODC))
                    ScheduleOut = Strings.Schedule1PerHour;

                if ((strArray == str24Hour2TimesPerHour) || (strArray == str24Hour2TimesPerHourRODC))
                    ScheduleOut = Strings.Schedule2PerHour;

                if ((strArray == str24Hour4TimesPerHour) || (strArray == str24Hour4TimesPerHourRODC))
                    ScheduleOut = Strings.Schedule4PerHour;

                if (strArray == strOnceEvery3Hours)
                    ScheduleOut = Strings.Schedule3Hours;
            }
            else
                if ((strArray == str24by7) || (strArray == String.Empty))
                ScheduleOut = Strings.ScheduleNonStop;

            return ScheduleOut;
        }

        private void DoAggregate()
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
        #endregion
    }
}
