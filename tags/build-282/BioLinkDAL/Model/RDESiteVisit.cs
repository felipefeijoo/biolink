using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    [Serializable()]
    public class RDESiteVisit : RDEObject {

        public RDESiteVisit() {
            SiteVisitID = -1;
            SiteID = -1;
        }

        public int SiteVisitID { get; set; }
        public int SiteID { get; set; }
        public string VisitName { get; set; }
        public string Collector { get; set; }
        public int? DateStart { get; set; }
        public int? DateEnd { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.SiteVisitID; }
        }

    }
}
