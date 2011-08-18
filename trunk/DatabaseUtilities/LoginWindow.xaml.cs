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

        public LoginWindow() {
            InitializeComponent();

            Loaded += new RoutedEventHandler(LoginWindow_Loaded);
        }

        void LoginWindow_Loaded(object sender, RoutedEventArgs e) {
            cmbServer.ItemsSource = GetProfileServers();
        }

        private List<string> GetProfileServers() {
            var list = new List<string>();

            var profiles = Config.GetGlobal<List<ConnectionProfile>>("connection.profiles", new List<ConnectionProfile>());
            foreach (ConnectionProfile profile in profiles) {
                if (!list.Contains(profile.Server, StringComparer.CurrentCultureIgnoreCase)) {
                    list.Add(profile.Server);
                }
            }

            LegacySettings.TraverseSubKeys("Client", "UserProfiles", (key) => {
                ConnectionProfile profile = new ConnectionProfile();
                var server = key.GetValue("DatabaseServer") as string;
                if (server != null && !list.Contains(server, StringComparer.CurrentCultureIgnoreCase)) {
                    list.Add(server);

                }
            });

            return list;
        }

        private void StatusMessage(string message) {
            lblMessage.InvokeIfRequired(() => {
                lblMessage.Content = message;
            });
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (Connect()) {
                this.Close();
            }
        }

        private bool Connect() {
            if (string.IsNullOrWhiteSpace(cmbServer.Text)) {
                ErrorMessage.Show("You must supply a database server or instance name!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmbServer.Text)) {
                ErrorMessage.Show("You must supply a database server or instance name!");
                return false;
            }

            ServerConnection conn = new ServerConnection(cmbServer.SelectedItem as string);
            if (chkIntegratedSecurity.IsChecked == true) {
                conn.LoginSecure = false;
            } else {
                conn.Login = txtUsername.Text;
                conn.Password = txtPassword.Password;
                conn.LoginSecure = false;
            }

            try {
                Server svr = new Server(conn);
                return true;
            } catch (Exception ex) {
                StatusMessage(ex.Message);
                return false;
            }

        }

        public Server Server { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {

            DoSearch();
        }

        private void DoSearch() {

            JobExecutor.QueueJob(() => {

                try {
                    
                    lblMessage.InvokeIfRequired(() => {
                        cmbServer.IsEnabled = false;
                        txtPassword.IsEnabled = false;
                        txtUsername.IsEnabled = false;
                        btnOk.IsEnabled = false;
                        btnCancel.IsEnabled = false;
                    });
                    StatusMessage("Searching for servers...");
                    DataTable dt = SmoApplication.EnumAvailableSqlServers(false);
                    var names = GetProfileServers();
                    if (dt.Rows.Count > 0) {
                        foreach (DataRow dr in dt.Rows) {
                            var name = dr["Name"] as string;
                            if (!string.IsNullOrEmpty(name) && !names.Contains(name, StringComparer.CurrentCultureIgnoreCase)) {
                                names.Add(name);
                            }
                        }
                    }
                    cmbServer.InvokeIfRequired(() => {
                        cmbServer.ItemsSource = names;
                        if (names.Count > 0) {
                            cmbServer.SelectedIndex = 0;
                        }
                    });
                    
                } catch (Exception ex) {
                    ErrorMessage.Show(ex.Message);
                } finally {
                    lblMessage.InvokeIfRequired(() => {
                        cmbServer.IsEnabled = true;
                        txtPassword.IsEnabled = true;
                        txtUsername.IsEnabled = true;
                        btnOk.IsEnabled = true;
                        btnCancel.IsEnabled = true;
                    });
                }
            });

        }

    }
}
