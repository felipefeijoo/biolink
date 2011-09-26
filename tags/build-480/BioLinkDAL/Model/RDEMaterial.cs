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

namespace BioLink.Data.Model {

    [Serializable()]
    public class RDEMaterial : RDEObject {

        public RDEMaterial() {
            MaterialID = -1;
        }

        public int SiteVisitID { get; set; }	
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }
        public int BiotaID { get; set; }	
        public string TaxaDesc { get; set; }
        public string AccessionNo { get; set; }
        public string RegNo { get; set; }
        public string CollectorNo { get; set; }
        public DateTime? IDDate { get; set; }
        public string Institution { get; set; }
        public string ClassifiedBy { get; set; }
        public string MicroHabitat { get; set; }
        public string MacroHabitat { get; set; }
        public int? TrapID { get; set; }
        public string TrapName { get; set; }
        public string MaterialSource { get; set; }
        public string CollectionMethod { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.MaterialID; }
        }
    }
}
