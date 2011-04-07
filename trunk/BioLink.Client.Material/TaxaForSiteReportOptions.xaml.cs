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

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for TaxaForSiteReportOptions.xaml
    /// </summary>
    public partial class TaxaForSiteReportOptions : Window {

        public TaxaForSiteReportOptions(User user, SiteExplorerNode node) {
            InitializeComponent();
            this.User = user;            
            txtRegion.BindUser(user, LookupType.SiteOrRegion);
            txtRegion.ObjectID = node.ObjectID;
            txtRegion.Text = node.Name;
            this.SiteRegionID = node.ElemID;
            this.SiteRegionElemType = node.ElemType;
            this.SiteRegionName = node.Name;

            txtTaxon.BindUser(user, LookupType.Taxon);            
        }

        protected User User { get; private set; }

        public int? SiteRegionID { get; private set; }

        public string SiteRegionName { get; private set; }

        public string SiteRegionElemType { get; private set; }
        

        public int? TaxonID { 
            get { 
                return txtTaxon.ObjectID; 
            } 
        }

        public string TaxonName {
            get { return txtTaxon.Text; }
        }

        private void btnCnacel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                if (txtRegion.SelectedObject != null) {
                    this.SiteRegionID = txtRegion.SelectedObject.LookupObjectID;
                    this.SiteRegionName = txtRegion.Text;
                    this.SiteRegionElemType = txtRegion.SelectedObject.LookupType.ToString();
                }                
                this.DialogResult = true;
                this.Close();
            }
        }

        private bool Validate() {

            if (!txtRegion.ObjectID.HasValue) {
                ErrorMessage.Show("You must select a valid site or region!");
                return false;
            }

            return true;
        }

    }
}
