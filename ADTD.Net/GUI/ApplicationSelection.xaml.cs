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
    /// Interaction logic for ApplicationSelection.xaml
    /// </summary>
    public partial class ApplicationSelection : UserControl
    {
        public bool Selected
        {
            get { return chkDrawApplications.IsChecked == true; }
        }

        public ApplicationSelection()
        {
            InitializeComponent();
        }
    }
}
