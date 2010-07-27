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

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for ConectionProfiles.xaml
    /// </summary>
    public partial class ConnectionProfiles : Window {


        private ObservableCollection<ConnectionProfile> _model;

        public ConnectionProfiles() {
            InitializeComponent();
            List<ConnectionProfile> list = Preferences.GetGlobal<List<ConnectionProfile>>("connection.profiles", new List<ConnectionProfile>());

            _model = new ObservableCollection<ConnectionProfile>(list);
            cmbProfiles.ItemsSource = _model;
            String lastProfile = Preferences.GetGlobal<string>("connection.lastprofile", null);
            if (!String.IsNullOrEmpty(lastProfile)) {
                // Look in the list for the profile with the same name.
                ConnectionProfile lastUserProfile = _model.FirstOrDefault((item) => { return item.Name.Equals(lastProfile); });
                if (lastUserProfile != null) {
                    cmbProfiles.SelectedItem = lastUserProfile;
                }
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void cmbProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ConnectionProfile profile = cmbProfiles.SelectedItem as ConnectionProfile;
            if (profile != null) {
                ProfileBorder.DataContext = profile;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNewProfile();
        }

        private void AddNewProfile() {
            ConnectionProfile profile = new ConnectionProfile();
            profile.Name = "<New Profile>";           
            _model.Add(profile);
            cmbProfiles.SelectedItem = profile;
            txtName.Focus();
            txtName.SelectAll();
        }

        public ConnectionProfile SelectedProfile {
            get {                
                return cmbProfiles.SelectedItem as ConnectionProfile;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            List<ConnectionProfile> profiles =  new List<ConnectionProfile>(_model);
            Preferences.SetGlobal<List<ConnectionProfile>>("connection.profiles", profiles);
            this.DialogResult = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void DeleteSelected() {
            ConnectionProfile profile = SelectedProfile;
            if (profile != null) {
                _model.Remove(profile);
                if (_model.Count > 0) {
                    cmbProfiles.SelectedIndex = 0;
                }
            }
        }
    }
}
