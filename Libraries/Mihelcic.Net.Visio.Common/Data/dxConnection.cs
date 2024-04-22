using System;
using System.Xml;

namespace Mihelcic.Net.Visio.Data
{
    public class dxConnection : dxObject
    {
        #region Public Properties

        public dxShape From { get; set; }
        public dxShape To { get; set; }
        public bool Bidirectional { get; set; }
        public Double? Tickness { get; set; }

        #endregion

        #region Constructors

        public dxConnection(string name, dxShape from, dxShape to, string master, bool bidirectional = true)
            : base(name, master)
        {
            this.From = from;
            this.To = to;
            this.Bidirectional = bidirectional;
        }

        public dxConnection(string prefix, string name, dxShape from, dxShape to, string master, bool bidirectional = true)
            : base(prefix, name, master)
        {
            this.From = from;
            this.To = to;
            this.Bidirectional = bidirectional;
        }
        public dxConnection(string id, string prefix, string name, dxShape from, dxShape to, string master, bool bidirectional = true)
            : this(prefix, name, from, to, master, bidirectional)
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public dxConnection(XmlNode xConn, IData data)
        {
            string from = GetXmlAttribute(xConn, "From");
            this.From = data.GetNode(from);
            string to = GetXmlAttribute(xConn, "To");
            this.To = data.GetNode(to);
            this.Bidirectional = (bool)bool.Parse(GetXmlAttribute(xConn, "Bidirectional"));
            string tickness = GetXmlAttribute(xConn, "Tickness");
            if (!String.IsNullOrWhiteSpace(tickness))
                this.Tickness = Double.Parse(tickness);
        }

        #endregion

        #region Public Methods

        public XmlElement Xml(XmlDocument doc)
        {
            XmlElement xConn = base.Xml("Connection", doc);

            xConn.Attributes.Append(GetAttr(doc, "From", this.From.Id));
            xConn.Attributes.Append(GetAttr(doc, "To", this.To.Id));
            xConn.Attributes.Append(GetAttr(doc, "Bidirectional", this.Bidirectional.ToString()));
            if (this.Tickness.HasValue)
                xConn.Attributes.Append(GetAttr(doc, "Tickness", this.Tickness.Value.ToString()));

            return xConn;
        }

        #endregion
    }
}
