using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Manipulating Visio vs?x document
    /// </summary>
    public class XVisioPackage : IDisposable
    {
        #region Private Variables

        private bool _isStencil;
        private bool _readOnly = false;
        private Package _visioPackage;
        private XVisioPackage _stencil = null;
        private XVisioMasters _masters;
        private XVisioPages _pages;
        private Dictionary<string, XVisioPart> _parts;
        private XVisioPart _document;
        private Dictionary<string, Dictionary<TextStyle, XVisioTextStyleRefs>> _shapeTextStyles;

        #endregion

        #region Public Properties
        /// <summary>
        /// Is this Stencil Document
        /// </summary>
        public bool IsStencil { get { return _isStencil; } }

        /// <summary>
        /// Visio Document Package
        /// </summary>
        public Package Package { get { return _visioPackage; } }

        /// <summary>
        /// Masters in the document
        /// </summary>
        public XVisioMasters Masters { get { return _masters; } }

        /// <summary>
        /// Document's Pages
        /// </summary>
        public XVisioPages Pages { get { return _pages; } }

        /// <summary>
        /// Parts in the Document root
        /// </summary>
        public Dictionary<string, XVisioPart> Parts { get { return _parts; } }

        /// <summary>
        /// Appropriate Stencil Document
        /// </summary>
        public XVisioPackage Stencil { get { return _stencil; } }

        /// <summary>
        /// Maping Shape Text Style names to style definitions
        /// </summary>
        public Dictionary<string, Dictionary<TextStyle, XVisioTextStyleRefs>> ShapeTextStyles { get { return _shapeTextStyles; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs XVisioPackage reading Visio document
        /// </summary>
        /// <param name="fileName">Visio Document File</param>
        /// <param name="stencilFileName">Stencil Document File</param>
        public XVisioPackage(string fileName, string stencilFileName = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName", "File name shouldn't be empty");

            Initialize();

            OpenPackage(fileName);

            GetPackageParts();

            if (!String.IsNullOrWhiteSpace(stencilFileName))
            {
                _stencil = new XVisioPackage(stencilFileName);
                AddStencilStyles();
            }
        }

        /// <summary>
        /// Constructs XVisioPackage reading Visio document stream
        /// </summary>
        /// <param name="stream">Visio Document stream</param>
        /// <param name="stencilStream">Stencil Document stream</param>
        public XVisioPackage(Stream stream, Stream stencilStream = null)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "Stream shouldn't be null");
            _readOnly = !stream.CanWrite;

            Initialize();

            OpenPackage(stream);

            GetPackageParts();

            if (stencilStream != null)
            {
                _stencil = new XVisioPackage(stencilStream);
                AddStencilStyles();
            }
        }

        #endregion

        #region Initialization Private Methods
        /// <summary>
        /// Inicialize private variables
        /// </summary>
        private void Initialize()
        {
            _isStencil = false;
            _parts = new Dictionary<string, XVisioPart>();
            _shapeTextStyles = new Dictionary<string, Dictionary<TextStyle, XVisioTextStyleRefs>>();
        }

        /// <summary>
        /// Opens Visio Document as Package variable
        /// </summary>
        /// <param name="fileName">Visio File to open</param>
        private void OpenPackage(string fileName)
        {
            Package visioPackage = null;

            // Get the Visio file from the location.
            if (File.Exists(fileName))
            {
                string extension = Path.GetExtension(fileName);
                _isStencil = extension.ToLower() == XVisioUtils.VISIO_STENCIL;
                _readOnly = _isStencil;

                FileAccess access = FileAccess.ReadWrite;

                if (_isStencil)
                {
                    access = FileAccess.Read;
                }
                // Open the Visio file as a package with
                // read/write file access.
                try
                {
                    visioPackage = Package.Open(
                        fileName,
                        FileMode.Open,
                        access);
                }
                catch (FileFormatException)
                {
                    throw new FileFormatException("Supported are only Visio2013 file formats: .vsdx and .vssx");
                }
            }
            else
                throw new ArgumentException(String.Format("File {0} doesn't exists", fileName), "fileName");

            // Return the Visio file as a package.
            _visioPackage = visioPackage;
        }

        /// <summary>
        /// Open Package
        /// </summary>
        /// <param name="stream">Document stream</param>
        private void OpenPackage(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "You must supply valid document stream");

            Package visioPackage = null;
            _readOnly = !stream.CanWrite;

            //this code originally required the stream to be read/write, but the system requires read only stream support
            visioPackage = Package.Open(
                stream,
                FileMode.Open,
                stream.CanWrite ? FileAccess.ReadWrite : FileAccess.Read);

            // Return the Visio file as a package.
            _visioPackage = visioPackage;
        }

        /// <summary>
        /// Read Parts from Package
        /// </summary>
        private void GetPackageParts()
        {
            foreach (PackageRelationship rel in _visioPackage.GetRelationships())
            {
                Uri absUri = PackUriHelper.ResolvePartUri(rel.SourceUri, rel.TargetUri);
                XVisioPart xPart = new XVisioPart(this, absUri, null);
                _parts.Add(absUri.OriginalString, xPart);
                if (rel.RelationshipType == XVisioUtils.VISIO_DOCUMENT_URI)
                    _document = xPart;
            }
        }

        #endregion

        /// <summary>
        /// Register Masters
        /// </summary>
        /// <param name="masters">Masters Object</param>
        internal void RegisterMasters(XVisioMasters masters)
        {
            _masters = masters;
        }

        /// <summary>
        /// Create and Register Masters Part
        /// </summary>
        public void AddMasters()
        {
            if (_document != null)
            {
                Uri mastersUri = new Uri(XVisioUtils.VISIO_MASTERS_PATH, UriKind.Relative);
                PackagePart newPart = this.Package.CreatePart(PackUriHelper.CreatePartUri(mastersUri), XVisioUtils.VISIO_MASTERS_CONTENT, CompressionOption.SuperFast);
                XVisioPackage.SavePart(newPart, CreateEmpty(XVisioUtils.VISIO_MASTERS));
                PackageRelationship rel = _document.Part.CreateRelationship(mastersUri, TargetMode.Internal, XVisioUtils.VISIO_MASTERS_URI);
                _masters = new XVisioMasters(this, mastersUri, _document);
                _parts.Add(mastersUri.OriginalString, _masters);
            }
        }

        /// <summary>
        /// Register Pages Object
        /// </summary>
        /// <param name="pages">Pages Object</param>
        internal void RegisterPages(XVisioPages pages)
        {
            _pages = pages;
        }

        /// <summary>
        /// Register Document Object
        /// </summary>
        /// <param name="document">Document Object</param>
        internal void RegisterDocument(XVisioPart document)
        {
            _document = document;
        }

        /// <summary>
        /// Set Visio document for recalculation
        /// </summary>
        public void RecalcDocument()
        {
            // Get the Custom File Properties part from the package and
            // and then extract the XML from it.
            XDocument customPartXML = _parts[XVisioUtils.VISIO_CUSTOMPROPERTIES_PATH].Xml;

            // Check to see whether document recalculation has already been 
            // set for this document. If it hasn't, use the integer
            // value returned by CheckForRecalc as the property ID.
            int pidValue = CheckForRecalc(customPartXML);

            if (pidValue > -1)
            {

                XElement customPartRoot = customPartXML.Elements().ElementAt(0);

                // Two XML namespaces are needed to add XML data to this 
                // document. Here, we're using the GetNamespaceOfPrefix and 
                // GetDefaultNamespace methods to get the namespaces that 
                // we need. You can specify the exact strings for the 
                // namespaces, but that is not recommended.
                XNamespace customVTypesNS = customPartRoot.GetNamespaceOfPrefix("vt");
                XNamespace customPropsSchemaNS = customPartRoot.GetDefaultNamespace();

                // Construct the XML for the new property in the XDocument.Add method.
                // This ensures that the XNamespace objects will resolve properly, 
                // apply the correct prefix, and will not default to an empty namespace.
                customPartRoot.Add(
                    new XElement(customPropsSchemaNS + "property",
                        new XAttribute("pid", pidValue.ToString()),
                        new XAttribute("name", "RecalcDocument"),
                        new XAttribute("fmtid",
                            "{D5CDD505-2E9C-101B-9397-08002B2CF9AE}"),
                        new XElement(customVTypesNS + "bool", "true")
                    ));
            }
        }

        private static int CheckForRecalc(XDocument customPropsXDoc)
        {

            // Set the inital pidValue to -1, which is not an allowed value.
            // The calling code tests to see whether the pidValue is 
            // greater than -1.
            int pidValue = -1;

            // Get all of the property elements from the document. 
            IEnumerable<XElement> props = GetXElementsByName(
                customPropsXDoc, "property");

            // Get the RecalcDocument property from the document if it exists already.
            XElement recalcProp = GetXElementByAttribute(props,
                "name", "RecalcDocument");

            // If there is already a RecalcDocument instruction in the 
            // Custom File Properties part, then we don't need to add another one. 
            // Otherwise, we need to create a unique pid value.
            if (recalcProp != null)
            {
                return pidValue;
            }
            else
            {
                // Get all of the pid values of the property elements and then
                // convert the IEnumerable object into an array.
                IEnumerable<string> propIDs =
                    from prop in props
                    where prop.Name.LocalName == "property"
                    select prop.Attribute("pid").Value;
                string[] propIDArray = propIDs.ToArray();

                // Increment this id value until a unique value is found.
                // This starts at 2, because 0 and 1 are not valid pid values.
                int id = 2;
                while (pidValue == -1)
                {
                    if (propIDArray.Contains(id.ToString()))
                    {
                        id++;
                    }
                    else
                    {
                        pidValue = id;
                    }
                }
            }
            return pidValue;

        }

        private void AddStencilStyles()
        {
            XDocument docStencilXml = _stencil.Parts[XVisioUtils.VISIO_DOCUMENT_PATH].Xml;
            XDocument docXml = _parts[XVisioUtils.VISIO_DOCUMENT_PATH].Xml;

            //Clear current DocumentPart
            XElement colorsElement = docXml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_COLORS));
            colorsElement.RemoveAll();
            XElement facesElement = docXml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_FACENAMES));
            facesElement.RemoveAll();
            XElement stylesElement = docXml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_STYLESHEETS));
            stylesElement.RemoveAll();

            //Add from stencil
            XElement stencilColors = docStencilXml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_COLORS));
            colorsElement.Add(stencilColors.Elements());
            XElement stencilFaces = docStencilXml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_FACENAMES));
            facesElement.Add(stencilFaces.Elements());
            XElement stencilStyles = docStencilXml.Root.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_STYLESHEETS));
            stylesElement.Add(stencilStyles.Elements());
        }

        /// <summary>
        /// Reads Text Style from shape
        /// </summary>
        /// <param name="shapeName">Shape name</param>
        /// <param name="style">Text Style to add</param>
        /// <param name="styleRef">Output Style refference</param>
        /// <returns>True if succes</returns>
        public bool AddShapeStyle(string shapeName, TextStyle style, out XVisioTextStyleRefs styleRef)
        {
            if (String.IsNullOrWhiteSpace(shapeName))
                throw new ArgumentNullException("shapeName", "Shape Name should be supplied.");

            if (!_shapeTextStyles.ContainsKey(shapeName))
                _shapeTextStyles.Add(shapeName, new Dictionary<TextStyle, XVisioTextStyleRefs>());

            if (_shapeTextStyles[shapeName].ContainsKey(style))
            {
                styleRef = _shapeTextStyles[shapeName][style];
                return false;
            }

            styleRef = new XVisioTextStyleRefs(style);
            _shapeTextStyles[shapeName].Add(style, styleRef);
            return true;
        }

        /// <summary>
        /// Returns Next Text style Id
        /// </summary>
        /// <param name="shapeName">Shape Name where style should be added</param>
        /// <param name="section">Section to look for</param>
        /// <returns>Next Text style Id</returns>
        public int GetNextTextStyleId(string shapeName, string section)
        {
            if (String.IsNullOrWhiteSpace(shapeName))
                throw new ArgumentNullException("shapeName", "Shape Name should be supplied.");

            if (String.IsNullOrWhiteSpace(shapeName))
                throw new ArgumentNullException("section", "Section should be supplied.");

            if (!_shapeTextStyles.ContainsKey(shapeName))
                return 0;
            if (_shapeTextStyles[shapeName].Count == 0)
                return 0;

            if (section.IndexOf('}') > -1)
                section = section.Substring(section.IndexOf('}') + 1);

            switch (section.ToLower())
            {
                case "paragraph":
                    return _shapeTextStyles[shapeName].Values.Max(r => r.Para) + 1;
                case "character":
                    return _shapeTextStyles[shapeName].Values.Max(r => r.Char) + 1;
                case "tab":
                    return _shapeTextStyles[shapeName].Values.Max(r => r.Tab) + 1;
                default: return -1;
            }
        }

        /// <summary>
        /// Close document
        /// </summary>
        public void Close()
        {
            this.Save();
            if (_stencil != null)
                _stencil._visioPackage.Close();
            _visioPackage.Close();
            disposed = true;
        }

        /// <summary>
        /// Save Document
        /// </summary>
        public void Save()
        {
            if (!_readOnly)
            {
                foreach (XVisioPart part in _parts.Values)
                {
                    _visioPackage.Flush();
                    part.Save();
                }
            }
            if (_stencil != null)
            {
                _stencil.Save();
            }
        }

        internal static void SavePart(PackagePart packagePart, XDocument partXML)
        {
            // Create a new XmlWriterSettings object to 
            // define the characteristics for the XmlWriter
            XmlWriterSettings partWriterSettings = new XmlWriterSettings();
            partWriterSettings.Encoding = Encoding.UTF8;
            partWriterSettings.NewLineChars = "\n";
            partWriterSettings.CloseOutput = true;

            using (Stream partStream = packagePart.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                // Create a new XmlWriter and then write the XML
                // back to the document part.
                StreamWriter streamWriter = new StreamWriter(partStream);
                XmlWriter partWriter = XmlWriter.Create(streamWriter, partWriterSettings);

                partXML.Save(partWriter);

                // Flush and close the XmlWriter.
                partWriter.Flush();
                partWriter.Close();

            }
        }

        /// <summary>
        /// Creates Empty XDocument as a part of Visio Document
        /// </summary>
        /// <param name="name">Root element name</param>
        /// <returns>XDocument as a part of Visio Document</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Xml.Linq.XDocument.Parse(System.String)")]
        public static XDocument CreateEmpty(string name)
        {
            string xmlText = String.Format("<{2} xmlns=\"{0}\" xmlns:r=\"{1}\" xml:space=\"preserve\"></{2}>",
                XVisioUtils.VISIO_MAIN_NS, XVisioUtils.VISIO_RELATIONSHIPS_URI, name);
            return XDocument.Parse(xmlText);
        }

        /// <summary>
        /// Returns IEnumerable of XElement containing XElements from PackagePart of requested Type
        /// </summary>
        /// <param name="packagePart">PackagePart to search</param>
        /// <param name="elementType">Element Type requested</param>
        /// <returns>IEnumerable of XElement containing XElements from PackagePart of requested Type</returns>
        public static IEnumerable<XElement> GetXElementsByName(XDocument packagePart, string elementType)
        {
            if (packagePart == null)
                throw new ArgumentNullException("packagePart", "Package Part shouldn't be null");

            if (String.IsNullOrWhiteSpace(elementType))
                throw new ArgumentNullException("elementType", "Element Type shuld be specified.");

            // Construct a LINQ query that selects elements by their element type.
            IEnumerable<XElement> elements =
                from element in packagePart.Descendants()
                where element.Name.LocalName == elementType
                select element;

            // Return the selected elements to the calling code.
            return elements.DefaultIfEmpty(null);
        }

        /// <summary>
        /// Returns IEnumerable of XElement containing XElements from IEnumerable of XElement with specific attribute value
        /// </summary>
        /// <param name="elements">IEnumerable<XElement> to search</param>
        /// <param name="attributeName">Attribute to search</param>
        /// <param name="attributeValue">Attribute value</param>
        /// <returns>IEnumerable of XElement containing XElements from IEnumerable of XElement with specific attribute value</returns>
        public static XElement GetXElementByAttribute(IEnumerable<XElement> elements, string attributeName, string attributeValue)
        {
            if (elements == null)
                throw new ArgumentNullException("elements", "Elements shouldn't be null.");

            if (String.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentNullException("attributeName", "Attribute Name shouldn't be empty.");

            // Construct a LINQ query that selects elements from a group
            // of elements by the value of a specific attribute.
            IEnumerable<XElement> selectedElements =
                from el in elements
                where el.Attribute(attributeName).Value == attributeValue
                select el;

            // If there aren't any elements of the specified type
            // with the specified attribute value in the document,
            // return null to the calling code.
            return selectedElements.DefaultIfEmpty(null).FirstOrDefault();
        }

        /// <summary>
        /// Set custom property for the document
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        /// <param name="propertyValue">Property Value</param>
        /// <param name="propertyType">Data Type</param>
        /// <returns>Property Value if exists</returns>
        public string SetCustomProperty(string propertyName, object propertyValue, PropertyTypes propertyType)
        {
            if (String.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException("propertyName", "Property Name shouldn't be empty");
            if (propertyValue == null)
                throw new ArgumentNullException("propertyValue", "Property Value shouldn't be null");

            string returnValue = null;

            Uri customUri = new Uri(XVisioUtils.VISIO_CUSTOMPROPERTIES_PATH, UriKind.Relative);
            if (_parts.ContainsKey(customUri.OriginalString))
            {
                XDocument customPartXML = _parts[customUri.OriginalString].Xml;

                XElement customPartRoot = customPartXML.Elements().ElementAt(0);

                XNamespace customVTypesNS = customPartRoot.GetNamespaceOfPrefix("vt");
                XNamespace customPropsSchemaNS = customPartRoot.GetDefaultNamespace();

                int pidValue = 2; // Minimal pidValue is 2
                if (customPartRoot.Elements().Count() > 0)
                    pidValue = customPartRoot.Elements().Max(e => Int32.Parse(e.Attribute("pid").Value)) + 1;

                if (customPartRoot.Elements().Any(e => e.Attribute("name").Name == propertyName))
                    customPartRoot.Elements().First(e => e.Attribute("name").Name == propertyName).Elements().First().SetValue(propertyValue);
                else
                {
                    XElement valueElement = null;
                    switch (propertyType)
                    {
                        case PropertyTypes.NumberInteger:
                            valueElement = new XElement(customVTypesNS + "i4", propertyValue);
                            break;
                        case PropertyTypes.Text:
                            valueElement = new XElement(customVTypesNS + "lpwstr", propertyValue);
                            break;
                        case PropertyTypes.DateTime:
                            valueElement = new XElement(customVTypesNS + "filetime", string.Format("{0:s}Z", Convert.ToDateTime(propertyValue)));
                            break;
                        case PropertyTypes.YesNo:
                            valueElement = new XElement(customVTypesNS + "bool", Convert.ToBoolean(propertyValue).ToString().ToLower());
                            break;
                    }
                    if (valueElement != null)
                        customPartRoot.Add(
                            new XElement(customPropsSchemaNS + "property",
                                new XAttribute("pid", pidValue.ToString()),
                                new XAttribute("name", propertyName),
                                new XAttribute("fmtid", "{D5CDD505-2E9C-101B-9397-08002B2CF9AE}"),
                                valueElement));
                }
            }

            return returnValue;
        }

        public string GetCustomProperty(string propertyName)
        {
            if (String.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException("propertyName", "property name shouldn't be null");

            string returnValue = null;

            Uri customUri = new Uri(XVisioUtils.VISIO_CUSTOMPROPERTIES_PATH, UriKind.Relative);
            if (_parts.ContainsKey(customUri.OriginalString))
            {
                XDocument customPartXML = _parts[customUri.OriginalString].Xml;
                if (customPartXML.Root.Elements().Any(e => e.Attribute("name").Value == propertyName))
                {
                    XElement valueElement = customPartXML.Root.Elements().First(e => e.Attribute("name").Value == propertyName).Elements().FirstOrDefault();
                    if (valueElement != null)
                        returnValue = valueElement.Value;
                }
            }

            return returnValue;
        }

        public PackagePart AddPart(Uri partUri)
        {
            PackagePart newPart = this.Package.CreatePart(PackUriHelper.CreatePartUri(partUri), System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.SuperFast);
            XVisioPackage.SavePart(newPart, CreateEmpty(XVisioUtils.VISIO_PROPERTIES));
            XVisioPart xPart = new XVisioPart(this, partUri, _document);
            _parts.Add(partUri.OriginalString, xPart);
            return newPart;
        }

        #region IDispose Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (!_isStencil && !_readOnly)
                    _visioPackage.Flush();
                _visioPackage.Close();
            }

            disposed = true;
        }

        ~XVisioPackage()
        {
            Dispose(false);
        }

        #endregion
    }
}