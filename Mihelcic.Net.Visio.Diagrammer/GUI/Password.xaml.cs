using Mihelcic.Net.Visio.Data;
using System;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

namespace Mihelcic.Net.Visio.Diagrammer
{
    /// <summary>
    /// Interaction logic for Password.xaml
    /// </summary>
    public partial class Password : Window
    {
        string _dcName;
        ApplicationConfiguration _configuration;

        public bool ValidCredentials { get; set; }
        public Password(ApplicationConfiguration configuration)
        {
            if (configuration == null || (String.IsNullOrWhiteSpace(configuration.Server) && String.IsNullOrWhiteSpace(configuration.DnsForestName)))
                throw new ArgumentException("Missing server or forest data", "configuration");
            InitializeComponent();
            _configuration = configuration;
            _dcName = String.IsNullOrWhiteSpace(_configuration.Server) ? _configuration.DnsForestName : _configuration.Server;
            txtUserName.Text = _configuration.Login.UserName;
            txtDomain.Text = String.IsNullOrWhiteSpace(_configuration.Login.UserDomain) ? _configuration.DnsForestName : _configuration.Login.UserDomain;
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUserName.Text;
            SecureString password = txtPassword.SecurePassword;
            string domain = txtDomain.Text;

            if (LoginInfo.ValidateCredentials(_dcName, username, password, domain))
            {
                _configuration.Login.UserName = username;
                _configuration.Login.UserPassword = password;
                _configuration.Login.UserDomain = domain;
                _configuration.Login.Validated = true;
            }
            else
            {
                MessageBox.Show("Invalid credentials.");
                _configuration.Login.Validated = false;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
