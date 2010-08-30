using System.Windows;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonNameDetails.xaml
    /// </summary>
    public partial class TaxonNameDetails : Window {

        private TaxonViewModel _taxon;
        private TaxonRank _rank;
        private TaxonNameViewModel _model;


        #region DesignerConstructor
        public TaxonNameDetails() {
            InitializeComponent();
        }
        #endregion

        public TaxonNameDetails(TaxonViewModel taxon, TaxaService service) {            
            _taxon = taxon;
            _rank = service.GetTaxonRank(_taxon.ElemType);
            _model = new TaxonNameViewModel(taxon, _rank.LongName);
            this.DataContext = _model;            
            InitializeComponent();
            

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

    class TaxonNameViewModel {
        public TaxonNameViewModel(TaxonViewModel taxon, string rank) {
            this.Kingdom = KingdomTypeFactory.FromCode(taxon.KingdomCode);
            this.Name = taxon.Epithet;
            this.Author = taxon.Author;
            this.Year = taxon.YearOfPub;
            this.IsChangedCombination = taxon.ChgComb.GetValueOrDefault(false);
            this.IsVerified = !taxon.Unverified.GetValueOrDefault(false);
            this.Rank = rank;
        }

        public KingdomType Kingdom { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public bool IsChangedCombination { get; set; }
        public string Year { get; set; }
        public bool IsVerified { get; set; }
        private string Rank { get; set; }
    }
}
