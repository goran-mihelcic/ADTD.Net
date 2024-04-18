using System.Windows;
using System.Xml;

namespace Mihelcic.Net.Visio.Diagrammer
{
    public class ConfigurationParameter
    {
        #region Private Fields

        private readonly string _name;
        private readonly string _title;
        private object _value;
        private readonly string _toolTip;
        private readonly string _type;
        private readonly bool _picker = false;
        private readonly string _pickerFilter;
        private readonly string _key;

        #endregion

        #region Public Properties

        public string Name { get { return _name; } }
        public string Title { get { return _title; } }
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(ItemControl), new PropertyMetadata(""));
        public string ToolTip { get { return _toolTip; } }
        public string Type { get { return _type; } }
        public bool Picker { get { return _picker; } }
        public string PickerFilter { get { return _pickerFilter; } }
        public string Key { get { return _key; } }

        #endregion

        public ConfigurationParameter(XmlNode element, XmlElement paramsRoot)
        {
            _pickerFilter = "All Files|*.*";
            _name = element.Attributes["Name"].Value;
            _title = element.Attributes["Title"].Value;
            _toolTip = element.Attributes["ToolTip"].Value;
            _type = element.Attributes["Type"].Value;
            if (element.Attributes["Picker"] != null)
                bool.TryParse(element.Attributes["Picker"].Value, out _picker);
            //_picker = element.Attributes["Picker"] != null;

            XmlNode pickerFilterNode = element.Attributes["PickerFilter"];
            if (pickerFilterNode != null)
                _pickerFilter = pickerFilterNode.Value;

            XmlNode keyNode = element.Attributes["Key"];
            if (keyNode != null)
                _key = keyNode.Value;
            else
                _key = _name;

            XmlNode valueNode = paramsRoot.SelectSingleNode(_name);

            if (valueNode != null)
                _value = ApplicationConfiguration.GetConfigurationValue(valueNode);
            else
            {
                string strValue = element.Attributes["Value"].Value;

                _value = ApplicationConfiguration.GetConfigurationValue(_type, strValue);
                this.Save(paramsRoot);
            }
        }

        public void Save(XmlElement root)
        {
            XmlNode propNode = root.SelectSingleNode(this.Name);
            if (propNode == null)
            {
                propNode = root.OwnerDocument.CreateElement(this.Name);
                XmlAttribute newAttr = root.OwnerDocument.CreateAttribute("Value");
                newAttr.Value = this.Value.ToString();
                propNode.Attributes.Append(newAttr);
                newAttr = root.OwnerDocument.CreateAttribute("Type");
                newAttr.Value = this.Type.ToString();
                propNode.Attributes.Append(newAttr);
                root.AppendChild(propNode);
            }
            else
                propNode.Attributes["Value"].Value = this.Value.ToString();
        }

    }
}
