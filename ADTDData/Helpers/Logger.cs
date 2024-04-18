using System;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.IO;
using System.Runtime.CompilerServices;

namespace Mihelcic.Net.Visio.Common
{
    public class Logger
    {
        public const string ETWProviderId = "75253064-0F62-470A-A3E3-7AEC9B287BD5";
        private static TraceSource _myTraceSource;

        #region Public Methods

        public static void Init(string name)
        {
            if (_myTraceSource == null)
                _myTraceSource = new TraceSource(name);
        }

        public static void RegisterTrace(string traceFileName)
        {
            TextWriterTraceListener myListener = new TextWriterTraceListener(traceFileName)
            {
                Filter = new EventTypeFilter(SourceLevels.Verbose)
            };
            EventProviderTraceListener etwListener = new EventProviderTraceListener(ETWProviderId, "ETWListener");

            _myTraceSource.Listeners.Clear();
            _myTraceSource.Listeners.Add(myListener);
            _myTraceSource.Listeners.Add(etwListener);
            _myTraceSource.Switch.Level = SourceLevels.All;
            _myTraceSource.TraceEvent(TraceEventType.Start, 1008);
        }

        public static void RegisterDebug(string debugFileName, bool console = false)
        {
            Debug.Listeners.Clear();
            Debug.Listeners.Add(new TextWriterTraceListener(debugFileName));
            if (console)
                Debug.Listeners.Add(new ConsoleTraceListener());
        }

        public static void ChangeTrace(string traceFileName)
        {
            foreach (TraceListener listener in _myTraceSource.Listeners)
            {
                if(listener is TextWriterTraceListener)
                listener.Flush();
                listener.Close();
            }
            TextWriterTraceListener myListener = new TextWriterTraceListener(traceFileName)
            {
                Filter = new EventTypeFilter(SourceLevels.Verbose)
            };

            _myTraceSource.Listeners.Clear();
            _myTraceSource.Listeners.Add(myListener);
            _myTraceSource.Switch.Level = SourceLevels.All;
            _myTraceSource.TraceEvent(TraceEventType.Start, 1008);
        }

        public static void ChangeDebug(string debugFileName, bool console = false)
        {
            foreach (TraceListener listener in Debug.Listeners)
            {
                listener.Flush();
                listener.Close();
            }
            Debug.Listeners.Add(new TextWriterTraceListener(debugFileName));
            if (console)
                Debug.Listeners.Add(new ConsoleTraceListener());
        }

        public static void Close()
        {
            foreach (TraceListener listener in _myTraceSource.Listeners)
            {
                _myTraceSource.TraceEvent(TraceEventType.Stop, 1009);
                listener.Flush();
                listener.Close();
            }
            _myTraceSource.Close();
            foreach (TraceListener listener in Debug.Listeners)
            {
                listener.Flush();
                listener.Close();
            }
        }

        public static void Trace(TraceEventType type, int id, string message, params object[] args)
        {
            _myTraceSource.TraceEvent(type, id, message, args);
        }

        public static void TraceException(string message, [CallerMemberName] string callerName = "", params object[] args)
        {
            _myTraceSource.TraceEvent(TraceEventType.Error, 1000, $"Exception in {callerName}");
            _myTraceSource.TraceEvent(TraceEventType.Error, 1000, message, args);
        }

        public static void TraceException(string message, [CallerMemberName] string callerName = "")
        {
            _myTraceSource.TraceEvent(TraceEventType.Error, 1000, $"Exception in {callerName}");
            _myTraceSource.TraceEvent(TraceEventType.Error, 1000, message);
        }

        public static void TraceDebug(string message, params object[] args)
        {
            _myTraceSource.TraceEvent(TraceEventType.Information, 1001, message, args);
        }

        public static void TraceDebug(string message)
        {
            _myTraceSource.TraceEvent(TraceEventType.Information, 1001, message);
        }

        public static void TraceVerbose(string message, params object[] args)
        {
            _myTraceSource.TraceEvent(TraceEventType.Verbose, 1002, message, args);
        }

        public static void TraceVerbose(string message)
        {
            _myTraceSource.TraceEvent(TraceEventType.Verbose, 1002, message);
        }

        public static string ParsePath(string directoryName)
        {
            const string userProfile = "%USERPROFILE%";
            const string userDocuments = "%DOCUMENTS%";
            const string userLocalAppData = "%LOCALAPPDATA%";

            if (directoryName.Contains(userProfile))
            {
                directoryName = directoryName.Replace(userProfile, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            }
            if (directoryName.Contains(userDocuments))
            {
                directoryName = directoryName.Replace(userDocuments, Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            }
            if (directoryName.Contains(userLocalAppData))
            {
                directoryName = directoryName.Replace(userLocalAppData, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            }

            if(!Directory.Exists(directoryName))
            {
                try
                {
                    Directory.CreateDirectory(directoryName);
                }
                catch
                {
                    directoryName = Path.GetTempPath();
                }
            }

            return directoryName;
        }

        public static string ParseFilePath(string fileName)
        {
            string path = Path.GetDirectoryName(fileName);
            string file = Path.GetFileName(fileName);
            if (!String.IsNullOrWhiteSpace(path))
                path = ParsePath(path);
            return Path.Combine(path, file);
        }

        #endregion
    }
}
