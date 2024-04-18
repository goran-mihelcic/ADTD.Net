using Microsoft.ADTD.Data;
using Microsoft.ADTD.Visio.Arrange;
using System;
using System.Diagnostics;


namespace Microsoft.ADTD.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Logger logger = new Logger(true, true, @"my.log", @"my.trc", true);
            Debug.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
            //logger.Init(true, true, @"my.log", @"my.trc", true);
            //Debug.WriteLine("XXXXXXX");
            Scheduler scheduler = new Scheduler(ReportProgress, EndSchedule);

            IDataReader reader = new SiteDataReader();
            WorkerParameters wParameters = new WorkerParameters("SiteDiagram", reader, LayoutType.Web);
            wParameters.Add(Microsoft.ADTD.Data.ParameterNames.readServers, true);
            wParameters.Add(Microsoft.ADTD.Data.ParameterNames.agregateSites, false);
            wParameters.Add(Microsoft.ADTD.Data.ParameterNames.expandSiteLinks, false);
            wParameters.Add(Microsoft.ADTD.Data.ParameterNames.allSites, true);
            wParameters.Add(Microsoft.ADTD.Data.ParameterNames.selected, new Microsoft.ADTD.Data.ScopeItem(""));
            wParameters.Add(ParameterNames.VisioFileName, @"C:\temp\Visio Reports\AD Sites.vsdx");

            IDataReader dReader = new DomainDataReader();
            WorkerParameters wDParameters = new WorkerParameters("DomainDiagram", dReader, LayoutType.Web);
            wDParameters.Add(Microsoft.ADTD.Data.ParameterNames.readServers, true);
            wDParameters.Add(Microsoft.ADTD.Data.ParameterNames.useGC, true);
            wDParameters.Add(ParameterNames.VisioFileName, @"C:\temp\Visio Reports\AD Domains.vsdx");

            scheduler.Start(wParameters);
            scheduler.Start(wDParameters);
            Scheduler.Done.WaitOne();
            Console.WriteLine("Scheduler ended");
            Logger.Close();
            Console.ReadKey();
        }

        private static void ReportProgress(string message)
        {
            Console.WriteLine(message);
        }

        private static void EndSchedule(string message)
        {
            Console.WriteLine(message);
        }
    }
}
