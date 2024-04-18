using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace Mihelcic.Net.Visio.Data
{
    public class CsvData
    {
        #region Private Fields

        private readonly string _type;
        private readonly string _header;
        private readonly StringCollection _data;

        #endregion

        #region Public Properties

        public string Header { get { return _header; } }
        public StringCollection Data { get { return _data; } }

        #endregion

        private CsvData()
        {
            _header = "Name\tType";
            _data = new StringCollection();
        }

        public CsvData(string type, IData myData) : this()
        {
            _type = type;
            IEnumerable<dxShape> typeShapes = myData.AllShapes.Where(s => s.Value.Type.ToLowerInvariant() == type.ToLowerInvariant()).Select(n => n.Value);
            if (typeShapes.Count() > 0)
            {
                dxShape hShape = typeShapes.First();
                foreach (string prop in hShape.Attributes.Keys)
                    _header = String.Format("{0}\t{1}", _header, prop);

                foreach (dxShape shape in typeShapes)
                {
                    string line = String.Format("{0}\t{1}", shape.Name, shape.Type);
                    foreach (object value in shape.Attributes.Values)
                        line = String.Format("{0}\t{1}", line, value);
                    _data.Add(line);
                }
            }
        }

        #region Public Methods

        public void Save(string fileName)
        {
            string folder = Path.GetDirectoryName(fileName);
            string file = Path.GetFileName(fileName);
            string extension = Path.GetExtension(fileName);
            string baseName = file.Substring(0, file.Length - extension.Length);
            fileName = Path.Combine(folder, String.Concat(baseName, ".", _type, extension));
            StreamWriter stream = File.CreateText(fileName);
            stream.WriteLine(_header);
            foreach (string line in _data)
                stream.WriteLine(line);
            stream.Close();
        }

        #endregion
    }
}
