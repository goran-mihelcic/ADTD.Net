using Mihelcic.Net.Visio.Diagrammer;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace ADTD.Net
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //ChangeCulture("hr-HR");
        }

        public void ChangeCulture(string cultureCode)
        {
            CultureInfo culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Optionally, refresh views if needed
            RefreshAllViews();
        }

        private void RefreshAllViews()
        {
            Window currentWindow = Application.Current.MainWindow;

            // Create a new instance of the window
            MainWindow newWindow = new MainWindow();

            // Show the new window
            newWindow.Show();

            // Update the Application.Current.MainWindow to the new window
            Application.Current.MainWindow = newWindow;

            // Close the old window if it's not the same as the new window
            if (currentWindow != null && currentWindow != newWindow)
            {
                currentWindow.Close();
            }
        }
    }
}
