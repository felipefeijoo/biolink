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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for UserProperties.xaml
    /// </summary>
    public partial class UserProperties : Window {

        private BiolinkUserViewModel _model;
        private bool _isNew = false;

        public UserProperties(User currentUser, string username) {
            InitializeComponent();
            this.User = currentUser;

            Username = username;

            var service = new SupportService(currentUser);

            if (string.IsNullOrEmpty(username)) {
                _model = new BiolinkUserViewModel(new BiolinkUser());
                _isNew = true;
            } else {                
                _model = new BiolinkUserViewModel(service.GetUser(username));
                txtUsername.IsReadOnly = true;
                btnOk.IsEnabled = false;
                _model.DataChanged += new DataChangedHandler(_model_DataChanged);
            }
            
            var groups = service.GetGroups().Select((m) => {
                var vm = new GroupViewModel(m);
                if (vm.GroupID == _model.GroupID) {
                    _model.Group = vm;
                }
                return vm;
            });

            cmbGroup.ItemsSource = groups;

            this.DataContext = _model;
        }

        void _model_DataChanged(ChangeableModelBase viewmodel) {
            btnOk.IsEnabled = true;
        }

        public User User { get; private set; }

        public string Username { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            if (_model != null && _model.IsChanged) {
                if (this.DiscardChangesQuestion()) {
                    this.DialogResult = false;
                    this.Close();
                } else {
                    e.Handled = true;
                }
            } else {
                this.Close();
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            try {
                if (ValidateUser(_model, _isNew)) {
                    var service = new SupportService(User);
                    if (_isNew) {
                        service.InsertUser(_model.Model);
                    } else {
                        service.UpdateUser(_model.Model);
                    }

                    if (!string.IsNullOrEmpty(txtPassword.Password)) {
                        service.UpdateUserPassword(_model.UserName, txtPassword.Password);
                    }

                    this.DialogResult = true;
                    this.Close();
                }
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }

        private bool ValidateUser(BiolinkUserViewModel model, bool isNew) {

            if (model.GroupID == 0) {
                ErrorMessage.Show("Please select a group before continuing");
                return false;
            }

            if (string.IsNullOrEmpty(model.UserName)) {
                ErrorMessage.Show("You must supply a username");
                return false;
            }

            if (isNew || !string.IsNullOrEmpty(txtPassword.Password)) {

                if (string.IsNullOrEmpty(txtPassword.Password)) {
                    ErrorMessage.Show("Please supply a password");
                    return false;
                }

                if (!txtPassword.Password.Equals(txtConfirmPassword.Password)) {
                    ErrorMessage.Show("The password and the password confirmation do not match!");
                    return false;
                }
            }

            return true;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e) {
            btnOk.IsEnabled = true;
        }

    }

    public class InsertBiolinkUserCommand : GenericDatabaseCommand<BiolinkUser> {

        public InsertBiolinkUserCommand(BiolinkUser model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.InsertUser(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.USERMANAGER_USER, PERMISSION_MASK.INSERT);
        }

    }

    public class UpdateBiolinkUserCommand : GenericDatabaseCommand<BiolinkUser> {

        public UpdateBiolinkUserCommand(BiolinkUser model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateUser(Model);                        
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.USERMANAGER_USER, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteBiolinkUserCommand : GenericDatabaseCommand<BiolinkUser> {

        public DeleteBiolinkUserCommand(BiolinkUser model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteUser(Model.UserName);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.USERMANAGER_USER, PERMISSION_MASK.DELETE);
        }

    }

    public class BiolinkUserViewModel : GenericViewModelBase<BiolinkUser> {
        public BiolinkUserViewModel(BiolinkUser model) : base(model, ()=>0) { }
       
        public string UserName {
            get { return Model.UserName; }
            set { SetProperty(() => Model.UserName, value); }
        }

        public string GroupName {
            get { return Model.GroupName; }
            set { SetProperty(() => Model.GroupName, value); }
        }

        public int GroupID {
            get { return Model.GroupID; }
            set { SetProperty(() => Model.GroupID, value); }
        }

        public string FullName {
            get { return Model.FullName; }
            set { SetProperty(() => Model.FullName, value); }
        }

        public string Description {
            get { return Model.Description; }
            set { SetProperty(() => Model.Description, value); }
        }

        public string Notes {
            get { return Model.Notes; }
            set { SetProperty(() => Model.Notes, value); }
        }

        public bool CanCreateUsers {
            get { return Model.CanCreateUsers; }
            set { SetProperty(() => Model.CanCreateUsers, value); }
        }

        private GroupViewModel _group;

        public GroupViewModel Group {
            get { return _group; }
            set {
                _group = value;
                if (value != null) {
                    this.GroupID = value.GroupID;
                    this.GroupName = value.GroupName;
                } else {
                    this.GroupID = 0;
                    this.GroupName = null;
                }
            }
        }
    }
}
