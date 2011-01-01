using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public class RefLinkViewModel : GenericViewModelBase<RefLink> {

        public RefLinkViewModel(RefLink model) : base(model, ()=>model.RefLinkID) { }

        public override string DisplayLabel {
            get { return string.Format("RefLink: {0}->{1}", IntraCatID, RefID); }
        }

        public int RefLinkID {
            get { return Model.RefLinkID; } 
            set { SetProperty(()=>Model.RefLinkID, value); }
        }

        public int? IntraCatID {
            get { return Model.IntraCatID; }
            set { SetProperty(() => Model.IntraCatID, value); }
        }

        public int RefID {
            get { return Model.RefID; }
            set { SetProperty(() => Model.RefID, value); }
        }

        public string RefPage {
            get { return Model.RefPage; }
            set { SetProperty(() => Model.RefPage, value); }
        }

        public string RefQual {
            get { return Model.RefQual; }
            set { SetProperty(() => Model.RefQual, value); }
        }

        public int? Order {
            get { return Model.Order; }
            set { SetProperty(() => Model.Order, value); }
        }

        public bool? UseInReport {
            get { return Model.UseInReport; }
            set { SetProperty(()=>Model.UseInReport, value); }
        }

        public string RefLinkType {
            get { return Model.RefLinkType; }
            set { SetProperty(() => Model.RefLinkType, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string FullRTF {
            get { return Model.FullRTF; }
            set { SetProperty(() => Model.FullRTF, value); }
        }

        public int? StartPage {
            get { return Model.StartPage; }
            set { SetProperty(() => Model.StartPage, value); }
        }

        public int? EndPage {
            get { return Model.EndPage; }
            set { SetProperty(() => Model.EndPage, value); }
        }

    }
}
