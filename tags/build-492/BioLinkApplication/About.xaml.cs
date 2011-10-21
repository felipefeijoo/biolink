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
using System.Reflection;
using BioLink.Client.Extensibility;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window {

        public About() {
            InitializeComponent();
            var v = this.GetType().Assembly.GetName().Version;
            var version = String.Format("Version {0}.{1} (build {2}) {3}", v.Major, v.Minor, v.Revision, BuildLabel.BUILD_LABEL);
            lblVersion.Content = version;

            var model = new List<PluginVersionInfo>();
            PluginManager.Instance.TraversePlugins((plugin) => {
                model.Add(plugin.Version);
            });

            lvw.ItemsSource = model;
        }

        private void label2_MouseUp(object sender, MouseButtonEventArgs e) {            
            ShowCredits();
        }

        private void ShowCredits() {
            var frm = new ThirdPartyLicenses();
            frm.Owner = this;
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
        }

    }
    
}
