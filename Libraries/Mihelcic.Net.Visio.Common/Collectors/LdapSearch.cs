using Mihelcic.Net.Visio.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;

namespace Mihelcic.Net.Visio.Data
{
    public class LdapSearch
    {
        #region Private Fields

        private SearchScope _scope = SearchScope.Subtree;
        private readonly StringCollection _PropertiesToLoad = new StringCollection();
        private DirectorySearcher _searcher;

        #endregion

        #region Public Properties
        public string filter { get; set; }

        public SearchScope Scope { get { return _scope; } set { _scope = value; } }

        public string PropertiesToLoad
        {
            set
            {
                string[] propArray = value.Split(',');
                foreach (string s in propArray)
                {
                    _PropertiesToLoad.Add(s);
                }
            }
        }

        #endregion

        public LdapSearch()
        {
        }

        public LdapSearch(DirectoryEntry dEntry)
        {
            _searcher = new DirectorySearcher(dEntry)
            {
                PageSize = 999,
                CacheResults = false
            };
        }

        public LdapSearch(string path, LoginInfo loginInfo)
        {
            _searcher = new DirectorySearcher(LdapReader.GetDirectoryEntry(path, loginInfo))
            {
                PageSize = 999,
                CacheResults = false
            };
        }

        #region Public Methods

        public List<SearchResult> GetAll()
        {
            List<SearchResult> result = new List<SearchResult>();

            if (this.filter != null && this.filter != "")
            {
                _searcher.Filter = this.filter;
            }
            _searcher.SearchScope = Scope;

            if (_PropertiesToLoad.Count > 0)
            {
                foreach (string prop in _PropertiesToLoad)
                {
                    _searcher.PropertiesToLoad.Add(prop);
                }
            }
            try
            {
                SearchResultCollection results = _searcher.FindAll();
                foreach (System.DirectoryServices.SearchResult record in results)
                {
                    result.Add(new SearchResult(record, _PropertiesToLoad));
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                return null;
            }
        }

        public SearchResult GetOne()
        {
            if (this.filter != null && this.filter != "")
            {
                _searcher.Filter = this.filter;
            }
            _searcher.SearchScope = Scope;

            if (_PropertiesToLoad.Count > 0)
            {
                foreach (string prop in _PropertiesToLoad)
                {
                    _searcher.PropertiesToLoad.Add(prop);
                }
            }
            try
            {
                return new SearchResult(_searcher.FindOne(), _PropertiesToLoad);
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                foreach (string prop in _PropertiesToLoad)
                    Debug.WriteLine("\t* {0}", prop as object);
                return null;
            }
        }

        public string SearchForReference(string baseObj, string refAttribute, string getAttribute, LoginInfo loginInfo)
        {
            try
            {
                this._searcher = new DirectorySearcher(LdapReader.GetDirectoryEntry(LdapReader.GCPrefixS + baseObj, loginInfo));
                this.Scope = SearchScope.Base;
                this.PropertiesToLoad = refAttribute;
                SearchResult obj = this.GetOne();

                LdapSearch refSearch = new LdapSearch(LdapReader.GetDirectoryEntry(LdapReader.GCPrefixS + obj.Properties[refAttribute].ToString(), loginInfo))
                {
                    Scope = SearchScope.Base,
                    PropertiesToLoad = getAttribute
                };
                string result = refSearch.GetOne().Properties[getAttribute].ToString();
                refSearch = null;
                return result;
            }
            catch(Exception ex)
            {
                Logger.TraceException(ex.ToString());
                return "Unknown";
            }
        }

        public string SearchForReference(string domainToSearch, string baseObj, string refAttribute, string replacerefPfx, string getAttribute, LoginInfo loginInfo)
        {
            try
            {
                string result;
                this._searcher = new DirectorySearcher(LdapReader.GetDirectoryEntry(LdapReader.LdapPrefix + domainToSearch + "/" + baseObj, loginInfo));
                this.Scope = SearchScope.Base;
                this.PropertiesToLoad = refAttribute;
                SearchResult obj = this.GetOne();
                if (obj != null)
                {
                    LdapSearch refSearch = new LdapSearch(LdapReader.GetDirectoryEntry(LdapReader.LdapPrefix + domainToSearch + "/" + obj.Properties[refAttribute].ToString().Replace(replacerefPfx, ""), loginInfo))
                    {
                        Scope = SearchScope.Base,
                        PropertiesToLoad = getAttribute
                    };
                    result = refSearch.GetOne().Properties[getAttribute].ToString();
                    refSearch = null;
                }
                else { result = $"{Strings.NoData}..."; }
                return result;
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                return "Unknown";
            }
        }

        public string SearchForReference(string baseObj, string getAttribute, LoginInfo loginInfo)
        {
            try
            {
                if (baseObj != "")
                {
                    this._searcher = new DirectorySearcher(LdapReader.GetDirectoryEntry(LdapReader.GCPrefixS + baseObj, loginInfo));
                    this.Scope = SearchScope.Base;
                    this.PropertiesToLoad = getAttribute;
                    return (this.GetOne()).Properties[getAttribute].ToString();
                }
                else { return $"{Strings.NoData}..."; }
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                return "Unknown";
            }
        }

        #endregion
    }
}
