using System;
using System.Collections.Generic;
using System.Xml;

namespace Mihelcic.Net.Visio.Diagrammer
{
    public class ConfigurationItem
    {
        #region Private Fields

        private string _name;
        private string _label;
        private string _toolTip;
        private string _dll;
        private bool _selected;
        private string _confNodeName { get { return String.Format("{0}_Data", _name); } }
        private List<ConfigurationParameter> _parameters;

        #endregion

        #region Public Properties
        public string Name { get { return _name; } }
        public string Label { get { return _label; } }
        public string ToolTip { get { return _toolTip; } }
        public string Dll { get { return _dll; } }
        public bool Selected { get { return _selected; } set { _selected = value; } }
        public List<ConfigurationParameter> Parameters { get { return _parameters; } set { _parameters = value; } }

        #endregion

        #region Constructors

        private ConfigurationItem()
        {
            _parameters = new List<ConfigurationParameter>();
        }

        public ConfigurationItem(XmlElement confElement, XmlElement paramsRoot)
            : this()
        {
            if (confElement.HasAttribute("Name"))
                _name = confElement.Attributes["Name"].Value;
            else
                throw new ApplicationException("Name not defined");
            if (confElement.HasAttribute("Label"))
                _label = confElement.Attributes["Label"].Value;
            else
                throw new ApplicationException("Label not defined");
            if (confElement.HasAttribute("ToolTip"))
                _toolTip = confElement.Attributes["ToolTip"].Value;
            if (confElement.HasAttribute("Dll"))
                _dll = confElement.Attributes["Dll"].Value;

            XmlDocument settingsFile = paramsRoot.OwnerDocument;
            XmlNode selectedElement = settingsFile.DocumentElement.SelectSingleNode(_confNodeName);
            if (selectedElement != null)
                Boolean.TryParse(selectedElement.Attributes["Value"].Value, out _selected);
            
            foreach (XmlNode node in confElement.SelectNodes("Parameter"))
            {
                _parameters.Add(new ConfigurationParameter(node, paramsRoot));
            }
        }

        #endregion

        public void Save(XmlElement element)
        {
            XmlDocument settingsFile = element.OwnerDocument;

            XmlElement serverElement = element.SelectSingleNode(_confNodeName) as XmlElement;
            if (serverElement == null)
            {
                serverElement = settingsFile.CreateElement(_confNodeName);
                XmlAttribute newAttr = settingsFile.CreateAttribute("Value");
                newAttr.Value = _selected.ToString();
                serverElement.Attributes.Append(newAttr);
                element.AppendChild(serverElement);
            }
            else
                serverElement.Attributes["Value"].Value = _selected.ToString(); 

            foreach (ConfigurationParameter prop in this.Parameters)
            {
                prop.Save(element);
            }
        }
    }
}
