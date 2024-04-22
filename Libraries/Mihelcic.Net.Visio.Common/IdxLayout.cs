using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.ADTD.Data
{
    public interface IdxLayout
    {
        string diagramType { get; set; }
        Dictionary<string, object> LayoutData { get; set; }
        void doLayout(dxDiagram diagram);
    }
}
