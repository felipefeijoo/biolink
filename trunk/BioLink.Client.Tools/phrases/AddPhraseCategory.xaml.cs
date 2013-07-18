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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for AddPhraseCategory.xaml
    /// </summary>
    public partial class AddPhraseCategory : Window {
        public AddPhraseCategory() {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if (CreatePhraseCategory()) {
                this.DialogResult = true;
                this.Close();
            }
        }

        private bool CreatePhraseCategory() {
            var category = txtCategory.Text;
            if (String.IsNullOrWhiteSpace(category)) {
                ErrorMessage.Show("You must supply a category name!");
                return false;
            }

            var supportService = new SupportService(PluginManager.Instance.User);
            var existingId = supportService.GetPhraseCategoryId(category, false);
            if (existingId > 0) {
                ErrorMessage.Show("A Phrase Category already exists with this name!");
                return false;
            }

            var newId = supportService.InsertPhraseCategory(category, false);

            if (newId <= 0) {
                ErrorMessage.Show("Failed to create new category!");
                return false;
            }

            return true;
        }
    }
}
