using System.Windows;
using System.Windows.Controls;

namespace Mihelcic.Net.Visio.Diagrammer
{
    /// <summary>
    /// Interaction logic for Selection.xaml
    /// </summary>
    public delegate void UpdateSettings();

    public partial class Selection : UserControl
    {
        public UpdateSettings DoUpdate;
        public Selection()
        {
            InitializeComponent();
        }

        public void SetConfiguration(ApplicationConfiguration configuration)
        {
            this.grdDetail.DataContext = configuration.Items;
            this.grdTitle.DataContext = configuration;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.BindingGroup.UpdateSources();
            DoUpdate();
        }
    }
}
