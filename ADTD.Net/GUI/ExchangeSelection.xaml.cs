using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.ADTD.Data;

namespace Microsoft.ADTD.Net
{
    /// <summary>
    /// Interaction logic for ExchangeSelection.xaml
    /// </summary>
    public partial class ExchangeSelection : UserControl
    {
        public ExchangeSelection()
        {
            InitializeComponent();
        }

        private void chkUseADSites_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as CheckBox).IsChecked == true)
                    MessageBox.Show("By selecting this feature, ADTD must read all defined subnetworks from the Active Directory. The time needed for this operation depends on the number of subnets in your Active Directory forest.", "Warning !", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex.ToString());
            }
        }

    }
}
