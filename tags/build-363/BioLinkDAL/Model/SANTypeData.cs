using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class SANTypeData : GUIDObject {

        public int SANTypeDataID { get; set; }
        public int BiotaID { get; set; }
        public string Type { get; set; }
        public string Museum { get; set; }
        public string AccessionNumber { get; set; }
        public string Material { get; set; }
        public string Locality { get; set; }
        public bool IDConfirmed { get; set; }
        public int? MaterialID { get; set; }
        public string MaterialName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.SANTypeDataID; }
        }

    }
}
