using Mihelcic.Net.Visio.Data;
using Mihelcic.Net.Visio.Arrange;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Mihelcic.Net.Visio.Common
{
    public class WorkerParameters
    {
        #region Private Fields

        private Dictionary<string, object> _data;
        private string _name;
        private string _dll;
        private LayoutType _layout;
        private IDataReader _reader;

        #endregion

        #region Public Properties

        public IDataReader Reader { get { return _reader; } }
        public object this[string key]
        {
            get
            {
                return _data[key];
            }
            set
            {
                _data[key] = value;
            }
        }
        public string Name { get { return _name; } }
        public string Dll { get { return _dll; } }
        public LayoutType Layout { get { return _layout; } }
        public Dictionary<string, object> Parameters { get { return _data; } }

        #endregion

        #region Constructors

        public WorkerParameters(XmlElement element)
        {
            _data = new Dictionary<string, object>();
            _name = element.Attributes["Name"].Value;
            _dll = element.Attributes["Dll"].Value;
            _layout = (LayoutType)Enum.Parse(typeof(LayoutType), element.Attributes["Layout"].Value);

            string readerDll = Path.Combine(Environment.CurrentDirectory, _dll);
            Assembly asm = Assembly.LoadFile(readerDll);
            Type[] types = asm.GetExportedTypes();
            Type type = types.FirstOrDefault(n => n.Name.ToLowerInvariant() == _name.ToLowerInvariant());

            if (type != null && type.GetInterfaces().Contains(typeof(Mihelcic.Net.Visio.Data.IDataReader)))
            {
                _reader = Activator.CreateInstance(type) as IDataReader;
            }
        }

        public WorkerParameters(string name, IDataReader reader, LayoutType layout)
        {
            _name = name;
            _reader = reader;
            _layout = layout;
            _data = new Dictionary<string, object>();
        }

        #endregion

        #region Public Properties

        public void Add(string key, object value)
        {
            if (!String.IsNullOrWhiteSpace(key))
                _data.Add(key, value);
        }

        public void AddAll(Dictionary<string, object> parameters)
        {
            if (parameters != null)
                foreach (string key in parameters.Keys)
                    _data.Add(key, parameters[key]);
        }

        #endregion
    }
}
