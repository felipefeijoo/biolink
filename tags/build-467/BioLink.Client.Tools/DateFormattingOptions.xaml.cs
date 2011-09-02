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
using Microsoft.VisualBasic;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for DateFormattingOptions.xaml
    /// </summary>
    public partial class DateFormattingOptions : Window {

        public DateFormattingOptions(string formatOption) {
            InitializeComponent();
            FormatOption = formatOption;
            if (string.IsNullOrWhiteSpace(formatOption)) {                
                optDefault.IsChecked = true;
            } else {
                txtUserFormat.Text = FormatOption;
                optUserDefined.IsChecked = true;
            }
            UpdatePreview();
        }

        private void UpdatePreview() {
            var format = FormatOption;            
            if (string.IsNullOrWhiteSpace(format)) {
                format = "d MMM, yyyy";
            }
            lblPreview.Content = String.Format("Preview: {0}", SupportService.FormatDate(DateTime.Now, format));
        }

        public String FormatOption { get; set; }

        private void optDefault_Checked(object sender, RoutedEventArgs e) {
            FormatOption = "";
            txtUserFormat.IsEnabled = false;
            UpdatePreview();
        }

        private void optUserDefined_Checked(object sender, RoutedEventArgs e) {
            FormatOption = txtUserFormat.Text;
            txtUserFormat.IsEnabled = true;                 
            UpdatePreview();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

        private void txtUserFormat_TextChanged(object sender, TextChangedEventArgs e) {
            FormatOption = txtUserFormat.Text;
            UpdatePreview();
        }

    }
}
