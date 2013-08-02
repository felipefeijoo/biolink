using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class SiteGroup : OwnedDataObject {
        public int SiteGroupID { get; set; }
        public int ParentType { get; set; }
        public int ParentID { get; set; }
        public int PoliticalRegionID { get; set; }
        public String SiteGroupName { get; set; }
        public String Parentage { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => SiteGroupID; }
        }
    }

}
