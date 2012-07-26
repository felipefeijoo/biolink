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
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class CurationEventViewModel : GenericViewModelBase<CurationEvent> {

        public CurationEventViewModel(CurationEvent model) : base(model, ()=>model.CurationEventID) { }

        public override string DisplayLabel {
            get {
                return String.Format("{0}  {1}  {2}", EventType, SubpartName, Who);
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

        public string SubpartName {
            get { return Model.SubpartName; }
            set { SetProperty(() => Model.SubpartName, value); }
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
