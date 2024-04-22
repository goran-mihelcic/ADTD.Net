using System.Windows;

namespace Mihelcic.Net.Visio.Diagrammer
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblVersion.Content = $"{Strings.VersionLbl} {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            lblDescription.Text = $"{Strings.DevelopedBy}: \nGoran Mihelčić, {Strings.MyTitle}, U.A.E.";
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
