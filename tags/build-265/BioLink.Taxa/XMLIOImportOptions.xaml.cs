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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.IO;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for XMLIOImportOptions.xaml
    /// </summary>
    public partial class XMLIOImportOptions : Window {

        public XMLIOImportOptions() {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartImport();
        }

        private void StartImport() {
            string filename = txtFilename.Text;
            if (string.IsNullOrWhiteSpace(filename)) {
                ErrorMessage.Show("You must supply the name of an XML file to import from");
                return;
            }

            if (!File.Exists(filename)) {
                ErrorMessage.Show("The specified file does not exist!");
                return;
            }

        }
    }
}
