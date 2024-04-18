using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices;


namespace Mihelcic.Net.Visio.Data
{
    public class SearchResult
    {
        #region Private Fields

        private System.DirectoryServices.SearchResult _result;
        private Dictionary<string, object> _properties;

        #endregion

        #region Public Properties

        public Dictionary<string, object> Properties { get { return _properties; } }
        public string Path { get; set; }

        #endregion

        public SearchResult(System.DirectoryServices.SearchResult result, StringCollection propertyList = null)
        {
            _properties = new Dictionary<string, object>();
            this._result = result;
            this.Path = result.Path;

            if (propertyList == null || propertyList.Count == 0)
            {
                foreach (string propName in result.Properties.PropertyNames)
                {
                    if (result.Properties[propName].Count == 1)
                        _properties.Add(propName, result.Properties[propName][0] ?? "");
                    else if (result.Properties[propName].Count > 1)
                        _properties.Add(propName, result.Properties[propName]);
                    else
                        _properties.Add(propName, null);
                }
            }
            else
            {
                foreach (string propName in propertyList)
                {
                    if (result.Properties.Contains(propName))
                    {
                        if (result.Properties[propName].Count == 1)
                            _properties.Add(propName, result.Properties[propName][0] ?? "");
                        else if (result.Properties[propName].Count > 1)
                            _properties.Add(propName, result.Properties[propName]);
                        else
                            _properties.Add(propName, null);
                    }
                    else
                        _properties.Add(propName, null);
                }
            }
        }

        #region Public Methods

        public object GetPropertyValue(string name)
        {
            if (_properties.ContainsKey(name))
                return _properties[name];
            Debug.WriteLine("Property {0} for object {1} doesn't exists", name, Path);
            return null;
        }

        public string GetPropertyString(string name, bool throwException = false)
        {
            object value = GetPropertyValue(name);
            if (value == null)
                if (throwException)
                    throw new KeyNotFoundException(String.Format("Property {0} doesn't exists", name));
                else
                    return string.Empty;
            else
                return value.ToString();
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            return _result.GetDirectoryEntry();
        }

        #endregion
    }
}
