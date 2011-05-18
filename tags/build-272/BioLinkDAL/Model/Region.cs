using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {
    public class Region : OwnedDataObject {

        public int PoliticalRegionID { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Parentage { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.PoliticalRegionID; }
        }

    }

}
