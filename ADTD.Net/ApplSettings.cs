using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Xml;

namespace Mihelcic.Net.Visio.Diagrammer
{
    public class ApplSettings : ApplicationSettingsBase
    {
        private static readonly ApplSettings _defaultInstance = (
        (ApplSettings)
        (ApplicationSettingsBase.Synchronized(new ApplSettings())));

        #region Public Properties

        public static ApplSettings Default
        {
            get { return _defaultInstance; }
        }

        [UserScopedSettingAttribute()]
        [DebuggerNonUserCodeAttribute()]
        [DefaultSettingValueAttribute("Resources\\Template.vtx")]
        public String TemplateFile
        {
            get { return this["TemplateFile"].ToString(); }
            set { this["TemplateFile"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DebuggerNonUserCodeAttribute()]
        [DefaultSettingValueAttribute("Resources\\Stencil.vsx")]
        public String StencilFile
        {
            get { return this["StencilFile"].ToString(); }
            set { this["StencilFile"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DebuggerNonUserCodeAttribute()]
        //[SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public Hashtable Readers
        {
            get
            {
                return Xml2HashTable(this["Readers"].ToString());
            }
            set { this["Readers"] = HashTable2Xml(value); }
        }

        #endregion

        #region Private Methods

        private Hashtable Xml2HashTable(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            Hashtable table = new Hashtable();
            foreach (XmlElement element in doc.DocumentElement.SelectNodes("//Entry"))
            {
                if (element.HasAttribute("Key"))
                {
                    string value = String.Empty;
                    string key = element.Attributes["Key"].Value;
                    if (element.HasAttribute("Value"))
                        value = element.Attributes["Value"].Value;
                    table.Add(key, value);
                }
            }
            return table;
        }

        private string HashTable2Xml(Hashtable table)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Root");
            foreach (string key in table.Keys)
            {
                XmlElement entry = doc.CreateElement("Entry");
                XmlAttribute keyAttr = doc.CreateAttribute("Key");
                keyAttr.Value = key;
                entry.AppendChild(keyAttr);
                XmlAttribute valueAttr = doc.CreateAttribute("Value");
                valueAttr.Value = table[key].ToString();
                entry.AppendChild(valueAttr);
                root.AppendChild(entry);
            }
            doc.AppendChild(root);
            return doc.OuterXml;
        }

        #endregion
    }
}
