using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Generic Visio Part
    /// </summary>
    public class XVisioPart
    {
        private PackagePart _part;
        private XDocument _xml;
        private XVisioPart _parent;

        private XVisioPackage _visioPackage;
        private Uri _uri;
        private string _container;
        private Dictionary<Uri, XVisioPart> _parts;
        private List<PackageRelationship> _crossRelations;

        /// <summary>
        /// PackagePart
        /// </summary>
        public PackagePart Part { get { return _part; } internal set { _part = value; } }

        /// <summary>
        /// Xml Document representing Part
        /// </summary>
        public XDocument Xml { get { return _xml; } }

        /// <summary>
        /// Part uri
        /// </summary>
        public Uri PartUri { get { return _uri; } }

        /// <summary>
        /// Visio Document where Part belongs
        /// </summary>
        public XVisioPackage VisioPackage { get { return _visioPackage; } }

        /// <summary>
        /// Package Part subparts
        /// </summary>
        public Dictionary<Uri, XVisioPart> Parts { get { return _parts; } }

        /// <summary>
        /// Package Part parent
        /// </summary>
        public XVisioPart Parent { get { return _parent; } }

        /// <summary>
        /// Part non-hierarchical relationships
        /// </summary>
        public List<PackageRelationship> CrossRelations { get { return _crossRelations; } }

        /// <summary>
        /// Does Part have PackagePart
        /// </summary>
        public bool HasPart { get { return _part != null; } }

        /// <summary>
        /// Constructs VisioPart
        /// </summary>
        /// <param name="package">Visio Document where Part belongs</param>
        /// <param name="uri">Part uri</param>
        /// <param name="parent">Package Part parent</param>
        public XVisioPart(XVisioPackage package, Uri uri, XVisioPart parent)
            : this()
        {
            if (package == null)
                throw new ArgumentNullException("package", "Package shouldn't be null.");

            if (uri == null || String.IsNullOrWhiteSpace(uri.OriginalString))
                throw new ArgumentNullException("uri", "You must supply valid package uri.");

            _visioPackage = package;
            _uri = uri;
            _parent = parent;
            bool partFound = GetPackagePart();
            if (partFound)
            {
                _container = _uri.OriginalString.Substring(0, _uri.OriginalString.LastIndexOf('/'));
                if (_container != XVisioUtils.VISIO_DOC_PROPERTIES)
                {
                    _xml = this.GetXMLFromPart();
                    ExpandPackagePart();
                }
                if (_xml == null && this.Part.ContentType.EndsWith("+xml"))
                    _xml = this.GetXMLFromPart();
            }
        }

        /// <summary>
        /// Constructs VisioPart from Stencil Part
        /// </summary>
        /// <param name="stencilPart">Stencil VisioPart</param>
        public XVisioPart(XVisioPart stencilPart)
            : this()
        {
            if (stencilPart == null)
                throw new ArgumentNullException("stencilPart", "Stencil Part shouldn't be null.");

            _parent = stencilPart.Parent;
            _visioPackage = stencilPart.VisioPackage;
            _uri = stencilPart.PartUri;
            _xml = stencilPart.Xml;
        }

        private XVisioPart()
        {
            _parts = new Dictionary<Uri, XVisioPart>();
            _crossRelations = new List<PackageRelationship>();
        }

        /// <summary>
        /// Extracts Package Part from Package
        /// </summary>
        private bool GetPackagePart()
        {
            if (_uri.OriginalString.StartsWith("/"))
                if (_visioPackage.Package.PartExists(_uri))
                    _part = _visioPackage.Package.GetPart(_uri);
            return _part != null;
        }

        private PackagePart FindPackagePart(Uri uri)
        {
            if (_visioPackage.Package.PartExists(uri))
                return _visioPackage.Package.GetPart(_uri);
            return null;
        }

        internal virtual void ExpandPackagePart()
        {
            //var rels = _part.GetRelationships().Where(r => r.Id.StartsWith("rId"));
            foreach (PackageRelationship rel in _part.GetRelationships())
            {
                Uri absUri = PackUriHelper.ResolvePartUri(rel.SourceUri, rel.TargetUri);
                if (IsDownlevelUri(rel.TargetUri) || rel.TargetUri.OriginalString.StartsWith(_container))
                {
                    switch (rel.RelationshipType)
                    {
                        case XVisioUtils.VISIO_MASTERS_URI:
                            {
                                XVisioMasters masters = new XVisioMasters(_visioPackage, absUri, this);
                                _visioPackage.RegisterMasters(masters);
                                _parts.Add(absUri, masters);
                            }
                            break;
                        case XVisioUtils.VISIO_PAGES_URI:
                            XVisioPages pages = new XVisioPages(_visioPackage, absUri, this);
                            _visioPackage.RegisterPages(pages);
                            _parts.Add(absUri, pages);
                            break;
                        case XVisioUtils.VISIO_DOCUMENT_URI:
                            XVisioPart document = new XVisioPart(_visioPackage, absUri, this);
                            _parts.Add(absUri, document);
                            _visioPackage.RegisterDocument(document);
                            break;
                        default:
                            _parts.Add(absUri, new XVisioPart(_visioPackage, absUri, this));
                            break;

                    }
                }
                else if (!rel.TargetUri.OriginalString.StartsWith(_container))
                    _crossRelations.Add(rel);
            }
        }

        private bool IsDownlevelUri(Uri uri)
        {
            return !((uri.OriginalString.StartsWith("/") || uri.OriginalString.StartsWith("..")));
        }

        /// <summary>
        /// Save Xml to PackagePart
        /// </summary>
        public void Save()
        {
            if (_xml != null)
            {
                // Create a new XmlWriterSettings object to 
                // define the characteristics for the XmlWriter
                XmlWriterSettings partWriterSettings = new XmlWriterSettings();
                partWriterSettings.Encoding = Encoding.UTF8;
                partWriterSettings.NewLineChars = "\n";
                partWriterSettings.CloseOutput = true;

                if (!this.VisioPackage.IsStencil)
                {
                    using (Stream partStream = _part.GetStream(FileMode.Create, FileAccess.ReadWrite))
                    {
                        // Create a new XmlWriter and then write the XML
                        // back to the document part.
                        StreamWriter streamWriter = new StreamWriter(partStream);
                        XmlWriter partWriter = XmlWriter.Create(streamWriter, partWriterSettings);

                        _xml.Save(partWriter);

                        // Flush and close the XmlWriter.
                        partWriter.Flush();
                        partWriter.Close();
                    }
                }
                foreach (XVisioPart part in _parts.Values)
                    part.Save();
            }
        }

        /// <summary>
        /// Open the packagePart and return it as XDocument
        /// </summary>
        /// <returns>XDocument from PackagePart</returns>
        public XDocument GetXMLFromPart()
        {
            XDocument partXml = null;

            // Open the packagePart as a stream and then 
            // open the stream in an XDocument object.
            using (Stream partStream = _part.GetStream())
            {
                partXml = XDocument.Load(partStream);
            }

            return partXml;
        }
    }
}
