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
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Data;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.DatabaseUtilities {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {

        public LoginWindow(string serverName) {
            InitializeComponent();
            ServerName = serverName;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (Connect()) {
                this.DialogResult = true;
                this.Close();
            }
        }

        protected string ServerName { get; private set; }

        private bool Connect() {

            try {
                using (new OverrideCursor(Cursors.Wait)) {

                    ServerConnection conn = new ServerConnection(ServerName);
                    if (chkIntegratedSecurity.IsChecked == true) {
                        conn.LoginSecure = true;
                    } else {
                        conn.LoginSecure = false;
                        conn.Login = txtUsername.Text;
                        conn.Password = txtPassword.Password;
                    }

                    Server = new Server(conn);
                    Server.ConnectionContext.Connect();
                }
                return true;
            } catch (Exception ex) {
                ErrorMessage.Show(ex.Message);
                Server = null;
                return false;
            }


        }

        public Server Server { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

    }
}
