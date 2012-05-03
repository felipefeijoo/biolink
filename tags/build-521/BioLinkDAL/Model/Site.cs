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

    public class Site : OwnedDataObject {

        public int SiteID { get; set; }
        public string SiteName { get; set; }
        public int PoliticalRegionID { get; set; }
        public int SiteGroupID { get; set; }
        [MappingInfo("tintLocalType")]
        public int LocalityType { get; set; }
        [MappingInfo("vchrLocal")]
        public string Locality { get; set; }
        public string DistanceFromPlace { get; set; }
        public string DirFromPlace { get; set; }
        public string InformalLocal { get; set; }
        public int PosCoordinates { get; set; }
        public int PosAreaType { get; set; }
        public double? PosY1 { get; set; }
        public double? PosX1 { get; set; }
        public double? PosY2 { get; set; }
        public double? PosX2 { get; set; }
        public int PosXYDisplayFormat { get; set; }
        public string PosSource { get; set; }
        public string PosError { get; set; }
        public string PosWho { get; set; }
        public string PosDate { get; set; }
        public string PosOriginal { get; set; }
        public string PosUTMSource { get; set; }
        public string PosUTMMapProj { get; set; }
        public string PosUTMMapName { get; set; }
        public string PosUTMMapVer { get; set; }
        public int ElevType { get; set; }
        public double? ElevUpper { get; set; }
        public double? ElevLower { get; set; }
        public double? ElevDepth { get; set; }
        public string ElevUnits { get; set; }
        public string ElevSource { get; set; }
        public string ElevError { get; set; }
        public string GeoEra { get; set; }
        public string GeoState { get; set; }
        public string GeoPlate { get; set; }
        public string GeoFormation { get; set; }
        public string GeoMember { get; set; }
        public string GeoBed { get; set; }
        public string GeoName { get; set; }
        public string GeoAgeBottom { get; set; }
        public string GeoAgeTop { get; set; }
        public string GeoNotes { get; set; }
        public int Order { get; set; }
        [MappingInfo("tintTemplate")]
        public bool IsTemplate { get; set; }
        public string PoliticalRegion { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.SiteID; }
        }
    }
}
