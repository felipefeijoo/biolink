using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class CurationEventViewModel : GenericViewModelBase<CurationEvent> {

        public CurationEventViewModel(CurationEvent model)
            : base(model) {
                DataChanged += new DataChangedHandler(CurationEventViewModel_DataChanged);
        }

        void CurationEventViewModel_DataChanged(ChangeableModelBase viewmodel) {
            RaisePropertyChanged("DisplayLabel");
        }

        public override string DisplayLabel {
            get {
                return String.Format("{0}  {1}  {2}", EventType, SubPartName, Who);
            }
            set {
                base.DisplayLabel = value;
            }
        }

        public int CurationEventID {
            get { return Model.CurationEventID; }
            set { SetProperty(() => Model.CurationEventID, value); }
        }

        public int MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public string SubPartName {
            get { return Model.SubPartName; }
            set { SetProperty(() => Model.SubPartName, value); }
        }

        public string Who {
            get { return Model.Who; }
            set { SetProperty(() => Model.Who, value); }
        }

        public DateTime? When {
            get { return Model.When; }
            set { SetProperty(() => Model.When, value); }
        }

        public string EventType {
            get { return Model.EventType; }
            set { SetProperty(() => Model.EventType, value); }
        }

        public string EventDesc {
            get { return Model.EventDesc; }
            set { SetProperty(() => Model.EventDesc, value); }
        }

    }
}
