using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {

    public class RDESiteVisitViewModel : RDEViewModel<RDESiteVisit> {

        public RDESiteVisitViewModel(RDESiteVisit model) : base(model, ()=>model.SiteVisitID) {
            Material = new ObservableCollection<ViewModelBase>();
        }

        public int SiteVisitID {
            get { return Model.SiteVisitID; }
            set { SetProperty(() => Model.SiteVisitID, value); }
        }

        public int SiteID {
            get { return Model.SiteID; }
            set { SetProperty(() => Model.SiteID, value); }
        }

        public string VisitName {
            get { return Model.VisitName; }
            set { SetProperty(() => Model.VisitName, value); }
        }

        public string Collector {
            get { return Model.Collector; }
            set { SetProperty(() => Model.Collector, value); }
        }

        public int? DateStart {
            get { return Model.DateStart; }
            set { SetProperty(() => Model.DateStart, value); }
        }

        public int? DateEnd {
            get { return Model.DateEnd; }
            set { SetProperty(() => Model.DateEnd, value); }
        }

        public RDESiteViewModel Site { get; set; }

        public ObservableCollection<ViewModelBase> Material { get; set; }

    }
}
