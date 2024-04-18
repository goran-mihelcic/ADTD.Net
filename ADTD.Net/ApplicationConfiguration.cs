using Mihelcic.Net.Visio.Common;
using Mihelcic.Net.Visio.Data;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Mihelcic.Net.Visio.Diagrammer
{
    public class ApplicationConfiguration : ViewModelBase
    {
        #region Private Fields

        private readonly XmlDocument _settingsFile;
        private readonly string _settingsFilePath;

        private readonly ObservableCollection<ConfigurationItem> _items;

        #endregion

        #region Public Properties

        public string Title
        {
            get { return GetValue<string>(nameof(Title)); }
            set { SetValue(value, nameof(Title)); }
        }
        public string Server
        {
            get { return GetValue<string>(nameof(Server)); }
            set { SetValue(value, nameof(Server)); }
        }
        public bool DomainJoined
        {
            get { return GetValue<bool>(nameof(DomainJoined)); }
            set { SetValue(value, nameof(DomainJoined)); }
        }
        public bool AADJoined
        {
            get { return GetValue<bool>(nameof(AADJoined)); }
            set { SetValue(value, nameof(AADJoined)); }
        }
        public string DnsForestName
        {
            get { return GetValue<string>(nameof(DnsForestName)); }
            set { SetValue(value, nameof(DnsForestName)); }
        }
        public bool Validated
        {
            get { return GetValue<bool>(nameof(Validated)); }
            set { SetValue(value, nameof(Validated)); }
        }

        public LoginInfo Login
        {
            get { return GetValue<LoginInfo>(nameof(Login)); }
            set { SetValue(value, nameof(Login)); }
        }

        public ObservableCollection<ConfigurationItem> Items { get { return _items; } }
        public ObservableCollection<ConfigurationParameter> Options { get; set; }

        #endregion

        public ApplicationConfiguration()
        {
            Login = new LoginInfo();
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), @"Microsoft\ADTD.Net\Settings.xml");
            _settingsFile = new XmlDocument();
            if (File.Exists(_settingsFilePath))
                _settingsFile.Load(_settingsFilePath);
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(_settingsFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));
                _settingsFile.LoadXml("<root></root>");
            }

            _items = new ObservableCollection<ConfigurationItem>();
            string fileName = Strings.ConfigurationFile;
            string resourceName = $"Mihelcic.Net.Visio.Resources.{fileName}";
            XmlDocument doc = new XmlDocument();
            Assembly resourceAssembly = Assembly.Load("Mihelcic.Net.Visio.ResourceDictionary");
            if (resourceAssembly != null)
            {
                Stream resourceStream = resourceAssembly.GetManifestResourceStream(resourceName);
                if (resourceStream != null)
                {
                    doc.Load(resourceStream);
                    Title = doc.DocumentElement.Attributes["Title"].Value;
                    if (_settingsFile.DocumentElement.SelectSingleNode("Server") is XmlElement serverElement)
                        Server = serverElement.Attributes["Value"].Value;
                    else
                        Server = doc.DocumentElement.Attributes["Server"].Value;

                    foreach (XmlElement element in doc.DocumentElement.SelectNodes("/root/item"))
                        _items.Add(new ConfigurationItem(element, _settingsFile.DocumentElement));

                    Options = new ObservableCollection<ConfigurationParameter>();
                    foreach (XmlElement element in doc.DocumentElement.SelectNodes("/root/configuration/row"))
                        Options.Add(new ConfigurationParameter(element, _settingsFile.DocumentElement));
                }
            }
        }

        #region Public Methods

        public void Save()
        {
            XmlElement element = _settingsFile.DocumentElement;
            if (!(element.SelectSingleNode("Server") is XmlElement serverElement))
            {
                serverElement = _settingsFile.CreateElement("Server");
                XmlAttribute newAttr = _settingsFile.CreateAttribute("Value");
                newAttr.Value = Server;
                serverElement.Attributes.Append(newAttr);
                element.AppendChild(serverElement);
            }
            else
                serverElement.Attributes["Value"].Value = Server;

            foreach (ConfigurationItem item in _items)
            {
                item.Save(element);
            }

            foreach (ConfigurationParameter param in Options)
                param.Save(element);

            _settingsFile.Save(_settingsFilePath);
        }

        //public void Validate()
        //{
        //    bool isValid = false;
        //    if (!String.IsNullOrWhiteSpace(Server))
        //    {

        //    }
        //}

        public static object GetConfigurationValue(XmlNode valueNode)
        {
            object value = null;
            if (valueNode.Attributes["Value"] != null && valueNode.Attributes["Type"] != null)
            {
                string type = valueNode.Attributes["Type"].Value;
                string strVal = valueNode.Attributes["Value"].Value;
                value = GetConfigurationValue(type, strVal);
            }
            return value;
        }

        public static object GetConfigurationValue(string type, string strVal)
        {
            object value;
            switch (type)
            {
                case "Integer":
                    value = Int32.Parse(strVal);
                    break;
                case "Boolean":
                    value = Boolean.Parse(strVal);
                    break;
                case "Double":
                    value = Double.Parse(strVal);
                    break;
                default:
                    value = strVal;
                    break;
            }
            return value;
        }

        #endregion

    }
}
