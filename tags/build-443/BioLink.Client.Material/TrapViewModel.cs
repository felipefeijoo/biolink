using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class TrapViewModel : GenericViewModelBase<Trap> {

        public TrapViewModel(Trap model) : base(model, ()=>model.TrapID) { }

        public int TrapID {
            get { return Model.TrapID; }
            set { SetProperty(() => Model.TrapID, value); }
        }

        public int SiteID {
            get { return Model.SiteID; }
            set { SetProperty(() => Model.SiteID, value); }
        }

        public string TrapName {
            get { return Model.TrapName; }
            set { SetProperty(() => Model.TrapName, value); }
        }

        public string TrapType {
            get { return Model.TrapType; }
            set { SetProperty(() => Model.TrapType, value); }
        }

        public string Description {
            get { return Model.Description; }
            set { SetProperty(() => Model.Description, value); }
        }

        public string SiteName {
            get { return Model.SiteName; }
            set { SetProperty(() => Model.SiteName, value); }
        }

    }
}
