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
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for DuplicateItemOptions.xaml
    /// </summary>
    public partial class DuplicateItemOptions : Window {

        #region Designer Constructor
        public DuplicateItemOptions() {
            InitializeComponent();
        }
        #endregion

        public DuplicateItemOptions(Multimedia duplicate, int sizeInBytes, Boolean managerMode = false) {
            InitializeComponent();
            this.DuplicateItem = duplicate;
            lblDescription.Content = "There already exists a multimedia item with the name and size ('" + duplicate.Name + "', " +  SystemUtils.ByteCountToSizeString(sizeInBytes) + ").";
            if (managerMode) {
                optContinue.IsChecked = true;
                optLinkToExisting.IsEnabled = false;
                optLinkToExisting.IsChecked = false;                
            }
        }

        internal Multimedia DuplicateItem { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;

            if (optCancel.IsChecked.ValueOrFalse()) {
                SelectedAction = MultimediaDuplicateAction.Cancel;
            } else if (optContinue.IsChecked.ValueOrFalse()) {
                SelectedAction = MultimediaDuplicateAction.InsertDuplicate;
            } else if (optReplace.IsChecked.ValueOrFalse()) {
                SelectedAction = MultimediaDuplicateAction.ReplaceExisting;
            } else if (optLinkToExisting.IsChecked.ValueOrFalse()) {
                SelectedAction = MultimediaDuplicateAction.UseExisting;
            } else {
                throw new Exception("Unhandled option!");
            }

            this.Close();
        }

        public MultimediaDuplicateAction SelectedAction { get; private set; }

    }
}
