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

namespace BioLink.Client.Taxa {
    public class TypeListReport : TaxonReportBase {

        public TypeListReport(User user, TaxonViewModel taxon) : base(user, taxon) {
            RegisterViewer(new TabularDataViewerSource());
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            return Service.GetTaxonTypes(Taxon.TaxaID.Value);
        }

        public override string Name {
            get { return string.Format("Taxon Type list: {0}", Taxon.DisplayLabel); }
        }

    }
}
