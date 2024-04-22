using Mihelcic.Net.Visio.Common;
using System;
using System.Collections.Generic;
using System.Windows;


namespace Mihelcic.Net.Visio.Diagrammer
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        ApplicationConfiguration _configuration;

        IEnumerable<ConfigurationParameter> _items;
        public Options()
        {
            try
            {
                InitializeComponent();

            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
            }
        }

        public void SetConfiguration(ApplicationConfiguration configuration)
        {
            _configuration = configuration;
            _items = configuration.Options;
            this.grdItem.DataContext = _configuration;
            //this.grdTitle.DataContext = configuration;
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            _configuration.Save();
            this.Close();
        }
    }
}