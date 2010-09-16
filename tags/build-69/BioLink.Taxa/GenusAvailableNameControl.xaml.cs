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
using BioLink.Data;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for GenusAvailableNameControl.xaml
    /// </summary>
    public partial class GenusAvailableNameControl : DatabaseActionControl {

        private GenusAvailableNameViewModel _model;

        #region designer constructor
        public GenusAvailableNameControl() {
            InitializeComponent();
        }
        #endregion

        public GenusAvailableNameControl(TaxonViewModel taxon, User user)
            : base(user, "Taxon::GenusAvailableNames::" + taxon.TaxaID) {

            InitializeComponent();

            txtReference.BindUser(user, LookupType.Reference);
            txtNameStatus.BindUser(user, PickListType.Phrase, "GAN Name Status", TraitCategoryType.Taxon);

            var data = Service.GetGenusAvailableName(taxon.TaxaID.Value);
            if (data == null) {
                data = new GenusAvailableName();
                data.BiotaID = taxon.TaxaID.Value;                
            }
            _model = new GenusAvailableNameViewModel(data);

            this.DataContext = _model;

        }

        protected TaxaService Service { get { return new TaxaService(User); } }
    }


    public class GenusAvailableNameViewModel : GenericViewModelBase<GenusAvailableName> {
        public GenusAvailableNameViewModel(GenusAvailableName model)
            : base(model) {
        }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); } 
        }

        public string RefPage {
            get { return Model.RefPage; }
            set { SetProperty(() => Model.RefPage, value); }
        }

        public string RefQual {
            get { return Model.RefQual; }
            set { SetProperty(() => Model.RefQual, value); }
        }

        public int Designation {
            get { return Model.Designation; }
            set { SetProperty(() => Model.Designation, value); }
        }

        public bool? TSCUChgComb {
            get { return Model.TSCUChgComb; }
            set { SetProperty(() => Model.TSCUChgComb, value); }
        }

        public string TSFixationMethod {
            get { return Model.TSFixationMethod; }
            set { SetProperty(() => Model.TSFixationMethod, value); }
        }

        public int? RefID {
            get { return Model.RefID; }
            set { SetProperty(() => Model.RefID, value); }
        }

        public string TypeSpecies {
            get { return Model.TypeSpecies; }
            set { SetProperty(()=>Model.TypeSpecies, value); }
        }

        public string AvailableNameStatus {
            get { return Model.AvailableNameStatus; }
            set { SetProperty(() => Model.AvailableNameStatus, value); }
        }

    }
}
