using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Maps {

    public class RDEMaterialViewModel : GenericViewModelBase<RDEMaterial> {

        public RDEMaterialViewModel(RDEMaterial model) : base(model, ()=>model.MaterialID) { }

        public int SiteVisitID {
            get { return Model.SiteVisitID; }
            set { SetProperty(() => Model.SiteVisitID, value); }
        }

        public int MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public string MaterialName {
            get { return Model.MaterialName; }
            set { SetProperty(() => Model.MaterialName, value); }
        }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public string TaxaDesc {
            get { return Model.TaxaDesc; }
            set { SetProperty(() => Model.TaxaDesc, value); }
        }

        public string AccessionNo {
            get { return Model.AccessionNo; }
            set { SetProperty(() => Model.AccessionNo, value); }
        }

        public string RegNo {
            get { return Model.RegNo; }
            set { SetProperty(() => Model.RegNo, value); }
        }

        public string CollectorNo {
            get { return Model.CollectorNo; }
            set { SetProperty(() => Model.CollectorNo, value); }
        }

        public DateTime? IDDate {
            get { return Model.IDDate; }
            set { SetProperty(() => Model.IDDate, value); }
        }

        public string Institution {
            get { return Model.Institution; }
            set { SetProperty(() => Model.Institution, value); }
        }

        public string ClassifiedBy {
            get { return Model.ClassifiedBy; }
            set { SetProperty(() => Model.ClassifiedBy, value); }
        }

        public string MicroHabitat {
            get { return Model.MicroHabitat; }
            set { SetProperty(() => Model.MicroHabitat, value); }
        }

        public string MacroHabitat {
            get { return Model.MacroHabitat; }
            set { SetProperty(() => Model.MacroHabitat, value); }
        }

        public int? TrapID {
            get { return Model.TrapID; }
            set { SetProperty(() => Model.TrapID, value); }
        }

        public string TrapName {
            get { return Model.TrapName; }
            set { SetProperty(() => Model.TrapName, value); }
        }

        public string MaterialSource {
            get { return Model.MaterialSource; }
            set { SetProperty(() => Model.MaterialSource, value); }
        }

        public string CollectionMethod {
            get { return Model.CollectionMethod; }
            set { SetProperty(() => Model.CollectionMethod, value); }
        }

        public int? Changes {
            get { return Model.Changes; }
            set { SetProperty(() => Model.Changes, value); }
        }

        public bool Locked {
            get { return Model.Locked; }
            set { SetProperty(() => Model.Locked, value); }
        }

        public int? TemplateID {
            get { return Model.TemplateID; }
            set { SetProperty(() => Model.TemplateID, value); }
        }

        public RDESiteVisitViewModel SiteVisit { get; set; }

    }
}
