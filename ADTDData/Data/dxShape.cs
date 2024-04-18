using Mihelcic.Net.Visio.Arrange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Mihelcic.Net.Visio.Data
{
    public class dxShape : dxObject
    {
        #region Private Fields

        List<dxShape> _children;
        dxShape _parent;

        #endregion

        #region Public Properties

        public LayoutType Layout { get; }
        public dxShape Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null)
                    _parent.RemoveChild(this);
                _parent = value;
                _parent.AddChild(this);
            }
        }
        public IEnumerable<dxShape> Children { get { return _children; } }
        public int Width { get; set; }
        public int Height { get; set; }
        public ColorSchema ColorSchema { get; set; }

        #endregion

        #region Constructors

        public dxShape(string prefix, string name, string master, LayoutType layout = LayoutType.Node) : base(prefix, name, master)
        {
            _children = new List<dxShape>();
            this.Layout = layout;
            this.ColorSchema = ColorSchema.AllColors;
        }

        public dxShape(XmlNode xShape)
        {
            _children = new List<dxShape>();

            this.Layout = (LayoutType)Enum.Parse(typeof(LayoutType), GetXmlAttribute(xShape, "Layout"));
            this.ColorSchema = (ColorSchema)Enum.Parse(typeof(ColorSchema), GetXmlAttribute(xShape, "ColorSchema"));
            this.Height = Int32.Parse(GetXmlAttribute(xShape, "Height"));
            this.Width = Int32.Parse(GetXmlAttribute(xShape, "Width"));
        }

        #endregion

        #region Public Methods

        public void Size(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public XmlElement Xml(XmlDocument doc)
        {
            XmlElement xShape = base.Xml("Shape", doc);
            xShape.Attributes.Append(GetAttr(doc, "Layout", this.Layout.ToString()));
            if (this.Parent != null)
                xShape.Attributes.Append(GetAttr(doc, "Parent", this.Parent.Id));
            xShape.Attributes.Append(GetAttr(doc, "ColorSchema", this.ColorSchema.ToString()));
            xShape.Attributes.Append(GetAttr(doc, "Height", this.Height.ToString()));
            xShape.Attributes.Append(GetAttr(doc, "Width", this.Width.ToString()));


            return xShape;
        }

        #endregion

        #region Internal Methods

        internal void AddChild(dxShape child)
        {
            _children.Add(child);
        }

        internal void RemoveChild(dxShape child)
        {
            if (_children.Any(c => c == child))
                _children.Remove(child);
        }

        #endregion
    }
}
