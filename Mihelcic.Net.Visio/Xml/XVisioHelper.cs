using System;
using System.Drawing;
using System.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// High level Visio document manipulation
    /// </summary>
    public class XVisioHelper
    {
        /// <summary>
        /// Add shape to the Visio diagram
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeType">Master name</param>
        /// <param name="shapeNameU">Shape name</param>
        /// <param name="x">Shape's X position</param>
        /// <param name="y">Shape's Y position</param>
        /// <param name="width">Shape Width</param>
        /// <param name="height">Shape Height</param>
        /// <param name="page">Visio Page where to Add node</param>
        public static void AddNode(
            XVisioPackage package, 
            string shapeType, 
            string shapeNameU, 
            Double x, 
            Double y, 
            Double width = 0, 
            Double height = 0, 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            if (String.IsNullOrWhiteSpace(shapeNameU))
                throw new ArgumentNullException("shapeNameU", "Please supply shape name");

            XVisioPage pageObject = GetPage(package, page);
            
            pageObject.AddShape(shapeType, shapeNameU, x, y, width, height);
        }

        /// <summary>
        /// Add 1-Dimensional shape connecting two existing Diagram shapes
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeType">Master name</param>
        /// <param name="shapeNameU">Shape name</param>
        /// <param name="fromShape">From Shape name</param>
        /// <param name="toShape">To Shape name</param>
        /// <param name="twoWay">Is Connection two way or directed</param>
        /// <param name="tickness">Cnnection tickness</param>
        /// <param name="page">Visio Page where to Add connection</param>
        public static void AddConnection(
            XVisioPackage package,
            string shapeType,
            string shapeNameU,
            string fromShape,
            string toShape,
            bool twoWay,
            double? tickness,
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            if (String.IsNullOrWhiteSpace(shapeNameU))
                throw new ArgumentNullException("shapeNameU", "Please supply shape name");

            XVisioPage pageObject = GetPage(package, page);

            pageObject.AddShape(shapeType, shapeNameU, null, null);
            pageObject.ConnectShapes(fromShape, toShape, shapeNameU, twoWay, tickness);
        }

        /// <summary>
        /// Append Shape Text
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeNameU">Shape Name</param>
        /// <param name="text">Text to append</param>
        /// <param name="style">Text Style</param>
        /// <param name="page">Visio Page where Shape is located</param>
        public static void AddShapeText(
            XVisioPackage package, 
            string shapeNameU, 
            string text, 
            TextStyle style, 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            GetPage(package, page);

            XVisioShape shape = null;
            if (package.Pages != null && package.Pages[page] != null && package.Pages[page].Shapes != null)
                shape = package.Pages[page].Shapes.FirstOrDefault(s => s.NameU == shapeNameU);

            if (shape == null)
                throw new ArgumentException("Shape doesn't exist in the package!", "page");

            shape.AddShapeText(text, style);
        }

        /// <summary>
        /// Changes document Page size
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="width">Page width</param>
        /// <param name="height">Page Height</param>
        /// <param name="page">Visio Page to resize</param>
        public static void SetPageSize(
            XVisioPackage package, 
            Double width, 
            Double height, 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            if (package.Pages == null || package.Pages[page] == null)
                throw new ArgumentException("Page doesn't exist in the package", "page");

            XVisioPage pageObject = GetPage(package, page);
            pageObject.SetPageSize(width, height);
        }

        /// <summary>
        /// Sets Shape color
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeNameU">Shape Name</param>
        /// <param name="color">Color hex string to set (for example: #12345678)</param>
        /// <param name="shapeToColor">Subshape to color - default is main shape</param>
        /// <param name="page">Visio Page where Shape is located</param>
        public static void ChangeShapeColor(
            XVisioPackage package, 
            string shapeNameU, 
            string colorStr, 
            string shapeToColor = "", 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            GetPage(package, page);

            XVisioShape shape = null;
            if (package.Pages != null && package.Pages[page] != null && package.Pages[page].Shapes != null)
                shape = package.Pages[page].Shapes.FirstOrDefault(s => s.NameU == shapeNameU);

            if (shape == null)
                throw new ArgumentException("Shape doesn't exists in the package!", "page");


            colorStr = colorStr.TrimStart(new Char[] { '#' });
            int r = int.Parse(String.Format("{0}", colorStr.Substring(0, 2)), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(String.Format("{0}", colorStr.Substring(2, 2)), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(String.Format("{0}", colorStr.Substring(4, 2)), System.Globalization.NumberStyles.HexNumber);

            Color color = Color.FromArgb(r, g, b);
            shape.SetColor(color, shapeToColor);
        }

        /// <summary>
        /// Sets Shape color
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeNameU">Shape Name</param>
        /// <param name="color">Color hex string to set (for example: #12345678)</param>
        /// <param name="shapeToColor">Subshape to color - default is main shape</param>
        /// <param name="page">Visio Page where Shape is located</param>
        public static void ChangeShapeColor(
            XVisioPackage package, 
            string shapeNameU, 
            Color color, 
            string shapeToColor = "", 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            GetPage(package, page);

            XVisioShape shape = null;
            if (package.Pages != null && package.Pages[page] != null && package.Pages[page].Shapes != null)
                shape = package.Pages[page].Shapes.FirstOrDefault(s => s.NameU == shapeNameU);

            if (shape == null)
                throw new ArgumentException("Shape doesn't exist in the package!", "page");

            shape.SetColor(color, shapeToColor);
        }

        /// <summary>
        /// Sets Shape color
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeName">Shape Name</param>
        /// <param name="color">Color to set</param>
        /// <param name="shapeToColor">Subshape to color - default is main shape</param>
        /// <param name="page">Visio Page where Edge is located</param>
        public static void ChangeEdgeColor(
            XVisioPackage package, 
            string edgeName, 
            Color color, 
            string shapeToColor = "", 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            GetPage(package, page);

            XVisioShape shape = null;
            if (package.Pages != null && package.Pages[page] != null && package.Pages[page].Shapes != null)
                shape = package.Pages[page].Shapes.FirstOrDefault(s => s.NameU == edgeName);

            if (shape == null)
                throw new ArgumentException("Shape doesn't exist in the package!", "page");

            shape.SetEdgeColor(color, shapeToColor);
        }

        /// <summary>
        /// Changes Shape size by setting its Width and Height cells
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeNameU">Shape Name</param>
        /// <param name="width">Shape Width</param>
        /// <param name="height">Shape Height</param>
        /// <param name="page">Visio Page where Shape is located</param>
        public static void SetShapeSize(
            XVisioPackage package, 
            string shapeNameU, 
            int width, 
            int height, 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            GetPage(package, page);

            XVisioShape shape = null;
            if (package.Pages != null && package.Pages[page] != null && package.Pages[page].Shapes != null)
                shape = package.Pages[page].Shapes.FirstOrDefault(s => s.NameU == shapeNameU);

            if (shape == null)
                throw new ArgumentException("Shape doesn't exists in the package!", "page");

            shape.SetSize(width, height);
        }

        /// <summary>
        /// Add pop-up comment to the existing Visio Shape
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeNameU">Shape Name</param>
        /// <param name="comment">Comment to add</param>
        /// <param name="page">Visio Page where Shape is located</param>
        public static void AddComment(
            XVisioPackage package, 
            string shapeNameU, 
            string comment, 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            GetPage(package, page);

            XVisioShape shape = null;
            if (package.Pages != null && package.Pages[page] != null && package.Pages[page].Shapes != null)
                shape = package.Pages[page].Shapes.FirstOrDefault(s => s.NameU == shapeNameU);

            if (shape == null)
                throw new ArgumentException("Shape doesn't exists in the package!", "page");

            shape.SetComment(comment);
        }

        /// <summary>
        /// Add invisible container object for the hierarchy
        /// </summary>
        /// <param name="package"></param>
        /// <param name="shapeNameU"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="page">Visio Page where Shape should be located</param>
        public static void AddGhostNode(
            XVisioPackage package, 
            string shapeNameU, 
            Double x, 
            Double y, 
            Double width, 
            Double height, 
            string parent = null, 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            if (String.IsNullOrWhiteSpace(shapeNameU))
                throw new ArgumentNullException("shapeNameU", "Please supply shape name");

            XVisioPage pageObject = GetPage(package, page);

            pageObject.AddGhostNode(shapeNameU, x, y, width, height, parent);
        }

        /// <summary>
        /// Add shape relative to the existing shape in the Visio Diagram
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeType">Master name</param>
        /// <param name="shapeNameU">Shape name</param>
        /// <param name="parent">Parent Shape name</param>
        /// <param name="order">Ordinal place relative to the parent shape</param>
        /// <param name="count">Total number of slave shapes for this parent</param>
        /// <param name="page">Visio Page where Shape shuld be located</param>
        public static void AddSlaveNode(
            XVisioPackage package, 
            string shapeType, 
            string shapeNameU,
            string parent, 
            int order, 
            int count, 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            if (String.IsNullOrWhiteSpace(shapeType))
                throw new ArgumentNullException("shapeType", "Please supply shapeType");

            if (String.IsNullOrWhiteSpace(shapeNameU))
                throw new ArgumentNullException("shapeNameU", "Please supply shape name");

            XVisioPage pageObject = GetPage(package, page);

            pageObject.AddSlaveNode(shapeType, shapeNameU, parent, order, count);
        }

        /// <summary>
        /// Add shape relative to the existing shape in the Visio Diagram
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeType">Master name</param>
        /// <param name="name">Shape name</param>
        /// <param name="parent">Parent Shape name</param>
        /// <param name="order">Ordinal place relative to the parent shape</param>
        /// <param name="count">Total number of slave shapes for this parent</param>
        /// <param name="page">Visio Page where Shape shuld be located</param>
        public static void AddSlaveNode(
            XVisioPackage package, 
            string shapeType, 
            string name, 
            string parent, 
            Double x, 
            Double y, 
            Double width,
            Double height, 
            string page = "0")
        {
            if (package == null)
                throw new ArgumentNullException("package", "Package shoudn't be null");

            if (String.IsNullOrWhiteSpace(shapeType))
                throw new ArgumentNullException("shapeType", "Please supply shapeType");

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Please supply shape name");

            XVisioPage pageObject = GetPage(package, page);

            pageObject.AddSlaveNode(shapeType, name, parent, x, y, width, height);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="package">Package representing Visio diagram</param>
        /// <param name="shapeType">Master name</param>
        /// <returns>Size f Master shape</returns>
        public static XSize GetMasterSize(XVisioPackage package, string shapeType)
        {
            if (package == null)
                throw new ArgumentNullException("package", "Visio Package shouldn't be null");

            XVisioPackage stencil = package.Stencil;

            if (stencil == null)
                throw new ArgumentException("Package doesn't have assigned stencil", "package");

            XVisioMaster master = stencil.Masters.ByNameU(shapeType);

            if (master == null)
                throw new ArgumentException("Master doesn't exists");

            return master.GetSize();
        }

        private static XVisioPage GetPage(XVisioPackage package, string page)
        {
            XVisioPage pageObject = null;
            if (package.Pages != null)
                pageObject = package.Pages[page];

            if (pageObject == null)
                throw new ArgumentException("Page doesn't exists in the package!", "page");

            return pageObject;
        }
    }
}
