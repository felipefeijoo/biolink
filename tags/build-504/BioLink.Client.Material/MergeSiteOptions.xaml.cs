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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for MergeSiteOptions.xaml
    /// </summary>
    public partial class MergeSiteOptions : Window {

        #region Designer Constructor
        public MergeSiteOptions() {
            InitializeComponent();
        }
        #endregion

        public MergeSiteOptions(Window owner, User user, SiteExplorerNodeViewModel source, SiteExplorerNodeViewModel dest) {
            this.Owner = owner;
            this.User = user;
            InitializeComponent();            
            this.SourceNode = source;
            this.DestNode = dest;
            lblCaption.Content = string.Format("Are you sure you want to merge '{0}' with '{1}'?", source.Name, dest.Name);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Hide();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            Hide();
        }

        private void btnDifferences_Click(object sender, RoutedEventArgs e) {
            var list = new List<SiteExplorerNodeViewModel>();
            list.Add(SourceNode);
            var frm = new SiteCompare(this, User, DestNode, list);
            if (frm.ShowDialog().GetValueOrDefault(false)) {
                SelectedNodes = frm.SelectedSites;
            }
            this.DialogResult = SelectedNodes != null;
            Hide();
        }

        public User User { get; private set; }
        public SiteExplorerNodeViewModel SourceNode { get; private set; }
        public SiteExplorerNodeViewModel DestNode { get; private set; }

        public List<SiteExplorerNodeViewModel> SelectedNodes { get; private set; }

    }
}
