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
    /// Interaction logic for OUSelection.xaml
    /// </summary>
    public partial class OUSelection : UserControl
    {
        public bool Selected
        {
            get { return chkDrawOUs.IsChecked == true; }
        }
        public OUSelection()
        {
            InitializeComponent();
        }

        private void chkAllOUs_Checked(object sender, RoutedEventArgs e)
        {
            if (selOUDomain != null)
                if (((CheckBox)sender).IsChecked == true)
                    selOUDomain.IsEnabled = false;
                else
                    selOUDomain.IsEnabled = true;
        }
    }
}
