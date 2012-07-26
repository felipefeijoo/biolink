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
    /// Interaction logic for RegionBasedReportOptions.xaml
    /// </summary>
    public partial class RegionBasedReportOptions : Window {

        public RegionBasedReportOptions(User user, List<TaxonViewModel> taxa, string title) {
            InitializeComponent();
            Taxa = taxa;
            if (taxa == null || taxa.Count == 0) {
                throw new Exception("No taxa selected!");
            }

            if (taxa.Count > 1) {
                txtTaxa.Text = "Multiple Taxa";
            } else {
                txtTaxa.Text = taxa[0].TaxaFullName;                     
            }
            txtRegion.BindUser(user, LookupType.Region);
            txtRegion.WatermarkText = "All Regions";

            this.Title = title;
        }

        protected List<TaxonViewModel> Taxa { get; private set; }

        private void button1_Click(object sender, RoutedEventArgs e) {
            if (!txtRegion.ObjectID.HasValue && !string.IsNullOrWhiteSpace(txtRegion.Text)) {
                ErrorMessage.Show("You must select a valid region!");
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void btnCnacel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

    }
}
