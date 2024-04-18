using Mihelcic.Net.Visio.Common;
using Mihelcic.Net.Visio.Arrange;
using Mihelcic.Net.Visio.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mihelcic.Net.Visio.Data
{
    /// <summary>
    /// Reads Site Topology data
    /// </summary>
    public class DomainDataReader : IDataReader
    {
        #region private fields

        private readonly dxData _data;
        private bool _readServers;
        private bool _useGc;
        private bool _connected;
        private LoginInfo _loginInfo;

        #endregion

        // Data
        // Connected
        #region Public Properties

        /// <summary>
        /// Collected Data container
        /// </summary>
        public IData Data { get { return _data; } }

        /// <summary>
        /// Connected to Data Source
        /// </summary>
        public bool Connected { get { return _connected; } }

        #endregion

        /// <summary>
        /// Create new instance of DomainDataReader
        /// </summary>
        public DomainDataReader()
        {
            _data = new dxData();
            _connected = false;
        }

        // Read
        // Connect
        #region Public Methods

        /// <summary>
        /// Read data 
        /// </summary>
        /// <param name="parameters">Dictionary of reader parameters</param>
        /// <returns>Success of read</returns>
        public bool? Read(Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return null;
            try
            {
                if (parameters.ContainsKey(Mihelcic.Net.Visio.Data.ParameterNames.readServers))
                    _readServers = (bool)(parameters[Mihelcic.Net.Visio.Data.ParameterNames.readServers]);
                else
                    _readServers = true;
                if (parameters.ContainsKey(Mihelcic.Net.Visio.Data.ParameterNames.useGC))
                    _useGc = (bool)(parameters[Mihelcic.Net.Visio.Data.ParameterNames.useGC]);
                else
                    _useGc = false;
                if (parameters.ContainsKey(ParameterNames.LoginInfo))
                    _loginInfo = (LoginInfo)parameters[ParameterNames.LoginInfo];

                bool? result = GetData();

                return result;
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine(TrcStrings.Exception, ex.Message as object);
                return null;
            }
        }

        /// <summary>
        /// Connect to data source
        /// </summary>
        /// <param name="parameters">Reader Parameters</param>
        /// <returns>Success</returns>
        public bool Connect(Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters[ParameterNames.LoginInfo] == null || !(parameters[ParameterNames.LoginInfo] is LoginInfo))
                return false;
            _connected = Mihelcic.Net.Visio.Data.LdapReader.VerifyAuthentication(true, false, false, (LoginInfo)parameters[ParameterNames.LoginInfo]);
            return _connected;
        }

        #endregion

        #region Private Methods
        private bool GetData()
        {
            try
            {
                Logger.TraceVerbose(TrcStrings.StartDomainRdr);
                ReadDomainData();
                return true;
            }
            catch (Exception ex)
            {
                Logger.TraceVerbose(ex.ToString());
                Debug.WriteLine(TrcStrings.Exception, ex.Message as object);
                return false;
            }
        }

        private void ReadDomainData()
        {
            try
            {
                HashSet<string> realms = new HashSet<string>();
                this.Data.Clear();
                int schemaVersion = 0;
                string ldapPrefix = _useGc ? LdapReader.GCPrefix : LdapReader.LdapPrefixS;

                string forestName = LdapReader.ForestRoot.Name;

                string strObject = $"{LdapReader.LdapPrefixS}{LdapStrings.PartitionsCn},{LdapReader.ConfigNC}";
                LdapSearch partitionsSearch = new LdapSearch(LdapReader.GetDirectoryEntry(strObject, _loginInfo))
                {
                    Scope = SearchScope.Base,
                    PropertiesToLoad = $"{LdapAttrNames.FSMORoleOwner},{LdapAttrNames.msDSBehaviorVersion}"
                };
                SearchResult partResult = partitionsSearch.GetOne();

                LdapSearch schemaSearch = new LdapSearch(LdapReader.GetDirectoryEntry($"{LdapReader.LdapPrefixS}{LdapReader.SchemaNC}", _loginInfo))
                {
                    Scope = SearchScope.Base,
                    PropertiesToLoad = $"{LdapAttrNames.FSMORoleOwner},{LdapAttrNames.ObjectVersion}"
                };
                SearchResult schResult = schemaSearch.GetOne();

                string schemaMaster = (new LdapSearch()).SearchForReference(schResult.Properties[LdapAttrNames.FSMORoleOwner].ToString().Replace($"{LdapStrings.NtdsSettingsCn},", String.Empty), LdapAttrNames.DNSHostName, _loginInfo);
                string ffl = "No Data";
                if (partResult.Properties.ContainsKey(LdapAttrNames.msDSBehaviorVersion) && partResult.Properties[LdapAttrNames.msDSBehaviorVersion] != null)
                    ffl = GetForestFunctionalLevel(Convert.ToInt32(partResult.Properties[LdapAttrNames.msDSBehaviorVersion].ToString() ?? "0"));
                string domainNamingMaster = (new LdapSearch()).SearchForReference(partResult.Properties[LdapAttrNames.FSMORoleOwner].ToString().Replace($"{LdapStrings.NtdsSettingsCn},", String.Empty), LdapAttrNames.DNSHostName, _loginInfo);
                schemaVersion = Convert.ToInt32(schResult.Properties[LdapAttrNames.ObjectVersion]);
                String schemaVersionDecoded = GetSchemaVersion(schemaVersion.ToString());

                // ADD DOMAINS
                foreach (ScopeItem domainResult in LdapReader.DomainList)
                {
                    StringBuilder text = new StringBuilder();
                    StringBuilder comment = new StringBuilder();

                    string domainName = domainResult.Name;
                    realms.Add(domainName);
                    string dfl = GetDomainFunctionalLevel(Convert.ToInt32(domainResult.Properties[LdapAttrNames.msDSBehaviorVersion].ToString() ?? "0"),
                        Convert.ToInt32(domainResult.Properties[LdapAttrNames.nTMixedDomain].ToString() ?? "0"));
                    string pdc = (new LdapSearch()).SearchForReference(domainResult.Properties[LdapAttrNames.dnsRoot].ToString(), domainResult.Properties[LdapAttrNames.nCName].ToString(), LdapAttrNames.FSMORoleOwner, $"{LdapStrings.NtdsSettingsCn},", LdapAttrNames.DNSHostName, _loginInfo);
                    string rid = (new LdapSearch()).SearchForReference(domainResult.Properties[LdapAttrNames.dnsRoot].ToString(), $"{LdapStrings.RIDManagerDn},{domainResult.Properties[LdapAttrNames.nCName]}", LdapAttrNames.FSMORoleOwner, $"{LdapStrings.NtdsSettingsCn},", LdapAttrNames.DNSHostName, _loginInfo);
                    string im = new LdapSearch().SearchForReference(domainResult.Properties[LdapAttrNames.dnsRoot].ToString(), $"{LdapStrings.InfrastructureCn},{domainResult.Properties[LdapAttrNames.nCName]}", LdapAttrNames.FSMORoleOwner, $"{LdapStrings.NtdsSettingsCn},", LdapAttrNames.DNSHostName, _loginInfo);

                    dxShape newDomain = new dxShape(ShapeRsx.DomainPfx, domainResult.Name, ShapeRsx.DomainShape, LayoutType.Matrix);
                    newDomain.AddAttribute(AttributeNames.DomainFunctionalLevel, dfl);
                    newDomain.AddAttribute(AttributeNames.PDC, pdc);
                    newDomain.AddAttribute(AttributeNames.RIDMaster, rid);
                    newDomain.AddAttribute(AttributeNames.InfrastructureMaster, im);

                    if (domainName == forestName)
                    {
                        text.AppendLine(String.Format(Strings.FFLComment, ffl));
                        text.AppendLine(String.Format(Strings.DFLComment, dfl));
                        text.AppendLine(String.Format(Strings.SchMasterComment, schemaMaster));
                        text.AppendLine(String.Format(Strings.DnMasterComment, domainNamingMaster));
                    }
                    else
                    {
                        text.AppendLine(String.Format(Strings.DFLComment, dfl));
                    }
                    text.AppendLine(String.Format(Strings.PDCComment, pdc));
                    text.AppendLine(String.Format(Strings.RIDComment, rid));
                    text.AppendLine(String.Format(Strings.IMComment, im));

                    newDomain.Text = text.ToString();
                    newDomain.Comment = text.ToString();
                    newDomain.Header = domainName;
                    newDomain.LayoutParams = new DomainLayoutParameters().GetLayoutParameters();
                    int numLines = Regex.Matches(text.ToString(), Environment.NewLine).Count;
                    if (newDomain.LayoutParams.ContainsKey(LayoutParameters.TopMargin))
                        newDomain.LayoutParams[LayoutParameters.TopMargin] = 10 + (numLines * 5);
                    else
                        newDomain.LayoutParams.Add(LayoutParameters.TopMargin, 10 + (numLines * 5));

                    this.Data.AddShape(newDomain);
                }

                // ADD Servers
                if (_readServers)
                {

                    // Read Server Data
                    LdapSearch serverSearch = new LdapSearch(ldapPrefix + LdapReader.ConfigNC, _loginInfo);
                    StringBuilder objfilter = null;
                    objfilter = new System.Text.StringBuilder();
                    objfilter.Append($"({LdapAttrNames.ObjectClass}={LdapStrings.NtDsDsaClass})");
                    serverSearch.filter = objfilter.ToString();
                    serverSearch.Scope = SearchScope.Subtree;
                    serverSearch.PropertiesToLoad = $"{LdapAttrNames.ObjectCategory},{LdapAttrNames.ServerReference},{LdapAttrNames.options}";
                    List<SearchResult> serverResults = serverSearch.GetAll();
                    foreach (SearchResult serverResult in serverResults)
                    {
                        try
                        {
                            Logger.TraceVerbose(TrcStrings.SrvCollection, serverResult.Path);
                            StringBuilder text = new StringBuilder();
                            StringBuilder comment = new StringBuilder();

                            bool rodc = false;
                            string objCat = (serverResult.Properties[LdapAttrNames.ObjectCategory] ?? String.Empty).ToString();
                            rodc = objCat.Contains(LdapStrings.NtDsDsaRoClass);

                            LdapSearch srvSearch = new LdapSearch(LdapReader.GetDirectoryEntry($"{ldapPrefix}{LdapReader.GetParentComponent(serverResult.Path)}", _loginInfo))
                            {
                                Scope = SearchScope.Base,
                                PropertiesToLoad = LdapAttrNames.ServerReference,
                                filter = $"({LdapAttrNames.ObjectClass}=*)"
                            };
                            SearchResult srvResult = srvSearch.GetOne();
                            string serverRef = srvResult.GetPropertyString(LdapAttrNames.ServerReference);

                            Logger.TraceVerbose($"serverReference={serverRef}");
                            LdapSearch cmpSearch = new LdapSearch(LdapReader.GetDirectoryEntry($"{ldapPrefix}{serverRef}", _loginInfo))
                            {
                                Scope = SearchScope.Base,
                                PropertiesToLoad = $"{LdapAttrNames.Name},{LdapAttrNames.DNSHostName},{LdapAttrNames.OperatingSystem},{LdapAttrNames.OperatingSystemSP}",
                                filter = $"({LdapAttrNames.ObjectClass}=*)"
                            };
                            SearchResult cmpResult = cmpSearch.GetOne();
                            string dcName = cmpResult.Properties[LdapAttrNames.Name].ToString().Replace(LdapStrings.CnComponent, String.Empty).ToLower();
                            bool isGC = (Convert.ToInt32(serverResult.Properties[LdapAttrNames.options]) & 1) == 1;
                            string os = (cmpResult.Properties[LdapAttrNames.OperatingSystem] ?? Strings.UnKnown).ToString();
                            string sp = (cmpResult.Properties[LdapAttrNames.OperatingSystemSP] ?? String.Empty).ToString();
                            string osLong = $"{os} {sp}".TrimEnd();
                            string osShort = OsTranslator.OsTranslate(osLong);
                            string domainFQDN = null;// cmpResult.Properties[LdapAttrNames.DNSHostName].ToString().ToLower().Replace(dcName.ToLower() + ".", "");

                            string ncPath = GetDomainNC(serverRef);
                            ScopeItem item = LdapReader.DomainList.FirstOrDefault(d => d.Properties[LdapAttrNames.nCName].ToString().ToLowerInvariant() == ncPath.ToLowerInvariant());
                            if (item != null)
                                domainFQDN = item.Properties[LdapAttrNames.dnsRoot].ToString();
                            comment.AppendLine(dcName);
                            comment.AppendLine(domainFQDN);
                            if (isGC) comment.AppendLine(Strings.GCComment);
                            if (rodc) comment.AppendLine(Strings.RodcComment);
                            comment.AppendLine(osLong);
                            string master = ShapeRsx.DCShape;
                            if (rodc) master = ShapeRsx.RODCShape;
                            else if (isGC) master = ShapeRsx.GCShape;

                            dxShape newServer = new dxShape(ShapeRsx.DCPfx, dcName, master, LayoutType.Node)
                            {
                                Header = dcName
                            };
                            newServer.AddAttribute(AttributeNames.ServerFQDN, cmpResult.Properties[LdapAttrNames.DNSHostName].ToString());
                            newServer.AddAttribute(AttributeNames.GlobalCatalog, isGC);
                            newServer.AddAttribute(AttributeNames.RODC, rodc);
                            newServer.AddAttribute(AttributeNames.OS, osLong);
                            newServer.AddAttribute(AttributeNames.OSShort, osShort);
                            newServer.AddAttribute(AttributeNames.DomainFQDN, domainFQDN);
                            newServer.Comment = comment.ToString();
                            newServer.HeaderStyle = TextStyle.SmallNormalBold;
                            newServer.ColorCodedAttribute = AttributeNames.DomainFQDN;
                            newServer.LayoutParams = new dxLayoutParameters().GetLayoutParameters();

                            dxShape domainShape = _data.GetNode(ShapeRsx.DomainPfx, domainFQDN);
                            this.Data.AddShape(newServer, domainShape);
                        }
                        catch (Exception exception)
                        {
                            Logger.TraceException(exception.ToString());
                            Debug.WriteLine(TrcStrings.Exception, exception.Message as object);
                        }
                    }
                }

                //Read Realm Data
                DirectoryContext forestContext = LdapReader.GetDirectoryContext(DirectoryContextType.Forest, _loginInfo);
                Forest forest = Forest.GetForest(forestContext);
                foreach (TrustRelationshipInformation trust in forest.GetAllTrustRelationships())
                {
                    string source = trust.SourceName;
                    string target = trust.TargetName;
                    TrustDirection direction = trust.TrustDirection;
                    TrustType type = trust.TrustType;
                    dxShape targetRealm = null;
                    if (!realms.Any(r => r.ToLowerInvariant() == target.ToLowerInvariant()))
                    {
                        StringBuilder comment = new StringBuilder();
                        targetRealm = new dxShape(ShapeRsx.RealmPfx, target, ShapeRsx.RealmShape, LayoutType.Matrix);
                        comment.AppendLine(target);
                        targetRealm.Comment = comment.ToString();
                        targetRealm.LayoutParams = new DomainLayoutParameters().GetLayoutParameters();
                        targetRealm.Header = target;
                        this.Data.AddShape(targetRealm);
                        realms.Add(target);
                    }
                    else
                        targetRealm = this.Data.GetNode(new string[] { ShapeRsx.DomainPfx, ShapeRsx.RealmPfx }, target);
                    dxShape sourceRealm = this.Data.GetNode(new string[] { ShapeRsx.DomainPfx, ShapeRsx.RealmPfx }, source);

                    // Add Connection
                    StringBuilder commentL = new StringBuilder();
                    DecodeTrust(sourceRealm, targetRealm, type, direction, out dxConnection connection);

                    connection.HeaderStyle = TextStyle.SmallNormalItalic;
                    connection.AddAttribute(AttributeNames.TrustType, type);
                    connection.AddAttribute(AttributeNames.TrustDirection, direction);
                    commentL.AppendLine(connection.Name);
                    commentL.AppendLine(String.Format(Strings.TrustTypeComment, type));
                    commentL.AppendLine(String.Format(Strings.TrustDirectionComment, direction));
                    connection.Comment = commentL.ToString();
                    this.Data.AddConnection(connection);
                }

                //Read Domain Trusts
                foreach (ScopeItem myDomain in LdapReader.DomainList)
                {
                    DirectoryContext domainContext = LdapReader.GetDirectoryContext(DirectoryContextType.Domain, myDomain.Name, _loginInfo);
                    Domain domain = Domain.GetDomain(domainContext);
                    foreach (TrustRelationshipInformation trust in domain.GetAllTrustRelationships())
                    {
                        string source = trust.SourceName;
                        string target = trust.TargetName;
                        TrustDirection direction = trust.TrustDirection;
                        TrustType type = trust.TrustType;
                        dxShape targetRealm = null;
                        if (!realms.Any(r => r.ToLowerInvariant() == target.ToLowerInvariant()))
                        {
                            StringBuilder comment = new StringBuilder();
                            targetRealm = new dxShape(ShapeRsx.RealmPfx, target, ShapeRsx.RealmShape, LayoutType.Matrix);
                            comment.AppendLine(target);
                            targetRealm.Comment = comment.ToString();
                            targetRealm.LayoutParams = new DomainLayoutParameters().GetLayoutParameters();
                            targetRealm.Header = target;
                            this.Data.AddShape(targetRealm);
                            realms.Add(target);
                        }
                        else
                            targetRealm = this.Data.GetNode(new string[] {ShapeRsx.DomainPfx, ShapeRsx.RealmPfx }, target);
                        dxShape sourceRealm = this.Data.GetNode(new string[] {ShapeRsx.DomainPfx, ShapeRsx.RealmPfx }, source);

                        // Add Connection
                        StringBuilder commentL = new StringBuilder();
                        DecodeTrust(sourceRealm, targetRealm, type, direction, out dxConnection connection);

                        connection.HeaderStyle = TextStyle.SmallNormalItalic;
                        connection.AddAttribute(AttributeNames.TrustType, type);
                        connection.AddAttribute(AttributeNames.TrustDirection, direction);
                        commentL.AppendLine(connection.Name);
                        commentL.AppendLine(String.Format(Strings.TrustTypeComment, type));
                        commentL.AppendLine(String.Format(Strings.TrustDirectionComment, direction));
                        connection.Comment = commentL.ToString();
                        this.Data.AddConnection(connection);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.TraceException(exception.ToString());
                Debug.WriteLine(TrcStrings.Exception, exception.Message as object);
            }
        }

        private string GetDomainNC(string path)
        {
            int pos = path.IndexOf(LdapStrings.DcComponent);
            return pos >= 0 ? path.Substring(pos) : String.Empty;
        }

        private string GetForestFunctionalLevel(int FFL)
        {
            switch (FFL)
            {
                case 0:
                    return Strings.Win2000;
                case 1:
                    return Strings.Win2003Interim;
                case 2:
                    return Strings.Win2003Native;
                case 3:
                    return Strings.Win2008;
                case 4:
                    return Strings.Win2008R2;
                case 5:
                    return Strings.Win2012;
                case 6:
                    return Strings.Win2012R2;
                case 7:
                    return Strings.Win2016;
                case 8:
                    return Strings.Win2019;
                default:
                    return Strings.UnKnown;
            }
        }

        private string GetDomainFunctionalLevel(int DFL, int mixedMode)
        {
            switch (DFL)
            {
                case 0:
                    if (mixedMode == 1)
                        return Strings.Win2000Mix;
                    else
                        return Strings.Win2000Native;
                case 1:
                    return Strings.Win2003Interim; 
                case 2:
                    return Strings.Win2003Native;
                case 3:
                    return Strings.Win2008;
                case 4:
                    return Strings.Win2008R2;
                case 5:
                    return Strings.Win2012;
                case 6:
                    return Strings.Win2012R2;
                case 7:
                    return Strings.Win2016;
                case 8:
                    return Strings.Win2019;
                default:
                    return Strings.UnKnown;
            }
        }

        private string GetSchemaVersion(string SchemaVersion)
        {
            switch (SchemaVersion)
            {
                case "13":
                    return Strings.Ver2000;
                case "30":
                    return Strings.Ver2003;
                case "31":
                    return $"{Strings.Ver2003} R2";
                case "44":
                    return Strings.Ver2008;
                case "47":
                    return $"{Strings.Ver2008} R2";
                case "56":
                    return Strings.Ver2012;
                case "69":
                    return $"{Strings.Ver2012} R2";
                case "87":
                    return Strings.Ver2016;
                case "88":
                    return Strings.Ver2019;
                case "":
                    return Strings.Error;
                default:
                    return $"{Strings.Raw}: {SchemaVersion}";
            }
        }

        private void DecodeTrust(dxShape sourceRealm, dxShape targetRealm, TrustType type, TrustDirection direction, out dxConnection connection)
        {
            Color shapeColor;
            string master;
            switch (type)
            {
                case TrustType.Forest:
                case TrustType.External:
                    master = ShapeRsx.Trust2000Shape;
                    shapeColor = Color.Red;
                    break;
                case TrustType.TreeRoot:
                case TrustType.ParentChild:
                    master = ShapeRsx.Trust2000Shape;
                    shapeColor = Color.Black;
                    break;
                case TrustType.CrossLink:
                    master = ShapeRsx.TrustCrossForestShape;
                    shapeColor = Color.Blue;
                    break;
                case TrustType.Kerberos:
                    master = ShapeRsx.TrustDownShape;
                    shapeColor = Color.Green;
                    break;
                default:
                    master = ShapeRsx.TrustDownShape; ;
                    shapeColor = Color.Red;
                    break;
            }

            string name;
            switch (direction)
            {
                case TrustDirection.Inbound:
                    name = String.Format("{0} <= {1}", sourceRealm.Name, targetRealm.Name);
                    connection = new dxConnection(name, targetRealm, sourceRealm, master, false);
                    break;
                case TrustDirection.Outbound:
                    name = String.Format("{0} => {1}", sourceRealm.Name, targetRealm.Name);
                    connection = new dxConnection(name, sourceRealm, targetRealm, master, false);
                    break;
                default:
                    name = String.Format("{0} <=> {1}", sourceRealm.Name, targetRealm.Name);
                    connection = new dxConnection(name, sourceRealm, targetRealm, master, true);
                    break;
            }

            connection.ShapeColor = shapeColor;
        }

        #endregion
    }
}
