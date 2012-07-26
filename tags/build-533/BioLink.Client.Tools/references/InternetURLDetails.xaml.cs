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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for InternetURLDetails.xaml
    /// </summary>
    public partial class InternetURLDetails : UserControl {
        public InternetURLDetails() {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e) {
            try {
                string urlString = txtSource.Text;
                if (!String.IsNullOrEmpty(urlString)) {
                    Uri url = new Uri(urlString, UriKind.Absolute);
                    if (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps) {
                        System.Diagnostics.Process.Start(txtSource.Text);
                    } else {
                        ErrorMessage.Show("{0} does not appear to be a valid HTTP URL.", txtSource.Text);
                    }
                }
            } catch (Exception ex) {
                ErrorMessage.Show("Unable to browse to '{0}' : {1}", txtSource.Text, ex.Message);
            }
        }
    }
}
