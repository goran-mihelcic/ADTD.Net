using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Represents Visio Master
    /// </summary>
    public class XVisioMaster : XVisioPart
    {
        private string _name;
        private string _nameU;
        private string _id;
        private XElement _xmlHeader;
        private string _layer;

        /// <summary>
        /// Master Name
        /// </summary>
        public string Name { get { return _name; } }
        
        /// <summary>
        /// Master Universal Name
        /// </summary>
        public string NameU { get { return _nameU; } }
        
        /// <summary>
        /// Master Id
        /// </summary>
        public string Id { get { return _id; } }
        
        /// <summary>
        /// Xml element representing Master header in Masters PackagePart
        /// </summary>
        public XElement XHeader { get { return _xmlHeader; } }
        
        /// <summary>
        /// Layer of Master
        /// </summary>
        public string Layer { get { return _layer; } }
        
        /// <summary>
        /// Xml od Layer
        /// </summary>
        public XElement LayerSection
        {
            get
            {
                XElement section = this.XHeader.Descendants(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SECTION))
                .FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE) != null &&
                                e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_LAYER);
                if (section != null)
                    return section.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_ROW));
                return null;
            }
        }

        /// <summary>
        /// Constructs new Master object in the package
        /// </summary>
        /// <param name="package">Visio Package</param>
        /// <param name="uri">Package Part Uri</param>
        /// <param name="parent">Parent Part</param>
        /// <param name="header">Master header XElement</param>
        public XVisioMaster(XVisioPackage package, Uri uri, XVisioPart parent, XElement header)
            : base(package, uri, parent)
        {
            _name = XVisioUtils.GetAttributeValue(header, XVisioUtils.VISIO_NAME_ATTRIBUTE);
            _nameU = XVisioUtils.GetAttributeValue(header, XVisioUtils.VISIO_NAMEU_ATTRIBUTE);
            _id = XVisioUtils.GetAttributeValue(header, XVisioUtils.VISIO_ID_ATTRIBUTE);
            _xmlHeader = header;
            if (this.LayerSection != null)
            {
                _layer = (this.LayerSection.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL))
                    .FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE) != null
                                    && e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == 
                                        XVisioUtils.VISIO_NAMEUNIV_ATTRIBUTE)).Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value;
            }
        }

        /// <summary>
        /// Constructs Master from Stencil Master
        /// </summary>
        /// <param name="stencilMaster">Stencil Master object</param>
        /// <param name="package">Visio Package</param>
        public XVisioMaster(XVisioMaster stencilMaster, XVisioPackage package)
            : base(stencilMaster)
        {
            if (package == null)
                throw new ArgumentNullException("package", "Package shouldn't be null.");

            _name = XVisioUtils.GetAttributeValue(stencilMaster._xmlHeader, XVisioUtils.VISIO_NAME_ATTRIBUTE);
            _nameU = XVisioUtils.GetAttributeValue(stencilMaster._xmlHeader, XVisioUtils.VISIO_NAMEU_ATTRIBUTE);
            _id = XVisioUtils.GetAttributeValue(stencilMaster._xmlHeader, XVisioUtils.VISIO_ID_ATTRIBUTE);
            _xmlHeader = stencilMaster.XHeader;
            PackagePart newPart = package.Package.CreatePart(PackUriHelper.CreatePartUri(PartUri), stencilMaster.Part.ContentType, CompressionOption.SuperFast);
            XVisioPackage.SavePart(newPart, this.Xml);
            this.Part = newPart;

            if (this.LayerSection != null)
            {
                _layer = (this.LayerSection.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL))
                    .FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE) != null
                                    && e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_NAMEUNIV_ATTRIBUTE)).Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value;
            }
        }

        /// <summary>
        /// Get Master Size
        /// </summary>
        /// <returns>Master Size</returns>
        public XSize GetSize()
        {
            Double width = 0;
            Double height = 0;
            XElement cell = null;

            if (Xml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES)) != null &&
                Xml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES)).Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE)) != null)
            {
                IEnumerable<XElement> shapes = Xml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES)).Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE))
                    .Where(e => e.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)) != null);
                if (shapes != null)
                {
                    XElement shape = shapes.FirstOrDefault(s => s.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL))
                        .Any(c => c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_WIDTH_ATTRIBUTE));
                    IEnumerable<XElement> elements = shape.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
                    if (elements != null)
                    {
                        if (elements.Any(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_WIDTH_ATTRIBUTE))
                        {
                            cell = elements.First(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_WIDTH_ATTRIBUTE);
                            if (cell.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE) != null)
                                width = Double.Parse(cell.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value, CultureInfo.CreateSpecificCulture("en-US")) * XVisioUtils.INCH_TO_MM;
                        }
                        if (elements.Any(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_HEIGHT_ATTRIBUTE))
                        {
                            cell = elements.First(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_HEIGHT_ATTRIBUTE);
                            if (cell.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE) != null)
                                height = Double.Parse(cell.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value, CultureInfo.CreateSpecificCulture("en-US")) * XVisioUtils.INCH_TO_MM;
                        }
                    }
                }
            }

            return new XSize(width, height);
        }

        /// <summary>
        /// Get Cell from Master
        /// </summary>
        /// <param name="cellName">Cell to return (N element)</param>
        /// <returns>Cell xml</returns>
        public XElement GetCell(string cellName)
        {
            if (String.IsNullOrWhiteSpace(cellName))
                throw new ArgumentNullException("cellName", "CellName should be supplied.");

            XElement cell = null;
            IEnumerable<XElement> cells = this.Xml.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
            if (cells != null && cells
                .Any(c => c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE) != null &&
                    c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == cellName))
                cell = cells.First(c => c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == cellName);
            return cell;
        }
    }
}
