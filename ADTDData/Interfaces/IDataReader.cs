using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Data
{
    public interface IDataReader
    {
        IData Data { get; }
        bool Connected { get; }
        bool? Read(Dictionary<string, object> parameters);
        bool Connect(Dictionary<string, object> parameters);
    }
}
