using System.Windows;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Collections.Generic;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonNameDetails.xaml
    /// </summary>
    public partial class TaxonNameDetails : Window {

        private TaxonViewModel _taxon;
        private TaxonRank _rank;
        private TaxonNameViewModel _model;
        private List<Kingdom> _kingdomList;

        #region DesignerConstructor
        public TaxonNameDetails() {
            InitializeComponent();
        }
        #endregion

        public TaxonNameDetails(TaxonViewModel taxon, TaxaService service) {            
            _taxon = taxon;
            _rank = service.GetTaxonRank(_taxon.ElemType);
            _kingdomList = service.GetKingdomList();
            Kingdom kingdom = _kingdomList.Find((k) => k.KingdomCode.Equals(_taxon.KingdomCode));

            _model = new TaxonNameViewModel(taxon, _rank.LongName, kingdom);

            this.DataContext = _model;            
            InitializeComponent();

            

            cmbKingdom.ItemsSource = _kingdomList;

            if (taxon.AvailableName.ValueOrFalse() || taxon.LiteratureName.ValueOrFalse()) {

                string phraseCategory = "ALN Name Status";

                if (taxon.AvailableName.ValueOrFalse()) {
                    TaxonRank rank = service.GetTaxonRank(taxon.ElemType);
                
                    switch (rank.Category.ToLower()) {
                        case "g": phraseCategory = "GAN Name Status";
                            break;
                        case "s": phraseCategory = "SAN Name Status";
                            break;
                    }
                }

                txtNameStatus.BindUser(PluginManager.Instance.User, phraseCategory, true);
            } else {
                txtNameStatus.Visibility = System.Windows.Visibility.Collapsed;
                lblNameStatus.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.chkChangedCombination.Visibility = (_rank.Category == "S" ? Visibility.Visible : Visibility.Hidden);
        }

        public TaxonViewModel Taxon { 
            get { return _taxon; } 
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            SaveChanges();
            this.Hide();
        }

        private void SaveChanges() {

        }

    }

    class TaxonNameViewModel : ViewModelBase {

        public TaxonNameViewModel(TaxonViewModel taxon, string rank, Kingdom kingdom) {
            this.Kingdom = kingdom;            
            this.Name = taxon.Epithet;
            this.Author = taxon.Author;
            this.Year = taxon.YearOfPub;
            this.IsChangedCombination = taxon.ChgComb.GetValueOrDefault(false);
            this.IsVerified = !taxon.Unverified.GetValueOrDefault(false);
            this.Rank = rank;
            this.NameStatus = taxon.NameStatus;
        }

        public Kingdom Kingdom { get; set; }        
        public string Name { get; set; }
        public string Author { get; set; }
        public bool IsChangedCombination { get; set; }
        public string Year { get; set; }
        public bool IsVerified { get; set; }
        private string Rank { get; set; }
        private string NameStatus { get; set; }
    }
}
