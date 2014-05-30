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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using Microsoft.Win32;


namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ImportFilterSelection.xaml
    /// </summary>
    public partial class ImportFilterSelection : WizardPage {

        private List<TabularDataImporter> _model;

        public ImportFilterSelection() {
            InitializeComponent();

            _model = PluginManager.Instance.GetExtensionsOfType<TabularDataImporter>();
            listBox.ItemsSource = _model;
            listBox.SelectedIndex = 0;
        }

        public override string PageTitle {
            get { return "Select data source"; }
        }

        public override bool CanMoveNext() {
            return listBox.SelectedItem != null;
        }
        
        public override bool OnPageExit(WizardDirection todirection) {
            var selected = listBox.SelectedItem as TabularDataImporter;
            if (selected != null) {
                var c = WizardContext as ImportWizardContext;
                if (c != null) {
                    if (selected.GetOptions(this.FindParentWindow(), c)) {                    
                        c.Importer = selected;                        
                        return true;
                    }
                }
            }
            return false;
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (listBox.SelectedItem != null) {
                RaiseRequestNextPage();
            }
        }

        private void btnLoadTemplate_Click(object sender, RoutedEventArgs e) {
            LoadTemplate();
        }

        protected ImportWizardContext ImportContext { get { return WizardContext as ImportWizardContext; } }

        private void LoadTemplate() {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Import Template Files (*.bip)|*.bip|All files (*.*)|*.*";
            if (dlg.ShowDialog().GetValueOrDefault()) {
                var inifile = new IniFile();
                inifile.Load(dlg.FileName);

                var filtername = inifile.GetValue("Import", "ImportFilter");

                if (string.IsNullOrWhiteSpace(filtername)) {
                    ErrorMessage.Show("Invalid import profile - the name of import filter could not be read!");
                    return;
                }

                var importer = _model.Find((filter) => {
                    return filter.Name.Equals(filtername, StringComparison.CurrentCultureIgnoreCase);
                });

                if (importer == null) {
                    ErrorMessage.Show("Invalid import profile - the unrecognized or unloaded import filter: " + filtername);
                    return;
                }

                var epStr = inifile.GetValue("Import", "ProfileStr", "");

                if (string.IsNullOrWhiteSpace(epStr)) {
                    ErrorMessage.Show("Invalid import profile - No profile string specified.");
                    return;
                }

                importer.InitFromProfileString(epStr);
                ImportContext.Importer = importer;

                listBox.SelectedItem = importer;

                var mappings = new List<ImportFieldMapping>();

                // Now the mappings...
                var colCount = inifile.GetInt("Import", "FieldCount");
                for (int i = 0; i < colCount; ++i) {
                    String fieldKey = string.Format("Field{0}", i);
                    var mapStr = inifile.GetValue("Mappings", fieldKey);
                    if (!string.IsNullOrWhiteSpace(mapStr)) {
                        var mapEp = EntryPoint.Parse(mapStr);
                        var mapping = new ImportFieldMapping { SourceColumn = mapEp.Name, TargetColumn = mapEp["Mapping"], DefaultValue = mapEp["Default", null], IsFixed = Boolean.Parse(mapEp["IsFixed", "false"]) };
                        mappings.Add(mapping);

                        var transformStr = inifile.GetValue("Transformations", fieldKey);
                        if (!string.IsNullOrWhiteSpace(transformStr)) {
                            mapping.Transformer = TransformationPipline.BuildFromState(transformStr);
                        }

                    }

                }
                ImportContext.FieldMappings = mappings;

                RaiseRequestNextPage();

            }
            
        }

    }
}
