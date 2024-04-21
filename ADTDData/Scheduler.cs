using Mihelcic.Net.Visio.Data;
using Mihelcic.Net.Visio.Arrange;
using Mihelcic.Net.Visio.Arrange.Data;
using Mihelcic.Net.Visio.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mihelcic.Net.Visio.Common
{
    public class Scheduler
    {
        //public delegate void ReportProgress(string message);
        public delegate void EndRun(string message);

        #region Private Fields

        private static int _count;
        private ReportProgress _report;
        private EndRun _end;
        private List<BackgroundWorker> _workers = new List<BackgroundWorker>();
        private static readonly object _workerLock = new object();

        #endregion

        public static ManualResetEvent Done { get; private set; }

        public Scheduler(ReportProgress progress, EndRun end)
        {
            _report = progress;
            _end = end;
        }

        #region Public Methods

        public void Start(WorkerParameters parameters)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(MyWorker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(MyWorker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyWorker_RunWorkerCompleted);
            Register(worker);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerAsync(parameters);
        }

        // Call this method to request cancellation of all workers
        public void Cancel()
        {
            lock (_workerLock)
            {
                foreach (var worker in _workers)
                {
                    if (worker.WorkerSupportsCancellation)
                    {
                        worker.CancelAsync();
                        worker.Dispose();
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private void MyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                WorkerParameters parameters = e.Argument as WorkerParameters;
                worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusConnecting}");

                if (parameters.Reader.Connect(parameters.Parameters, _report))
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusConnected}");
                    worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusCollecting}");

                    bool? result = parameters.Reader.Read(parameters.Parameters, _report);
                    if (result.HasValue && result.Value)
                    {
                        Mihelcic.Net.Visio.Data.IData data = parameters.Reader.Data;
                        string visioFileName = Logger.ParseFilePath(parameters[ParameterNames.VisioFileName].ToString());

                        if (parameters[ParameterNames.ExportXML].ToString().ToLowerInvariant() == "true")
                        {
                            string dataFile = Path.ChangeExtension(visioFileName, "Xml");
                            worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusXml}");
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                            data.SaveXml(dataFile);
                        }

                        if (parameters[ParameterNames.ExportCSV].ToString().ToLowerInvariant() == "true")
                        {
                            string dataFile = Path.ChangeExtension(visioFileName, "csv");
                            worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusCsv}");
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                            data.SaveCsv(dataFile);
                        }

                        worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusCreating}");

                        byte[] template = Mihelcic.Net.Visio.Common.Properties.Resources.Template_Empty;
                        byte[] stencil = Mihelcic.Net.Visio.Common.Properties.Resources.Stencil;

                        using (MemoryStream templateStream = new MemoryStream(template))
                        {
                            using (FileStream docStream = new FileStream(visioFileName, FileMode.Create))
                            {
                                templateStream.WriteTo(docStream);
                            }
                        }
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        string stencilFileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

                        using (MemoryStream stencilStream = new MemoryStream(stencil))
                        {
                            using (FileStream stencilTmpStream = new FileStream(stencilFileName, FileMode.Create))
                            {
                                stencilStream.WriteTo(stencilTmpStream);
                            }
                        }

                        XVisioPackage document = new XVisioPackage(visioFileName, stencilFileName);
                        Diagram diagram = new Diagram(parameters.Layout);
                        foreach (dxShape shape in data.Shapes)
                        {
                            IDiagramNode node = VisioAddShape(shape, document, diagram);
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }

                        foreach (dxConnection connection in data.Connections)
                        {
                            VisioAddConnection(connection, document, diagram);
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }

                        worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusArranging}");
                        diagram.Arrange();

                        // Change Page size
                        XVisioHelper.SetPageSize(document, diagram.Width, diagram.Height);

                        // Add Nodes
                        worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusNodes}");
                        foreach (DiagramNode node in diagram.Nodes)
                        {
                            AddDiagramNode(diagram, node, document);
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }

                        worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusConnections}");
                        foreach (DiagramEdge edge in diagram.Edges.Where(c => c.Visible))
                        {
                            Double? tickness = edge.Presentation.Thickness;
                            XVisioHelper.AddConnection(document, edge.Presentation.MasterName, edge.Name, edge.From.Name, edge.To.Name, edge.TwoWay, tickness);
                            if (edge.Presentation.Color != null)
                                XVisioHelper.ChangeEdgeColor(document, edge.Name, edge.Presentation.Color.Value);

                            if (!String.IsNullOrWhiteSpace(edge.Presentation.Heading))
                                XVisioHelper.AddShapeText(document, edge.Name, edge.Presentation.Heading, (TextStyle)Enum.Parse(typeof(TextStyle), edge.Presentation.HeadingStyle));
                            if (!String.IsNullOrWhiteSpace(edge.Presentation.Comment))
                                XVisioHelper.AddComment(document, edge.Name, edge.Presentation.Comment);
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }

                        document.RecalcDocument();
                        worker.ReportProgress(0, $"{parameters.Name} - {Mihelcic.Net.Visio.Strings.StatusSaving}");
                        document.Save();
                        document.Close();
                    }
                    else worker.ReportProgress(0, $"{parameters.Name} - FAILED");
                }
                else
                {
                    worker.ReportProgress(0, $"{Mihelcic.Net.Visio.Strings.StatusConnectionFailed}");
                }
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
            }
        }

        private void MyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                Logger.TraceVerbose("***Progress reported: " + e.UserState.ToString());
                _report(e.UserState.ToString());
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine("EXCEPTION \n{0}", ex);
            }
        }

        private void MyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            DeRegister(worker);
            lock (_workerLock)
                if (_count == 0)
                    _end($"{Mihelcic.Net.Visio.Strings.StatusCompleted}");
        }

        private IDiagramNode VisioAddShape(dxShape shape, XVisioPackage document, Diagram diagram)
        {
            string masterName = shape.Master;
            Double size = 1;        // SIZE IS MISSING
            if (size == 0) size = 1;

            XSize masterSize = XVisioHelper.GetMasterSize(document, masterName);
            int width = shape.Width == 0 ? Convert.ToInt32(masterSize.Width) : shape.Width;
            int height = shape.Height == 0 ? Convert.ToInt32(masterSize.Height) : shape.Height;

            XSize shapeSize = new XSize(width, height);

            LayoutType layout = shape.Layout;
            string colorCoded = shape.ColorCodedAttribute;
            string comment = shape.Comment;
            string header = shape.Header;
            TextStyle headerStyle = shape.HeaderStyle;
            string text = shape.Text;

            dxShape parent = shape.Parent;
            Color? shapeColor = shape.ShapeColor;
            string subShape = shape.SubShapeToColor;
            ColorSchema cSchema = shape.ColorSchema;

            // ColorCode
            if (!String.IsNullOrWhiteSpace(shape.ColorCodedAttribute) &&
                shape.Attributes.ContainsKey(colorCoded))
            {
                string value = shape.Attributes[colorCoded].ToString();
                shapeColor = ColorPicker.GetColor(colorCoded, value, cSchema);
            }

            ShapePresentation presentation;

            presentation = new ShapePresentation(masterName);

            if (!String.IsNullOrWhiteSpace(comment))
                presentation.Comment = comment;

            if (!String.IsNullOrWhiteSpace(header))
            {
                presentation.Heading = header;
                presentation.HeadingStyle = headerStyle.ToString();
            }

            if (!String.IsNullOrWhiteSpace(text))
            {
                presentation.Text = text;
                presentation.TextStyle = shape.TextStyle.ToString();
            }

            if (shapeColor != null)
                presentation.Color = shapeColor.Value;

            if (subShape != null)
                presentation.SubShapeToColor = subShape;

            if (subShape != null)
                presentation.SubShapeToColor = subShape;

            IDiagramNode parentNode = null;

            if (parent != null)
                parentNode = diagram.GetNodeByName(parent.Id);

            IDiagramNode newNode = diagram.AddDiagramNode(shape.Id, parentNode, layout, presentation, width, height);
            newNode.Layout.SetParameters(shape.LayoutParams);

            foreach (dxShape childShape in shape.Children)
                VisioAddShape(childShape, document, diagram);

            return newNode;
        }

        private void VisioAddConnection(dxConnection connection, XVisioPackage document, Diagram diagram)
        {
            Logger.TraceVerbose("VisioAddConnection - From: {0} To: {1}", connection.From.Id, connection.To.Id);
            Dictionary<string, Color> colorCode = new Dictionary<string, Color>();
            // Get parameters
            string masterName = connection.Master;
            string colorCoded = connection.ColorCodedAttribute;
            string comment = connection.Comment; ;
            string header = connection.Header;
            TextStyle headerStyle = connection.HeaderStyle;
            string text = connection.Text;
            Color? shapeColor = connection.ShapeColor;
            string subShape = connection.SubShapeToColor;
            Double? tickness = connection.Tickness;
            bool bidirectional = connection.Bidirectional;

            DiagramEdge newEdge;

            ShapePresentation presentation = new ShapePresentation(masterName);

            IDiagramNode from = null;
            IDiagramNode to = null;

            from = diagram.GetNodeByName(connection.From.Id);
            to = diagram.GetNodeByName(connection.To.Id);

            if (from == null)
            {
                Debug.WriteLine("VisioAddConnection - From '{0}; doesn't exists.", connection.From.Id);
                return;
            }
            if (to == null)
            {
                Debug.WriteLine("VisioAddConnection - To '{0}; doesn't exists.", connection.To.Id);
                return;
            }

            newEdge = diagram.Connect(connection.Id, from, to, bidirectional);

            if (!String.IsNullOrWhiteSpace(comment))
                presentation.Comment = comment;

            if (!String.IsNullOrWhiteSpace(header))
            {
                presentation.Heading = header;
                presentation.HeadingStyle = headerStyle.ToString();
            }

            if (!String.IsNullOrWhiteSpace(text))
            {
                presentation.Text = text;
                presentation.TextStyle = connection.TextStyle.ToString();
            }

            if (shapeColor != null)
                presentation.Color = shapeColor.Value;

            if (subShape != null)
                presentation.SubShapeToColor = subShape;

            presentation.Thickness = tickness;

            newEdge.Presentation = presentation;

            Logger.TraceVerbose("VisioAddConnection - From: {0} To: {1} ADDED", connection.From.Id, connection.To.Id);
        }

        private void AddDiagramNode(Diagram diagram, DiagramNode node, XVisioPackage document)
        {
            HierarchyLayout hLayout = node.Layout as HierarchyLayout;
            if (hLayout == null)
            {
                if (node.Presentation != null)
                {
                    if (node.ShapeWidth == 0)
                        XVisioHelper.AddNode(document, node.Presentation.MasterName, node.Name, node.X, node.Y);
                    else
                        XVisioHelper.AddNode(document, node.Presentation.MasterName, node.Name, node.X, node.Y, node.ShapeWidth, node.ShapeHeight);

                    if (node.Presentation.Color != null)
                        if (String.IsNullOrWhiteSpace(node.Presentation.SubShapeToColor))
                            XVisioHelper.ChangeShapeColor(document, node.Name, node.Presentation.Color.Value);
                        else
                            XVisioHelper.ChangeShapeColor(document, node.Name, node.Presentation.Color.Value, node.Presentation.SubShapeToColor);
                    if (!String.IsNullOrWhiteSpace(node.Presentation.Heading))
                        XVisioHelper.AddShapeText(document, node.Name, node.Presentation.Heading, (TextStyle)Enum.Parse(typeof(TextStyle), node.Presentation.HeadingStyle));
                    if (!String.IsNullOrWhiteSpace(node.Presentation.Text))
                        XVisioHelper.AddShapeText(document, node.Name, node.Presentation.Text, (TextStyle)Enum.Parse(typeof(TextStyle), node.Presentation.TextStyle));
                    if (!String.IsNullOrWhiteSpace(node.Presentation.Comment))
                        XVisioHelper.AddComment(document, node.Name, node.Presentation.Comment);

                    // Add subNodes
                    ReadSlaves(diagram, document, node, node.Nodes);
                }
            }
            else
            {
                XVisioHelper.AddGhostNode(document, node.Name, node.X, node.Y, node.ShapeWidth, node.ShapeHeight);
                AddSlaveNode(diagram, hLayout.Root, node, document);
            }
        }

        private void ReadSlaves(Diagram diagram, XVisioPackage document, IDiagramNode parent, IEnumerable<IDiagramNode> nodes)
        {
            foreach (DiagramNode node in nodes.Select(n => n as DiagramNode))
            {
                HierarchyLayout hLayout = node.Layout as HierarchyLayout;

                if (hLayout == null)
                {
                    AddSlaveNode(diagram, node, parent, document);
                }
                else
                {
                    XVisioHelper.AddGhostNode(document, node.Name, node.X, node.Y, node.ShapeWidth, node.ShapeHeight, parent.Name);
                    AddSlaveNode(diagram, hLayout.Root, node, document);
                }

            }
        }

        private void AddSlaveNode(Diagram diagram, DiagramNode node, IDiagramNode parent, XVisioPackage document)
        {
            if (node.Presentation != null)
            {
                XVisioHelper.AddSlaveNode(document, node.Presentation.MasterName,
                                node.Name, parent.Name,
                                node.X, node.Y, node.ShapeWidth, node.ShapeHeight);
                if (node.Presentation.Color != null)
                    if (String.IsNullOrWhiteSpace(node.Presentation.SubShapeToColor))
                        XVisioHelper.ChangeShapeColor(document, node.Name, node.Presentation.Color.Value);
                    else
                        XVisioHelper.ChangeShapeColor(document, node.Name, node.Presentation.Color.Value, node.Presentation.SubShapeToColor);
                if (!String.IsNullOrWhiteSpace(node.Presentation.Heading))
                    XVisioHelper.AddShapeText(document, node.Name, node.Presentation.Heading, (TextStyle)Enum.Parse(typeof(TextStyle), node.Presentation.HeadingStyle));
                if (!String.IsNullOrWhiteSpace(node.Presentation.Text))
                    XVisioHelper.AddShapeText(document, node.Name, node.Presentation.Text, (TextStyle)Enum.Parse(typeof(TextStyle), node.Presentation.TextStyle));
                if (!String.IsNullOrWhiteSpace(node.Presentation.Comment))
                    XVisioHelper.AddComment(document, node.Name, node.Presentation.Comment);
            }

            HierarchyMemberNode hNode = node as HierarchyMemberNode;

            // Add subNodes
            ReadSlaves(diagram, document, node, node.Nodes);

            // Add nodes in the hierarchy
            if (hNode != null)
                ReadSlaves(diagram, document, node, hNode.Children);
        }

        private void Register(BackgroundWorker worker)
        {
            lock (_workerLock)
            {
                _workers.Add(worker);
                if (_count == 0)
                    Done = new ManualResetEvent(false);
                _count++;
            }
        }

        private void DeRegister(BackgroundWorker worker)
        {
            //if(_count == 1)
            //    worker.ReportProgress(0, $"{Mihelcic.Net.Visio.Strings.StatusCompleted}");
            lock (_workerLock)
            {
                _workers.Remove(worker);
                _count--;
                if (_count == 0)
                {
                    //ReportProgress(0, $"{Mihelcic.Net.Visio.Strings.StatusCompleted}");
                    Done.Set();
                }
            }
        }

        #endregion
    }
}
