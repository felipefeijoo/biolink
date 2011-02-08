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

        public UserProperties(User currentUser, string username) {
            InitializeComponent();
            this.User = currentUser;
            Username = username;

            var service = new SupportService(currentUser);
            _model = new BiolinkUserViewModel(service.GetUser(username));
            
            var groups = service.GetGroups().Select((m) => {
                var vm = new GroupViewModel(m);
                if (vm.GroupID == _model.GroupID) {
                    _model.Group = vm;
                }
                return vm;
            });

            btnOk.IsEnabled = false;

            _model.DataChanged += new DataChangedHandler(_model_DataChanged);

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
                if (this.Question("You have unsaved changes. Are you sure you want to discard those changes?", "Discard changes?")) {
                    this.DialogResult = false;
                    this.Close();
                }
            } else {
                this.Close();
            }
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
