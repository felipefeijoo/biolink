using System.Windows;
using System.Windows.Controls;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Collections.Generic;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonNameDetails.xaml
    /// </summary>
    public partial class TaxonNameDetails : DatabaseActionControl {
        
        private TaxonRank _rank;
        private TaxonNameViewModel _model;
        private List<Kingdom> _kingdomList;

        #region DesignerConstructor
        public TaxonNameDetails() {
            InitializeComponent();
        }
        #endregion

        public TaxonNameDetails(int? taxonId, User user)  : base(user, "TaxonNameDetails::" + taxonId.Value) {
            var service = new TaxaService(user);
            Taxon taxon = service.GetTaxon(taxonId.Value);
            _rank = service.GetTaxonRank(taxon.ElemType);
            _kingdomList = service.GetKingdomList();
            Kingdom kingdom = _kingdomList.Find((k) => k.KingdomCode.Equals(taxon.KingdomCode));

            _model = new TaxonNameViewModel(taxon, kingdom, _rank);

            _model.DataChanged += new DataChangedHandler(_model_DataChanged);

            InitializeComponent();

            cmbKingdom.ItemsSource = _kingdomList;

            this.chkChangedCombination.Visibility = (_rank.Category == "S" ? Visibility.Visible : Visibility.Hidden);

            if (taxon.AvailableName.ValueOrFalse() || taxon.LiteratureName.ValueOrFalse()) {

                string phraseCategory = "ALN Name Status";
                chkChangedCombination.Visibility = System.Windows.Visibility.Hidden;
                if (taxon.AvailableName.ValueOrFalse()) {
                    TaxonRank rank = service.GetTaxonRank(taxon.ElemType);
                
                    switch (rank.Category.ToLower()) {
                        case "g": phraseCategory = "GAN Name Status";
                            break;
                        case "s": phraseCategory = "SAN Name Status";
                            break;
                    }
                }

                txtNameStatus.BindUser(PluginManager.Instance.User,  PickListType.Phrase, phraseCategory, TraitCategoryType.Taxon);
            } else {
                txtNameStatus.Visibility = System.Windows.Visibility.Collapsed;
                lblNameStatus.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.DataContext = _model;
        }

        void _model_DataChanged(ChangeableModelBase model) {
            RegisterUniquePendingChange(new UpdateTaxonDatabaseAction(_model.Taxon));
        }

    }

    class TaxonNameViewModel : TaxonViewModel {

        private Kingdom _kingdom;
        private TaxonRank _rank;

        public TaxonNameViewModel(Taxon taxon, Kingdom kingdom, TaxonRank rank) : base(null, taxon, null) {
            _kingdom = kingdom;
            _rank = rank;
        }

        public Kingdom Kingdom {
            get { return _kingdom; }
            set { 
                SetProperty("Kingdom", ref _kingdom, value); 
                base.KingdomCode = _kingdom.KingdomCode;
            }
        }        

        public string RankLongName {
            get { return _rank.GetElementTypeLongName(this.Taxon); }            
        }

        public bool IsVerified {
            get { return !Taxon.Unverified.ValueOrFalse(); }
            set { SetProperty(()=> Taxon.Unverified, Taxon, !value); }
        }

    }
}
