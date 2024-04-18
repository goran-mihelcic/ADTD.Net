using Mihelcic.Net.Visio.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Mihelcic.Net.Visio.Data
{
    public class dxData : IData
    {
        #region Private Fields

        private readonly Dictionary<string, dxShape> _allShapes;
        private readonly List<dxShape> _shapes;
        private readonly List<dxConnection> _connections;

        #endregion

        #region Public Properties

        public IList<dxConnection> Connections { get { return _connections; } }
        public List<dxShape> Shapes { get { return _shapes; } }
        public IDictionary<string, dxShape> AllShapes { get { return _allShapes; } }

        #endregion

        #region Constructors

        public dxData()
        {
            _connections = new List<dxConnection>();
            _shapes = new List<dxShape>();
            _allShapes = new Dictionary<string, dxShape>();
        }
        public dxData(string fileName) : this()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            foreach (XmlNode node in doc.SelectNodes("root/Shapes/*"))
            {
                dxShape shape = new dxShape(node);
                string parent = GetParentFromXml(node);
                if (!String.IsNullOrWhiteSpace(parent))
                    this.AddShape(shape, parent);
                else
                    this.AddShape(shape);
            }

            foreach (XmlNode conn in doc.SelectNodes("root/Connections/*"))
            {

            }
        }

        #endregion

        #region Public Methods

        public void Clear()
        {
            _shapes.Clear();
            _allShapes.Clear();
            _connections.Clear();
        }

        public void AddShape(dxShape shape, dxShape parent = null)
        {
            string shapeId = shape.Id.ToLowerInvariant();
            if (!_allShapes.ContainsKey(shapeId))
            {
                _allShapes.Add(shapeId, shape);
                if (parent == null)
                {
                    _shapes.Add(shape);
                }
                else
                {
                    shape.Parent = parent;
                }
            }
            else
                Logger.TraceDebug("Duplicate Shape Id: {0}", shape.Id);
        }

        public void AddShape(dxShape shape, string parentId)
        {
            parentId = parentId.ToLowerInvariant();
            dxShape parent = null;
            if (_allShapes.ContainsKey(parentId))
                parent = _allShapes[parentId];
            else
                Logger.TraceDebug("Parent {0} for shape {1} Not Found.", parentId, shape.Id);
            AddShape(shape, parent);
        }

        public void AddConnection(dxConnection shape)
        {
            _connections.Add(shape);
        }

        public dxShape GetNode(string id)
        {
            id = id.ToLowerInvariant();
            if (_allShapes.ContainsKey(id))
                return _allShapes[id];
            else
                return null;
        }

        public dxShape GetNode(string prefix, string name)
        {
            string id = String.Format("{0}-{1}", prefix, name).ToLowerInvariant();
            if (_allShapes.ContainsKey(id))
                return _allShapes[id];
            else
                return null;
        }

        public dxShape GetNode(string[] prefixes, string name)
        {
            foreach (string prefix in prefixes)
            {
                dxShape result = GetNode(prefix, name);
                if (result != null) return result;
            }
            return null;
        }


        public IEnumerable<dxObject> GetNodes(string prefix)
        {
            return _allShapes.Where(s => s.Value.Type == prefix).Select(s => s.Value);
        }

        public void SaveXml(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("root");
            XmlNode shapes = doc.CreateElement("Shapes");
            XmlNode connections = doc.CreateElement("Connections");

            foreach (dxShape node in _allShapes.Values)
            {
                shapes.AppendChild(node.Xml(doc));
            }
            root.AppendChild(shapes);

            foreach (dxConnection conn in _connections)
                connections.AppendChild(conn.Xml(doc));
            root.AppendChild(connections);
            doc.AppendChild(root);

            doc.Save(fileName);
        }

        public void SaveCsv(string fileName)
        {
            IEnumerable<string> types = this.AllShapes.Select(s => s.Value.Type).Distinct();
            foreach (string type in types)
            {
                CsvData typeData = new CsvData(type, this);
                typeData.Save(fileName);
            }
        }

        #endregion

        #region PrivateMethods

        private string GetParentFromXml(XmlNode xNode)
        {
            XmlElement element = xNode as XmlElement;
            if (element.HasAttribute("Parent"))
                return element.Attributes["Parent"].Value;
            else return null;
        }

        #endregion
    }
}
