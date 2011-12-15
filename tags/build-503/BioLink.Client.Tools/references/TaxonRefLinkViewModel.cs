/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class TaxonRefLinkViewModel : GenericViewModelBase<TaxonRefLink> {

        public TaxonRefLinkViewModel(TaxonRefLink model) : base(model, ()=>model.RefLinkID) { }

        public override string DisplayLabel {
            get {
                return string.Format("{0} ({1}) pg. {2}", FullName, RefLink, RefPage);
            }
        }

        public int RefLinkID {
            get { return Model.RefLinkID; }
            set { SetProperty(() => Model.RefLinkID, value); }
        }

        public string RefLink {
            get { return Model.RefLink; }
            set { SetProperty(() => Model.RefLink, value); }
        }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public string FullName {
            get { return Model.FullName; }
            set { SetProperty(() => Model.FullName, value); }
        }

        public string RefPage {
            get { return Model.RefPage; }
            set { SetProperty(() => Model.RefPage, value); }
        }

        public string RefQual {
            get { return Model.RefQual; }
            set { SetProperty(() => Model.RefQual, value); }
        }

        public bool UseInReports {
            get { return Model.UseInReports; }
            set { SetProperty(() => Model.UseInReports, value); }
        }
        
    }
}
