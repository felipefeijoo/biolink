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

namespace BioLink.DatabaseUtilities {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {

        public LoginWindow() {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Connect();
        }

        private void Connect() {
            if (string.IsNullOrWhiteSpace(cmbServer.Text)) {
                ErrorMessage.Show("You must supply a database server or instance name!");
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbServer.Text)) {
                ErrorMessage.Show("You must supply a database server or instance name!");
                return;
            }

        }

        public Server Server { get; private set; }

    }
}
