using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Data
{
    public interface IData
    {
        IList<dxConnection> Connections { get; }
        List<dxShape> Shapes { get; }
        IDictionary<string, dxShape> AllShapes { get; }
        void Clear();
        void AddShape(dxShape shape, string parentId);
        void AddShape(dxShape shape, dxShape parent=null);
        void AddConnection(dxConnection shape);
        dxShape GetNode(string id);
        dxShape GetNode(string prefix, string name);
        dxShape GetNode(string[] prefixes, string name);
        IEnumerable<dxObject> GetNodes(string prefix);
        void SaveXml(string fileName);
        void SaveCsv(string fileName);
    }
}
