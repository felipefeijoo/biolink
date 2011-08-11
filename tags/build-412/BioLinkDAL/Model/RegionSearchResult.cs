using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class RegionSearchResult : BioLinkDataObject {

        public int RegionID { get; set; }
        public string Region { get; set; }
        public int ParentID { get; set; }
        public int NumChildren { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.RegionID; }
        }

    }
}
