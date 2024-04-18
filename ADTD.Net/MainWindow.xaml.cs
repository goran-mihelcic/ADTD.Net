﻿using ADTD.Net;
using Mihelcic.Net.Visio.Arrange;
using Mihelcic.Net.Visio.Common;
using Mihelcic.Net.Visio.Data;
using Mihelcic.Net.Visio.Diagrammer.Unsafe;
using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Mihelcic.Net.Visio.Diagrammer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        readonly ToolSelection _selection = new ToolSelection();
        ApplicationConfiguration _configuration;
        readonly Scheduler _scheduler;

        public ApplicationConfiguration Configuration { get { return _configuration; } set { _configuration = value; } }

        public MainWindow()
        {
            Properties.Settings.Default.Save();
            _configuration = new ApplicationConfiguration
            {
                DomainJoined = NETAPI32.IsInADDomain()
            };
            if (_configuration.DomainJoined)
            {
                DCInfo dcInfo = NETAPI32.DsGetDcName(null, null, null, NETAPI32.GetDcFlags.ForceRediscovery | NETAPI32.GetDcFlags.OnlyLdapNeeded, null);
                if (dcInfo != null)
                {
                    _configuration.DnsForestName = dcInfo.DnsForestName;
                    if (String.IsNullOrWhiteSpace(_configuration.Server))
                        _configuration.Server = dcInfo.Name.Trim('\\');
                }
            }
            else
            {
                Join_Info joinInfo = NETAPI32.GetAadJoinInformation();
                if (joinInfo != null && joinInfo.JoinType > 0)
                {
                    _configuration.AADJoined = true;
                }
            }

            InitializeComponent();

            _scheduler = new Scheduler(ReportProgress, EndSchedule);

            mySelection.SetConfiguration(Configuration);
            mySelection.DoUpdate = new UpdateSettings(myProperties.SetConfiguration);
            myProperties.SetConfiguration(Configuration);
            Logger.Init("ADTD.Net");

            ConfigurationParameter debugParam = _configuration.Options.FirstOrDefault(p => p.Name == "DebugPath");
            string debugPath = debugParam == null ? "ADTD.log" : Path.Combine(Logger.ParsePath(debugParam.Value.ToString()), "ADTD.log");

            ConfigurationParameter traceParam = _configuration.Options.FirstOrDefault(p => p.Name == "TracePath");
            string tracePath = traceParam == null ? "ADTD.trc.log" : Path.Combine(Logger.ParsePath(traceParam.Value.ToString()), "ADTD.trc.log");

            Logger.RegisterTrace(tracePath);
            //Logger.TraceVerbose("TEST");
            Logger.RegisterDebug(debugPath);
            //Debug.WriteLine("TEST");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.BindingGroup.BeginEdit();
                ReportProgress("Validate settings first");
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine("EXCEPTION \n{0}", ex);
            }
        }

        private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (new About()).Show();
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
            }
        }

        private void mnuToolsOptions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Options optionsDlg = new Options();
                optionsDlg.SetConfiguration(_configuration);
                optionsDlg.Owner = this;
                optionsDlg.ShowDialog();

                ConfigurationParameter debugParam = _configuration.Options.FirstOrDefault(p => p.Name == "DebugPath");
                string debugPath = debugParam == null ? "ADTD.log" : Path.Combine(Logger.ParsePath(debugParam.Value.ToString()), "ADTD.log");

                ConfigurationParameter traceParam = _configuration.Options.FirstOrDefault(p => p.Name == "TracePath");
                string tracePath = traceParam == null ? "ADTD.trc.log" : Path.Combine(Logger.ParsePath(traceParam.Value.ToString()), "ADTD.trc.log");

                Logger.ChangeTrace(tracePath);
                Logger.TraceVerbose("Trace file location changed");
                Logger.ChangeDebug(debugPath);
                Debug.WriteLine("Debug Log file location changed");
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine("EXCEPTION \n{0}", ex);
            }
        }

        private void saveSettings()
        {
            try
            {
                Properties.Settings.Default.Save();
                Logger.TraceVerbose("Settings saved!");
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine("EXCEPTION \n{0}", ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            saveSettings();
            Logger.Close();
        }

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _scheduler.Cancel();
                this.BindingGroup.UpdateSources();
                Logger.Close();
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine("EXCEPTION \n{0}", ex);
            }
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DebugSelection();
                if (Configuration.Items.Any(i => i.Selected))
                {
                    SetVisibility(false);
                    LdapReader.Initialize(_configuration.Server);
                    //Scheduler scheduler = new Scheduler(ReportProgress, EndSchedule);
                    foreach (ConfigurationItem item in Configuration.Items.Where(i => i.Selected))
                    {
                        Debug.WriteLine("Schedulling {0}", item.Name as object);
                        string readerDll = Path.Combine(Environment.CurrentDirectory, item.Dll);
                        Assembly asm = Assembly.LoadFile(readerDll);
                        Type[] types = asm.GetExportedTypes();
                        Type type = types.FirstOrDefault(n => n.Name.ToLowerInvariant() == item.Name.ToLowerInvariant());
                        if (type != null && type.GetInterfaces().Contains(typeof(Mihelcic.Net.Visio.Data.IDataReader)))
                        {
                            IDataReader reader = Activator.CreateInstance(type) as IDataReader;

                            WorkerParameters wParameters = new WorkerParameters(item.Name, reader, LayoutType.Web);
                            foreach (ConfigurationParameter param in item.Parameters)
                            {
                                wParameters.Add(param.Key, param.Value);
                            }
                            wParameters.Add(ParameterNames.LoginInfo, Configuration.Login);
                            _scheduler.Start(wParameters);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine("EXCEPTION \n{0}", ex);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            _scheduler.Cancel();
            _configuration.Save();
            this.Close();
        }

        private void SetVisibility(bool visibility)
        {
            btnDraw.IsEnabled = visibility;
            btnExit.IsEnabled = visibility;
            this.Cursor = visibility ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.Wait;
        }

        private void DebugSelection()
        {
            try
            {
                Logger.TraceVerbose("********** SELECTIONS **********");
                Debug.WriteLine("********** SELECTIONS **********");

                string server = String.IsNullOrWhiteSpace(_configuration.Server) ? "<Null>" : _configuration.Server;
                Logger.TraceVerbose("Server: {0}", server);
                Debug.WriteLine("Server: {0}", server as object);

                Logger.TraceVerbose("OPTIONS:");
                Debug.WriteLine("OPTIONS:");

                foreach (ConfigurationParameter option in _configuration.Options)
                {
                    Logger.TraceVerbose("\t{0} = {1}", option.Name, option.Value);
                    Debug.WriteLine("\t{0} = {1}", option.Name, option.Value);
                }

                Logger.TraceVerbose("SECTIONS:");
                Debug.WriteLine("SECTIONS:");
                foreach (ConfigurationItem item in _configuration.Items)
                {
                    Logger.TraceVerbose("\t{0}", item.Name);
                    Logger.TraceVerbose("\tSelected = {0}", item.Selected);
                    Debug.WriteLine("\t{0}", item.Name as object);
                    Debug.WriteLine("\tSelected = {0}", item.Selected as object);
                    foreach (ConfigurationParameter option in item.Parameters)
                    {
                        Logger.TraceVerbose("\t\t{0} = {1}", option.Name, option.Value);
                        Debug.WriteLine("\t\t{0} = {1}", option.Name, option.Value);
                    }
                }

                Logger.TraceVerbose("********************************");
                Debug.WriteLine("********************************");
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine("EXCEPTION \n{0}", ex);
            }
        }

        private void ReportProgress(string message)
        {
            this.sbItem1.Content = message;
        }

        private void EndSchedule(string message)
        {
            ReportProgress(message);
            SetVisibility(true);

        }

        private void logonBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Password passwordDlg = new Password(_configuration)
                {
                    Owner = this
                };
                passwordDlg.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                Debug.WriteLine("EXCEPTION \n{0}", ex);
            }
        }

        private void validateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(SelServerName.Text))
            {
                string domain = GetRootDomainName(SelServerName.Text);
                if (!String.IsNullOrWhiteSpace(domain))
                {
                    SelForest.Text = domain;
                    _configuration.Validated = true;
                }
            }
            else if (!String.IsNullOrWhiteSpace(SelForest.Text))
            {
                string domain = GetRootDomainName(SelForest.Text);
                if (!String.IsNullOrWhiteSpace(domain))
                {
                    SelForest.Text = domain;
                    string server = GetServerName(domain);
                    if (!String.IsNullOrWhiteSpace(server))
                    {
                        SelServerName.Text = server;
                        _configuration.Validated = true;
                    }
                }
            }
            if(_configuration.Validated)
                ReportProgress("Validated");
        }

        private string GetRootDomainName(string serverName)
        {
            RootDSE rootDSE = new RootDSE(serverName);
            if (rootDSE != null)
            {
                LdapSearch cmpSearch = new LdapSearch(LdapReader.GetDirectoryEntry($"LDAP://{serverName}/CN=Partitions,{rootDSE.ConfigurationNamingContext}", _configuration.Login))
                {
                    Scope = SearchScope.OneLevel,
                    PropertiesToLoad = "dnsRoot",
                    filter = $"(nCName={rootDSE.RootDomainNamingContext})"
                };
                Data.SearchResult cmpResult = cmpSearch.GetOne();
                if (cmpResult != null)
                {
                    return cmpResult.GetPropertyString("dnsRoot");
                }
            }
            return null;
        }

        private string GetServerName(string domainName)
        {
            RootDSE rootDSE = new RootDSE(domainName);
            if (rootDSE != null)
            {
                return rootDSE.ServerName;
            }
            return null;
        }

        private void mnuLanguageOptions_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current is App app)
            {
                if (sender is System.Windows.Controls.MenuItem languageOptionsMenuItem)
                {
                    switch (languageOptionsMenuItem.Name)
                    {
                        case "EnglishMnu":
                            app.ChangeCulture("en-GB");
                            break;
                        case "CroatiaMnu":
                            app.ChangeCulture("hr-HR");
                            break;
                        case "SpainMnu":
                            app.ChangeCulture("es-ES");
                            break;
                    }

                }
            }
        }
    }
}