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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for PermissionProperties.xaml
    /// </summary>
    public partial class PermissionProperties : Window {

        private PermissionViewModel _model;
        private PERMISSION_TYPE _permType;

        public PermissionProperties(User user, PermissionViewModel viewModel) {
            InitializeComponent();
            this.User = user;
            _model = viewModel;            

            _permType = User.GetPermissionType(viewModel.Permission);

            if (_permType == PERMISSION_TYPE.ALLOWDISALLOW) {
                gridAllowDisallow.Visibility = Visibility.Visible;
                gridRWIUD.Visibility = Visibility.Collapsed;

                bool isAllowed = ((int) _model.Permission & (int) PERMISSION_MASK.ALLOW) != 0;

                if (isAllowed) {
                    optAllowed.IsChecked = true;
                } else {
                    optNotAllowed.IsChecked = true;
                }

            } else {

                gridAllowDisallow.Visibility = Visibility.Collapsed;
                gridRWIUD.Visibility = Visibility.Visible;

                optNoPermissions.IsChecked = true;

                if ((_model.Mask & (int) PERMISSION_MASK.READ) != 0) {
                    if ((_model.Mask & (int)PERMISSION_MASK.WRITE) != 0) {
                        optModify.IsChecked = true;
                        chkDelete.IsChecked = (_model.Mask & (int)PERMISSION_MASK.DELETE) != 0;
                        chkInsert.IsChecked = (_model.Mask & (int)PERMISSION_MASK.INSERT) != 0;
                        chkUpdate.IsChecked = (_model.Mask & (int)PERMISSION_MASK.UPDATE) != 0;
                    } else {
                        optReadOnly.IsChecked = true;
                    }
                }
            }

            grp.Header = String.Format("{0}/{1}", PermissionGroups.GetDescriptionForPermission(_model.Permission), _model.Permission);
        }

        protected User User { get; private set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
            this.DialogResult = true;
            this.Hide();
        }

        private void ApplyChanges() {
            int mask = 0;
            var service = new SupportService(User);
            if (_permType == PERMISSION_TYPE.ALLOWDISALLOW) {
                if (optAllowed.IsChecked.ValueOrFalse()) {
                    mask = (int)PERMISSION_MASK.ALLOW;
                }
            } else {
                if (optReadOnly.IsChecked.ValueOrFalse()) {
                    mask = (int) PERMISSION_MASK.READ;
                } else if (optModify.IsChecked.ValueOrFalse()) {
                    mask = (int) PERMISSION_MASK.READ | (int) PERMISSION_MASK.WRITE;
                    if (chkDelete.IsChecked.ValueOrFalse()) {
                        mask |= (int) PERMISSION_MASK.DELETE;
                    }
                    if (chkInsert.IsChecked.ValueOrFalse()) {
                        mask |= (int)PERMISSION_MASK.INSERT;
                    }
                    if (chkUpdate.IsChecked.ValueOrFalse()) {
                        mask |= (int)PERMISSION_MASK.UPDATE;
                    }
                }
            }

            service.SetPermission(_model.GroupID, (int) _model.Permission, mask);
            _model.Mask = mask;            
        }

    }
}
