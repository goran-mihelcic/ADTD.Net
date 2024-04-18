using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Represents Masters Part
    /// </summary>
    public class XVisioMasters : XVisioPart
    {
        HashSet<int> _masterIds = new HashSet<int>();

        /// <summary>
        /// Next Master Id
        /// </summary>
        public string NextId
        {
            get
            {
                return (_masterIds.Max() + 1).ToString();
            }
        }

        /// <summary>
        /// IEnumerable of Master Names
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                return Parts.Values.Select(p => ((XVisioMaster)p).Name);
            }
        }

        /// <summary>
        /// Constructs new Masters part
        /// </summary>
        /// <param name="package">Visio package</param>
        /// <param name="uri">Visio package uri</param>
        /// <param name="parent">Visio Package Parent package</param>
        public XVisioMasters(XVisioPackage package, Uri uri, XVisioPart parent)
            : base(package, uri, parent)
        {

        }

        internal override void ExpandPackagePart()
        {
            if (this.Xml != null)
            {
                IEnumerable<XElement> masters = from element in Xml.Root.Descendants()
                                                where element.Name.LocalName == XVisioUtils.VISIO_MASTER
                                                select element;
                foreach (XElement master in masters)
                {
                    string rel = master.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_REL))
                        .Attribute(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_id_ATTRIBUTE, XVisioNamespace.Relationships)).Value;
                    PackageRelationship relationship = this.Part.GetRelationship(rel);
                    Uri target = relationship.TargetUri;
                    if (!target.OriginalString.StartsWith("/"))
                    {
                        string baseUri = relationship.SourceUri.OriginalString.Substring(0, relationship.SourceUri.OriginalString.LastIndexOf("/"));
                        target = new Uri(String.Format("{0}/{1}", baseUri, relationship.TargetUri.OriginalString), UriKind.Relative);
                    }
                    PackagePart part = this.Part.Package.GetPart(target);
                    string id = XVisioUtils.GetAttributeValue(master, XVisioUtils.VISIO_ID_ATTRIBUTE);
                    Parts.Add(target, new XVisioMaster(VisioPackage, target, this, master));
                    int idInt;
                    if (int.TryParse(id, out idInt))
                        _masterIds.Add(idInt);
                }
            }
        }

        /// <summary>
        /// Find Master object by Id
        /// </summary>
        /// <param name="id">Master Id</param>
        /// <returns>Master Object</returns>
        public XVisioMaster this[string id]
        {
            get
            {
                return (XVisioMaster)Parts.Values.Where(p => p.GetType() == typeof(XVisioMaster)).FirstOrDefault(p => ((XVisioMaster)p).Id == id);
            }
        }

        /// <summary>
        /// Find Master object by Name
        /// </summary>
        /// <param name="name">Master Name</param>
        /// <returns>Master Object</returns>
        public XVisioMaster ByName(string name)
        {
            return (XVisioMaster)Parts.Values.Where(p => p.GetType() == typeof(XVisioMaster)).FirstOrDefault(p => ((XVisioMaster)p).Name == name);
        }

        /// <summary>
        /// Find Master object by Universal Name
        /// </summary>
        /// <param name="name">Master Universal Name</param>
        /// <returns>Master Object</returns>
        public XVisioMaster ByNameU(string name)
        {
            return (XVisioMaster)Parts.Values.Where(p => p.GetType() == typeof(XVisioMaster)).FirstOrDefault(p => ((XVisioMaster)p).NameU == name);
        }

        /// <summary>
        /// Add Master to Masters
        /// </summary>
        /// <param name="stencilMaster">Master from Stencil</param>
        public void AddMaster(XVisioMaster stencilMaster)
        {
            if (stencilMaster == null)
                throw new ArgumentNullException("stencilMaster", "Stencil Master shouldn't be null.");

            string relId = stencilMaster.XHeader.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_REL))
                .Attribute(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_id_ATTRIBUTE, XVisioNamespace.Relationships)).Value;
            Uri uri = stencilMaster.PartUri;

            XElement xBase = this.Xml.Element(XVisioUtils.AddNameSpace(XVisioUtils.VISIO_MASTERS));
            XElement xRef = new XElement(stencilMaster.XHeader);
            xBase.Add(xRef);
            XVisioMaster newMaster = new XVisioMaster(stencilMaster, this.VisioPackage);
            this.Part.CreateRelationship(newMaster.PartUri, TargetMode.Internal, XVisioUtils.VISIO_MASTER_URI, relId);
            Parts.Add(uri, newMaster);

            if (stencilMaster.Part.GetRelationships().Any())
            {
                foreach (PackageRelationship relation in stencilMaster.Part.GetRelationships())
                {
                    PackagePart relatedPart = stencilMaster.VisioPackage.Package.GetPart(
                        PackUriHelper.ResolvePartUri(stencilMaster.Part.Uri, relation.TargetUri));
                    Uri newPartUri = PackUriHelper.ResolvePartUri(newMaster.Part.Uri, relatedPart.Uri);
                    PackagePart newRelatedPart = null;
                    if (!this.VisioPackage.Package.PartExists(newPartUri))
                    {
                        newRelatedPart = this.VisioPackage.Package.CreatePart(newPartUri, relatedPart.ContentType, relatedPart.CompressionOption);

                        using (Stream streamReader = relatedPart.GetStream())
                        using (Stream streamWriter = newRelatedPart.GetStream(FileMode.Create, FileAccess.Write))
                        {
                            byte[] contents = new byte[streamReader.Length];
                            streamReader.Read(contents, 0, contents.Length);
                            streamWriter.Write(contents, 0, contents.Length);
                        }
                    }
                    else
                        newRelatedPart = this.VisioPackage.Package.GetPart(newPartUri);

                    newMaster.Part.CreateRelationship(newRelatedPart.Uri, TargetMode.Internal, relation.RelationshipType, relation.Id);

                }
            }
        }
    }
}
