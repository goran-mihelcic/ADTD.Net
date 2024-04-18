using Mihelcic.Net.Visio.Common;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mihelcic.Net.Visio.Diagrammer
{
    /// <summary>
    /// Interaction logic for ItemControl.xaml
    /// </summary>
    public partial class ItemControl : UserControl
    {
        public string Type { get; set; }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
      "Type", typeof(string), typeof(ItemControl), new PropertyMetadata("Boolean"));

        public ItemControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigurationParameter parameter = (ConfigurationParameter)this.DataContext;
            this.Type = parameter.Type;

            Binding valueBinding = new Binding("Value");
            valueBinding.Mode = BindingMode.TwoWay;
            valueBinding.Source = this.DataContext;
            valueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            UIElement lineControll = null;

            switch (Type)
            {
                case "Boolean":
                    CheckBox cBox = new CheckBox();
                    cBox.SetBinding(CheckBox.ContentProperty, "Title");
                    cBox.SetBinding(CheckBox.ToolTipProperty, "ToolTip");
                    cBox.SetBinding(CheckBox.IsCheckedProperty, valueBinding);
                    lineControll = cBox;
                    break;
                default:
                    StackPanel panel = new StackPanel();
                    panel.Orientation = Orientation.Horizontal;
                    Label label = new Label();
                    label.SetBinding(Label.ContentProperty, "Title");
                    TextBox tBox = new TextBox();
                    tBox.SetBinding(TextBox.ToolTipProperty, "ToolTip");
                    tBox.SetBinding(TextBox.TextProperty, valueBinding);
                    tBox.VerticalAlignment = VerticalAlignment.Center;

                    tBox.Width = 300;
                    Button btnBrowse = null;
                    if (parameter.Picker)
                    {
                        btnBrowse = new Button();
                        btnBrowse.Content = Strings.Browse;
                        btnBrowse.Click += btnBrowse_Click;
                        btnBrowse.Height = 18;
                    }
                    panel.Children.Add(label);
                    panel.Children.Add(tBox);
                    if (parameter.Picker)
                        panel.Children.Add(btnBrowse);
                    lineControll = panel;
                    break;
            }

            this.MyControl.Children.Add(lineControll);

        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                StackPanel panel = (StackPanel)btn.Parent;
                TextBox tBox = (TextBox)panel.Children[1];
                string text = tBox.Text;
                string path = Logger.ParsePath(text);
                bool isFolder = Directory.Exists(path);

                if (isFolder)
                {
                    using (System.Windows.Forms.FolderBrowserDialog dlgBrowseOpen = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        dlgBrowseOpen.SelectedPath = path;
                        dlgBrowseOpen.ShowNewFolderButton = true;
                        if (dlgBrowseOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            tBox.Text = dlgBrowseOpen.SelectedPath;
                    }
                }
                else
                {
                    OpenFileDialog dlgBrowseOpen = new OpenFileDialog();
                    dlgBrowseOpen.Filter = ((ConfigurationParameter)btn.DataContext).PickerFilter; //"Visio Drawings (*.vsdx)|*.vsdx";
                    dlgBrowseOpen.AddExtension = true;
                    dlgBrowseOpen.CheckFileExists = false;

                    dlgBrowseOpen.InitialDirectory = System.IO.Path.GetDirectoryName(path);
                    dlgBrowseOpen.FileName = System.IO.Path.GetFileName(path);
                    // Display the Open dialog box
                    if (dlgBrowseOpen.ShowDialog() == true)
                        tBox.Text = dlgBrowseOpen.FileName;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
            }
        }

        private void UserControl_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }
    }
}
