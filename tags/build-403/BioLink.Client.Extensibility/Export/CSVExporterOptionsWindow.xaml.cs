using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using BioLink.Client.Utilities;
using System.Text;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility.Export {
    /// <summary>
    /// Interaction logic for CSVExporterOptions.xaml
    /// </summary>
    public partial class CSVExporterOptionsWindow : Window {

        public CSVExporterOptionsWindow() {
            InitializeComponent();
            List<DelimiterItem> model = new List<DelimiterItem>();
            model.Add( new DelimiterItem(",", ","));
            model.Add( new DelimiterItem("Tab", "\t"));
            model.Add(new DelimiterItem("|", "|"));
            model.Add(new DelimiterItem(";", ";"));

            cmbDelimiter.Items.Clear();
            cmbDelimiter.ItemsSource = model;
            cmbDelimiter.SelectedIndex = 0;

            var encodings = new ObservableCollection<Encoding>();

            encodings.Add(Encoding.GetEncoding(1252));
            encodings.Add(Encoding.UTF8);
            encodings.Add(Encoding.UTF7);
            encodings.Add(Encoding.UTF32);
            encodings.Add(Encoding.ASCII);
            encodings.Add(Encoding.BigEndianUnicode);
            encodings.Add(Encoding.Unicode);

            cmbEncoding.ItemsSource = encodings;
            cmbEncoding.SelectedIndex = 0;

        }

        public CSVExporterOptions Options { 
            get { 
                var item = cmbDelimiter.SelectedItem as DelimiterItem;
                var options = new CSVExporterOptions();
                options.Delimiter = (item == null ? cmbDelimiter.Text : item.Value);
                options.ColumnHeadersAsFirstRow = chkFirstRowHeaders.IsChecked.GetValueOrDefault(false);
                options.Filename = txtFilename.Text;
                options.QuoteValues = chkQuoteValues.IsChecked.GetValueOrDefault(false);
                options.Encoding = cmbEncoding.SelectedItem as Encoding;
                return options;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {

            if (String.IsNullOrEmpty(txtFilename.Text)) {
                ErrorMessage.Show("You must select a file to export to!");
                return;
            }

            this.DialogResult = true;
            this.Hide();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Export"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.OverwritePrompt = false;
            dlg.Filter = "Text documents (.txt)|*.txt|All files (*.*)|*.*"; // Filter files by extension            
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {
                
                txtFilename.Text = dlg.FileName;
            }
        }

        private void btnCodePage_Click(object sender, RoutedEventArgs e) {
            InputBox.Show(this, "Custom code page encoding", "Please enter the Unicode code page number that you would like to use during the export", (value) => {
                if (value.IsInteger()) {
                    var codepage = Int32.Parse(value);
                    if (codepage < 0 || codepage > Int16.MaxValue) {
                        ErrorMessage.Show("Code page numbers must be between 0 and " + Int16.MaxValue + "!");
                        return;
                    }
                    var encoding = Encoding.GetEncoding(codepage);
                    if (encoding == null) {
                        ErrorMessage.Show("Unrecognized encoding: " + codepage);
                        return;
                    }

                    var model = cmbEncoding.ItemsSource as ObservableCollection<Encoding>;
                    if (model != null) {
                        model.Add(encoding);
                        cmbEncoding.SelectedItem = encoding;
                    }

                } else {
                    ErrorMessage.Show("You must enter an integer code page!");
                }
            });
        }
    }

    public class DelimiterItem {

        public DelimiterItem(string display, string value) {
            DisplayName = display;
            Value = value;
        }

        public string DisplayName { get; set; }
        public string Value { get; set; }
    }

    public class CSVExporterOptions {
        public string Filename { get; set; }
        public string Delimiter { get; set; }
        public bool ColumnHeadersAsFirstRow { get; set; }
        public bool QuoteValues { get; set; }
        public Encoding Encoding { get; set; }
    }
}
