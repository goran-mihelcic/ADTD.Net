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
    /// Interaction logic for DFSRSelection.xaml
    /// </summary>
    public partial class DFSRSelection : UserControl
    {
        public bool Selected
        {
            get { return chkDrawDFSR.IsChecked == true; }
        }
        public DFSRSelection()
        {
            InitializeComponent();
        }

        //private void chkDrawDFSR_Checked(object sender, RoutedEventArgs e)
        //{
        //    if ((sender as CheckBox).IsChecked == false) 
        //    selDFSDomain.IsEnabled = false;
        //else
        //    selDFSDomain.IsEnabled = true;
        //}
    }
}
