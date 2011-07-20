using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class SiteExplorerNode : BioLinkDataObject {

        public int ElemID { get; set; }
        public string Name { get; set; }
        public int ParentID { get; set; }
        public string ElemType { get; set; }
        public int NumChildren { get; set; }
        public int RegionID { get; set; }
        public string RegionName { get; set; }
        public bool IsTemplate { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.ElemID; }
        }

    }

}
