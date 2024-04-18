using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ADTD.Data;
using Microsoft.ADTD;
using System.Diagnostics;


namespace Microsoft.ADTD.Layouts
{
    public class Layout : IdxLayout
    {

        private dxDiagram Diagram;

        public string diagramType { get; set; }
        public Dictionary<string, object> LayoutData { get; set; }

        public void doLayout(dxDiagram diagram)
        {
            Diagram = diagram;
        }
    }
}
