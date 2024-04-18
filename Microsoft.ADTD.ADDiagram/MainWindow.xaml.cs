using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Xml;
using Microsoft.ADTD.XMLVisio;
using Microsoft.ADTD.Data;
using System.Diagnostics;

namespace Microsoft.ADTD.ADDiagram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string STENCIL_FILE = @"Resources\\Stencil.vsx";
        const string TEMPLATE_FILE = @"Resources\\Template.vtx";

        private OpenFileDialog selectOutputDlg = new OpenFileDialog();
        private XmlDataProvider engagementProvider;

        bool siteRunning = false;
        bool domainRunning = false;

        public string fName
        {
            get { return (string)this.GetValue(fNameProperty); }
            set { this.SetValue(fNameProperty, value); }
        }

        public static DependencyProperty fNameProperty =
            DependencyProperty.Register("fName",
            typeof(string),
            typeof(MainWindow),
            new PropertyMetadata(Properties.Settings.Default.Output == "" ?
                "C:\\ADRAP\\Public\\Output\\EngagementManifest.ctx" : Properties.Settings.Default.Output, TestPropertyChanged));

        private string filename;

        private static void TestPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mainWindow = source as MainWindow;
            string newValue = e.NewValue as string;
            // Do additional logic 
        }

        private string dirName
        {
            get { return fName.Substring(0, fName.LastIndexOf("\\") + 1); }
        }

        public MainWindow()
        {
            InitializeComponent();

            engagementProvider = (XmlDataProvider)FindResource("engagementData");
            engagementProvider.Source = new Uri(fName);
            engagementProvider.Refresh();
            tbOutput.DataContext = this;
            cbDomainDiagram.IsChecked = Properties.Settings.Default.DomainSelected;
            cbSiteDiagram.IsChecked = Properties.Settings.Default.SiteSelected;
            tbOutput.Text = Properties.Settings.Default.Output;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btSelectOutput_Click(object sender, RoutedEventArgs e)
        {
            this.selectOutputDlg.CheckFileExists = true;
            this.selectOutputDlg.Filter = "Engagement Manifest File|EngagementManifest.ctx";
            this.selectOutputDlg.Multiselect = false;
            if (this.selectOutputDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Xml.XmlElement elm = null;

                Properties.Settings.Default.Output = this.selectOutputDlg.FileName;
                Properties.Settings.Default.Save();
                engagementProvider.Source = new Uri(Properties.Settings.Default.Output);
                engagementProvider.Refresh();
                fName = Properties.Settings.Default.Output;

                System.Collections.ICollection nd = (System.Collections.ICollection)engagementProvider.Data;
                foreach (System.Xml.XmlElement X in nd.AsQueryable()) elm = X;
                string verString = elm.Attributes["RapPkgVersion"].Value;
                int verNum = Convert.ToInt32(verString.Substring(0, verString.IndexOf(".")));
                //MsgBox("verString: " & verString & vbNewLine & "verNum: " & verNum)
                verNum = 800;
                if (verNum < 800)
                {
                    System.Windows.MessageBox.Show("Read from Output is supported for data \ncollected with RAP ver 8.0 or later...", "Data Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    btnCreate.IsEnabled = false;
                }
                else
                {
                    btnCreate.IsEnabled = true;
                    //tbOutput.Text = this.selectOutputDlg.FileName;
                    //this.Title = "AD Diagram - " + elm.Attributes["CustomerFriendlyName"].Value;
                    //lblCustomer.Content = elm.Attributes["CustomerFriendlyName"].Value;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.DomainSelected = (bool)cbDomainDiagram.IsChecked;
            Properties.Settings.Default.SiteSelected = (bool)cbSiteDiagram.IsChecked;
            Properties.Settings.Default.Output = tbOutput.Text;
            Properties.Settings.Default.Save();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {

            if ((bool)cbSiteDiagram.IsChecked)
            {
                setVisibility(false);
                startSiteDraw();
            }
            if ((bool)cbDomainDiagram.IsChecked)
            {
                setVisibility(false);
                //Microsoft.ADTD.Data.DomainDataReader dr = new Microsoft.ADTD.Data.DomainDataReader();
                //filename = fName;
                //dr.fName = this.filename;
                //dr.read();
                //setVisibility(true);
                startDomainDraw();
            }
        }

        private void setVisibility(bool visibility)
        {
            btnCreate.IsEnabled = visibility;
            btnExit.IsEnabled = visibility;
            cbSiteDiagram.IsEnabled = visibility;
            cbDomainDiagram.IsEnabled = visibility;
            this.Cursor = visibility ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.Wait;
        }

        private string getFileName(string directory, string fileName)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            if (di.GetFiles(fileName).Count() == 0)
            {
                return directory + fileName;
            }
            else
            {
                string name = fileName.Substring(0, fileName.LastIndexOf("."));
                string extension = fileName.Substring(fileName.LastIndexOf("."));
                var files = di.GetFiles(name + "*" + extension).OrderBy(f => f.Name);
                SortedList<byte, string> additions = new SortedList<byte, string>();
                foreach (FileInfo fn in files)
                {
                    string ad = fn.Name.Remove(0, name.Length);
                    ad = ad.Substring(0, ad.LastIndexOf("."));
                    byte b;
                    if (ad != "" && Byte.TryParse(ad, out b)) additions.Add(b, ad);
                }
                return directory + name + (additions.Count > 0 ? (additions.Last().Key + 1).ToString("000") : "001") + extension;
            }
            //return "C:\\Temp\\A-Test\\SiteDiagram.vdx";
        }

        private BackgroundWorker mySiteWorker;

        [MTAThread]
        private void startSiteDraw()
        {
            siteRunning = true;
            filename = fName;
            this.mySiteWorker = new BackgroundWorker();
            this.mySiteWorker.DoWork += new DoWorkEventHandler(this.mySiteWorker_DoWork);
            this.mySiteWorker.ProgressChanged += new ProgressChangedEventHandler(mySiteWorker_ProgressChanged);
            this.mySiteWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.mySiteWorker_RunWorkerCompleted);
            this.mySiteWorker.WorkerReportsProgress = true;
            this.mySiteWorker.WorkerSupportsCancellation = true;
            this.mySiteWorker.RunWorkerAsync();
        }

        private void mySiteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress(0, "Site Diagram - Read Data from output...");

            Microsoft.ADTD.Data.SiteDataReader dr = new Microsoft.ADTD.Data.SiteDataReader();
            dr.fName = this.filename;
            dr.read();

            worker.ReportProgress(0, "Site Diagram - Creating Document...");

            vxDocument doc = new vxDocument(TEMPLATE_FILE);
            vxDocument stencil = new vxDocument(STENCIL_FILE);

            List<string> domainList = ((from dm in dr.diagram.Nodes[DiagramObjectNames.ServerObject].Values
                                        select dm.getStringAttribute(AttributeNames.ServerFQDN)).Distinct()).ToList<string>();
            worker.ReportProgress(0, "Site Diagram - Adding Nodes...");

            foreach (dxNode node in dr.diagram.Nodes[DiagramObjectNames.SiteObject].Values)
            {
                dr.getData(node);
                doc.addShape(stencil, ShapeType.Site, dr.getData(node));
                vxDefinitions.UpdateMiscSection(doc.Shapes[node.Type + "-" + node.ID], "Comment", dr.getPopupText(node));

                foreach (dxNode srv in node.Children.Values)
                {
                    ShapeType sType = ShapeType.DC;
                    if ((bool)srv.getAttribute(AttributeNames.GlobalCatalog)) sType = ShapeType.GC;
                    if ((bool)srv.getAttribute(AttributeNames.RODC)) sType = ShapeType.RODC;
                    doc.addShape(stencil, sType, dr.getData(srv));
                    vxDefinitions.UpdateMiscSection(doc.Shapes[srv.Type + "-" + srv.ID], "Comment", dr.getPopupText(srv));
                    int colorNum = domainList.IndexOf(srv.getStringAttribute(AttributeNames.ServerFQDN));
                    int paternNum = (colorNum / 23) +1;
                    colorNum = (colorNum % 23) + 1;
                    //Debug.WriteLine("FillBkgnd = " + colorNum.ToString() + ", FillPattern = " + paternNum.ToString());
                    vxDefinitions.FillSection(doc.Shapes[srv.Type + "-" + srv.ID], "Front", "0", "FillBkgnd");
                    vxDefinitions.FillSection(doc.Shapes[srv.Type + "-" + srv.ID], "Front", paternNum.ToString(), "FillPattern");
                    vxDefinitions.FillSection(doc.Shapes[srv.Type + "-" + srv.ID], "Front", colorNum.ToString(), "FillForegnd");
                }
            }

            if (dr.diagram.Nodes.ContainsKey(DiagramObjectNames.ConnectionPointObject))
                foreach (dxNode node in dr.diagram.Nodes[DiagramObjectNames.ConnectionPointObject].Values)
                {
                    doc.addShape(stencil, ShapeType.Site, dr.getData(node));
                }

            worker.ReportProgress(0, "Site Diagram - Adding connections...");

            foreach (dxEdge edge in dr.diagram.Edges[DiagramObjectNames.SiteLinkObject])
            {
                doc.addShape(stencil, ShapeType.IPSiteLink, dr.getData(edge));
                doc.ConnectNodes(edge.FromNode.Type + "-" + edge.FromNode.ID, edge.ToNode.Type + "-" + edge.ToNode.ID, edge.Type + "-" + edge.ID, 0);
                vxDefinitions.UpdateMiscSection(doc.Shapes[edge.Type + "-" + edge.ID], "Comment", dr.getPopupText(edge));
            }

            foreach (dxNode srv in dr.diagram.Nodes[DiagramObjectNames.ServerObject].Values)
            {
                foreach (dxEdge co in srv.Edges)
                {
                    if (co.FromNode != null)    //Skip bad data objects
                    {
                        if (srv.Parent.ID == co.FromNode.Parent.ID)
                            doc.addShape(stencil, ShapeType.IntraSiteCO, dr.getData(co));
                        else
                            doc.addShape(stencil, ShapeType.InterSiteCO, dr.getData(co));
                        doc.ConnectNodes(co.FromNode.Type + "-" + co.FromNode.ID, srv.Type + "-" + srv.ID, co.Type + "-" + co.ID, 1);
                        vxDefinitions.UpdateMiscSection(doc.Shapes[co.Type + "-" + co.ID], "Comment", dr.getPopupText(co));
                        if (!(bool)co.getAttribute(AttributeNames.Authoritative))
                            vxDefinitions.ChangeLine(doc.Shapes[co.Type + "-" + co.ID], "#FF0000");
                        foreach (string nc in co.Children.Keys)
                            vxDefinitions.addLayer(doc, doc.Shapes[co.Type + "-" + co.ID], nc);
                    }
                }
            }

            XmlDocument d = doc.xml;

            d.PreserveWhitespace = true;
            d.Save(getFileName("C:\\Temp\\A-Test\\", "SiteDiagram.vdx"));
        }

        private void mySiteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            setStatusMsg(e.UserState.ToString());
        }

        private void mySiteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!domainRunning)
            {
                setVisibility(true);
                btnCreate.IsEnabled = false;
            }
            setStatusMsg("Site Diagram - Done!");
            siteRunning = false;
        }

        private BackgroundWorker myDomainWorker;

        [MTAThread]
        private void startDomainDraw()
        {
            filename = fName;
            domainRunning = true;
            this.myDomainWorker = new BackgroundWorker();
            this.myDomainWorker.DoWork += new DoWorkEventHandler(this.myDomainWorker_DoWork);
            this.myDomainWorker.ProgressChanged += new ProgressChangedEventHandler(myDomainWorker_ProgressChanged);
            this.myDomainWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.myDomainWorker_RunWorkerCompleted);
            this.myDomainWorker.WorkerReportsProgress = true;
            this.myDomainWorker.WorkerSupportsCancellation = true;
            this.myDomainWorker.RunWorkerAsync();
        }

        private void myDomainWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress(0, "Domain Diagram - Read Data from output...");

            Microsoft.ADTD.Data.DomainDataReader dr = new Microsoft.ADTD.Data.DomainDataReader();
            dr.fName = this.filename;
            dr.read();

            worker.ReportProgress(0, "Domain Diagram - Creating Document...");

            vxDocument doc = new vxDocument(TEMPLATE_FILE);
            vxDocument stencil = new vxDocument(STENCIL_FILE);

            worker.ReportProgress(0, "Domain Diagram - Adding Nodes...");

            foreach (dxNode node in dr.diagram.Nodes[DiagramObjectNames.DomainObject].Values)
            {
                dr.getData(node);
                doc.addShape(stencil, ShapeType.ADDomain, dr.getData(node));
                vxDefinitions.UpdateMiscSection(doc.Shapes[node.Type + "-" + node.ID], "Comment", dr.getPopupText(node));
                if (!(bool)node.getAttribute(AttributeNames.InternalDomain))
                    vxDefinitions.FillSection(doc.Shapes[node.Type + "-" + node.ID], "Process", "#008080", "FillForegnd");

                foreach (dxNode srv in node.Children.Values)
                {
                    ShapeType sType = ShapeType.DC;
                    if ((bool)srv.getAttribute(AttributeNames.GlobalCatalog)) sType = ShapeType.GC;
                    if ((bool)srv.getAttribute(AttributeNames.RODC)) sType = ShapeType.RODC;
                    doc.addShape(stencil, sType, dr.getData(srv));
                    int colorNum = dr.diagram.Nodes[DiagramObjectNames.DomainObject].Values.ToList().IndexOf(node);
                    int paternNum = (colorNum / 23) + 1;
                    colorNum = (colorNum % 23) + 1;

                    vxDefinitions.FillSection(doc.Shapes[srv.Type + "-" + srv.ID], "Front", "0", "FillBkgnd");
                    vxDefinitions.FillSection(doc.Shapes[srv.Type + "-" + srv.ID], "Front", paternNum.ToString(), "FillPattern");
                    vxDefinitions.FillSection(doc.Shapes[srv.Type + "-" + srv.ID], "Front", colorNum.ToString(), "FillForegnd");

                    vxDefinitions.UpdateMiscSection(doc.Shapes[srv.Type + "-" + srv.ID], "Comment", dr.getPopupText(srv));
                    //vxDefinitions.FillSection(doc.Shapes[srv.Type + "-" + srv.ID], "Front", 
                    //    dr.diagram.Nodes[DiagramObjectNames.DomainObject].Values.ToList().IndexOf(node).ToString()); //***********************************
                }
            }

            worker.ReportProgress(0, "Domain Diagram - Adding connections...");

            if (dr.diagram.Edges.ContainsKey(DiagramObjectNames.Windows2000TrustObject))
                foreach (dxEdge edge in dr.diagram.Edges[DiagramObjectNames.Windows2000TrustObject])
                {
                    doc.addShape(stencil, ShapeType.W2000Trust, dr.getData(edge));
                    doc.ConnectNodes(edge.FromNode.Type + "-" + edge.FromNode.ID, edge.ToNode.Type + "-" + edge.ToNode.ID, edge.Type + "-" + edge.ID, 0);
                    vxDefinitions.UpdateMiscSection(doc.Shapes[edge.Type + "-" + edge.ID], "Comment", dr.getPopupText(edge));
                }

            XmlDocument d = doc.xml;

            d.PreserveWhitespace = true;
            d.Save(getFileName("C:\\Temp\\A-Test\\", "Domain Diagram.vdx"));
        }

        private void myDomainWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            setStatusMsg(e.UserState.ToString());
        }

        private void myDomainWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!siteRunning)
            {
                setVisibility(true);
                btnCreate.IsEnabled = false;
            }
            setStatusMsg("Domain Diagram - Done!");
            domainRunning = false;
        }

        private void setStatusMsg(string message)
        {
            sbiInfo.Content = message;
        }
    }
}
