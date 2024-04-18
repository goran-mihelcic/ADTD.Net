using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.ADTD.Net
{
    /// <summary>
    /// Interaction logic for SiteSelection.xaml
    /// </summary>
    public partial class SiteSelection : UserControl
    {
        public bool Selected
        {
            get { return chkDrawSites.IsChecked == true; }
        }
        public SiteSelection()
        {
            InitializeComponent();
        }

        private void chkAllSites_Checked(object sender, RoutedEventArgs e)
        {
            if (selSite != null)
                if (((CheckBox)sender).IsChecked == true)
                {
                    selSite.IsEnabled = false;
                    chkInterReplCon.IsEnabled = true;
                }
                else
                {
                    selSite.IsEnabled = true;
                    chkInterReplCon.IsEnabled = false;
                    chkInterReplCon.IsChecked= false;
                }
        }
    }
}
