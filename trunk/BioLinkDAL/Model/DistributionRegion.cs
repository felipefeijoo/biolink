using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class DistributionRegion : BioLinkDataObject {

        public int DistRegionID	{ get; set; }
        public int DistRegionParentID { get; set;}
        public string DistRegionName { get; set; }
        public int NumChildren { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => DistRegionID; }
        }

    }
}
