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
    /// Interaction logic for AddToLabelSet.xaml
    /// </summary>
    public partial class AddToLabelSet : Window {

        public AddToLabelSet(User user, int materialId) {
            InitializeComponent();
            this.User = user;
            this.MaterialID = materialId;

            Loaded +=new RoutedEventHandler(AddToLabelSet_Loaded);

            optExisting.Checked += new RoutedEventHandler(optExisting_Checked);
            optNewLabelSet.Checked += new RoutedEventHandler(optNewLabelSet_Checked);
        }

        void optNewLabelSet_Checked(object sender, RoutedEventArgs e) {
            cmbLabelSets.IsEnabled = false;
            txtNewSetName.IsEnabled = true;
        }

        void optExisting_Checked(object sender, RoutedEventArgs e) {
            cmbLabelSets.IsEnabled = true;
            txtNewSetName.IsEnabled = false;
        }

        void AddToLabelSet_Loaded(object sender, RoutedEventArgs e) {
            var service = new MaterialService(User);
            this.Material = service.GetMaterial(MaterialID);
            txtItem.Text = Material.MaterialName;

            var supportService = new SupportService(User);
            var sets = supportService.GetLabelSets();
            cmbLabelSets.ItemsSource = sets;
            if (sets.Count > 0) {
                cmbLabelSets.SelectedIndex = 0;
                optExisting.IsChecked = true;
            } else {
                optExisting.IsEnabled = false;
                optNewLabelSet.IsChecked = true;
            }
            
        }


        protected Data.Model.Material Material { get; private set; }
        public User User { get; private set; }
        public int MaterialID { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {

            if (optExisting.IsChecked.ValueOrFalse()) {
                if (cmbLabelSets.SelectedItem == null) {
                    ErrorMessage.Show("You must select an existing label set!");
                    return;
                }
            } else {
                if (string.IsNullOrWhiteSpace(txtNewSetName.Text)) {
                    ErrorMessage.Show("You must enter a name for the new label set!");
                    return;
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        public String LabelSetName {
            get {
                if (optExisting.IsChecked.ValueOrFalse()) {
                    var selected =cmbLabelSets.SelectedItem as LabelSet;
                    if(selected != null) {
                        return selected.Name;
                    }                    
                } else {
                    if (!string.IsNullOrWhiteSpace(txtNewSetName.Text)) {
                        return txtNewSetName.Text;
                    }
                }

                return null;
            }
        }

    }
}
