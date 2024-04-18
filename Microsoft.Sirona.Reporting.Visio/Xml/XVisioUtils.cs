using System;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    public class XVisioUtils
    {
        #region Constants
        // Namespaces inside Visio Document
        public const string VISIO_DOCUMENT_URI = "http://schemas.microsoft.com/visio/2010/relationships/document";
        public const string VISIO_COREPROPERTIES_URI = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties";
        public const string VISIO_CUSTOMPROPERTIES_URI = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/custom-properties";
        public const string VISIO_EXTENDEDPROPERTIES_URI = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties";
        public const string VISIO_MASTERS_URI = "http://schemas.microsoft.com/visio/2010/relationships/masters";
        public const string VISIO_PAGES_URI = "http://schemas.microsoft.com/visio/2010/relationships/pages";
        public const string VISIO_WINDOWS_URI = "http://schemas.microsoft.com/visio/2010/relationships/windows";
        public const string VISIO_MASTER_URI = "http://schemas.microsoft.com/visio/2010/relationships/master";
        public const string VISIO_PAGE_URI = "http://schemas.microsoft.com/visio/2010/relationships/page";
        public const string VISIO_RELATIONSHIPS_URI = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/";
        public const string VISIO_THEME_URI = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme";

        public const string VISIO_MAIN_NS = "http://schemas.microsoft.com/office/visio/2012/main";
        public const string VISIO_TEXTSETTINGS_NS = "http://schemas.microsoft.com/sirona/visio/textsettings";
        public const string VISIO_RELATIONSHIPS_NS = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        public const string VISIO_MEDIA_PATH = "/visio/media/";
        public const string VISIO_MASTERS_PATH = "/visio/masters/masters.xml";
        public const string VISIO_DOCUMENT_PATH = "/visio/document.xml";
        public const string VISIO_CUSTOMPROPERTIES_PATH = "/docProps/custom.xml";
        public const string VISIO_DOC_PROPERTIES = "/docProps";

        public const string VISIO_MASTERS_CONTENT = "application/vnd.ms-visio.masters+xml";
        public const string VISIO_MASTER_CONTENT = "application/vnd.ms-visio.master+xml";

        public const string VISIO_STENCIL = ".vssx";
        public const string VISIO_DOCUMENT = ".vsdx";

        public const double INCH_TO_MM = 25.4;
        public const double MM_TO_POINT = 72;

        //Elements
        public const string VISIO_PROPERTIES = "Properties";
        public const string VISIO_REL = "Rel";
        public const string STENCIL_DOC = "StencilDocument";
        public const string VISIO_MASTER = "Master";
        public const string VISIO_MASTERS = "Masters";
        public const string VISIO_SHAPE = "Shape";
        public const string VISIO_SHAPES = "Shapes";
        public const string VISIO_CELL = "Cell";
        public const string VISIO_ROW = "Row";
        public const string VISIO_SECTION = "Section";
        public const string VISIO_LAYER = "Layer";
        public const string VISIO_COLORS = "Colors";
        public const string VISIO_FACENAMES = "FaceNames";
        public const string VISIO_STYLESHEETS = "StyleSheets";
        public const string VISIO_PAGESHEET = "PageSheet";
        public const string VISIO_CONNECTS = "Connects";
        public const string VISIO_CONNECT = "Connect";
        public const string VISIO_PAGE = "Page";
        public const string VISIO_PAGES = "Pages";
        public const string VISIO_STYLE = "Style";
        public const string VISIO_TEXT = "Text";

        //Attributes
        public const string VISIO_N_ATTRIBUTE = "N";
        public const string VISIO_V_ATTRIBUTE = "V";
        public const string VISIO_F_ATTRIBUTE = "F";
        public const string VISIO_U_ATTRIBUTE = "U";
        public const string VISIO_WIDTH_ATTRIBUTE = "Width";
        public const string VISIO_HEIGHT_ATTRIBUTE = "Height";
        public const string VISIO_NAME_ATTRIBUTE = "Name";
        public const string VISIO_NAMEU_ATTRIBUTE = "NameU";
        public const string VISIO_ID_ATTRIBUTE = "ID";
        public const string VISIO_id_ATTRIBUTE = "id";
        public const string VISIO_NAMEUNIV_ATTRIBUTE = "NameUniv";
        public const string VISIO_IX_ATTRIBUTE = "IX";
        public const string VISIO_TYPE_ATTRIBUTE = "Type";
        public const string VISIO_PINX_ATTRIBUTE = "PinX";
        public const string VISIO_LOCPINX_ATTRIBUTE = "LocPinX";
        public const string VISIO_PINY_ATTRIBUTE = "PinY";
        public const string VISIO_LOCPINY_ATTRIBUTE = "LocPinY";
        public const string VISIO_LINESTYLE_ATTRIBUTE = "LineStyle";
        public const string VISIO_FILLSTYLE_ATTRIBUTE = "FillStyle";
        public const string VISIO_TEXTSTYLE_ATTRIBUTE = "TextStyle";
        public const string VISIO_MASTERSHAPE_ATTRIBUTE = "MasterShape";
        public const string VISIO_T_ATTRIBUTE = "T";
        public const string VISIO_LAYER_ATTRIBUTE = "Layer";
        public const string VISIO_FROMSHEET_ATTRIBUTE = "FromSheet";
        public const string VISIO_FROMPART_ATTRIBUTE = "FromPart";
        public const string VISIO_FROMCELL_ATTRIBUTE = "FromCell";
        public const string VISIO_TOSHEET_ATTRIBUTE = "ToSheet";
        public const string VISIO_TOPART_ATTRIBUTE = "ToPart";
        public const string VISIO_TOCELL_ATTRIBUTE = "ToCell";
        public const string VISIO_TEXT_ATTRIBUTE = "Text";
        public const string VISIO_CP_ATTRIBUTE = "cp";
        public const string VISIO_PP_ATTRIBUTE = "pp";
        public const string VISIO_TP_ATTRIBUTE = "tp";
        public const string VISIO_PAGEWIDTH_ATTRIBUTE = "PageWidth";
        public const string VISIO_PAGEHEIGHT_ATTRIBUTE = "PageHeight";
        public const string VISIO_X_ATTRIBUTE = "X";
        public const string VISIO_Y_ATTRIBUTE = "Y";

        public const string VISIO_SCRATCH_SECTION = "Scratch";
        public const string VISIO_GEOMETRY_SECTION = "Geometry";

        public const string VISIO_RELMOVETO_ROW = "RelMoveTo";
        public const string VISIO_RELLINETO_ROW = "RelLineTo";

        public const string VISIO_ANGLE_CELL = "Angle";
        public const string VISIO_BEGINX_CELL = "BeginX";
        public const string VISIO_BEGINY_CELL = "BeginY";
        public const string VISIO_BEGTRIGGER_CELL = "BegTrigger";
        public const string VISIO_COMMENT_CELL = "Comment";
        public const string VISIO_ENDARROW_CELL = "EndArrow";
        public const string VISIO_ENDTRIGGER_CELL = "EndTrigger";
        public const string VISIO_ENDX_CELL = "EndX";
        public const string VISIO_ENDY_CELL = "EndY";
        public const string VISIO_FILLFOREGND_CELL = "FillForegnd";
        public const string VISIO_FILLGRADIENTENABLED_CELL = "FillGradientEnabled";
        public const string VISIO_FILLPATTERN_CELL = "FillPattern";
        public const string VISIO_FLIPX_CELL = "FlipX";
        public const string VISIO_FLIPY_CELL = "FlipY";
        public const string VISIO_GLUETYPE_CELL = "GlueType";
        public const string VISIO_LAYERMEMBER_CELL = "LayerMember";
        public const string VISIO_LINECOLOR_CELL = "LineColor";
        public const string VISIO_LINEGRADIENTENABLED_CELL = "LineGradientEnabled";
        public const string VISIO_LINEWEIGHT_CELL = "LineWeight";
        public const string VISIO_NOFILL_CELL = "NoFill";
        public const string VISIO_NOLINE_CELL = "NoLine";
        public const string VISIO_NOQUICKDRAG_CELL = "NoQuickDrag";
        public const string VISIO_NOSHOW_CELL = "NoShow";
        public const string VISIO_NOSNAP_CELL = "NoSnap";
        public const string VISIO_RESIZEMODE_CELL = "ResizeMode";
        public const string VISIO_ROTATEGRADIENTWITHSHAPE_CELL = "RotateGradientWithShape";
        public const string VISIO_TXTANGLE_CELL = "TxtAngle";
        public const string VISIO_TXTWIDTH_CELL = "TxtWidth";
        public const string VISIO_USEGROUPGRADIENT_CELL = "UseGroupGradient";
        public const string VISIO_WALKPREFERENCE_CELL = "WalkPreference";

        #endregion

        /// <summary>
        /// Add Namespace prefix to name
        /// </summary>
        /// <param name="name">Name to extend with namespace</param>
        /// <returns>Name extended with namespace</returns>
        public static string AddNameSpace(string name, XVisioNamespace ns = XVisioNamespace.Main)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Name is empty.");

            return String.Format("{0}{1}{2}{3}", "{", GetNameSpace(ns), "}", name);
        }

        /// <summary>
        /// Return Attribute Value from XElement
        /// </summary>
        /// <param name="element">XElement</param>
        /// <param name="attributeName">Attribute Name</param>
        /// <returns>Attribute Value</returns>
        public static string GetAttributeValue(XElement element, string attributeName)
        {
            if (element == null)
                throw new ArgumentNullException("element", "Element is Empty.");

            if (string.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentNullException("attributeName", "AttributeName is empty");

            if (element.HasAttributes && element.Attribute(attributeName) != null)
                return element.Attribute(attributeName).Value;
            return String.Empty;
        }

        private static string GetNameSpace(XVisioNamespace ns)
        {
            switch (ns)
            {
                case XVisioNamespace.Main:
                    return VISIO_MAIN_NS;
                case XVisioNamespace.TextSettings:
                    return VISIO_TEXTSETTINGS_NS;
                case XVisioNamespace.Relationships:
                    return VISIO_RELATIONSHIPS_NS;
                default:
                    return String.Empty;
            }
        }
    }
}
