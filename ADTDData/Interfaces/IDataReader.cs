using System.Collections.Generic;

namespace Mihelcic.Net.Visio.Data
{
    public delegate void ReportProgress(string message);

    public interface IDataReader
    {
        IData Data { get; }
        bool Connected { get; }
        bool? Read(Dictionary<string, object> parameters, ReportProgress reportStatus);
        bool Connect(Dictionary<string, object> parameters, ReportProgress reportStatus);
    }
}
