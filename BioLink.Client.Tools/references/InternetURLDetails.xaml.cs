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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Utilities;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for InternetURLDetails.xaml
    /// </summary>
    public partial class InternetURLDetails : UserControl {
        public InternetURLDetails() {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e) {
            try {
                string urlString = txtSource.Text;
                if (!String.IsNullOrEmpty(urlString)) {
                    Uri url = new Uri(urlString, UriKind.Absolute);
                    if (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps) {
                        System.Diagnostics.Process.Start(txtSource.Text);
                    } else {
                        ErrorMessage.Show("{0} does not appear to be a valid HTTP URL.", txtSource.Text);
                    }
                }
            } catch (Exception ex) {
                ErrorMessage.Show("Unable to browse to '{0}' : {1}", txtSource.Text, ex.Message);
            }
        }
    }
}
