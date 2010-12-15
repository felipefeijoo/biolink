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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for InternetURLDetails.xaml
    /// </summary>
    public partial class InternetURLDetails : UserControl {
        public InternetURLDetails() {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start(txtSource.Text);
        }
    }
}
