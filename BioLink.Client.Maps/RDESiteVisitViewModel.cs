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

namespace BioLink.Client.Maps {
    public class RDESiteVisitViewModel : GenericViewModelBase<RDESiteVisit> {

        public RDESiteVisitViewModel(RDESiteVisit model) : base(model, ()=>model.SiteVisitID) {}

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

        public int? Changes {
            get { return Model.Changes; }
            set { SetProperty(() => Model.Changes, value); }
        }

        public int? TemplateID {
            get { return Model.TemplateID; }
            set { SetProperty(() => Model.TemplateID, value); }
        }

        public bool Locked {
            get { return Model.Locked; }
            set { SetProperty(() => Model.Locked, value); }
        }

        public RDESiteViewModel Site { get; set; }

    }
}
