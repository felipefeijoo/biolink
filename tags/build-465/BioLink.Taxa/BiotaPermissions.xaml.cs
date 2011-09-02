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
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for BiotaPermissions.xaml
    /// </summary>
    public partial class BiotaPermissions : DatabaseCommandControl, IPreferredSizeHolder {

        private bool _setting = false;

        #region Designer
        public BiotaPermissions() {
            InitializeComponent();
        }
        #endregion

        public BiotaPermissions(User user, TaxonViewModel taxon, bool readOnlyMode) : base(user, "BiotaPermission:" + taxon.TaxaID.Value) {
            InitializeComponent();
            this.Taxon = taxon;

            Loaded += new RoutedEventHandler(BiotaPermissions_Loaded);
            lvw.SelectionChanged += new SelectionChangedEventHandler(lvw_SelectionChanged);

            optModify.Checked += new RoutedEventHandler(UpdatePermissions);
            optOwnership.Checked += new RoutedEventHandler(UpdatePermissions);
            optReadOnly.Checked += new RoutedEventHandler(UpdatePermissions);
            chkDelete.Checked += new RoutedEventHandler(UpdatePermissions);
            chkDelete.Unchecked += new RoutedEventHandler(UpdatePermissions);
            chkInsert.Checked +=new RoutedEventHandler(UpdatePermissions);
            chkInsert.Unchecked +=new RoutedEventHandler(UpdatePermissions);
            chkUpdate.Checked +=new RoutedEventHandler(UpdatePermissions);
            chkUpdate.Unchecked +=new RoutedEventHandler(UpdatePermissions);

            detailsGrid.IsEnabled = !readOnlyMode;
        }

        void UpdatePermissions(object sender, RoutedEventArgs e) {
            if (_setting) {
                return;
            }

            var entity = lvw.SelectedItem as UserEntityViewModel;
            if (entity == null) {
                return;
            }

            PERMISSION_MASK mask = 0;
            if (optReadOnly.IsChecked.ValueOrFalse()) {
                mask = PERMISSION_MASK.READ;
            } else if (optOwnership.IsChecked.ValueOrFalse()) {
                mask = PERMISSION_MASK.OWNER;
            } else if (optModify.IsChecked.ValueOrFalse()) {
                mask = PERMISSION_MASK.WRITE;
                if (chkDelete.IsChecked.ValueOrFalse()) {
                    mask |= PERMISSION_MASK.DELETE;
                }
                if (chkInsert.IsChecked.ValueOrFalse()) {
                    mask |= PERMISSION_MASK.INSERT;
                }
                if (chkUpdate.IsChecked.ValueOrFalse()) {
                    mask |= PERMISSION_MASK.UPDATE;
                }
            }

            try {
                RegisterUniquePendingChange(new UpdateBiotaPermissions(entity.GroupId, entity.UserId, mask, Taxon.Taxon));
                // If we get here we have permissions. We need to update our backing model...
                Debug.AssertNotNull(entity.Permission);
                entity.Permission.PermMask1 = (int) mask;

            } catch (NoPermissionException ex) {                
                entity.Permission = null; // force a reload of the entitys permissions for the current Taxa
                DisplayMaskForEntity(entity);
                throw ex;
            }

        }

        private void DisplayMaskForEntity(UserEntityViewModel entity) {

            if (entity == null) {
                return;
            }

            if (entity.Permission == null) {
                entity.Permission = new SupportService(User).GetBiotaPermission(entity.GroupId, entity.UserId, Taxon.TaxaID.Value);
            }

            if (entity.Permission != null) {

                var mask = (PERMISSION_MASK)entity.Permission.PermMask1;

                _setting = true;

                optReadOnly.IsChecked = false;
                optOwnership.IsChecked = false;
                optModify.IsChecked = false;
                chkDelete.IsChecked = false;
                chkInsert.IsChecked = false;
                chkUpdate.IsChecked = false;

                if ((mask & PERMISSION_MASK.OWNER) != 0) {
                    optOwnership.IsChecked = true;
                } else if ((mask & PERMISSION_MASK.WRITE) != 0) {
                    optModify.IsChecked = true;
                    chkDelete.IsChecked = (mask & PERMISSION_MASK.DELETE) != 0;
                    chkInsert.IsChecked = (mask & PERMISSION_MASK.INSERT) != 0;
                    chkUpdate.IsChecked = (mask & PERMISSION_MASK.UPDATE) != 0;
                } else if ((mask & PERMISSION_MASK.READ) != 0) {
                    optReadOnly.IsChecked = true;
                }                
                _setting = false;
            }
        }

        void lvw_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var entity = lvw.SelectedItem as UserEntityViewModel;
            if (entity != null) {
                DisplayMaskForEntity(entity);
            }
        }

        void BiotaPermissions_Loaded(object sender, RoutedEventArgs e) {
            var service = new SupportService(User);
            var groups = service.GetGroups();

            var model = new ObservableCollection<UserEntityViewModel>();
            foreach (Group g in groups) {
                model.Add(new UserEntityViewModel(g));
            }

            var users = service.GetUsers();
            foreach (UserSearchResult u in users) {
                model.Add(new UserEntityViewModel(u));
            }

            lvw.ItemsSource = model;
        }

        protected TaxonViewModel Taxon { get; private set; }

        public int PreferredHeight {            
            get { return 400; }
        }

        public int PreferredWidth {
            get { return 650; }
        }

    }

    public class UserEntityViewModel : ViewModelBase {

        private string _username;
        private string _groupName;
        private string _fullname;
        private string _description;

        public UserEntityViewModel(Group group) {
            this.Group = group;
            this.GroupName = group.GroupName;
            this.GroupId = group.GroupID;
        }

        public UserEntityViewModel(UserSearchResult user) {
            this.User = user;
            this.GroupId = user.GroupID;
            this.GroupName = user.Group;
            this.FullName = user.FullName;
            this.Description = user.Description;
            this.Username = user.Username;
            this.UserId = user.UserID;
        }

        private ImageSource _icon;

        public override ImageSource Icon {
            get {
                if (_icon == null) {
                    if (IsGroup) {
                        _icon = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Group.png");
                    } else {
                        _icon = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/User.png");
                    }
                }
                return _icon;
            }
            set {
                _icon = value;
            }
        }

        public string Username {
            get { return _username; }
            set { SetProperty("Username", ref _username, value); }
        }

        public string GroupName {
            get { return _groupName; }
            set { SetProperty("GroupName", ref _groupName, value); }
        }

        public string FullName {
            get { return _fullname; }
            set { SetProperty("FullName", ref _fullname, value); }
        }

        public string Description {
            get { return _description; }
            set { SetProperty("Description", ref _description, value); }
        }

        public int GroupId { get; set; }

        public int UserId { get; set; } 

        public Group Group { get; set; }

        public UserSearchResult User { get; set; }

        public bool IsGroup {
            get { return Group != null; }
        }

        public bool IsUser { 
            get { return User != null; } 
        }

        public override int? ObjectID {
            get { return null; }
        }

        public BiotaPermission Permission { get; set; }

    }

    public class UpdateBiotaPermissions : DatabaseCommand {

        public UpdateBiotaPermissions(int groupId, int userId, PERMISSION_MASK mask, Taxon taxon) {
            this.GroupID = groupId;
            this.UserID = userId;
            this.Mask = mask;
            this.Taxon = taxon;
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.AddBiota(Taxon.TaxaID.Value, PERMISSION_MASK.OWNER);
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateBiotaPermission(GroupID, UserID, Taxon.TaxaID.Value, Mask);
        }

        protected Taxon Taxon { get; private set; }
        protected int GroupID { get; private set; }        
        protected int UserID { get; private set; }
        protected PERMISSION_MASK Mask { get; private set; }
    }

}
