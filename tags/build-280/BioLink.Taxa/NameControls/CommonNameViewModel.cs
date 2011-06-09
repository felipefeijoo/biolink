using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Taxa {

    public class CommonNameViewModel : GenericViewModelBase<CommonName> {

        public CommonNameViewModel(CommonName model) : base(model, ()=>model.CommonNameID) { }

        public override string DisplayLabel {
            get {
                return GenerateDisplayLabel();
            }
        }

        public int CommonNameID {
            get { return Model.CommonNameID; }
            set { SetProperty(() => Model.CommonNameID, value); }
        }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set {                
                SetProperty(() => Model.Name, value);
                GenerateDisplayLabel();
            }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string RefPage {
            get { return Model.RefPage; }
            set { SetProperty(() => Model.RefPage, value); }
        }

        public int? RefID {
            get { return Model.RefID; }
            set {                
                SetProperty(() => Model.RefID, value);
                GenerateDisplayLabel();
            }
        }

        public string Notes {
            get { return Model.Notes; }
            set { SetProperty(() => Model.Notes, value); }
        }

        private string GenerateDisplayLabel() {
            string label;
            if (string.IsNullOrEmpty(RefCode)) {
                label = Name;
            } else {
                label = String.Format("{0} ({1})", Name, RefCode);
            }
            return label;
        }

    }

}
