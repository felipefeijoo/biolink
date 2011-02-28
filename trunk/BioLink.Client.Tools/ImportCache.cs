using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Tools {

    public class CachedRegion {

        public int RegionID { get; set; }

        public string PoliticalRegion { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string County { get; set; }

        public bool Equals(string political, string country, string state, string county) {
            if (political != this.PoliticalRegion) return false;
            if (country != this.Country) return false;
            if (state != this.State) return false;
            if (county != this.County) return false;

            return true;
        }

    }

    public class CachedSite {

        public int SiteID { get; set; }

        public string Name { get; set; }
        public string Locality { get; set; }
        public string OffsetDistance { get; set; }
        public string OffsetDirection { get; set; }
        public string InformalLocality { get; set; }
        public int LocalityType { get; set; }
        public double? X1 { get; set; }
        public double? Y1 { get; set; }
        public double? X2 { get; set; }
        public double? Y2 { get; set; }

        public bool Equals(string name, string locality, string offsetDist, string offsetDir, string informalLocality, int localityType, double? x1, double? y1, double? x2, double? y2) {
            if (name != Name) { return false; }
            if (locality != Locality) { return false; }
            if (offsetDist != OffsetDistance) { return false; }
            if (offsetDir != OffsetDirection) { return false; }
            if (informalLocality != InformalLocality) { return false; }
            if (localityType != LocalityType) { return false; }
            if (x1 != X1) { return false; }
            if (y1 != Y1) { return false; }
            if (x2 != X2) { return false; }
            if (y2 != Y2) { return false; }

            return true;
        }


    }
}
