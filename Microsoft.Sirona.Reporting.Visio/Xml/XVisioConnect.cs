using System;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Represents Visio Connect Object
    /// </summary>
    public class XVisioConnect
    {
        private string _toPart;
        private string _toCell;
        private string _toSheet;
        private string _fromPart;
        private string _fromCell;
        private string _fromSheet;

        /// <summary>
        /// Constructs Visio Connect Object
        /// </summary>
        /// <param name="element">XElement that represents Cnnect Object</param>
        public XVisioConnect(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "Element shouldn't be null.");

            _toPart = XVisioUtils.GetAttributeValue(element, XVisioUtils.VISIO_TOPART_ATTRIBUTE);
            _toCell = XVisioUtils.GetAttributeValue(element, XVisioUtils.VISIO_TOCELL_ATTRIBUTE);
            _toSheet = XVisioUtils.GetAttributeValue(element, XVisioUtils.VISIO_TOSHEET_ATTRIBUTE);
            _fromPart = XVisioUtils.GetAttributeValue(element, XVisioUtils.VISIO_FROMPART_ATTRIBUTE);
            _fromCell = XVisioUtils.GetAttributeValue(element, XVisioUtils.VISIO_FROMCELL_ATTRIBUTE);
            _fromSheet = XVisioUtils.GetAttributeValue(element, XVisioUtils.VISIO_FROMSHEET_ATTRIBUTE);
        }
    }
}
