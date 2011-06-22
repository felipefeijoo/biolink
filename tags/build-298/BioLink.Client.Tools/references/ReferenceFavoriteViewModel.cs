using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Tools {

    public class ReferenceFavoriteViewModel : FavoriteViewModel<ReferenceFavorite> {

        public ReferenceFavoriteViewModel(ReferenceFavorite model)
            : base(model) {
        }

        protected override string RelativeImagePath {
            get { return @"images\Reference.png"; }
        }

        public override string DisplayLabel {
            get { return IsGroup ? GroupName : RefCode; }
        }

        public int RefID {
            get { return Model.RefID; }
            set { SetProperty(()=>Model.RefID, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string FullRTF {
            get { return Model.FullRTF; }
            set { SetProperty(() => Model.FullRTF, value); }
        }        


    }
}
