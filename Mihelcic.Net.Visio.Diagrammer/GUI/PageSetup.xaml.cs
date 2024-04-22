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
using System.Windows.Shapes;

namespace Microsoft.ADTD.Net
{
    /// <summary>
    /// Interaction logic for PageSetup.xaml
    /// </summary>
    public partial class PageSetup : Window
    {
        public PageSetup()
        {
            InitializeComponent();
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Page Setup.
            this.btnLandscape.IsChecked = Properties.Settings.Default.LandscapeState;
            this.btnPortrait.IsChecked = !Properties.Settings.Default.LandscapeState;

            this.cboPageSize.Items.Add("(Automatic)"); // psAutomatic
            this.cboPageSize.Items.Add("A: 11in x 8.5"); // psA
            this.cboPageSize.Items.Add("B: 17in x 11in"); // psB
            this.cboPageSize.Items.Add("C: 22in x 17in"); //  psC
            this.cboPageSize.Items.Add("D: 34in x 22in"); // psD
            this.cboPageSize.Items.Add("E: 44in x 34in"); // psE
            this.cboPageSize.SelectedIndex = Properties.Settings.Default.PageSizeState;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.LandscapeState = (bool)this.btnLandscape.IsChecked;
            Properties.Settings.Default.PageSizeState = this.cboPageSize.SelectedIndex;
        }
    }
}
