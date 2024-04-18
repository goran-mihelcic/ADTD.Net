using Mihelcic.Net.Visio.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;

namespace Mihelcic.Net.Visio.Data
{
    public static class LdapReader
    {
        #region Private Fields

        private static bool _initialized = false;
        private static string _selectedDC = String.Empty;
        private static readonly object _lock = new object();
        private static DirectoryEntry _ldapRootObj;
        private static int _intExSchemaVersion = -1;

        #endregion
        public static string SelectedDC { get { return _selectedDC; } set { _selectedDC = value; } }

        #region Public Properties
        public static ScopeItem ForestRoot { get; set; }
        
        public static List<ScopeItem> DomainList { get; set; }
        
        public static List<ScopeItem> SiteList { get; set; }
        
        public static List<ScopeItem> NCList { get; set; }

        public static bool Connected
        {
            get { return (_ldapRootObj != null); }
        }

        public static string ConfigNC { get; set; } = String.Empty;

        public static string SchemaNC { get; set; } = String.Empty;

        public static string RootNC { get; set; } = String.Empty;

        // Get the LDAP prefix. It includes a specific server if defined
        // in the session
        public static string LdapPrefixS
        {
            get
            {
                if (SelectedDC.Length > 1)
                    return $"{LdapStrings.LdapPrefix}{SelectedDC}/";
                else
                    return LdapStrings.LdapPrefix;
            }
        }

        public static string LdapPrefix
        {
            get
            {
                return LdapStrings.LdapPrefix;
            }
        }

        // Get the LDAP prefix. It includes a specific server if defined
        // in the session
        public static string GCPrefix
        {
            get
            {
                if (SelectedDC.Length > 1)
                    return $"{LdapStrings.GcPrefix}{SelectedDC}/";
                else
                    return LdapStrings.GcPrefix;
            }
        }

        #endregion

        #region Public Methods

        public static void Initialize(string targetDC)
        {
            SelectedDC = targetDC.Trim();
        }

        public static bool VerifyAuthentication(bool getDomains, bool getSites, bool getNCs, LoginInfo loginInfo)
        {
            lock (_lock)
            {
                String strConfig;
                if (!_initialized)
                {
                    DomainList = new List<ScopeItem>();
                    SiteList = new List<ScopeItem>();
                    NCList = new List<ScopeItem>();
                }

                try
                {
                    if (!_initialized)
                    {
                        _ldapRootObj = new DirectoryEntry($"{LdapPrefixS}{LdapStrings.RootDSE}");

                        strConfig = _ldapRootObj.Properties[LdapAttrNames.ConfigurationNamingContext][0].ToString();
                        GetConfig();
                        GetRootNC();
                    }

                    if (DomainList.Count == 0)
                    {
                        if (getDomains)
                            FillForestRoot(loginInfo);
                        FillDomains(loginInfo);
                    }
                    if (getSites && SiteList.Count == 0)
                        FillSites(loginInfo);
                    if (getNCs && NCList.Count == 0)
                        FillNCs();

                    _initialized = true;

                    return true;
                }
                catch (Exception ex)
                {
                    _ldapRootObj = null;
                    Logger.TraceException(ex.ToString());
                    return false;
                }
            }
        }

        // Get the name of the configuration container
        public static string GetConfig()
        {
            string strConfig;

            if (String.IsNullOrWhiteSpace(ConfigNC))
            {
                try
                {
                    strConfig = String.Empty;
                    strConfig = _ldapRootObj.Properties[LdapAttrNames.ConfigurationNamingContext][0].ToString();
                    SchemaNC = _ldapRootObj.Properties[LdapAttrNames.SchemaNamingContext][0].ToString();
                    ConfigNC = strConfig;
                }
                catch (Exception ex)
                {
                    Logger.Trace(System.Diagnostics.TraceEventType.Error, 9009, "Cannot get Configuration NC");
                    Logger.TraceException(ex.ToString());
                }
            }
            return ConfigNC;
        }


        //getroot - get the name of the domain NC
        public static void GetRootNC()
        {
            string strRoot;

            if (String.IsNullOrWhiteSpace(RootNC))
            {
                try
                {
                    strRoot = _ldapRootObj.Properties[LdapAttrNames.RootDomainNamingContext][0].ToString();
                    RootNC = strRoot;
                }
                catch (Exception ex)
                {
                    Logger.Trace(System.Diagnostics.TraceEventType.Error, 9009, "Cannot get RootDSE");
                    Logger.TraceException(ex.ToString());
                }
            }
        }

        //getSchemaVersion - get the AD Schema Version
        public static string GetSchemaVersion(LoginInfo loginInfo)
        {
            DirectoryEntry objSchema;

            string strSchema;
            try
            {
                strSchema = _ldapRootObj.Properties[LdapAttrNames.SchemaNamingContext][0].ToString();
                objSchema = GetDirectoryEntry(LdapPrefixS + strSchema, loginInfo);
                strSchema = objSchema.Properties[LdapAttrNames.ObjectVersion][0].ToString();
                objSchema = null;
                return strSchema;
            }
            catch (Exception ex)
            {
                Logger.Trace(System.Diagnostics.TraceEventType.Error, 9009, "Cannot get Schema Version");
                Logger.TraceException(ex.ToString());
                return String.Empty;
            }
        }


        // convert DC type name to DNS name
        // input string of form  "DC=corp,DC=microsoft,DC=com"
        // returns DNS domain name ie "corp.microsof.com"
        public static string Dc2Dns(string dcname)
        {
            string[] Array1;
            string[] Array2;
            string s;
            if ((dcname == null) || DBNull.Value.Equals(dcname) || (dcname == String.Empty))
                return String.Empty;
            else
            {
                if (dcname.StartsWith(LdapStrings.DcComponent, StringComparison.CurrentCultureIgnoreCase))
                    return dcname;
                else
                {
                    Array1 = dcname.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    s = String.Empty;
                    foreach (string element in Array1)
                    {
                        Array2 = element.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        s = s + Array2[1] + ".";
                    }
                    return s.Remove(s.Length - 1);
                }
            }
        }

        // convert DN type String to DNS name
        // input string of form  "CN=Server,DC=corp,DC=microsoft,DC=com"
        // returns DNS domain name ie "Server.corp.microsof.com"
        public static string Dn2Dns(string dnname)
        {
            string[] Array1;
            string[] Array2;
            string s;

            if ((dnname == null) || DBNull.Value.Equals(dnname) || (dnname == String.Empty))
                return String.Empty;
            else
            {
                if (dnname.StartsWith(LdapStrings.CnComponent, StringComparison.CurrentCultureIgnoreCase))
                    return dnname;
                else
                {
                    Array1 = dnname.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    Array2 = Array1[0].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    s = Array2[1] + ".";
                    foreach (string element in Array1)
                    {
                        if (dnname.StartsWith(LdapStrings.DcComponent, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Array2 = element.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            s = s + Array2[1] + ".";
                        }
                    }
                }
                return s.Remove(s.Length - 1);
            }
        }

        //'convert DNS domain name to DC format
        //' input DNS domain name ie corp.microsoft.co.
        //' output DC format of name ie DC=corp,DC=microsoft,DC=com
        public static string Dns2Dc(string dnsname)
        {
            string[] myArray;
            string s;

            if ((dnsname == null) || DBNull.Value.Equals(dnsname) || (dnsname == String.Empty))
                return String.Empty;
            else if (dnsname.StartsWith(LdapStrings.DcComponent, StringComparison.CurrentCultureIgnoreCase))
                return dnsname;
            else
            {
                myArray = dnsname.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                s = String.Empty;
                foreach (string element in myArray)
                    s = $"{s},{LdapStrings.DcComponent}{element}";//s + ",DC=" + element;

                return s.Remove(0, 1);
            }
        }

        //'---------------------------------------------
        //' Split the LDAP path info components and return the item number v
        //' first element is zero
        //'
        public static string GetLdapComponent(string s, int v)
        {
            string[] Array1;
            string[] Array2;

            if ((s == null) || DBNull.Value.Equals(s) || (s == String.Empty))
                return String.Empty;
            else
            {
                Array1 = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Array2 = Array1[v].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                return Array2[1];
            }
        }

        //'---------------------------------------------
        //' Split the LDAP path info components and return parent
        //'
        public static string GetParentComponent(string s)
        {

            if ((s == null) || DBNull.Value.Equals(s) || (s == String.Empty))
                return String.Empty;
            else
                return s.Substring(s.IndexOf(',') + 1);
        }

        //'---------------------------------------------
        //' Split the String info components and return the item number v
        //' first element is zero
        //'
        public static Object GetComponent(string strString, string strSplit, short Index)
        {
            string[] Array1;

            if ((strString == null) || DBNull.Value.Equals(strString) || (strString == String.Empty))
                return String.Empty;
            else
            {
                Array1 = strString.Split(new string[] { strSplit }, StringSplitOptions.None);
                if (Index < Array1.Count())
                    return Array1[Index];
                else
                    return String.Empty;
            }
        }

        //'---------------------------------------------
        //' Split the String info components and return the number of components
        //' first element is zero
        //'
        public static Object GetNrOfComponents(string strString, string strSplit)
        {

            string[] Array1;

            if ((strString == null) || DBNull.Value.Equals(strString) || (strString == String.Empty))
                return 0;
            else
            {
                Array1 = strString.Split(new string[] { strSplit }, StringSplitOptions.None);
                return Array1.Count();
            }
        }

        //' This routine uses the "heap sort" algorithm to sort a VB collection.
        //' It returns the sorted collection.
        public static List<string> SortCollection(List<string> c)
        {
            _ = new SortedList<string, string>();
            c.Sort();
            return c;
        }

        public static int GetExSchemaVersion(LoginInfo loginInfo)
        {
            if (_intExSchemaVersion == -1)
            {
                string strObject = $"{LdapPrefixS}{LdapStrings.SchemaVersionDn},{ConfigNC}";
                DirectoryEntry objde = GetDirectoryEntry(strObject, loginInfo);
                if (objde != null)
                    _intExSchemaVersion = Convert.ToInt32(objde.Properties[LdapAttrNames.RangeUpper].Value);
                else _intExSchemaVersion = 0;
            }
            return _intExSchemaVersion;
        }


        public static string ServerNameFromNTDSSettings(string nTDSDSA)
        {
            string tmp = nTDSDSA.Replace($"{LdapStrings.NtdsSettingsCn},{LdapStrings.CnComponent}", String.Empty);
            return tmp.Remove(tmp.IndexOf(','));
        }

        public static DirectoryEntry GetDirectoryEntry(string path, LoginInfo loginInfo)
        {
            Logger.TraceDebug("getDirectoryEntry({0})", path);
            if (DirectoryEntry.Exists(path))
            {
                if (loginInfo.CurrentUserContext)
                    return new DirectoryEntry(path);
                else
                {
                    DirectoryEntry de = new DirectoryEntry(path, loginInfo.FullUserName, loginInfo.Password)
                    {
                        AuthenticationType = AuthenticationTypes.Secure
                    };
                    return de;
                }
            }
            else return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextType"></param>
        /// <returns></returns>
        public static DirectoryContext GetDirectoryContext(DirectoryContextType contextType, LoginInfo loginInfo)
        {
            try
            {
                if (loginInfo.Validated)
                    return new DirectoryContext(contextType, loginInfo.FullUserName, loginInfo.Password);
                else
                    return new DirectoryContext(contextType);
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                throw;
            }
        }

        public static DirectoryContext GetDirectoryContext(DirectoryContextType contextType, string target, LoginInfo loginInfo)
        {
            try
            {
                if (loginInfo.Validated)
                    return new DirectoryContext(contextType, target, loginInfo.FullUserName, loginInfo.Password);
                else
                    return new DirectoryContext(contextType, target);
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                throw;
            }
        }

        #endregion

        #region Private Methods

        private static void FillForestRoot(LoginInfo loginInfo)
        {
            try
            {
                string strObject = $"{LdapReader.LdapPrefixS}{LdapStrings.PartitionsCn},{LdapReader.ConfigNC}";  //LdapReader.ldapPrefixS + "CN=Partitions," + LdapReader.ConfigNC;
                System.Text.StringBuilder objfilter = new System.Text.StringBuilder();
                objfilter.Append($"(&({LdapAttrNames.ObjectClass}={LdapStrings.CrossRefClass})({LdapAttrNames.nCName}={LdapReader.RootNC}))");

                LdapSearch domainsSearch = new LdapSearch(LdapReader.GetDirectoryEntry(strObject, loginInfo))
                {
                    filter = objfilter.ToString(),
                    Scope = SearchScope.OneLevel,
                    PropertiesToLoad = $"{LdapAttrNames.Name},{LdapAttrNames.dnsRoot},{LdapAttrNames.msDSBehaviorVersion},{LdapAttrNames.nTMixedDomain},{LdapAttrNames.nCName},{LdapAttrNames.NetBIOSName}"
                };
                SearchResult forestResult = domainsSearch.GetOne();
                ForestRoot = new ScopeItem(forestResult.Properties[LdapAttrNames.dnsRoot].ToString());
                ForestRoot.Properties.Add(LdapAttrNames.dnsRoot, forestResult.Properties[LdapAttrNames.dnsRoot].ToString());
                ForestRoot.ItemDE = forestResult.GetDirectoryEntry();
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
            }
        }

        private static void FillDomains(LoginInfo loginInfo)
        {
            try
            {
                string strObject = $"{LdapReader.LdapPrefixS}{LdapStrings.PartitionsCn},{LdapReader.ConfigNC}";
                LdapSearch domainsSearch = new LdapSearch(LdapReader.GetDirectoryEntry(strObject, loginInfo));
                System.Text.StringBuilder objfilter = new StringBuilder();
                objfilter.Append($"(&({LdapAttrNames.ObjectCategory}={LdapStrings.CrossRefClass})({LdapAttrNames.NetBIOSName}=*))");
                domainsSearch.filter = objfilter.ToString();
                domainsSearch.PropertiesToLoad = $"{LdapAttrNames.Name},{LdapAttrNames.dnsRoot},{LdapAttrNames.msDSBehaviorVersion},{LdapAttrNames.nTMixedDomain},{LdapAttrNames.nCName},{LdapAttrNames.NetBIOSName}";
                domainsSearch.Scope = SearchScope.Subtree;
                List<SearchResult> domainsResults = domainsSearch.GetAll();
                foreach (SearchResult domainsResult in domainsResults)
                {
                    ScopeItem item = new ScopeItem(domainsResult.Properties[LdapAttrNames.dnsRoot].ToString());
                    item.Properties.Add(LdapStrings.DN, domainsResult.Path);
                    item.Properties.Add(LdapAttrNames.nCName, domainsResult.Properties[LdapAttrNames.nCName].ToString());
                    item.Properties.Add(LdapAttrNames.dnsRoot, domainsResult.Properties[LdapAttrNames.dnsRoot].ToString());
                    item.Properties.Add(LdapAttrNames.msDSBehaviorVersion, domainsResult.Properties[LdapAttrNames.msDSBehaviorVersion].ToString());
                    item.Properties.Add(LdapAttrNames.nTMixedDomain, domainsResult.Properties[LdapAttrNames.nTMixedDomain].ToString());
                    item.ItemDE = domainsResult.GetDirectoryEntry();
                    DomainList.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
            }
        }
        private static void FillSites(LoginInfo loginInfo)
        {
            try
            {
                string strObject = $"{LdapReader.LdapPrefixS}{LdapStrings.SitesCn},{LdapReader.GetConfig()}";
                System.Text.StringBuilder objfilter = new System.Text.StringBuilder();
                objfilter.Append($"({LdapAttrNames.ObjectClass}=site)");

                LdapSearch sitesSearch = new LdapSearch(LdapReader.GetDirectoryEntry(strObject, loginInfo))
                {
                    filter = objfilter.ToString(),
                    PropertiesToLoad = $"{LdapAttrNames.Name},{LdapAttrNames.Location},{LdapAttrNames.gPLink},{LdapAttrNames.options},{LdapAttrNames.interSiteTopologyGenerator},{LdapAttrNames.msDSPreferredGCSite}"
                };
                List<SearchResult> siteResults = sitesSearch.GetAll();

                foreach (SearchResult siteResult in siteResults)
                {
                    ScopeItem item = new ScopeItem(siteResult.Properties[LdapAttrNames.Name].ToString());
                    item.Properties.Add(LdapAttrNames.SiteOptions, GetSiteOptions(siteResult, loginInfo));
                    item.Properties.Add(LdapAttrNames.Location, (siteResult.Properties[LdapAttrNames.Location] ?? String.Empty).ToString());
                    item.Properties.Add(LdapAttrNames.gPLink, (siteResult.Properties[LdapAttrNames.gPLink] ?? String.Empty).ToString());
                    item.ItemDE = siteResult.GetDirectoryEntry();
                    SiteList.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
            }
        }

        private static void FillNCs()
        {
            try
            {

            }
            catch (Exception)
            {

            }
        }

        private static TypSiteOptions GetSiteOptions(SearchResult siteData, LoginInfo loginInfo)
        {
            try
            {
                string strObject = $"{LdapReader.LdapPrefixS}{LdapStrings.SiteSettingCn},{siteData.Path.Replace(LdapReader.LdapPrefixS, String.Empty)}"; 
                System.Text.StringBuilder objfilter = new System.Text.StringBuilder();
                objfilter.Append($"({LdapAttrNames.ObjectClass}={LdapStrings.SiteSettingClass})");

                LdapSearch sitesSearch = new LdapSearch(LdapReader.GetDirectoryEntry(strObject, loginInfo))
                {
                    filter = objfilter.ToString(),
                    PropertiesToLoad = $"{LdapAttrNames.options},{LdapAttrNames.interSiteTopologyGenerator},{LdapAttrNames.msDSPreferredGCSite}"
                };
                List<SearchResult> siteResults = sitesSearch.GetAll();

                TypSiteOptions RemSiteOptions = new TypSiteOptions();
                if (siteResults.Any())
                {
                    SearchResult siteOptions = siteResults.First();
                    if (siteOptions.Properties[LdapAttrNames.options] != null)
                    {
                        int SiteOptionsTemp = Convert.ToInt32(siteOptions.Properties[LdapAttrNames.options]);
                        RemSiteOptions.Universal_Group_Membership_Caching = (SiteOptionsTemp & 32) != 0;
                        RemSiteOptions.Stale_Link_Detection = (SiteOptionsTemp & 8) != 0;
                        RemSiteOptions.Optimizing_Edges = (SiteOptionsTemp & 4) != 0;
                        RemSiteOptions.Cleanup = (SiteOptionsTemp & 2) != 0;
                        RemSiteOptions.IntraSite_Topology_Generation = (SiteOptionsTemp & 1) != 0;
                    }

                    if (siteOptions.Properties[LdapAttrNames.interSiteTopologyGenerator] != null)
                    {
                        bool deleted = false;
                        string InterSiteTopologyGenerator = LdapReader.GetLdapComponent(siteOptions.Properties[LdapAttrNames.interSiteTopologyGenerator].ToString(), 1);
                        if (InterSiteTopologyGenerator.Contains("\\0ADEL")) deleted = true;
                        if (InterSiteTopologyGenerator.Contains('\\'))
                            InterSiteTopologyGenerator = InterSiteTopologyGenerator.Substring(0, InterSiteTopologyGenerator.IndexOf('\\') - 1);
                        RemSiteOptions.ISTG = deleted ? String.Format("[{0}]-Deleted", InterSiteTopologyGenerator) : InterSiteTopologyGenerator;
                    }
                    if (siteOptions.Properties[LdapAttrNames.msDSPreferredGCSite] != null)
                        RemSiteOptions.UniversalGroupCachingSite =
                            LdapReader.GetLdapComponent(siteOptions.Properties[LdapAttrNames.msDSPreferredGCSite].ToString(), 0);
                }
                return RemSiteOptions;
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                return null;
            }
        }

        #endregion
    }
}
