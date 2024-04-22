using System;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Represents Visio Cell Object
    /// </summary>
    public class XVisioCell
    {
        private string _name;
        private string _value;
        private string _formula;
        private XElement _xml;

        /// <summary>
        /// Cell "N" Attribute Value
        /// </summary>
        public string Name { get { return _name; } }
        
        /// <summary>
        /// Get or Set Cell's Value
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (value != null)
                    if (_xml.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE) == null)
                        _xml.Add(new XAttribute(XVisioUtils.VISIO_V_ATTRIBUTE, value));
                    else
                        _xml.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value = value;
            }
        }
        
        /// <summary>
        /// Get or Set Cell's Formula
        /// </summary>
        public string Formula
        {
            get
            {
                return _formula;
            }
            set
            {
                _value = value;
                if (value != null)
                    if (_xml.Attribute(XVisioUtils.VISIO_F_ATTRIBUTE) == null)
                        _xml.Add(new XAttribute(XVisioUtils.VISIO_F_ATTRIBUTE, value));
                    else

                        _xml.Attribute(XVisioUtils.VISIO_F_ATTRIBUTE).Value = value;
            }
        }
        
        /// <summary>
        /// Xml representing Cell Object
        /// </summary>
        public XElement Xml { get { return _xml; } }

        /// <summary>
        /// Construct Cell from Xml
        /// </summary>
        /// <param name="xml">Xml representing Visio Cell</param>
        public XVisioCell(XElement xml)
        {
            if (xml == null)
                throw new ArgumentNullException("xml", "xml should have value");
            if (xml.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE) == null)
                throw new ArgumentException("Invalid Xml - missing \"N\" attribute", "xml");
            _xml = xml;
            _name = xml.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value;
            if (xml.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE) != null)
                _value = xml.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value;
            if (xml.Attribute(XVisioUtils.VISIO_F_ATTRIBUTE) != null)
                _formula = xml.Attribute(XVisioUtils.VISIO_F_ATTRIBUTE).Value;
        }

        /// <summary>
        /// Construct Cell from attribute values
        /// </summary>
        /// <param name="name">N attribute value</param>
        /// <param name="value">V attribute value</param>
        /// <param name="formula">F attribute value</param>
        public XVisioCell(string name, string value, string formula)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Cell Name should be supplied.");

            _xml = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
            _name = name;
            _xml.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, name));
            if (value != null)
                this.Value = value;
            if (formula != null)
                this.Formula = formula;
        }
    }
}
