using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Represents Visio Shape Object
    /// </summary>
    public class XVisioShape
    {
        const string ANGLE_FORMULA = "ATAN2(IF(Geometry1.Y2>Geometry1.Y1,(Geometry1.Y2-Geometry1.Y1),(Geometry1.Y1-Geometry1.Y2)),IF(Geometry1.X2<Geometry1.X1,(Geometry1.X2-Geometry1.X1),(Geometry1.X1-Geometry1.X2)))";
        const string BEGIN_FORMULA = "_WALKGLUE(BegTrigger,EndTrigger,WalkPreference)";
        const string END_FORMULA = "_WALKGLUE(EndTrigger,BegTrigger,WalkPreference)";

        private XVisioPackage _visioPackage;
        private XElement _xml;
        private List<XVisioCell> _cells;
        private List<XVisioShape> _shapes;
        private string _master;
        private string _masterShape;
        private string _type;
        private string _name;
        private string _nameU;
        private string _id;

        /// <summary>
        /// Shape Id 
        /// </summary>
        public string Id { get { return _id; } }
        
        /// <summary>
        /// Shape Name
        /// </summary>
        public string Name { get { return _name; } }
        
        /// <summary>
        /// Shape Universal Name
        /// </summary>
        public string NameU { get { return _nameU; } }
        
        /// <summary>
        /// Shape Master Name
        /// </summary>
        public string Master { get { return _master; } }
        
        /// <summary>
        /// Shape Master Universal Name
        /// </summary>
        public string MasterNameU { get { return _visioPackage.Masters[this.Master].NameU; } }

        /// <summary>
        /// Shape Xml
        /// </summary>
        public XElement Xml { get { return _xml; } }


        /// <summary>
        /// Construct Visio Shape
        /// </summary>
        /// <param name="visioPackage"></param>
        /// <param name="xml"></param>
        public XVisioShape(XVisioPackage visioPackage, XElement xml)
        {
            if (visioPackage == null)
                throw new ArgumentNullException("visioPackage", "Visio Package shouldn't be null.");

            if (xml == null)
                throw new ArgumentNullException("xml", "Xml shouldn't be null.");

            _cells = new List<XVisioCell>();
            _shapes = new List<XVisioShape>();
            _xml = xml;

            _visioPackage = visioPackage;
            _master = XVisioUtils.GetAttributeValue(xml, XVisioUtils.VISIO_MASTER);
            _masterShape = XVisioUtils.GetAttributeValue(xml, XVisioUtils.VISIO_MASTERSHAPE_ATTRIBUTE);
            _type = XVisioUtils.GetAttributeValue(xml, XVisioUtils.VISIO_TYPE_ATTRIBUTE);
            _name = XVisioUtils.GetAttributeValue(xml, XVisioUtils.VISIO_NAME_ATTRIBUTE);
            _nameU = XVisioUtils.GetAttributeValue(xml, XVisioUtils.VISIO_NAMEU_ATTRIBUTE);
            _id = XVisioUtils.GetAttributeValue(xml, XVisioUtils.VISIO_ID_ATTRIBUTE);

            IEnumerable<XElement> cells = xml.Elements().Where(e => e.Name.LocalName == XVisioUtils.VISIO_CELL);
            foreach (XElement cell in cells)
                _cells.Add(new XVisioCell(cell));

            if (xml.HasElements && xml.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES)) != null)
            {
                IEnumerable<XElement> shapes = xml.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES)).Elements()
                    .Where(e => e.Name.LocalName == XVisioUtils.VISIO_SHAPE);
                foreach (XElement shape in shapes)
                    _shapes.Add(new XVisioShape(_visioPackage, shape));
            }
        }

        /// <summary>
        /// Append Shape Text
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeName">Shape Name</param>
        /// <param name="text">Text to append</param>
        /// <param name="style">Text Style</param>
        public void AddShapeText(string text, TextStyle style)
        {
            if (text == null)
                throw new ArgumentNullException("text", "Text should be supplied.");

            bool newText = false;
            XElement foundXml = _xml;

            if (_xml == null)
                throw new ArgumentException("Shape doesn't exists", "shapeName");

            // Locate Master
            string masterId = _xml.Attribute(XVisioUtils.VISIO_MASTER).Value;
            XElement shapeMaster_Shapes = _visioPackage.Masters[masterId].Xml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));

            // Locate master shape with text
            XElement textShape = shapeMaster_Shapes.Descendants(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE))
                .FirstOrDefault(s => s.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL))
                    .Any(c => c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == XVisioUtils.VISIO_TXTWIDTH_CELL));
            if (textShape != null)
            {
                string id = textShape.Attribute(XVisioUtils.VISIO_ID_ATTRIBUTE).Value;
                XElement foundShape = _xml.Descendants(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE))
                    .FirstOrDefault(n => n.Attribute(XVisioUtils.VISIO_MASTERSHAPE_ATTRIBUTE) != null && 
                        n.Attribute(XVisioUtils.VISIO_MASTERSHAPE_ATTRIBUTE).Value == id);  
                if (foundShape != null)
                    foundXml = foundShape;
            }


            XVisioTextStyleRefs styleRefs = AddShapeTextStyle(style);

            XElement textElement = foundXml.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_TEXT_ATTRIBUTE));

            if (textElement == null)
            {
                textElement = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_TEXT_ATTRIBUTE));
                newText = true;
            }
            else
            {
                string originalText = textElement.Value;
                XText oldText = (XText)textElement.LastNode;
                oldText.Value = String.Format("{0}\n", originalText);
            }

            if (styleRefs.Char > -1)
            {
                XElement cpElement = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CP_ATTRIBUTE));
                cpElement.Add(new XAttribute(XVisioUtils.VISIO_IX_ATTRIBUTE, styleRefs.Char));
                textElement.Add(cpElement);
            }

            if (styleRefs.Para > -1)
            {
                XElement ppElement = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_PP_ATTRIBUTE));
                ppElement.Add(new XAttribute(XVisioUtils.VISIO_IX_ATTRIBUTE, styleRefs.Para));
                textElement.Add(ppElement);
            }
            if (styleRefs.Tab > -1)
            {
                XElement tpElement = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_TP_ATTRIBUTE));
                tpElement.Add(new XAttribute(XVisioUtils.VISIO_IX_ATTRIBUTE, styleRefs.Char));
                textElement.Add(tpElement);
            }
            textElement.Add(new XText(text));
            if (newText)
                foundXml.Add(textElement);
        }

        /// <summary>
        /// Set Shape Color
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="shapeToColor">Subshape to color</param>
        public void SetColor(Color color, string shapeToColor)
        {
            XElement affectedShape = null;

            if (!String.IsNullOrWhiteSpace(shapeToColor))
            {
                affectedShape = _xml.Descendants(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE)).FirstOrDefault(s => s.Attribute(XVisioUtils.VISIO_NAMEU_ATTRIBUTE) != null && s.Attribute(XVisioUtils.VISIO_NAMEU_ATTRIBUTE).Value == shapeToColor);
            }
            else
                affectedShape = _xml;

            if (affectedShape != null)
            {
                ChangeSubShapesColor(affectedShape, color);

                SetCellValue(affectedShape, XVisioUtils.VISIO_FILLGRADIENTENABLED_CELL, "0");
            }
        }

        /// <summary>
        /// Change Edge (1-D shape) color
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="shapeToColor">Subshape to color</param>
        public void SetEdgeColor(Color color, string shapeToColor = "")
        {
            XElement affectedShape = null;
            if (!String.IsNullOrWhiteSpace(shapeToColor))
            {
                affectedShape = _xml.Descendants(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE)).FirstOrDefault(s => s.Attribute(XVisioUtils.VISIO_NAMEU_ATTRIBUTE) != null && s.Attribute(XVisioUtils.VISIO_NAMEU_ATTRIBUTE).Value == shapeToColor);
            }
            else
                affectedShape = _xml;

            if (affectedShape != null)
            {
                string argbStr = String.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));

                SetCellValue(affectedShape, XVisioUtils.VISIO_LINECOLOR_CELL, argbStr, String.Format("THEMEGUARD(RGB({0},{1},{2}))", color.R, color.G, color.B));
            }
            else
                throw new ArgumentException("Edge doesn't exists", "edgeName");
        }

        /// <summary>
        /// Set Shape Size
        /// </summary>
        /// <param name="width">New Width</param>
        /// <param name="height">New Hight</param>
        public void SetSize(int width, int height)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            if (_xml != null)
            {
                SetCellValue(XVisioUtils.VISIO_WIDTH_ATTRIBUTE, (width / XVisioUtils.INCH_TO_MM).ToString(culture));
                SetCellValue(XVisioUtils.VISIO_HEIGHT_ATTRIBUTE, (height / XVisioUtils.INCH_TO_MM).ToString(culture));
            }
        }

        /// <summary>
        /// Set Shape Comment
        /// </summary>
        /// <param name="comment">Cmment text</param>
        public void SetComment(string comment)
        {
            RemoveCommentFromMaster(_master);
            SetCellValue(XVisioUtils.VISIO_COMMENT_CELL, comment);
        }

        /// <summary>
        /// Configure Link Shape (1-D Shape)
        /// </summary>
        /// <param name="toShape">Connect To NameU</param>
        /// <param name="fromShape">Connect From NameU</param>
        /// <param name="twoWay">Is Connection two way</param>
        /// <param name="tickness">Shape tickness</param>
        public void ConfigureLinkShape(string toShape, string fromShape, bool twoWay, double? tickness)
        {
            if (String.IsNullOrWhiteSpace(toShape))
                throw new ArgumentNullException("toShape", "toShape should be supplied.");

            if (String.IsNullOrWhiteSpace(fromShape))
                throw new ArgumentNullException("fromShape", "fromShape should be supplied.");

            SetMasterCell(XVisioUtils.VISIO_WALKPREFERENCE_CELL);
            SetMasterCell(XVisioUtils.VISIO_GLUETYPE_CELL);

            SetCellValue(XVisioUtils.VISIO_BEGTRIGGER_CELL, null, String.Format("_XFTRIGGER('{0}'!EventXFMod)", toShape));
            SetCellValue(XVisioUtils.VISIO_ENDTRIGGER_CELL, null, String.Format("_XFTRIGGER('{0}'!EventXFMod)", fromShape));

            SetCellValue(XVisioUtils.VISIO_BEGINX_CELL, null, BEGIN_FORMULA);
            SetCellValue(XVisioUtils.VISIO_BEGINY_CELL, null, BEGIN_FORMULA);
            SetCellValue(XVisioUtils.VISIO_ENDX_CELL, null, END_FORMULA);
            SetCellValue(XVisioUtils.VISIO_ENDY_CELL, null, END_FORMULA);

            SetCellValue(XVisioUtils.VISIO_TXTANGLE_CELL, null, ANGLE_FORMULA);

            // Arrow Direction
            if (!twoWay)
            {
                XElement masterSection = GetSectionFromMaster(this.MasterNameU, XVisioUtils.VISIO_SCRATCH_SECTION);
                if (masterSection != null)
                {
                    XElement directionCell = masterSection.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_ROW))
                        .First().Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).First(c => c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == "B");
                    directionCell.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value = "2";
                    _xml.Add(new XElement(masterSection));
                }
                else
                {
                    SetCellValue(XVisioUtils.VISIO_ENDARROW_CELL, "8", null);
                }
            }

            // Tickness
            if (tickness != null)
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                double ticknessInMM = tickness.Value / XVisioUtils.MM_TO_POINT;
                SetCellValue(XVisioUtils.VISIO_LINEWEIGHT_CELL, ticknessInMM.ToString(culture));
            }
        }

        private bool CellExists(string cellName)
        {
            return _cells.Any(c => c.Name == cellName);
        }

        private XVisioCell GetCell(string cellName)
        {
            XVisioCell cell = null;

            if (CellExists(cellName))
            {
                cell = _cells.First(c => c.Name == cellName);
            }

            return cell;
        }

        internal XElement GetCellFromMaster(string shapeType, string cell)
        {
            XVisioMaster masterObject = _visioPackage.Masters.ByNameU(shapeType);
            if (masterObject != null)
            {
                XElement master = masterObject.Xml.Root;
                if (master != null)
                {
                    master = master.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
                    if (master != null)
                    {
                        master = master.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE));
                        if (master != null)
                        {
                                XElement masterCell = master.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).FirstOrDefault(c => c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == cell);
                                if (masterCell != null)
                                    return new XElement(masterCell);
                        }
                    }
                }
            }
            return null;
        }

        private XElement GetSectionFromMaster(string shapeType, string section)
        {
            XVisioMaster masterObject = _visioPackage.Masters.ByNameU(shapeType);
            if (masterObject != null)
            {
                XElement master = masterObject.Xml.Root;
                if (master != null)
                {
                    master = master.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
                    if (master != null)
                    {
                        master = master.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE));
                        if (master != null)
                        {
                            XElement masterSection = master.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SECTION)).FirstOrDefault(c => c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == section);
                            if (masterSection != null)
                                return new XElement(masterSection);
                        }
                    }
                }
            }
            return null;
        }

        private void SetCellValue(string cellName, string cellValue, string formula = null)
        {
            XVisioCell targetCell = GetCell(cellName);
            if (targetCell != null)
            {
                targetCell.Value = cellValue;
                if (formula != "")
                    targetCell.Formula = formula;
            }
            else
            {
                targetCell = new XVisioCell(cellName, cellValue, formula);
                _cells.Add(targetCell);
                _xml.Add(targetCell.Xml);
            }
        }

        private void SetMasterCell(string masterCellName)
        {
            XElement masterCell = GetCellFromMaster(this.MasterNameU, masterCellName);
            if (masterCell != null)
            {
                XVisioCell tmpCell = new XVisioCell(masterCell);
                SetCellValue(masterCellName, tmpCell.Value, tmpCell.Formula);
            }
        }

        /// <summary>
        /// Add Text style for the shape text
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeName">Shape Name</param>
        /// <param name="style">Text Style</param>
        /// <returns>Style refference</returns>
        private XVisioTextStyleRefs AddShapeTextStyle(TextStyle style)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("Mihelcic.Net.Visio.Xml.TextSettings.TextSettings.xml");
            XDocument template = XDocument.Load(stream);

            XElement styleXml = template.Root.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_STYLE, XVisioNamespace.TextSettings))
                .FirstOrDefault(s => s.Attribute(XVisioUtils.VISIO_NAME_ATTRIBUTE).Value == style.ToString());

            if (_xml == null)
                throw new ArgumentException("Shape doesn't exists", "shapeName");

            if (styleXml != null)
            {
                XVisioTextStyleRefs styleRef;
                bool styleExists = !_visioPackage.AddShapeStyle(_nameU, style, out styleRef);

                if (!styleExists)
                {
                    styleRef = new XVisioTextStyleRefs(style);
                    foreach (XElement templateSection in styleXml.Elements())
                    {
                        bool newSection = false;
                        string section = templateSection.Name.ToString();
                        if (section.IndexOf('}') > -1)
                            section = section.Substring(section.IndexOf('}') + 1);

                        XElement shapeStyleXml = null;
                        shapeStyleXml = _xml.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SECTION)).FirstOrDefault(s => s.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == section);
                        if (shapeStyleXml == null)
                        {
                            shapeStyleXml = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SECTION));
                            shapeStyleXml.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, section));
                            newSection = true;
                        }

                        XElement styleRowXml = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_ROW));
                        int index = _visioPackage.GetNextTextStyleId(_nameU, section);
                        styleRowXml.Add(new XAttribute(XVisioUtils.VISIO_IX_ATTRIBUTE, index));
                        foreach (XElement templateCell in templateSection.Elements())
                        {
                            XElement styleCellXml = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
                            styleCellXml.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, templateCell.Attribute(XVisioUtils.VISIO_NAME_ATTRIBUTE).Value));
                            styleCellXml.Add(new XAttribute(XVisioUtils.VISIO_V_ATTRIBUTE, templateCell.Attribute("Value").Value));
                            if (templateCell.Attribute("Formula") != null)
                                styleCellXml.Add(new XAttribute(XVisioUtils.VISIO_F_ATTRIBUTE, templateCell.Attribute("Formula").Value));
                            if (templateCell.Attribute("Unit") != null)
                                styleCellXml.Add(new XAttribute(XVisioUtils.VISIO_U_ATTRIBUTE, templateCell.Attribute("Unit").Value));
                            styleRowXml.Add(styleCellXml);
                        }
                        shapeStyleXml.Add(styleRowXml);
                        if (newSection)
                            _xml.Add(shapeStyleXml);

                        switch (section.ToLower())
                        {
                            case "paragraph":
                                styleRef.Para = index;
                                break;
                            case "character":
                                styleRef.Char = index;
                                break;
                            case "tab":
                                styleRef.Tab = index;
                                break;
                        }
                    }
                    _visioPackage.ShapeTextStyles[_nameU][style] = styleRef;
                }

                return styleRef;
            }
            return null;
        }

        internal static void SetCellValue(XElement element, string cellName, string cellValue, string formula = "")
        {
            XElement targetCell = null;
            IEnumerable<XElement> cellsSet = element.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
            if (cellsSet != null)
            {
                targetCell = cellsSet.FirstOrDefault(c => c.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == cellName);
            }
            if (targetCell != null)
            {
                targetCell.Attribute(XVisioUtils.VISIO_V_ATTRIBUTE).Value = cellValue;
                if (formula != "")
                    targetCell.Attribute(XVisioUtils.VISIO_F_ATTRIBUTE).Value = formula;
            }
            else
            {
                targetCell = new XElement(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL));
                targetCell.Add(new XAttribute(XVisioUtils.VISIO_N_ATTRIBUTE, cellName));
                if (cellValue != null)
                    targetCell.Add(new XAttribute(XVisioUtils.VISIO_V_ATTRIBUTE, cellValue));
                if (!String.IsNullOrWhiteSpace(formula))
                    targetCell.Add(new XAttribute(XVisioUtils.VISIO_F_ATTRIBUTE, formula));
                element.Add(targetCell);
            }
        }

        private bool RemoveCell(XElement section, string name)
        {
            XElement cellElement = section.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_CELL)).FirstOrDefault(s => s.Attribute(XVisioUtils.VISIO_N_ATTRIBUTE).Value == name);
            if (cellElement != null)
            {
                cellElement.Remove();
                return true;
            }
            return false;
        }

        private void ChangeSubShapesColor(XElement shape, Color color)
        {
            string argbStr = String.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));

            SetCellValue(shape, XVisioUtils.VISIO_FILLFOREGND_CELL, argbStr, String.Format("THEMEGUARD(RGB({0},{1},{2}))", color.R, color.G, color.B));

            foreach (XElement shapes in shape.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES)))
                foreach (XElement subShape in shapes.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE)))
                    ChangeSubShapesColor(subShape, color);
        }

        private void RemoveCommentFromMaster(string masterId)
        {
            XElement shapeMaster_Shapes = _visioPackage.Masters[masterId].Xml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
            RemoveCommentFromMaster(shapeMaster_Shapes);
        }

        private bool RemoveCommentFromMaster(XElement shapes)
        {
            bool result = false;

            foreach (XElement shape in shapes.Elements(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPE)))
            {
                result = result || RemoveCell(shape, XVisioUtils.VISIO_COMMENT_CELL);
                XElement subShapes = shape.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_SHAPES));
                if (subShapes != null)
                    result = result || RemoveCommentFromMaster(subShapes);
            }

            return result;
        }
    }
}
