using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class XMLIORegion : OwnedDataObject {

        public int PoliticalRegionID { get; set; }
        public string Name { get; set; }
        public int ParentID { get; set; }
        public int Order { get; set; }
        public string Parentage { get; set; }
        public string Rank { get; set; }
        public string FullName { get; set; }
        public string ParentCountry { get; set; }
        public string ParentPrimDiv { get; set; }
        [MappingInfo("vchrparentSecDiv")]
        public string ParentSecDiv { get; set; }
        public Guid? ParentGUID { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => PoliticalRegionID; }
        }

    }
}
