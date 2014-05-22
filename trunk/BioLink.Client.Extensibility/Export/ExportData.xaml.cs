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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ExportData.xaml
    /// </summary>
    public partial class ExportData : Window {

        private DataMatrix _data;
        private string _dataSetName;
        private IProgressObserver _progress;

        #region designer ctor
        public ExportData() {
            InitializeComponent();
        }
        #endregion

        public ExportData(DataMatrix data, string dataSetName, IProgressObserver progress) {
            InitializeComponent();
            _data = data;
            _dataSetName = dataSetName;
            _progress = progress;
            var candidates = PluginManager.Instance.GetExtensionsOfType<TabularDataExporter>();
            var exporters = candidates.FindAll((exporter) => {
                return exporter.CanExport(data);
            });

            listBox.ItemsSource = exporters;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (listBox.SelectedItem != null) {
                btnOk.IsEnabled = true;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {            
            ExportUsingSelectedExporter();            
        }

        private void ExportUsingSelectedExporter() {
            TabularDataExporter exporter = listBox.SelectedItem as TabularDataExporter;
            exporter.Export(this.Owner, _data, _dataSetName, _progress);
            this.DialogResult = true;
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (listBox.SelectedItem != null) {
                ExportUsingSelectedExporter();
            }
        }

    }
}
