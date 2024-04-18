using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using Mihelcic.Net.Visio.Xml;
using Mihelcic.Net.Visio.Arrange;
using Mihelcic.Net.Visio.Common;

namespace Mihelcic.Net.Visio.Data
{
    public class dxObject
    {
        #region Public Properties

        public string Id { get; set; }
        public string Type { get; private set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Comment { get; set; }
        public string Master { get; set; }
        public string Header { get; set; }
        public TextStyle HeaderStyle { get; set; }
        public TextStyle TextStyle { get; set; }
        public string ColorCodedAttribute { get; set; }
        public Color? ShapeColor { get; set; }
        public string SubShapeToColor { get; set; }
        public Dictionary<string, object> Attributes { get; }
        public Dictionary<LayoutParameters, object> LayoutParams { get; set; }

        #endregion

        #region Constructors

        public dxObject()
        {
            this.Type = "Unknown";
            this.Id = Guid.NewGuid().ToString();
            this.Attributes = new Dictionary<string, object>();
            this.LayoutParams = new Dictionary<LayoutParameters, object>();
            this.HeaderStyle = TextStyle.Header1;
            this.TextStyle = TextStyle.Normal;
        }

        public dxObject(string name, string master) : this()
        {
            this.Name = name;
            this.Master = master;
        }

        public dxObject(string prefix, string name, string master) :this()
        {
            this.Type = prefix;
            this.Id = String.Format("{0}-{1}", prefix, name);
            this.Name = name;
            this.Master = master;
        }

        public dxObject(XmlNode obj):
            this()
        {
            this.Name = GetXmlAttribute(obj, "Name");
            this.Id = GetXmlAttribute(obj, "Id");
            this.Type = GetXmlAttribute(obj, "Type");
            this.Text = GetXmlNodeValue(obj, "Text");
            this.Comment = GetXmlNodeValue(obj, "Comment");
            this.Master = GetXmlAttribute(obj, "Master");
            this.Header = GetXmlAttribute(obj, "Header");
            this.SubShapeToColor = GetXmlAttribute(obj, "SubShapeToColor");
            this.ColorCodedAttribute = GetXmlAttribute(obj, "ColorCodedAttribute");
            this.HeaderStyle = (TextStyle)Enum.Parse(typeof(TextStyle), GetXmlAttribute(obj, "HeaderStyle"));
            this.TextStyle = (TextStyle)Enum.Parse(typeof(TextStyle), GetXmlAttribute(obj, "TextStyle"));
            string shapeColor = GetXmlAttribute(obj, "ShapeColor");
            if (!String.IsNullOrWhiteSpace(shapeColor))
                this.ShapeColor = Color.FromName(shapeColor);
            XmlNode attributes = obj.SelectSingleNode("Attributes");
            if(attributes != null)
                foreach (XmlAttribute attr in attributes.Attributes)
                    this.Attributes.Add(attr.Name, attr.Value);
            XmlNode layoutParams = obj.SelectSingleNode("LayoutParameters");
            if (layoutParams != null)
                foreach (XmlAttribute attr in layoutParams.Attributes)
                    this.LayoutParams.Add((LayoutParameters)Enum.Parse(typeof(LayoutParameters), attr.Name), attr.Value);
        }

        #endregion

        #region Public Methods

        public void AddAttribute(string name, object value)
        {
            if (this.Attributes.ContainsKey(name))
                Logger.TraceDebug("Object {0} already contains attribute {1}.", this.Id, name);
            else
                this.Attributes.Add(name, value);
        }

        public XmlElement Xml(string tag, XmlDocument doc)
        {
            XmlElement xObj = doc.CreateElement(tag);
            xObj.Attributes.Append(GetAttr(doc, "Name", this.Name));
            xObj.Attributes.Append(GetAttr(doc, "Id", this.Id));
            xObj.Attributes.Append(GetAttr(doc, "Type", this.Type));
            xObj.AppendChild(GetNode(doc, "Text", this.Text));
            xObj.AppendChild(GetNode(doc, "Comment", this.Comment));
            xObj.Attributes.Append(GetAttr(doc, "Master", this.Master));
            xObj.Attributes.Append(GetAttr(doc, "Header", this.Header));
            xObj.Attributes.Append(GetAttr(doc, "HeaderStyle", this.HeaderStyle.ToString()));
            xObj.Attributes.Append(GetAttr(doc, "TextStyle", this.TextStyle.ToString()));
            if (this.ShapeColor.HasValue)
                xObj.Attributes.Append(GetAttr(doc, "ShapeColor", this.ShapeColor.Value.ToString()));
            xObj.Attributes.Append(GetAttr(doc, "SubShapeToColor", this.SubShapeToColor));
            xObj.Attributes.Append(GetAttr(doc, "ColorCodedAttribute", this.ColorCodedAttribute));

            XmlElement attributes = doc.CreateElement("Attributes");
            foreach (string attrName in this.Attributes.Keys)
                attributes.Attributes.Append(GetAttr(doc, attrName, this.Attributes[attrName].ToString()));

            xObj.AppendChild(attributes);

            XmlElement layoutParams = doc.CreateElement("LayoutParameters");
            foreach (LayoutParameters attrName in this.LayoutParams.Keys)
                layoutParams.Attributes.Append(GetAttr(doc, attrName.ToString(), this.LayoutParams[attrName].ToString()));

            xObj.AppendChild(layoutParams);

            return xObj;
        }

        #endregion

        #region Protected Methods

        protected XmlAttribute GetAttr(XmlDocument doc, string name, string value)
        {
            XmlAttribute attr = doc.CreateAttribute(name);
            attr.Value = value;
            return attr;
        }

        protected XmlElement GetNode(XmlDocument doc, string name, string value)
        {
            XmlElement node = doc.CreateElement(name);
            node.InnerText = value;
            return node;
        }

        protected string GetXmlAttribute(XmlNode obj, string attribute)
        {
            if (((XmlElement)obj).HasAttribute(attribute))
                return obj.Attributes[attribute].Value;
            else
                return null;
        }

        protected string GetXmlNodeValue(XmlNode obj, string name)
        {
            XmlNode found = obj.SelectSingleNode(name);
            if (found != null)
                return found.InnerText;
            else return null;
        }

        #endregion
    }
}
