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
using BioLink.Data.Model;
using BioLink.Data;


namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteVisitRDEControl.xaml
    /// </summary>
    public partial class SiteVisitRDEControl : UserControl, IItemsGroupBoxDetailControl {

        public SiteVisitRDEControl(User user) {
            InitializeComponent();
            txtCollector.BindUser(user);
            this.User = user;
        }

        private void txtCollector_Click(object sender, RoutedEventArgs e) {
            Func<IEnumerable<string>> itemsFunc = () => {
                var service = new MaterialService(User);
                return service.GetDistinctCollectors();
            };

            PickListWindow frm = new PickListWindow(User, "Select a collector", itemsFunc, null);
            if (frm.ShowDialog().ValueOrFalse()) {
                if (String.IsNullOrWhiteSpace(txtCollector.Text)) {
                    txtCollector.Text = frm.SelectedValue as string;
                } else {
                    txtCollector.Text += ", " + frm.SelectedValue;
                }
            }
        }

        public User User { get; private set; }


        public bool CanUnlock() {
            return User.HasPermission(PermissionCategory.SPARC_SITEVISIT, PERMISSION_MASK.UPDATE);
        }

        public bool CanAddNew() {
            return User.HasPermission(PermissionCategory.SPARC_SITEVISIT, PERMISSION_MASK.INSERT);
        }
    }
}
