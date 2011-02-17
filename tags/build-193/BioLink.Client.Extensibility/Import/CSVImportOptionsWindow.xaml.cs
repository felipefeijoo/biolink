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
using System.IO;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Web;
using System.Data;
using System.Data.OleDb;
using GenericParsing;

namespace BioLink.Client.Extensibility.Import {
    /// <summary>
    /// Interaction logic for CSVImportOptionsWindow.xaml
    /// </summary>
    public partial class CSVImportOptionsWindow : Window {

        private String[] _delimiterOptions = new String[] { ",", ";", "|", "&#9;" };

        public CSVImportOptionsWindow(CSVImporterOptions options) {
            InitializeComponent();

            cmbDelimiter.ItemsSource = _delimiterOptions;

            if (options != null) {
                txtFilename.Text = options.Filename;
                cmbDelimiter.Text = options.Delimiter;
                chkFirstRowNames.IsChecked = options.FirstRowContainsNames;
            }
        }

        private void txtFilename_Loaded(object sender, RoutedEventArgs e) {
        }

        public string Filename {
            get { return txtFilename.Text; }
        }

        public String Delimiter {
            get { return HttpUtility.HtmlDecode(cmbDelimiter.Text as string); }
        }

        public bool IsFirstRowContainNames {
            get { return chkFirstRowNames.IsChecked.GetValueOrDefault(false);  }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                this.DialogResult = true;
                this.Hide();
            }
        }

        private bool Validate() {
            if (!File.Exists(txtFilename.Text)) {
                ErrorMessage.Show("File does not exist!");
                return false;
            }

            return true;
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e) {
            Preview();
        }

        private void Preview() {

            using (var parser = new GenericParserAdapter(Filename)) {
                parser.ColumnDelimiter = Delimiter.ToCharArray()[0];
                parser.FirstRowHasHeader = IsFirstRowContainNames;
                parser.MaxRows = 50;
                parser.TextQualifier = '\"';
                var ds = parser.GetDataSet();
                previewGrid.AutoGenerateColumns = true;                

                previewGrid.ItemsSource = ds.Tables[0].DefaultView;
            }
        }

    }
}
