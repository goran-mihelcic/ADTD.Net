using System.Windows;

namespace Mihelcic.Net.Visio.Diagrammer
{
    /// <summary>
    /// Interaction logic for AllStatuses.xaml
    /// </summary>
    public partial class AllStatuses : Window
    {
        Statuses Statuses { get; set; }

        public AllStatuses(Statuses statuses)
        {
            this.Statuses = statuses;
            InitializeComponent();
            this.DataContext = this.Statuses;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
