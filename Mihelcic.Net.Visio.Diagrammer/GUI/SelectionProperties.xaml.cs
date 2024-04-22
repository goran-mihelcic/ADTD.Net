using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Mihelcic.Net.Visio.Diagrammer
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class SelectionProperties : UserControl
    {
        ApplicationConfiguration _configuration;

        IEnumerable<ConfigurationItem> _items;

        public IEnumerable<ConfigurationItem> Items { get { return _items; } set { _items = value; } }

        public SelectionProperties()
        {
            InitializeComponent();
        }

        public void SetConfiguration(ApplicationConfiguration configuration)
        {
            _configuration = configuration;
            _items = configuration.Items.Where(i=>i.Selected);
            this.grdDetail.DataContext = _items;
            //this.grdTitle.DataContext = configuration;
        }

        public void SetConfiguration()
        {
            _items = _configuration.Items.Where(i => i.Selected);
            this.grdDetail.DataContext = _items;
        }
    }
}
