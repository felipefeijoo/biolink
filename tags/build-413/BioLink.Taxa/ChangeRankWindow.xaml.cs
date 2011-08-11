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

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for ChangeRankWindow.xaml
    /// </summary>
    public partial class ChangeRankWindow : Window {

        #region Designer
        public ChangeRankWindow() {
            InitializeComponent();
        }
        #endregion

        public ChangeRankWindow(User user, TaxonViewModel taxon, List<TaxonRank> validRanks) {
            InitializeComponent();

            this.User = user;
            this.ViewModel = taxon;

            cmbTypes.ItemsSource = validRanks;
            optValidType.IsEnabled = (validRanks != null && validRanks.Count > 0);
            cmbTypes.SelectedIndex = 0;
        }

        protected User User { get; private set; }

        protected TaxonViewModel ViewModel { get; private set; }

        public TaxonRank SelectedRank {
            get {
                if (optUnranked.IsChecked == true) {
                    return null;
                } else {
                    return cmbTypes.SelectedItem as TaxonRank;
                }
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

    }
}
