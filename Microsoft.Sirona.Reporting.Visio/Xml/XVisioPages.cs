using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Represents Visio Pages Part
    /// </summary>
    public class XVisioPages : XVisioPart
    {
        /// <summary>
        /// Construct Visio Pages Object
        /// </summary>
        /// <param name="package">Visio Package</param>
        /// <param name="uri">Part uri</param>
        /// <param name="parent">Parent Part</param>
        public XVisioPages(XVisioPackage package, Uri uri, XVisioPart parent)
            : base(package, uri, parent)
        {
        }

        internal override void ExpandPackagePart()
        {
            if (this.Xml != null)
            {
                IEnumerable<XElement> pages = from element in Xml.Root.Descendants()
                                              where element.Name.LocalName == XVisioUtils.VISIO_PAGE
                                              select element;
                foreach (XElement page in pages)
                {
                    List<XVisioLayer> layers = new List<XVisioLayer>();
                    Uri target = null;

                    if (page.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_REL)) != null)
                    {
                        string rel = page.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_REL))
                            .Attribute(XVisioUtils.AddNameSpace("id", XVisioNamespace.Relationships)).Value;
                        PackageRelationship relationship = this.Part.GetRelationship(rel);
                        target = relationship.TargetUri;
                        if (!target.OriginalString.StartsWith("/"))
                        {
                            string baseUri = relationship.SourceUri.OriginalString.Substring(0, relationship.SourceUri.OriginalString.LastIndexOf("/"));
                            target = new Uri(String.Format("{0}/{1}", baseUri, relationship.TargetUri.OriginalString), UriKind.Relative);
                        }
                        PackagePart part = this.Part.Package.GetPart(target);

                    }
                    else
                        target = new Uri(XVisioUtils.GetAttributeValue(page, XVisioUtils.VISIO_NAMEU_ATTRIBUTE), UriKind.Relative);

                    Parts.Add(target, new XVisioPage(VisioPackage, target, this, page));
                }
            }
        }

        /// <summary>
        /// Page by Id
        /// </summary>
        /// <param name="id">Page Id</param>
        /// <returns>Page Object</returns>
        public XVisioPage this[string id]
        {
            get
            {
                return (XVisioPage)Parts.Values.Where(p => p.GetType() == typeof(XVisioPage)).FirstOrDefault(p => ((XVisioPage)p).Id == id);
            }
        }

        /// <summary>
        /// Page By Name
        /// </summary>
        /// <param name="name">Page Name</param>
        /// <returns>Page Object</returns>
        public XVisioPage ByName(string name)
        {
            return (XVisioPage)Parts.Values.Where(p => p.GetType() == typeof(XVisioPage)).FirstOrDefault(p => ((XVisioPage)p).Name == name);
        }

        /// <summary>
        /// Page By Universal Name
        /// </summary>
        /// <param name="name">Universal Page Name</param>
        /// <returns>Page Object</returns>
        public XVisioPage ByNameU(string name)
        {
            return (XVisioPage)Parts.Values.Where(p => p.GetType() == typeof(XVisioPage)).FirstOrDefault(p => ((XVisioPage)p).NameU == name);
        }
    }
}
