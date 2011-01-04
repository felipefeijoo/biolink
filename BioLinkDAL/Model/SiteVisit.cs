using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class SiteVisit : OwnedDataObject {

        public int SiteVisitID { get; set; }
        public int SiteID { get; set; }
        public string SiteVisitName { get; set; }
        public string FieldNumber { get; set; }
        public string Collector { get; set; }
        public int DateType { get; set; }
        public int? DateStart { get; set; }
        public int? DateEnd { get; set; }
        public int? TimeStart { get; set; }
        public int? TimeEnd { get; set; }
        public string CasualTime { get; set; }
        [MappingInfo("tintTemplate")]
        public bool IsTemplate { get; set; }
        public string SiteName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.SiteVisitID; }
        }
    }
}
