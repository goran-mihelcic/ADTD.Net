using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Represents Visio layer Object
    /// </summary>
    public class XVisioLayer
    {
        private string _ix;
        private Dictionary<string, string> _properties;

        /// <summary>
        /// Layer Id
        /// </summary>
        public string Id { get { return _ix; } }
        
        /// <summary>
        /// Layer Name
        /// </summary>
        public string Name
        {
            get
            {
                if (_properties.ContainsKey(XVisioUtils.VISIO_NAMEUNIV_ATTRIBUTE))
                    return _properties[XVisioUtils.VISIO_NAMEUNIV_ATTRIBUTE];
                return "<No Name>";
            }
        }

        /// <summary>
        /// Constructs Layer Object based on XElement
        /// </summary>
        /// <param name="element"></param>
        public XVisioLayer(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "Element shouldn't be null.");

            _properties = new Dictionary<string, string>();
            _ix = XVisioUtils.GetAttributeValue(element, XVisioUtils.VISIO_IX_ATTRIBUTE);

            IEnumerable<XElement> cels = element.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
            if (cels != null)
            {
                foreach (XElement cell in cels)
                    _properties.Add(XVisioUtils.GetAttributeValue(cell, XVisioUtils.VISIO_N_ATTRIBUTE), XVisioUtils.GetAttributeValue(cell, XVisioUtils.VISIO_V_ATTRIBUTE));
            }
        }
    }
}
