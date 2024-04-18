using Microsoft.ADTD.Visio.Arrange;
using System;
using System.Collections.Generic;


namespace Microsoft.ADTD.Data
{
    public class TestDataReader:IDataReader
    {
        private dxData _data;

        public IData Data { get { return _data; } }
        public bool Connected { get { return true; } }

        public TestDataReader()
        {
            _data = new dxData();
        }

        public bool? Read(Dictionary<string, object> parameters)
        {
            dxShape site1 = GetShape("Site", "Site One");
            _data.AddShape(site1);
            dxShape dc1 = GetShape("DC", "Domain Controller One");
            _data.AddShape(dc1, site1);
            dxShape gc = GetShape("GC", "Global Catalog One");
            _data.AddShape(gc, site1);
            dxShape rodc = GetShape("RODC", "RODC One");
            _data.AddShape(rodc, site1);

            dxShape site2 = GetShape("Site", "Site Two");
            _data.AddShape(site2);
            dxShape dc2 = GetShape("DC", "Domain Controller Two");
            _data.AddShape(dc2, site2);
            dxShape gc2 = GetShape("GC", "Global Catalog Two");
            _data.AddShape(gc2, site2);
            dxShape rodc2 = GetShape("RODC", "RODC Two");
            _data.AddShape(rodc2, site2);
            dxShape dc3 = GetShape("DC", "Domain Controller Three");
            _data.AddShape(dc3, site2);
            dxShape dc4 = GetShape("DC", "Domain Controller Four");
            _data.AddShape(dc4, site2);
            dxShape dc5 = GetShape("DC", "Domain Controller Five");
            _data.AddShape(dc5, site2);

            _data.AddConnection(GetConnection("SL", "Site Link One", site1, site2));

            _data.AddConnection(GetConnection("CO-INTRA", Guid.NewGuid().ToString(), dc1, gc));
            _data.AddConnection(GetConnection("CO-INTRA", Guid.NewGuid().ToString(), dc1, rodc));
            _data.AddConnection(GetConnection("CO-INTRA", Guid.NewGuid().ToString(), rodc, gc));

            _data.AddConnection(GetConnection("CO-INTRA", Guid.NewGuid().ToString(), dc2, gc2));
            _data.AddConnection(GetConnection("CO-INTRA", Guid.NewGuid().ToString(), dc2, rodc2));
            _data.AddConnection(GetConnection("CO-INTRA", Guid.NewGuid().ToString(), rodc2, gc2));

            _data.AddConnection(GetConnection("CO-INTER", Guid.NewGuid().ToString(), gc, gc2));
            _data.AddConnection(GetConnection("CO-INTER", Guid.NewGuid().ToString(), gc2, gc));

            return true;
        }

        public bool Connect(Dictionary<string, object> parameters)
        {
            return true;
        }

        private dxShape GetShape(string prefix, string name)
        {
            dxShape shape = null;
            dxLayoutParameters parameters = new dxLayoutParameters();
            switch(prefix)
            {
                case "Site":
                    shape = new dxShape("Site", name, "Site", LayoutType.Matrix);
                    parameters = new SiteLayoutParameters();
                    shape.LayoutParams = parameters.GetLayoutParameters();
                    shape.Text = String.Format("{0} Text", name);
                    shape.Comment = String.Format("{0} Comment", name); ;
                    shape.Header = name;
                    break;
                case "DC":
                    shape = new dxShape("DC", name, "Domain Controller", LayoutType.Node);
                    shape.HeaderStyle = Visio.Xml.TextStyle.SmallNormalBold;
                    shape.TextStyle = Visio.Xml.TextStyle.SmallNormal;
                    shape.LayoutParams = parameters.GetLayoutParameters();
                    shape.Text = String.Format("{0} Text", name);
                    shape.Comment = String.Format("{0} Comment", name); ;
                    shape.Header = name;
                    break;
                case "GC":
                    shape = new dxShape("DC", name, "Global Catalog", LayoutType.Node);
                    shape.HeaderStyle = Visio.Xml.TextStyle.SmallNormalBold;
                    shape.TextStyle = Visio.Xml.TextStyle.SmallNormal;
                    shape.LayoutParams = parameters.GetLayoutParameters();
                    shape.Text = String.Format("{0} Text", name);
                    shape.Comment = String.Format("{0} Comment", name); ;
                    shape.Header = name;
                    break;
                case "RODC":
                    shape = new dxShape("DC", name, "ReadOnlyDomain Controller.41", LayoutType.Node);
                    shape.HeaderStyle = Visio.Xml.TextStyle.SmallNormalBold;
                    shape.TextStyle = Visio.Xml.TextStyle.SmallNormal;
                    shape.LayoutParams = parameters.GetLayoutParameters();
                    shape.Text = String.Format("{0} Text", name);
                    shape.Comment = String.Format("{0} Comment", name); ;
                    shape.Header = name;
                    break;
            }
            return shape;
        }

        private dxConnection GetConnection(string prefix, string name, dxShape from, dxShape to)
        {
            dxConnection newSL = null;

            if (prefix == "SL")
            {
                newSL = new dxConnection("SL", name, from, to, "IP Site Link");
                newSL.HeaderStyle = Visio.Xml.TextStyle.NormalItalic;
                newSL.Comment = String.Format("{0} <==> {1}", from.Header, to.Header);
                newSL.Header = String.Format("{0} <--> {1}", from.Header, to.Header); 
                newSL.Text = String.Format("{0} Text", name);
            }
            else if (prefix == "CO-INTRA")
            {
                newSL = new dxConnection(name, from, to, "Directory Replication IntraSit", false);
                newSL.Comment = String.Format("{0} <==> {1}", from.Header, to.Header);
                newSL.LayoutParams = new dxLayoutParameters().GetLayoutParameters();
            }
            else if (prefix == "CO-INTER")
            {
                newSL = new dxConnection(name, from, to, "Directory Replication InterSit", false);
                newSL.Comment = String.Format("{0} <==> {1}", from.Header, to.Header);
                newSL.LayoutParams = new dxLayoutParameters().GetLayoutParameters();
            }

            return newSL;
        }
    }
}
