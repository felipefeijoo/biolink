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
    public class Associate : BioLinkDataObject {

        public int AssociateID { get; set; }
        public int FromIntraCatID { get; set; }
        public int FromCatID { get; set; }
        public int? ToIntraCatID	{ get; set; } 
        public int? ToCatID { get; set; }
        public string AssocDescription { get; set;}
        public string RelationFromTo { get; set; }
        public string RelationToFrom { get; set; }
        public int PoliticalRegionID { get; set; }
        public string Source { get; set; }
        public int RefID { get; set; }
        public string RefPage { get; set; }
        public bool Uncertain { get; set; }
        [IgnoreRTFFormattingChanges]
        public string Notes { get; set; }
        public string AssocName { get; set; }
        public string RefCode { get; set; }
        public string PoliticalRegion { get; set;}
        public string Direction { get; set; }
        public string FromCategory { get; set; }
        public string ToCategory { get; set; }
        public Guid AssocGUID { get; set; }
        public Guid RegionGUID { get; set; }

        public int Changes { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.AssociateID; }
        }
    }
}
