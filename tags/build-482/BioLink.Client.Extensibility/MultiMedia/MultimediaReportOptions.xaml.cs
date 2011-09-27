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
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for MultimediaReportOptions.xaml
    /// </summary>
    public partial class MultimediaReportOptions : Window {

        private List<string> _extensions;
        private List<string> _multimediaTypes;

        public MultimediaReportOptions() {

            InitializeComponent();

            var user = PluginManager.Instance.User;

            var service = new SupportService(user);


            _extensions = service.GetMultimediaExtensions();
            var types = service.GetMultimediaTypes();
            _extensions.Insert(0, "(All)");

            _multimediaTypes = new List<string>();
            _multimediaTypes.Add("(All)");
            foreach (MultimediaType type in types) {
                if (!string.IsNullOrWhiteSpace(type.Name)) {
                    _multimediaTypes.Add(type.Name);
                }
            }

            cmbExtension.ItemsSource = _extensions;
            cmbExtension.SelectedIndex = 0;
            cmbType.ItemsSource = _multimediaTypes;
            cmbType.SelectedIndex = 0;

        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Close();
        }

        public bool Recurse {
            get { return chkRecurse.IsChecked.ValueOrFalse(); }
        }

        public string ExtensionFilter {
            get { 
                var filter = cmbExtension.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(filter) || filter.Equals("(all)", StringComparison.CurrentCultureIgnoreCase)) {
                    return "";
                }
                return filter;
            }
        }

        public string TypeFilter {
            get {
                var filter = cmbType.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(filter) || filter.Equals("(all)", StringComparison.CurrentCultureIgnoreCase)) {
                    return "";
                }
                return filter;
            }
        }

        public bool IncludeMaterial {
            get {
                return chkIncludeSites.IsChecked == true;
            }
        }


    }
}
