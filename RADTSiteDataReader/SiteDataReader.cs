namespace Microsoft.ADTD.Data.RADT
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Runtime.CompilerServices;
    using Microsoft.ADTD.Net;

    public class SiteDataReader : IdxDataReader
    {
        public string fName { get; set; }
        private SelectData dataReader;
        public IdxLayout Layout { get; set; }

        public dxDiagram diagram { get; set; }

        public SiteDataReader()
        {
            try
            {
                this.diagram = new dxDiagram();
                this.Layout = new Microsoft.ADTD.Layouts.Layout("SiteDiagram", new Dictionary<string, object>()
            {
                {"InitialThetha", 2 * Math.PI},
                {"InitialDirection", Math.PI}
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
                this.dataReader.agregateSites = (bool)(parameters[ParameterNames.agregateSites]);
                this.dataReader.expandSL = (bool)(parameters[ParameterNames.expandSiteLinks]);
                bool? result = this.dataReader.getData();
                this.dataReader = null;
                this.diagram.resolvePageSize(new string[] { DiagramObjectNames.SiteObject });
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
                    case DiagramObjectNames.SiteObject:
                        data.Add("PinX", (((dxNode)element).X / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("PinY", (((dxNode)element).Y / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("Title", element.Name);

                        if (element.Attributes.ContainsKey(AttributeNames.AggregateCount))
                        {
                            addTextElement(ref textId, "\n", "Arial", 14, ref data);
                            addTextElement(ref textId, ((dxNode)element).getStringAttribute(AttributeNames.AggregateCount), "Arial", 40, ref data, false, true);
                            addTextElement(ref textId, "\nAggregated Sites", "Arial", 14, ref data);
                        }
                        else
                        {
                            addTextElement(ref textId, element.Name, "Arial", 14, ref data);
                            addTextElement(ref textId, "\nTotal Domain Controllers in Site: " + ((dxNode)element).Children.Count.ToString(), "Arial", 10, ref data);
                        }
                        break;
                    case DiagramObjectNames.ServerObject:
                        data.Add("Parent", ((dxNode)element).Parent.Type + "-" + ((dxNode)element).Parent.ID);
                        addTextElement(ref textId, element.Name, "Arial", 10, ref data);
                        if (sel.DrawFQDN)
                            addTextElement(ref textId, "\n" + element.getStringAttribute(AttributeNames.DomainFQDN), "Arial", 10, ref data);
                        if ((bool)element.getAttribute(AttributeNames.GlobalCatalog))
                            addTextElement(ref textId, "\nGlobal Catalog Server", "Arial", 8, ref data);
                        if ((bool)element.getAttribute(AttributeNames.RODC))
                            addTextElement(ref textId, "\nRead Only DC", "Arial", 8, ref data);
                        if (sel.ServerVersion)
                            addTextElement(ref textId, "\n" + element.getStringAttribute(AttributeNames.OSShort), "Arial", 8, ref data);
                        break;
                    case DiagramObjectNames.ConnectionPointObject:
                        data.Add("PinX", (((dxNode)element).X / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("PinY", (((dxNode)element).Y / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("Width", (((dxNode)element).Width / toIn).ToString("F5", new CultureInfo("en-US")));
                        data.Add("Height", (((dxNode)element).Height / toIn).ToString("F5", new CultureInfo("en-US")));
                        break;
                    case DiagramObjectNames.SiteLinkObject:
                        addTextElement(ref textId, element.Name, "Arial", 10, ref data);
                        addTextElement(ref textId, "\n(Type: IP-Link, Cost: " +
                            element.getAttribute(AttributeNames.Cost) + ", Interval: " +
                            element.getAttribute(AttributeNames.ReplicationInterval) + ")", "Arial", 8, ref data, true);
                        break;
                    case DiagramObjectNames.ConnectionObject:
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
                    case DiagramObjectNames.SiteObject:
                        if (element.Attributes.ContainsKey(AttributeNames.AggregateCount))
                            result = result + "\nSites: " + ((dxNode)element).getStringAttribute(AttributeNames.AggregateCount);
                        else
                            result = result + "\nDCs in Site: " + ((dxNode)element).Children.Count.ToString() +
                            "\nISTG: " + element.getAttribute(AttributeNames.ISTG);
                        break;
                    case DiagramObjectNames.ServerObject:
                        element.getStringAttribute(AttributeNames.ServerFQDN);
                        result = result + ((bool)element.getAttribute(AttributeNames.GlobalCatalog) ? "\nGlobal Catalog" : "") +
                            ((bool)element.getAttribute(AttributeNames.RODC) ? "\nRODC" : "") +
                            "\nMachine Type: " + element.getAttribute(AttributeNames.MachineType) +
                            "\n" + element.getAttribute(AttributeNames.OS);
                        break;
                    case DiagramObjectNames.ConnectionPointObject:
                        break;
                    case DiagramObjectNames.SiteLinkObject:
                        result = result + "\nCost: " + element.getAttribute(AttributeNames.Cost).ToString() +
                            "\nInterval: " + element.getAttribute(AttributeNames.ReplicationInterval).ToString() +
                            "\nNotification: " + element.getAttribute(AttributeNames.InterSiteChangeNotification) +
                            "\nSchedule: " + element.getAttribute(AttributeNames.SLSchedule);
                        break;
                    case DiagramObjectNames.ConnectionObject:
                        result = result + "\n" + ((dxEdge)element).FromNode.Name + "===>" + ((dxEdge)element).ToNode.Name +
                            "\nTransport: " + element.getAttribute(AttributeNames.TransportType) +
                            "\nType: " +
                            "\nSchedule: " + element.getAttribute(AttributeNames.Schedule);
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

        private void addTextElement(ref int id, string text, string face, int size, ref Dictionary<string, string> data, bool noPara = false, bool center = false)
        {
            try
            {
                data.Add("Text_" + id.ToString(), text);
                data.Add("FaceName_" + id.ToString(), face);
                data.Add("Size_" + id.ToString(), size.ToString());
                if (noPara) data.Add("NoPara_" + id.ToString(), "true");
                if (center) data.Add("Center_" + id.ToString(), "true");
                id += 1;
            }
            catch (Exception ex)
            {
                Logger.logDebug(ex.ToString());
            }
        }
    }
}
