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

        private char _textQualifier = '\"';

        public CSVImportOptionsWindow(CSVImporterOptions options) {
            InitializeComponent();

            cmbDelimiter.ItemsSource = _delimiterOptions;

            if (options != null) {
                txtFilename.Text = options.Filename;
                cmbDelimiter.Text = options.Delimiter;
                chkFirstRowNames.IsChecked = options.FirstRowContainsNames;
            } else {
                cmbDelimiter.Text = ",";
                chkFirstRowNames.IsChecked = true;
            }

            this.Closed += new EventHandler(CSVImportOptionsWindow_Closed);
        }

        void CSVImportOptionsWindow_Closed(object sender, EventArgs e) {
            this.previewGrid.DataContext = null;
            this.previewGrid.ItemsSource = null;            
        }

        private void txtFilename_Loaded(object sender, RoutedEventArgs e) {
        }

        public string Filename {
            get { return txtFilename.Text; }
        }

        public String Delimiter {
            get { return HttpUtility.HtmlDecode(cmbDelimiter.Text as string); }
        }

        public List<string> ColumnNames { get; private set; }

        public bool IsFirstRowContainNames {
            get { return chkFirstRowNames.IsChecked.GetValueOrDefault(false);  }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                this.DialogResult = true;
                this.Close();
            }
        }

        private bool Validate() {

            if (string.IsNullOrWhiteSpace(txtFilename.Text)) {
                ErrorMessage.Show("You must specify an input file!");
                return false;
            }

            if (!File.Exists(txtFilename.Text)) {
                ErrorMessage.Show("File does not exist!");
                return false;
            }

            // Extract Column Headers...
            ColumnNames = new List<String>();

            GenericParser parser = null;
            try {
                using (parser = new GenericParserAdapter(Filename)) {
                    parser.ColumnDelimiter = Delimiter.ToCharArray()[0];
                    parser.FirstRowHasHeader = IsFirstRowContainNames;
                    parser.MaxRows = 2;
                    parser.TextQualifier = _textQualifier;

                    if (parser.Read()) {
                        for (int i = 0; i < parser.ColumnCount; ++i) {
                            if (IsFirstRowContainNames) {
                                ColumnNames.Add(parser.GetColumnName(i));
                            } else {
                                ColumnNames.Add("Column" + i);
                            }
                        }
                    } else {
                        ErrorMessage.Show("Failed to extract column names from data source!");
                        return false;
                    }
                }
            } finally {
                if (parser != null) {
                    System.GC.SuppressFinalize(parser);
                }
            }

            return true;
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e) {
            Preview();
        }

        private void Preview() {

            GenericParserAdapter parser = null;
            try {
                using (parser = new GenericParserAdapter(Filename)) {
                    parser.ColumnDelimiter = Delimiter.ToCharArray()[0];
                    parser.FirstRowHasHeader = IsFirstRowContainNames;
                    parser.MaxRows = 50;
                    parser.TextQualifier = '\"';
                    var ds = parser.GetDataSet();
                    previewGrid.AutoGenerateColumns = true;
                    previewGrid.ItemsSource = ds.Tables[0].DefaultView;
                    System.GC.SuppressFinalize(parser);
                }
            } finally {
                if (parser != null) {
                    GC.SuppressFinalize(parser);
                }
            }
        }

        private void btnViewFile_Click(object sender, RoutedEventArgs e) {
            SystemUtils.ShellExecute(txtFilename.Text);
        }

    }
}
