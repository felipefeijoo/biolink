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
using System.Data.OleDb;
using System.Data;
using BioLink.Client.Utilities;
using System.IO;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ExcelImporterOptionsWindow.xaml
    /// </summary>
    public partial class ExcelImporterOptionsWindow : Window {

        public ExcelImporterOptionsWindow(ExcelImporterOptions options) {
            InitializeComponent();            
            if (options != null) {
                txtFilename.Text = options.Filename;
                cmbSheet.Text = options.Worksheet;
            }
            txtFilename.TextChanged += new TextChangedHandler(txtFilename_TextChanged);
        }

        void txtFilename_TextChanged(object source, string value) {
            // ListSheetNames(txtFilename.Text, true);
        }

        private void ListSheetNames(string filename, bool suppressErrorMessages) {
            if (!string.IsNullOrEmpty(filename)) {
                using (new OverrideCursor(Cursors.Wait)) {
                    List<String> sheetNames = null;
                    sheetNames = ExcelImporter.GetExcelSheetNames(filename, true);
                    cmbSheet.InvokeIfRequired(() => {
                        cmbSheet.ItemsSource = sheetNames;
                        if (sheetNames != null && sheetNames.Count > 0) {
                            cmbSheet.Text = sheetNames[0];
                        }
                    });
                }
            } else {
                cmbSheet.ItemsSource = null;
            }
        }

        public String Filename {
            get { return txtFilename.Text; }            
        }

        public String Worksheet {
            get { return cmbSheet.Text; }
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e) {
            Preview();
        }

        private void Preview() {
            using (new OverrideCursor(Cursors.Wait)) {
                ExcelImporter.WithWorksheetDataTable(Filename, String.Format("SELECT TOP 50 * FROM [{0}]", Worksheet), dt => {
                    previewGrid.ItemsSource = dt.DefaultView;
                });
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            ListSheetNames(txtFilename.Text, false);
        }

        private bool Validate() {

            if (string.IsNullOrWhiteSpace(Filename)) {
                ErrorMessage.Show("You must supply a filename!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Worksheet)) {
                ErrorMessage.Show("You must supply a worksheet name!");
                return false;
            }

            if (!File.Exists(Filename)) {
                ErrorMessage.Show("File does not exist!");
                return false;
            }

            return true;
        }

        private void button4_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            SystemUtils.ShellExecute(txtFilename.Text);
        }

    }
}
