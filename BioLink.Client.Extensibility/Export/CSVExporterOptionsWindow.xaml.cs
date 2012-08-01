/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
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
            var model = new List<DelimiterItem> {
                new DelimiterItem(",", ","),
                new DelimiterItem("Tab", "\t"),
                new DelimiterItem("|", "|"),
                new DelimiterItem(";", ";")
            };

            cmbDelimiter.Items.Clear();
            cmbDelimiter.ItemsSource = model;
            cmbDelimiter.SelectedIndex = 0;

            var encodings = new ObservableCollection<Encoding> {
                Encoding.GetEncoding(1252),
                Encoding.UTF8,
                Encoding.UTF7,
                Encoding.UTF32,
                Encoding.ASCII,
                Encoding.BigEndianUnicode,
                Encoding.Unicode
            };

            cmbEncoding.ItemsSource = encodings;
            cmbEncoding.SelectedIndex = 0;

        }

        public CSVExporterOptions Options {
            get {
                var item = cmbDelimiter.SelectedItem as DelimiterItem;
                var options = new CSVExporterOptions {
                    Delimiter = (item == null ? cmbDelimiter.Text : item.Value),
                    ColumnHeadersAsFirstRow = chkFirstRowHeaders.IsChecked.GetValueOrDefault(false),
                    Filename = txtFilename.Text,
                    QuoteValues = chkQuoteValues.IsChecked.GetValueOrDefault(false),
                    Encoding = cmbEncoding.SelectedItem as Encoding,
                    EscapeSpecial = chkEscapeSpecial.IsChecked.GetValueOrDefault(true)
                };
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
            var dlg = new Microsoft.Win32.SaveFileDialog {
                FileName = "Export",
                DefaultExt = ".txt",
                OverwritePrompt = false,
                Filter = "Text documents (.txt)|*.txt|All files (*.*)|*.*"
            };

            var result = dlg.ShowDialog();
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
        public bool EscapeSpecial { get; set; }

        public Encoding Encoding { get; set; }
    }
}
