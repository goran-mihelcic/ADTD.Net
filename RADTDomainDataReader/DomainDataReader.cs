using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.CompilerServices;
using Microsoft.ADTD.Net;

namespace Microsoft.ADTD.Data.RADT
{
    public class DomainDataReader : IdxDataReader
    {
        public string fName { get; set; }
        private SelectData dataReader;
        public IdxLayout Layout { get; set; }

        public dxDiagram diagram { get; set; }
        //private Dictionary<string, string> fsmo = new Dictionary<string, string>();

        public DomainDataReader()
        {
            try
            {
                this.diagram = new dxDiagram();
                this.Layout = new Microsoft.ADTD.Layouts.Layout("DomainDiagram", new Dictionary<string, object>()
            {
                {"InitialThetha", Math.PI},
                {"InitialDirection", Math.PI/2}
            });
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

        public bool? read(Dictionary<string, object> parameters)
        {
            try
            {
                this.dataReader = new SelectData();
                this.dataReader.diagram = this.diagram;
                this.dataReader.layout = Layout;
                this.dataReader.fName = this.fName;
                this.dataReader.readServers = (bool)(parameters[ParameterNames.readServers]);
                bool? result = this.dataReader.getData();
                this.dataReader = null;
                this.diagram.resolvePageSize(new string[] { DiagramObjectNames.DomainObject, DiagramObjectNames.RealmObject });
                //this.diagram.resolvePageSize(DiagramObjectNames.RealmObject);
                //return result;
                return true;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        public Dictionary<string, string> getData(dxBase element, toolSelection sel)
        {
            try
            {
                const double toIn = 25.4;
                Dictionary<string, string> data = new Dictionary<string, string>();
                int textId = 1;

                data.Add("Name", element.Type + "-" + element.ID);

                switch (element.Type)
                {
                    case DiagramObjectNames.DomainObject:
                        data.Add("PinX", (((dxNode)element).X / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("PinY", (((dxNode)element).Y / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("Title", element.Name);
                        if ((bool)element.getAttribute(AttributeNames.InternalDomain))
                        {
                            addTextElement(ref textId, element.Name + " (" + element.getStringAttribute(AttributeNames.DomainFunctionalLevel) + " Domain Mode)", "Arial", 14, ref data);
                            addTextElement(ref textId, "\nTotal DC's: " + ((dxNode)element).Children.Count.ToString(), "Arial", 6, ref data);
                            if (element.Name == diagram.Nodes[DiagramObjectNames.ForestObject].Values.First().Name)
                            {
                                addTextElement(ref textId, "\nfsmoSchema: " +
                                    diagram.Nodes[DiagramObjectNames.ForestObject].Values.First().getStringAttribute(AttributeNames.SchemaMaster), "Arial", 6, ref data);
                                addTextElement(ref textId, "\nfsmoDomain: " +
                                    diagram.Nodes[DiagramObjectNames.ForestObject].Values.First().getStringAttribute(AttributeNames.DomainNamingMaster), "Arial", 6, ref data);
                            }
                            addTextElement(ref textId, "\nfsmoPDC: " + element.getStringAttribute(AttributeNames.PDC), "Arial", 6, ref data);
                            addTextElement(ref textId, "\nfsmoINFRA: " + element.getStringAttribute(AttributeNames.InfrastructureMaster), "Arial", 6, ref data);
                            addTextElement(ref textId, "\nfsmoRID: " + element.getStringAttribute(AttributeNames.RIDMaster), "Arial", 6, ref data);
                            //addTextElement(ref textId, "\n\nfsmoSchema Version: " + element.getStringAttribute(AttributeNames.SchemaVersion), "Arial", 6, ref data);
                        }
                        else addTextElement(ref textId, element.Name, "Arial", 14, ref data);
                        break;
                    case DiagramObjectNames.RealmObject:
                        data.Add("PinX", (((dxNode)element).X / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("PinY", (((dxNode)element).Y / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("Title", element.Name);
                        addTextElement(ref textId, element.Name, "Arial", 14, ref data);
                        break;
                    case DiagramObjectNames.Windows2000TrustObject:
                        addTextElement(ref textId, element.Name, "Arial", 10, ref data, true);
                        break;
                    case DiagramObjectNames.ServerObject:
                        data.Add("Parent", ((dxNode)element).Parent.Type + "-" + ((dxNode)element).Parent.ID);
                        addTextElement(ref textId, element.Name, "Arial", 10, ref data);
                        if (sel.ServerVersion)
                        {
                            //if ((bool)element.getAttribute(AttributeNames.ExtendedServerAttributes))
                            //    addTextElement(ref textId, "\n" + element.getStringAttribute(AttributeNames.OSVersion), "Arial", 8, ref data);
                            //else
                            //    addTextElement(ref textId, "\n" + element.getStringAttribute(AttributeNames.OS), "Arial", 8, ref data);
                            addTextElement(ref textId, "\n" + element.getStringAttribute(AttributeNames.OSShort), "Arial", 8, ref data);
                        }
                        break;
                    case DiagramObjectNames.DownlevelTrustObject:
                        break;
                }

                return data;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        public string getPopupText(dxBase element, toolSelection sel)
        {
            try
            {
                string result = element.Name;

                switch (element.Type)
                {
                    case DiagramObjectNames.DomainObject:
                        if ((bool)element.getAttribute(AttributeNames.InternalDomain))
                        {
                            result = result + "\nDCs in Domain: " + ((dxNode)element).Children.Count.ToString() +
                                (element.Name == diagram.Nodes[DiagramObjectNames.ForestObject].Values.First().Name ?
                                "\nfsmoSchema:\t" + diagram.Nodes[DiagramObjectNames.ForestObject].Values.First().getStringAttribute(AttributeNames.SchemaMaster) +
                                "\nfsmoDomain:\t" + diagram.Nodes[DiagramObjectNames.ForestObject].Values.First().getStringAttribute(AttributeNames.DomainNamingMaster) :
                                "") +
                                "\nfsmoPDC:\t" + element.getStringAttribute(AttributeNames.PDC) +
                                "\nfsmoINFRA:\t" + element.getStringAttribute(AttributeNames.InfrastructureMaster) +
                                "\nfsmoRID:\t" + element.getStringAttribute(AttributeNames.RIDMaster);
                        }
                        break;
                    case DiagramObjectNames.RealmObject:
                        break;
                    case DiagramObjectNames.ServerObject:
                        result = sel.DrawFQDN ? element.getStringAttribute(AttributeNames.ServerFQDN) : element.Name;
                        result = result + ((bool)element.getAttribute(AttributeNames.GlobalCatalog) ? "\nGlobal Catalog" : "") +
                            ((bool)element.getAttribute(AttributeNames.RODC) ? "\nRODC" : "") +
                            "\nMachine Type: " + element.getAttribute(AttributeNames.MachineType) +
                            "\n" + element.getAttribute(AttributeNames.OS);
                        break;
                    case DiagramObjectNames.Windows2000TrustObject:
                        result = result + "\nTrust type: " + element.getAttribute(AttributeNames.TrustType).ToString() +
                            "\nDirection: " + element.getAttribute(AttributeNames.TrustDirection).ToString() +
                            "\nAttributes: " + element.getAttribute(AttributeNames.TrustAttributes);
                        break;
                    case DiagramObjectNames.DownlevelTrustObject:
                        result = result + "\nTrust type: " + element.getAttribute(AttributeNames.TrustType).ToString() +
                            "\nDirection: " + element.getAttribute(AttributeNames.TrustDirection).ToString() +
                            "\nAttributes: " + element.getAttribute(AttributeNames.TrustAttributes);
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
                return null;
            }
        }

        private void addTextElement(ref int id, string text, string face, int size, ref Dictionary<string, string> data, bool noPara = false)
        {
            try
            {
                data.Add("Text_" + id.ToString(), text);
                data.Add("FaceName_" + id.ToString(), face);
                data.Add("Size_" + id.ToString(), size.ToString());
                if (noPara) data.Add("NoPara_" + id.ToString(), "true");
                id += 1;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }

    }


}

