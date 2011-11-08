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

namespace BioLink.Client.Tools {

    /// <summary>
    /// Interaction logic for UserManager.xaml
    /// </summary>
    public partial class UserManager : UserControl {

        private ObservableCollection<UserSearchResultViewModel> _users;

        private ObservableCollection<GroupViewModel> _groups;

        public UserManager(User user, ToolsPlugin owner) {
            InitializeComponent();
            this.User = user;
            this.Owner = owner;

            ReloadModel();

            grpUsers.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_USER, PERMISSION_MASK.READ);
            grpUsers.Header = "Users " + User.MaskStr(user.GetPermissionMask(PermissionCategory.USERMANAGER_USER), user.Username);
    
            grpGroups.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_GROUP, PERMISSION_MASK.READ);
            grpGroups.Header = "Groups " + User.MaskStr(user.GetPermissionMask(PermissionCategory.USERMANAGER_GROUP), user.Username);
    
            btnDelete.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_USER, PERMISSION_MASK.DELETE);

            btnNewUser.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_USER, PERMISSION_MASK.INSERT);
            btnProperties.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_USER, PERMISSION_MASK.READ);
    
            btnCopy.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_GROUP, PERMISSION_MASK.INSERT);
            btnDeleteGroup.IsEnabled = User.HasPermission( PermissionCategory.USERMANAGER_GROUP, PERMISSION_MASK.DELETE);
            btnPermissions.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_GROUP, PERMISSION_MASK.READ);
            btnNewGroup.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_GROUP, PERMISSION_MASK.INSERT);
            btnRename.IsEnabled = User.HasPermission(PermissionCategory.USERMANAGER_GROUP, PERMISSION_MASK.UPDATE);

            tvwGroups.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(tvwGroups_SelectedItemChanged);
            tvwGroups.MouseDoubleClick += new MouseButtonEventHandler(tvwGroups_MouseDoubleClick);

            lvwUsers.KeyUp += new KeyEventHandler(lvwUsers_KeyUp);

        }

        void lvwUsers_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                ShowSelectedProperties();
            }
        }

        void tvwGroups_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selected = tvwGroups.SelectedItem as PermissionViewModel;
            if (selected != null) {
                EditPermission(selected);
            }
        }

        void tvwGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var selected = tvwGroups.SelectedItem;
            var isGroup = selected is GroupViewModel;
            btnPermissions.IsEnabled = selected is PermissionViewModel;
            btnRename.IsEnabled = isGroup;
            btnCopy.IsEnabled = isGroup;
            btnDeleteGroup.IsEnabled = isGroup;
        }

        private void ReloadModel() {

            var oldFilter = txtUserFilter.Text;
            FilterUsers("");

            var service = new SupportService(User);
            var users = service.GetUsers();
            _users = new ObservableCollection<UserSearchResultViewModel>(users.Select((m) => {
                return new UserSearchResultViewModel(m);
            }));

            var groups = service.GetGroups();
            _groups = new ObservableCollection<GroupViewModel>(groups.Select((m) => {
                var vm =  new GroupViewModel(m);                
                vm.LazyLoadChildren +=new HierarchicalViewModelAction(vm_LazyLoadChildren);
                vm.Children.Add(new ViewModelPlaceholder("Loading..."));
                return vm;
            }));

            lvwUsers.ItemsSource = _users;
            tvwGroups.ItemsSource = _groups;

            if (!string.IsNullOrWhiteSpace(oldFilter)) {
                FilterUsers(oldFilter);
            }

        }

        void vm_LazyLoadChildren(HierarchicalViewModelBase item) {
            using (new OverrideCursor(Cursors.Wait)) {
                var groupNode = item as GroupViewModel;
                if (groupNode != null) {
                    item.Children.Clear();
                    var service = new SupportService(User);
                    var permissions = service.GetPermissions(groupNode.GroupID);

                    var permGroupNodes = new Dictionary<string, PermissionGroupViewModel>();

                    foreach (PermissionCategory perm in Enum.GetValues(typeof(PermissionCategory))) {
                        String desc = PermissionGroups.GetDescriptionForPermission(perm);
                        PermissionGroupViewModel permGroupNode = null;
                        if (!permGroupNodes.ContainsKey(desc)) {
                            permGroupNode = new PermissionGroupViewModel(desc);
                            item.Children.Insert(0, permGroupNode);
                            permGroupNodes[desc] = permGroupNode;
                        } else {
                            permGroupNode = permGroupNodes[desc];
                        }

                        var permission = permissions.FirstOrDefault((p) => {
                            return p.PermissionID == (int)perm;
                        });

                        var mask = permission == null ? 0 : permission.Mask1;

                        permGroupNode.Children.Add(new PermissionViewModel(groupNode.GroupID, perm, mask));
                    }
                }
            }

        }

        public void tvwGroup_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
        }

        public User User { get; private set; }

        public ToolsPlugin Owner { get; private set; }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            ShowSelectedProperties();
        }

        private void ShowSelectedProperties() {
            var selected = lvwUsers.SelectedItem as UserSearchResultViewModel;
            if (selected != null) {
                var frm = new UserProperties(User, selected.Username);
                frm.Owner = this.FindParentWindow();
                frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (frm.ShowDialog() == true) {
                    ReloadModel();
                }
            }
        }

        private void txtUserFilter_TypingPaused(string text) {
            FilterUsers(text);
        }

        private void FilterUsers(string filter) {

            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwUsers.ItemsSource) as ListCollectionView;

            if (dataView == null) {
                return;
            }

            if (String.IsNullOrEmpty(filter)) {
                dataView.Filter = null;
                return;
            }

            filter = filter.ToLower();
            dataView.Filter = (obj) => {
                var field = obj as UserSearchResultViewModel;

                if (field != null) {

                    if (field.Username.ToLower().Contains(filter)) {
                        return true;
                    }

                    if (field.FullName.ToLower().Contains(filter)) {
                        return true;
                    }

                    return false;
                }
                return true;
            };

            dataView.Refresh();

        }

        private void txtUserFilter_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                lvwUsers.Focus();
            }
        }

        private void btnNewUser_Click(object sender, RoutedEventArgs e) {
            AddNewUser();
        }

        private void AddNewUser() {
            var frm = new UserProperties(User, null);
            frm.Owner = this.FindParentWindow();
            frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            frm.ShowDialog();

            ReloadModel();
            string username = frm.txtUsername.Text;
            var newItem = _users.FirstOrDefault((m) => {
                return m.Username.Equals(username);
            });

            lvwUsers.SelectedItem = newItem;

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelectedUser();
        }

        private void DeleteSelectedUser() {
            var selected = lvwUsers.SelectedItem as UserSearchResultViewModel;
            if (selected != null) {
                if (this.Question("Are you sure you wish to permanently delete the user '" + selected.Username + "'?", "Delete " + selected.Username)) {
                    var service = new SupportService(User);
                    service.DeleteUser(selected.Username);
                    ReloadModel();
                }
            }
        }

        private void btnNewGroup_Click(object sender, RoutedEventArgs e) {
            AddNewGroup();
        }

        private void AddNewGroup() {
            InputBox.Show(this.FindParentWindow(), "New group", "Enter the name of the new group", (newName) => {
                var service = new SupportService(User);
                service.InsertGroup(newName);
                ReloadModel();
                SelectGroup(newName);
            });
        }

        private void SelectGroup(string name) {
            var found = _groups.FirstOrDefault((vm) => {
                return vm.GroupName.Equals(name);
            });
            if (found != null) {
                found.IsSelected = true;                
            }
        }

        private void btnDeleteGroup_Click(object sender, RoutedEventArgs e) {
            DeleteSelectedGroup();
        }

        private void DeleteSelectedGroup() {

            var selected = tvwGroups.SelectedItem as GroupViewModel;
            if (selected != null) {
                if (this.Question("Are you sure you wish to permanently delete group '" + selected.GroupName + "'", "Delete group '" + selected.GroupName + "'?")) {
                    var service = new SupportService(User);
                    service.DeleteGroup(selected.GroupID);
                    ReloadModel();
                }
            }

        }

        private void btnRename_Click(object sender, RoutedEventArgs e) {
            RenameSelectedGroup();
        }

        private void RenameSelectedGroup() {
            var selected = tvwGroups.SelectedItem as GroupViewModel;
            if (selected != null) {
                InputBox.Show(this.FindParentWindow(), "Rename group '" + selected.GroupName + "'", "Enter the new name for this group", selected.GroupName, (newName) => {
                    var service = new SupportService(User);
                    service.RenameGroup(selected.GroupID, newName);
                    ReloadModel();
                    SelectGroup(newName);
                });
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e) {
            CopySelectedGroup();
        }

        private void CopySelectedGroup() {
            var selected = tvwGroups.SelectedItem as GroupViewModel;
            if (selected != null) {
                InputBox.Show(this.FindParentWindow(), "Copy group '" + selected.GroupName + "'", "Enter the new name for the new group", selected.GroupName, (newName) => {
                    var service = new SupportService(User);
                    // Create the new group
                    var newGroupID = service.InsertGroup(newName);
                    // and copy over the existing permissions...
                    service.CopyGroupPermissions(selected.GroupID, newGroupID);
                    // reload
                    ReloadModel();
                    SelectGroup(newName);
                });
            }
        }

        private void EditPermission(PermissionViewModel vm) {

            if (vm == null) {
                return;
            }

            var frm = new PermissionProperties(User, vm);
            frm.Owner = this.FindParentWindow();
            frm.ShowDialog();
            // ReloadModel();
        }

        private void btnPermissions_Click(object sender, RoutedEventArgs e) {
            EditPermission(tvwGroups.SelectedItem as PermissionViewModel);
        }

        private void lvwUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ShowSelectedProperties();
        }

        private void lvwUsers_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = lvwUsers.SelectedItem as UserSearchResultViewModel;
            if (selected != null) {
                var builder = new ContextMenuBuilder(null);
                builder.New("Add User").Handler(() => AddNewUser()).End();
                builder.Separator();
                builder.New("Delete " + selected.Username).Handler(() => DeleteSelectedUser()).End();
                builder.Separator();
                builder.New("Properties...").Handler(() => ShowSelectedProperties()).End();

                lvwUsers.ContextMenu = builder.ContextMenu;
            }
        }

    }

    public class PermissionViewModel : HierarchicalViewModelBase {

        private int _mask;

        public PermissionViewModel(int groupID, PermissionCategory permType, int mask) {
            this.GroupID = groupID;
            this.Permission = permType;
            _mask = mask;
        }

        public override string DisplayLabel {
            get {
                return Permission.ToString();
            }
        }

        public override ImageSource Icon {
            get {
                if (base.Icon == null) {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Permission.png");
                }
                return base.Icon;
            }
            set {
                base.Icon = value;
            }
        }

        public override int? ObjectID {
            get { return -1; }
        }

        public String MaskLabel {
            get { return User.MaskStr(Mask, null); }
        }
        
        public PermissionCategory Permission { get; set; }

        public int Mask {
            get { return _mask; }
            set { 
                SetProperty("Mask", ref _mask, value);
                RaisePropertyChanged("MaskLabel");
            }
        }

        public int GroupID { get; set; }

    }


    public class PermissionGroupViewModel : HierarchicalViewModelBase {

        public PermissionGroupViewModel(string name) {
            this.Name = name;
        }

        public override ImageSource Icon {
            get {
                if (base.Icon == null) {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Permissions.png");
                }
                return base.Icon;
            }
            set {
                base.Icon = value;
            }
        }

        public override int? ObjectID {
            get { return -1; }
        }


        public String Name { get; set; }
    }

    public class GroupViewModel : GenericHierarchicalViewModelBase<Group> {

        public GroupViewModel(Group model) : base(model, () => model.GroupID) { }

        public override ImageSource Icon {
            get {
                if (base.Icon == null) {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Group.png");
                }
                return base.Icon;
            }
            set {
                base.Icon = value;
            }
        }

        public int GroupID {
            get { return Model.GroupID; }
            set { SetProperty(() => Model.GroupID, value); }
        }

        public string GroupName {
            get { return Model.GroupName; }
            set { SetProperty(() => Model.GroupName, value); }
        }

    }

    public class UserSearchResultViewModel : GenericViewModelBase<UserSearchResult> {

        public UserSearchResultViewModel(UserSearchResult model) : base(model, () => model.UserID) { }

        public override ImageSource Icon {
            get {
                if (base.Icon == null) {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/User.png");
                }
                return base.Icon;
            }
            set {
                base.Icon = value;
            }
        }

        public int UserID {
            get { return Model.UserID; }
            set { SetProperty(() => Model.UserID, value); }
        }

        public int GroupID {
            get { return Model.GroupID; }
            set { SetProperty(() => Model.GroupID, value); }
        }

        public string Username {
            get { return Model.Username; }
            set { SetProperty(() => Model.Username, value); }
        }

        public string Group {
            get { return Model.Group; }
            set { SetProperty(() => Model.Group, value); }
        }

        public string FullName {
            get { return Model.FullName; }
            set { SetProperty(() => Model.FullName, value); }
        }

        public string Description {
            get { return Model.Description; }
            set { SetProperty(() => Model.Description, value); }
        }

        public bool CanCreateUsers {
            get { return Model.CanCreateUsers; }
            set { SetProperty(() => Model.CanCreateUsers, value); }
        }

    }
}
