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

            var service = new SupportService(user);
            var users = service.GetUsers();
            _users = new ObservableCollection<UserSearchResultViewModel>(users.Select((m) => {
                return new UserSearchResultViewModel(m);
            }));

            var groups = service.GetGroups();
            _groups = new ObservableCollection<GroupViewModel>(groups.Select((m) => {
                return new GroupViewModel(m);
            }));

            lvwUsers.ItemsSource = _users;
            tvwGroups.ItemsSource = _groups;
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
                frm.ShowDialog();
            }
        }

        private void txtUserFilter_TypingPaused(string text) {
            FilterUsers(text);
        }

        private void FilterUsers(string filter) {

            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwUsers.ItemsSource) as ListCollectionView;

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
        }

    }

    public class GroupViewModel : GenericHierarchicalViewModelBase<Group> {

        public GroupViewModel(Group model) : base(model, () => model.GroupID) { }

        public override BitmapSource Icon {
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

        public override BitmapSource Icon {
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
