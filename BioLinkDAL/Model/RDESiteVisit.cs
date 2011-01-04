using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class RDESiteVisit : RDEObject {

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
