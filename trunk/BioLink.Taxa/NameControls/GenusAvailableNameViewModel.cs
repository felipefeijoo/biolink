using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class GenusAvailableNameViewModel : GenericViewModelBase<GenusAvailableName> {

        public GenusAvailableNameViewModel(GenusAvailableName model) : base(model, null) { }

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
            set { SetProperty(() => Model.TypeSpecies, value); }
        }

        public string AvailableNameStatus {
            get { return Model.AvailableNameStatus; }
            set { SetProperty(() => Model.AvailableNameStatus, value); }
        }

    }

    public class GANIncludedSpeciesViewModel : GenericViewModelBase<GANIncludedSpecies> {
        public GANIncludedSpeciesViewModel(GANIncludedSpecies model) : base(model, ()=>model.GANISID) { }

        public int GANISID {
            get { return Model.GANISID; }
            set { SetProperty(() => Model.GANISID, value); }
        }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public string IncludedSpecies {
            get { return Model.IncludedSpecies; }
            set { SetProperty(() => Model.IncludedSpecies, value); }
        }

    }

}
