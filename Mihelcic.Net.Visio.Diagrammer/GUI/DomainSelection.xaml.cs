using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.ADTD.Data;

namespace Microsoft.ADTD.Net
{
    /// <summary>
    /// Interaction logic for DomainSelection.xaml
    /// </summary>
    public partial class DomainSelection : UserControl
    {
        public bool Selected
        {
            get { return chkDrawDomains.IsChecked == true; }
        }
        public DomainSelection()
        {
            InitializeComponent();
        }

        private void optDNSMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)(sender as RadioButton).IsChecked && optGCMode != null)
                {
                    optGCMode.IsChecked = false;
                    selDomain.IsEnabled = true;
                    chkWindows2000Trust.IsEnabled = true;
                    chkDownlevelTrusts.IsChecked = Properties.Settings.Default.Dom_DownLevelTrustState;
                    chkCrossForestTrust.IsChecked = Properties.Settings.Default.Dom_CrossForestTrustState;
                    chkDownlevelTrusts.IsEnabled = true;
                    chkCrossForestTrust.IsEnabled = true;
                    chkDrawExternalDomainDetails.IsEnabled = true;
                    //chkCountUsers.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex.ToString());
            }
        }

        private void optGCMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)(sender as RadioButton).IsChecked && optGCMode != null)
                {
                    selDomain.Text = "Draw entire Active Directory Structure";
                    selDomain.IsEnabled = false;
                    chkWindows2000Trust.IsEnabled = true;
                    chkDownlevelTrusts.IsChecked = false;
                    chkDownlevelTrusts.IsEnabled = false;
                    chkCrossForestTrust.IsChecked = false;
                    chkCrossForestTrust.IsEnabled = false;
                    chkDrawExternalDomainDetails.IsChecked = false;
                    chkDrawExternalDomainDetails.IsEnabled = false;
                    optDNSMode.IsChecked = false;
                    //chkCountUsers.IsEnabled = false;
                    //chkCountUsers.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex.ToString());
            }
        }

        private void selDomain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (chkCrossForestTrust != null)
                {
                    if (this.selDomain.Text != "Draw entire Active Directory Structure")
                    {
                        chkCrossForestTrust.IsChecked = false;
                        chkCrossForestTrust.IsEnabled = false;
                        chkWindows2000Trust.IsEnabled = true;
                        chkWindows2000Trust.IsChecked = false;
                    }
                    else
                    {
                        chkCrossForestTrust.IsChecked = Properties.Settings.Default.Dom_CrossForestTrustState;
                        chkCrossForestTrust.IsEnabled = true;
                        chkWindows2000Trust.IsEnabled = false;
                        chkWindows2000Trust.IsChecked = Properties.Settings.Default.Dom_Windows2000TrustState;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex.ToString());
            }
        }

        private void chkAllDomains_Checked(object sender, RoutedEventArgs e)
        {
            if (selDomain != null)
                if (((CheckBox)sender).IsChecked == true)
                {
                    selDomain.IsEnabled = false;
                    chkCrossForestTrust.IsEnabled = true;
                }
                else
                {
                    selDomain.IsEnabled = true;
                    chkCrossForestTrust.IsEnabled = false;
                    chkCrossForestTrust.IsChecked = false;
                }
        }
    }
}
