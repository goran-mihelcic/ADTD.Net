using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Represents Visio Page Object
    /// </summary>
    public class XVisioPage : XVisioPart
    {
        private XVisioPackage _visioPackage;
        private List<XVisioShape> _shapes;
        private List<XVisioConnect> _connects;
        private List<XVisioLayer> _layers;
        private string _name;
        private string _nameU;
        private XElement _xmlHeader;
        private string _id;
        private HashSet<int> _shapeIds = new HashSet<int>();
        private int _shapeNum = 0;
        private Dictionary<string, string> _layerList;

        private XElement _layerSection
        {
            get
            {
                IEnumerable<XElement> sections = this.XHeader.Descendants(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SECTION));
                if (sections != null)
                    return sections.FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE) != null &&
                                    e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_LAYER_ATTRIBUTE);
                return null;
            }
        }
        private int _nextLayer
        {
            get
            {
                if (_layerList.Count > 0)
                    return _layerList.Values.Max(k => Int32.Parse(k)) + 1;
                return 0;
            }
        }

        /// <summary>
        /// Next Page Id
        /// </summary>
        public int NextId
        {
            get
            {
                return ++_shapeNum;
            }
        }

        /// <summary>
        /// Page Name
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Page Universal Name
        /// </summary>
        public string NameU { get { return _nameU; } }
        
        /// <summary>
        /// Page Id
        /// </summary>
        public string Id { get { return _id; } }
        
        /// <summary>
        /// Page Object Header (in Pages)
        /// </summary>
        public XElement XHeader { get { return _xmlHeader; } }

        /// <summary>
        /// Shape Objects on the Page
        /// </summary>
        public List<XVisioShape> Shapes { get { return _shapes; } }

        /// <summary>
        /// Constructs Page Object
        /// </summary>
        /// <param name="package">Visio Package</param>
        /// <param name="uri">Page Part uri</param>
        /// <param name="parent">Parent Part</param>
        /// <param name="header">Page Header</param>
        public XVisioPage(XVisioPackage package, Uri uri, XVisioPart parent, XElement header)
            : base(package, uri, parent)
        {
            if (header == null)
                throw new ArgumentNullException("header", "Header should be supplied.");

            _visioPackage = package;
            _shapes = new List<XVisioShape>();
            _connects = new List<XVisioConnect>();
            _name = XVisioUtils.GetAttributeValue(header, XVisioUtils.VISIO_NAME_ATTRIBUTE);
            _nameU = XVisioUtils.GetAttributeValue(header, XVisioUtils.VISIO_NAMEU_ATTRIBUTE);
            _id = XVisioUtils.GetAttributeValue(header, XVisioUtils.VISIO_ID_ATTRIBUTE);
            _xmlHeader = header;
            _layers = new List<XVisioLayer>();
            _layerList = new Dictionary<string, string>();

            XElement layerElement = header.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == XVisioUtils.VISIO_SECTION && e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE) != null && e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_LAYER_ATTRIBUTE);
            if (layerElement != null)
            {
                IEnumerable<XElement> xlayers = from element in layerElement.Elements()
                                                where element.Name.LocalName == XVisioUtils.VISIO_ROW
                                                select element;
                foreach (XElement layer in xlayers)
                {
                    XVisioLayer currentLayer = new XVisioLayer(layer);
                    _layers.Add(currentLayer);
                    if (!_layerList.ContainsKey(currentLayer.Id))
                        _layerList.Add(currentLayer.Id, currentLayer.Name);
                }

            }


            if (this.Part != null && uri != null)
            {
                XElement shapesElement = Xml.Root.Elements().FirstOrDefault(e => e.Name.LocalName == XVisioUtils.VISIO_SHAPES);
                if (shapesElement != null)
                {
                    IEnumerable<XElement> shapes = from element in shapesElement.Elements()
                                                   where element.Name.LocalName == XVisioUtils.VISIO_SHAPE
                                                   select element;
                    foreach (XElement shape in shapes)
                        AddShape(shape);
                }

                XElement connectsElement = Xml.Root.Elements().FirstOrDefault(e => e.Name.LocalName == XVisioUtils.VISIO_CONNECTS);
                if (connectsElement != null)
                {
                    IEnumerable<XElement> connects = from element in connectsElement.Elements()
                                                     where element.Name.LocalName == XVisioUtils.VISIO_CONNECT
                                                     select element;
                    foreach (XElement connect in connects)
                        _connects.Add(new XVisioConnect(connect));
                }
            }
        }

        /// <summary>
        /// Change page size
        /// </summary>
        /// <param name="width">Page Width</param>
        /// <param name="height">Page Height</param>
        public void SetPageSize(Double width, Double height)
        {
            XElement pageSheet = this.XHeader.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_PAGESHEET));

            if (pageSheet != null)
            {
                XElement pageWidthCell = pageSheet.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL))
                    .FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_PAGEWIDTH_ATTRIBUTE);
                if (pageWidthCell != null)
                    pageWidthCell.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value = (width / XVisioUtils.INCH_TO_MM).ToString("F15", new CultureInfo("en-US"));

                XElement pageHeightCell = pageSheet.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL))
                    .FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_PAGEHEIGHT_ATTRIBUTE);
                if (pageHeightCell != null)
                    pageHeightCell.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value = (height / XVisioUtils.INCH_TO_MM).ToString("F15", new CultureInfo("en-US"));
            }
        }

        /// <summary>
        /// Get Shape on the page
        /// </summary>
        /// <param name="nameU">Shape Universal Name</param>
        /// <returns></returns>
        public XVisioShape GetShape(string nameU)
        {
            if (String.IsNullOrWhiteSpace(nameU))
                throw new ArgumentNullException(XVisioUtils.VISIO_NAMEU_ATTRIBUTE, "NameU shuld be specified");

            return _shapes.FirstOrDefault(s => s.NameU == nameU);
        }

        /// <summary>
        /// Add Shape to the Page
        /// </summary>
        /// <param name="shapeType">Master Name</param>
        /// <param name="name">Shape Name</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Shape Width</param>
        /// <param name="height">Shape Height</param>
        public void AddShape(string shapeType, string name, Double? x, Double? y, Double width = 0, Double height = 0)
        {
            XVisioPackage stencil = this.VisioPackage.Stencil;

            if (stencil == null)
                throw new ArgumentNullException("stencil", "Stencil shoudn't be null");

            if (String.IsNullOrWhiteSpace(shapeType))
                throw new ArgumentNullException("shapeType", "Please supply shapeType");

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(XVisioUtils.VISIO_NAME_ATTRIBUTE, "Please supply shape name");

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            Dictionary<string, string> data = new Dictionary<string, string>();

            if (x != null)
                data.Add(XVisioUtils.VISIO_PINX_ATTRIBUTE, (x.Value / XVisioUtils.INCH_TO_MM).ToString(culture));
            if (y != null)
                data.Add(XVisioUtils.VISIO_PINY_ATTRIBUTE, (y.Value / XVisioUtils.INCH_TO_MM).ToString(culture));

            if (width > 0)
                data.Add(XVisioUtils.VISIO_WIDTH_ATTRIBUTE, (width / XVisioUtils.INCH_TO_MM).ToString(culture));
            if (height > 0)
                data.Add(XVisioUtils.VISIO_HEIGHT_ATTRIBUTE, (height / XVisioUtils.INCH_TO_MM).ToString(culture));

            if (this.VisioPackage.Masters == null)
                this.VisioPackage.AddMasters();

            XVisioMaster master = this.VisioPackage.Masters.ByNameU(shapeType);
            if (master == null)
            {
                XVisioMaster stencilMaster = stencil.Masters.ByNameU(shapeType);
                this.VisioPackage.Masters.AddMaster(stencilMaster);
                master = this.VisioPackage.Masters.ByNameU(shapeType);
            }

            AddShape(master, name, data);
            AddRelationship(master);
        }

        /// <summary>
        /// Connect Two shapes with 1-D shape
        /// </summary>
        /// <param name="from">From Shape (NameU)</param>
        /// <param name="to">To Shape (NameU)</param>
        /// <param name="link">Link Shape (NameU)</param>
        /// <param name="twoWay">Two way connection</param>
        /// <param name="tickness">Link tickness</param>
        public void ConnectShapes(string from, string to, string link, bool twoWay, double? tickness = null)
        {
            if (String.IsNullOrWhiteSpace(from))
                throw new ArgumentNullException("from", "From shape should be specified");
            if (String.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException("to", "To shape should be specified");

            if (String.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException("link", "Link shape should be specified");

            XElement connects = this.Xml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CONNECTS));
            if (connects == null)
                connects = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CONNECTS));

            XVisioShape fromShape = GetShape(from);
            XVisioShape toShape = GetShape(to);
            XVisioShape linkShape = GetShape(link);

            if (linkShape != null)
            {
                if (toShape != null)
                {
                    XElement connect = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CONNECT));
                    AddConnectData(connect, linkShape.Id, toShape.Id);
                    connects.Add(connect);
                }
                else
                    throw new ArgumentException("To shape doesn't exists", "to");
                if (fromShape != null)
                {
                    XElement connect = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CONNECT));
                    AddConnectData(connect, linkShape.Id, fromShape.Id, true);
                    connects.Add(connect);
                }
                else
                    throw new ArgumentException("From shape doesn't exists", "from");
                linkShape.ConfigureLinkShape(to, from, twoWay, tickness);
            }
        }

        /// <summary>
        /// Adds invisible shape
        /// </summary>
        /// <param name="shapeNameU">New Shape NameU</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Shape Width</param>
        /// <param name="height">Shape Height</param>
        /// <param name="parent">Parent Shape NameU</param>
        public void AddGhostNode(string shapeNameU, Double x, Double y, Double width, Double height, string parent = null)
        {
            if (String.IsNullOrWhiteSpace(shapeNameU))
                throw new ArgumentNullException("shapeNameU", "Please supply shape universal name");

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            string formulaX = String.Empty;
            string formulaY = String.Empty;

            if (parent != null)
            {
                XElement parentXml = GetShape(parent).Xml;

                if (parentXml != null)
                {
                    formulaX = Geometry.GetXFormula(parent, x);
                    formulaY = Geometry.GetYFormula(parent, y);
                }
            }

            // Create Xml representation of the Shape
            XElement shapeElement = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE));

            // Add attributes
            XAttribute newAttr = new XAttribute(XVisioUtils.VISIO_ID_ATTRIBUTE, this.NextId.ToString());
            shapeElement.Add(newAttr);
            newAttr = new XAttribute(XVisioUtils.VISIO_TYPE_ATTRIBUTE, XVisioUtils.VISIO_SHAPE);
            shapeElement.Add(newAttr);
            newAttr = new XAttribute(XVisioUtils.VISIO_NAMEU_ATTRIBUTE, shapeNameU);
            shapeElement.Add(newAttr);
            newAttr = new XAttribute(XVisioUtils.VISIO_NAME_ATTRIBUTE, shapeNameU);
            shapeElement.Add(newAttr);
            newAttr = new XAttribute(XVisioUtils.VISIO_LINESTYLE_ATTRIBUTE, "3");
            shapeElement.Add(newAttr);
            newAttr = new XAttribute(XVisioUtils.VISIO_FILLSTYLE_ATTRIBUTE, "3");
            shapeElement.Add(newAttr);
            newAttr = new XAttribute(XVisioUtils.VISIO_TEXTSTYLE_ATTRIBUTE, "3");
            shapeElement.Add(newAttr);

            // Add Cells
            if (formulaX == String.Empty)
                XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_PINX_ATTRIBUTE, (x / XVisioUtils.INCH_TO_MM).ToString(culture));
            else
                XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_PINX_ATTRIBUTE, (x / XVisioUtils.INCH_TO_MM).ToString(culture), formulaX);

            if (formulaY == String.Empty)
                XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_PINY_ATTRIBUTE, (y / XVisioUtils.INCH_TO_MM).ToString(culture));
            else
                XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_PINY_ATTRIBUTE, (y / XVisioUtils.INCH_TO_MM).ToString(culture), formulaY);

            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_WIDTH_ATTRIBUTE, (width / XVisioUtils.INCH_TO_MM).ToString(culture));
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_HEIGHT_ATTRIBUTE, (height / XVisioUtils.INCH_TO_MM).ToString(culture));
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_LOCPINX_ATTRIBUTE, "1", "Width*0.5");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_LOCPINY_ATTRIBUTE, "1", "Height*0.5");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_ANGLE_CELL, "0");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_FLIPX_CELL, "0");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_FLIPY_CELL, "0");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_RESIZEMODE_CELL, "0");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_FILLPATTERN_CELL, "0");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_LINEGRADIENTENABLED_CELL, "0");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_FILLGRADIENTENABLED_CELL, "0");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_ROTATEGRADIENTWITHSHAPE_CELL, "0");
            XVisioShape.SetCellValue(shapeElement, XVisioUtils.VISIO_USEGROUPGRADIENT_CELL, "0");

            // Add Geometry Section
            XElement section = AddSection(shapeElement, XVisioUtils.VISIO_GEOMETRY_SECTION, "0");
            XVisioShape.SetCellValue(section, XVisioUtils.VISIO_NOFILL_CELL, "0");
            XVisioShape.SetCellValue(section, XVisioUtils.VISIO_NOLINE_CELL, "0");
            XVisioShape.SetCellValue(section, XVisioUtils.VISIO_NOSHOW_CELL, "0");
            XVisioShape.SetCellValue(section, XVisioUtils.VISIO_NOSNAP_CELL, "0");
            XVisioShape.SetCellValue(section, XVisioUtils.VISIO_NOQUICKDRAG_CELL, "0");
            XElement sectionRow = AddRow(XVisioUtils.VISIO_RELMOVETO_ROW, "1");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_X_ATTRIBUTE, "0");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_Y_ATTRIBUTE, "0");
            sectionRow = AddRow(XVisioUtils.VISIO_RELLINETO_ROW, "2");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_X_ATTRIBUTE, "1");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_Y_ATTRIBUTE, "0");
            sectionRow = AddRow(XVisioUtils.VISIO_RELLINETO_ROW, "3");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_X_ATTRIBUTE, "1");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_Y_ATTRIBUTE, "1");
            sectionRow = AddRow(XVisioUtils.VISIO_RELLINETO_ROW, "4");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_X_ATTRIBUTE, "0");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_Y_ATTRIBUTE, "1");
            sectionRow = AddRow(XVisioUtils.VISIO_RELLINETO_ROW, "5");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_X_ATTRIBUTE, "0");
            XVisioShape.SetCellValue(sectionRow, XVisioUtils.VISIO_Y_ATTRIBUTE, "0");

            AddShape(shapeElement);
            XElement shapesElement = GetOrAddShapesElement();
            shapesElement.Add(shapeElement);
        }

        /// <summary>
        /// Adds Shape positioned within the Parent
        /// </summary>
        /// <param name="shapeType">Master Name</param>
        /// <param name="shapeNameU">New Shape NameU</param>
        /// <param name="parent">Parent Shape NameU</param>
        /// <param name="order">Shape order</param>
        /// <param name="count">Number of slave Shapes</param>
        public void AddSlaveNode(string shapeType, string shapeNameU, string parent, int order, int count)
        {
            if (String.IsNullOrWhiteSpace(shapeType))
                throw new ArgumentNullException("shapeType", "Please supply shape type.");

            if (String.IsNullOrWhiteSpace(shapeNameU))
                throw new ArgumentNullException("shapeNameU", "Please supply shape universal name.");

            if (String.IsNullOrWhiteSpace(parent))
                throw new ArgumentNullException("parent", "Please supply parent shape.");
            
            Dictionary<string, string> data = new Dictionary<string, string>();

            XVisioMaster master = this.VisioPackage.Masters.ByNameU(shapeType);
            if (master == null)
            {
                XVisioMaster stencilMaster = this.VisioPackage.Stencil.Masters.ByNameU(shapeType);
                this.VisioPackage.Masters.AddMaster(stencilMaster);
                master = this.VisioPackage.Masters.ByNameU(shapeType);
            }

            AddShape(master, shapeNameU, data);
            AddRelationship(master);

            XVisioShape parentShape = GetShape(parent);
            if (parentShape != null)
            {
                XElement parentXml = parentShape.Xml;

                if (parentXml != null)
                {
                    XVisioShape.SetCellValue(parentXml, XVisioUtils.VISIO_WIDTH_ATTRIBUTE, Geometry.GetWidth(count));
                    XVisioShape.SetCellValue(parentXml, XVisioUtils.VISIO_HEIGHT_ATTRIBUTE, Geometry.GetHeight(count));

                    XVisioShape newShape = GetShape(shapeNameU);
                    XElement nodeXml = newShape.Xml;
                    if (nodeXml != null)
                    {
                        XElement masterCell = newShape.GetCellFromMaster(shapeType, XVisioUtils.VISIO_PINX_ATTRIBUTE);
                        masterCell.Add(new XAttribute(XVisioUtils.VISIO_F_ATTRIBUTE, Geometry.GetMatrixXFormula(parent, order, count)));
                        nodeXml.Add(masterCell);
                        masterCell = newShape.GetCellFromMaster(shapeType, XVisioUtils.VISIO_PINY_ATTRIBUTE);
                        masterCell.Add(new XAttribute(XVisioUtils.VISIO_F_ATTRIBUTE, Geometry.GetMatrixYFormula(parent, order, count)));
                        nodeXml.Add(masterCell);
                    }
                }
            }
            else
                throw new ArgumentException("parent", "parent shape doesn't exist");
        }

        /// <summary>
        /// Adds shape with position relative to Parent
        /// </summary>
        /// <param name="shapeType">Master Name</param>
        /// <param name="shapeNameU">New Shape NameU</param>
        /// <param name="parent">Parent Shape NameU</param>
        /// <param name="x">Horizontal relative position</param>
        /// <param name="y">Vertical relative position</param>
        /// <param name="width">Shape Width</param>
        /// <param name="height">Shape Height</param>
        public void AddSlaveNode(string shapeType, string shapeNameU, string parent, Double x, Double y, Double width, Double height)
        {
            if (String.IsNullOrWhiteSpace(shapeType))
                throw new ArgumentNullException("shapeType", "Please supply shape type.");

            if (String.IsNullOrWhiteSpace(shapeNameU))
                throw new ArgumentNullException("shapeNameU", "Please supply shape universal name.");

            if (String.IsNullOrWhiteSpace(parent))
                throw new ArgumentNullException("parent", "Please supply parent shape.");

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            Dictionary<string, string> data = new Dictionary<string, string>();

            if (width > 0)
                data.Add(XVisioUtils.VISIO_WIDTH_ATTRIBUTE, (width / XVisioUtils.INCH_TO_MM).ToString(culture));
            if (height > 0)
                data.Add(XVisioUtils.VISIO_HEIGHT_ATTRIBUTE, (height / XVisioUtils.INCH_TO_MM).ToString(culture));

            XVisioMaster master = this.VisioPackage.Masters.ByNameU(shapeType);
            if (master == null)
            {
                XVisioMaster stencilMaster = this.VisioPackage.Stencil.Masters.ByNameU(shapeType);
                this.VisioPackage.Masters.AddMaster(stencilMaster);
                master = this.VisioPackage.Masters.ByNameU(shapeType);
            }
            AddShape(master, shapeNameU, data);
            AddRelationship(master);

            XElement parentXml = GetShape(parent).Xml;

            if (parentXml != null)
            {
                XElement nodeXml = GetShape(shapeNameU).Xml;
                if (nodeXml != null)
                {
                    string formula = string.Empty;
                    XElement masterCell = master.GetCell(XVisioUtils.VISIO_PINX_ATTRIBUTE);
                    if (masterCell == null)
                    {
                        masterCell = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
                        masterCell.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, XVisioUtils.VISIO_PINX_ATTRIBUTE));
                    }
                    formula = Geometry.GetXFormula(parent, x);
                    masterCell.Add(new XAttribute(XVisioUtils.VISIO_F_ATTRIBUTE, formula));
                    nodeXml.Add(masterCell);

                    masterCell = master.GetCell(XVisioUtils.VISIO_PINY_ATTRIBUTE);
                    if (masterCell == null)
                    {
                        masterCell = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
                        masterCell.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, XVisioUtils.VISIO_PINY_ATTRIBUTE));
                    }
                    formula = Geometry.GetYFormula(parent, y);
                    masterCell.Add(new XAttribute(XVisioUtils.VISIO_F_ATTRIBUTE, formula));
                    nodeXml.Add(masterCell);
                }
            }
        }

        /// <summary>
        /// Adds new Shape
        /// </summary>
        /// <param name="master">Master Name</param>
        /// <param name="name">New Shape NameU</param>
        /// <param name="data">Data for cells</param>
        private void AddShape(XVisioMaster master, string name, Dictionary<string, string> data)
        {
            if (master == null)
                throw new ArgumentNullException("master", "Master shouldn't be null.");

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Please supply shape universal name.");

            if (data == null)
                throw new ArgumentNullException("data", "data shouldn't be null.");

            int shapenum = this.NextId;

            XElement cell = null;
            XElement shapesElement = GetOrAddShapesElement();
            XElement masterShapeElement = master.Xml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES)).Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE));

            XElement shape = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE));
            shape.Add(new XAttribute(XVisioUtils.VISIO_ID_ATTRIBUTE, shapenum++));

            shape.Add(new XAttribute(XVisioUtils.VISIO_NAMEU_ATTRIBUTE, name));
            shape.Add(new XAttribute(XVisioUtils.VISIO_NAME_ATTRIBUTE, name));
            shape.Add(new XAttribute(masterShapeElement.Attribute(XVisioUtils.VISIO_TYPE_ATTRIBUTE)));
            shape.Add(new XAttribute(XVisioUtils.VISIO_MASTER, master.XHeader.Attribute(XVisioUtils.VISIO_ID_ATTRIBUTE).Value));

            foreach (string celName in data.Keys)
            {
                cell = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
                cell.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, celName));
                cell.Add(new XAttribute(XVisioUtils.VISIO_V_ATTRIBUTE, data[celName]));
                shape.Add(cell);
            }

            // Add layer to shape
            string layer = "-";
            if (master.Layer != null && _layerList.ContainsKey(master.Layer))
                layer = _layerList[master.Layer];
            else if (master.LayerSection != null)
            {
                layer = AddLayer(master.LayerSection);
            }
            if (layer != "-")
            {
                cell = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
                cell.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, XVisioUtils.VISIO_LAYERMEMBER_CELL));
                cell.Add(new XAttribute(XVisioUtils.VISIO_V_ATTRIBUTE, layer));
                shape.Add(cell);
            }


            // Add text properties
            XElement txtCell = masterShapeElement.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == "VerticalAlign");
            if (txtCell != null)
                shape.Add(txtCell);
            txtCell = masterShapeElement.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == "TxtWidth");
            if (txtCell != null)
                shape.Add(txtCell);
            txtCell = masterShapeElement.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == "TxtHeight");
            if (txtCell != null)
                shape.Add(txtCell);

            XElement shapes = masterShapeElement.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
            if (shapes != null)
            {
                XElement subShapes = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
                shape.Add(subShapes);
                foreach (XElement subShape in shapes.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE)))
                {
                    AddSubShapes(subShape, subShapes);
                }
            }

            XVisioShape newXShape = new XVisioShape(_visioPackage, shape);
            _shapes.Add(newXShape);
            shapesElement.Add(shape);
        }

        /// <summary>
        /// Gets existing or new Shapes element
        /// </summary>
        /// <returns>Shapes element</returns>
        private XElement GetOrAddShapesElement()
        {
            XElement shapesElement = this.Xml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
            if (shapesElement == null)
            {
                shapesElement = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
                this.Xml.Root.Add(shapesElement);
            }
            return shapesElement;
        }

        private void AddSubShapes(XElement shapeElement, XElement newShapes)
        {
            if (shapeElement == null)
                throw new ArgumentNullException("shapeElement", "Shape Element shouldn't be null.");

            if (newShapes == null)
                throw new ArgumentNullException("newShapes", "NewShapes Element shouldn't be null.");

            XElement shape = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE));
            shape.Add(new XAttribute(XVisioUtils.VISIO_ID_ATTRIBUTE, this.NextId));

            if (shapeElement.Attribute(XVisioUtils.VISIO_NAME_ATTRIBUTE) != null)
                shape.Add(new XAttribute(shapeElement.Attribute(XVisioUtils.VISIO_NAME_ATTRIBUTE)));
            if (shapeElement.Attribute(XVisioUtils.VISIO_NAMEU_ATTRIBUTE) != null)
                shape.Add(new XAttribute(shapeElement.Attribute(XVisioUtils.VISIO_NAMEU_ATTRIBUTE)));
            if (shapeElement.Attribute(XVisioUtils.VISIO_TYPE_ATTRIBUTE) != null)
                shape.Add(new XAttribute(shapeElement.Attribute(XVisioUtils.VISIO_TYPE_ATTRIBUTE)));
            shape.Add(new XAttribute(XVisioUtils.VISIO_MASTERSHAPE_ATTRIBUTE, shapeElement.Attribute(XVisioUtils.VISIO_ID_ATTRIBUTE).Value));

            // Add text properties
            XElement txtCell = shapeElement.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == "VerticalAlign");
            if (txtCell != null)
                shape.Add(txtCell);
            txtCell = shapeElement.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == "TxtWidth");
            if (txtCell != null)
                shape.Add(txtCell);
            txtCell = shapeElement.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).FirstOrDefault(e => e.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == "TxtHeight");
            if (txtCell != null)
                shape.Add(txtCell);

            XElement shapes = shapeElement.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
            if (shapes != null)
            {
                XElement subShapes = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
                shape.Add(subShapes);
                foreach (XElement subShape in shapes.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE)))
                {
                    AddSubShapes(subShape, subShapes);
                }
            }
            newShapes.Add(shape);
        }

        private string AddLayer(XElement fromMaster)
        {
            string layerId = this._nextLayer.ToString();
            XElement newRow = new XElement(fromMaster);
            newRow.SetAttributeValue(XVisioUtils.VISIO_IX_ATTRIBUTE, layerId);
            if (_layerSection == null)
            {
                XElement layer = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SECTION));
                layer.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, XVisioUtils.VISIO_LAYER_ATTRIBUTE));
                this.XHeader.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_PAGESHEET)).Add(layer);
            }
            _layerSection.Add(newRow);
            XVisioLayer newLayer = new XVisioLayer(newRow);
            _layers.Add(newLayer);
            _layerList.Add(newLayer.Name, layerId);
            return layerId;
        }

        private void AddRelationship(XVisioMaster master)
        {
            if (!this.Part.GetRelationships().Any(r => r.TargetUri == master.PartUri))
                this.Part.CreateRelationship(master.PartUri, TargetMode.Internal, XVisioUtils.VISIO_MASTER_URI);
        }

        private static void AddConnectData(XElement connect, string linkID, string toID, bool end = false)
        {
            connect.Add(new XAttribute(XVisioUtils.VISIO_FROMSHEET_ATTRIBUTE, linkID));
            //connect.Add(new XAttribute("FromCell", end ? "EndX" : "BeginX"));
            //connect.Add(new XAttribute("FromPart", end ? "12" : "9"));
            connect.Add(new XAttribute(XVisioUtils.VISIO_TOSHEET_ATTRIBUTE, toID));
            //connect.Add(new XAttribute("ToCell", XVisioUtils.VISIO_PINX_ATTRIBUTE));
            //connect.Add(new XAttribute("ToPart", "3"));
        }

        private static XElement AddSection(XElement element, string name, string ix)
        {
            XElement sectionElement = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SECTION));
            sectionElement.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, name));
            XAttribute ixAttribute = new XAttribute(XVisioUtils.VISIO_IX_ATTRIBUTE, ix);
            sectionElement.Add(ixAttribute);
            element.Add(sectionElement);
            return sectionElement;
        }

        private static XElement AddRow(string name, string ix)
        {
            XElement rowElement = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_ROW));
            XAttribute tAttribute = new XAttribute(XVisioUtils.VISIO_T_ATTRIBUTE, name);
            rowElement.Add(tAttribute);
            XAttribute ixAttribute = new XAttribute(XVisioUtils.VISIO_IX_ATTRIBUTE, ix);
            rowElement.Add(ixAttribute);

            return rowElement;
        }

        private void AddShape(XElement shapeXml)
        {
            string shapeId = XVisioUtils.GetAttributeValue(shapeXml, XVisioUtils.VISIO_ID_ATTRIBUTE);
            _shapes.Add(new XVisioShape(_visioPackage, shapeXml));
            int idInt;
            if (int.TryParse(shapeId, out idInt))
                _shapeIds.Add(idInt);
        }

    }
}
