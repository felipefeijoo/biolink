using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class TaxonDistribution : GUIDObject {

        public int TaxonID { get; set; }
        public int BiotaDistID { get; set; }
        public int DistRegionID { get; set; }
        public bool Introduced { get; set; }
        public bool Uncertain { get; set; }
        public bool ThroughoutRegion { get; set; }
        public string Qual { get; set; }
        public string DistRegionFullPath { get; set; }

        public int Changes { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.BiotaDistID; }
        }
    }
}
