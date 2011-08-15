using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;

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

    public class CachedSiteVisit {

        public int SiteVisitID { get; set; }

        public int SiteID { get; set; }
        public string SiteVisitName { get; set; }
        public string Collector { get; set; }
        public int? DateStart { get; set; }
        public int? DateEnd { get; set; }
        public int? TimeStart { get; set; }
        public int? TimeEnd { get; set; }
        public string FieldNumber { get; set; }
        public string CasualDate { get; set; }

        public bool Equals(int siteID, string sitevisitname, string collector, int? dateStart, int? dateEnd, int? timeStart, int? timeEnd, string fieldNumber, string casualDate) {
            if (siteID != SiteID) { return false; }
            if (sitevisitname != this.SiteVisitName) { return false; }
            if (collector != this.Collector) { return false; }
            if (dateStart != this.DateStart) { return false; }
            if (dateEnd != this.DateEnd) { return false; }
            if (timeStart != TimeStart) { return false; }
            if (timeEnd != TimeEnd) { return false; }
            if (fieldNumber != FieldNumber) { return false; }
            if (casualDate != CasualDate) { return false; }

            return true;
        }
    }

    public class TaxonRankValue {

        public TaxonRankValue( TaxonRankName rank, string value) {
            this.Rank = rank;
            this.Value = value;
        }

        public string RankName { 
            get { return Rank.LongName; } 
        }

        public TaxonRankName Rank { get; private set; }

        public string Value { get; set; }

    }

    public class CachedTaxon {

        public CachedTaxon(List<TaxonRankValue> rankValues) {
            this.Ranks = rankValues;
        }

        public List<TaxonRankValue> Ranks { get; private set; }

        public int TaxonID { get; set; }

        public override bool Equals(object obj) {
            var other = obj as CachedTaxon;
            if (other != null) {                
                foreach (TaxonRankValue val in Ranks) {
                    var cmp = other.Ranks.Find((r) => {
                        return r.RankName.Equals(val.RankName, StringComparison.CurrentCultureIgnoreCase);
                    });
                    if (cmp != null) {
                        if (!cmp.Value.Equals(val.Value)) {
                            return false;
                        }
                    } else {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode() {
            return Ranks.GetHashCode();
        }

    }

    public class TaxonCache {

        private List<CachedTaxon> _cache = new List<CachedTaxon>();

        public bool FindInCache(CachedTaxon search, out int taxonID) {

            taxonID = -1;
            foreach (CachedTaxon t in _cache) {
                if (t.Equals(search)) {
                    taxonID = t.TaxonID;
                    return true;
                }
            }

            return false;
        }

        public void AddToCache(CachedTaxon taxon) {
            _cache.Add(taxon);
        }

    }
}
