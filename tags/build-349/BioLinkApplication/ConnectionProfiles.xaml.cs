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
using System.Windows.Shapes;
using BioLink.Data;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;
using BioLink.Data.Model;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for ConectionProfiles.xaml
    /// </summary>
    public partial class ConnectionProfiles : Window {


        private ObservableCollection<ConnectionProfileViewModel> _model;

        public ConnectionProfiles() {
           
            InitializeComponent();
            List<ConnectionProfile> list = Config.GetGlobal<List<ConnectionProfile>>("connection.profiles", new List<ConnectionProfile>());

            _model = new ObservableCollection<ConnectionProfileViewModel>(list.ConvertAll((model) => {
                return new ConnectionProfileViewModel(model);
            }));

            cmbProfiles.ItemsSource = _model;
            String lastProfile = Config.GetGlobal<string>("connection.lastprofile", null);
            if (!String.IsNullOrEmpty(lastProfile)) {
                // Look in the list for the profile with the same name.
                ConnectionProfileViewModel lastUserProfile = _model.FirstOrDefault((item) => { return item.Name.Equals(lastProfile); });
                if (lastUserProfile != null) {
                    cmbProfiles.SelectedItem = lastUserProfile;
                }
            }
            if (cmbProfiles.SelectedItem == null) {
                profileGrid.IsEnabled = false;
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void cmbProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ConnectionProfileViewModel profile = cmbProfiles.SelectedItem as ConnectionProfileViewModel;
            if (profile != null) {
                profileGrid.IsEnabled = true;
                ProfileBorder.DataContext = profile;
            } else {
                // Clear out the fields
                txtDatabase.Text = "";
                txtName.Text = "";
                txtServer.Text = "";
                txtTimeout.Text = "";
                chkIntegratedSecurity.IsChecked = false;
                // and disable them
                profileGrid.IsEnabled = false;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNewProfile();
        }

        private void AddNewProfile() {
            ConnectionProfile model = new ConnectionProfile();
            model.Name = "<New Profile>";

            var profile = new ConnectionProfileViewModel(model);
            _model.Add(profile);
            cmbProfiles.SelectedItem = profile;
            txtName.Focus();
            txtName.SelectAll();
        }

        public ConnectionProfileViewModel SelectedProfile {
            get {                
                return cmbProfiles.SelectedItem as ConnectionProfileViewModel;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {            
            var profiles = _model.Select((viewmodel) => viewmodel.Model);
            Config.SetGlobal("connection.profiles", profiles);
            this.DialogResult = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void DeleteSelected() {
            ConnectionProfileViewModel profile = SelectedProfile;
            if (profile != null) {
                _model.Remove(profile);
                if (_model.Count > 0) {
                    cmbProfiles.SelectedIndex = 0;
                }
            }
        }

    }
}
