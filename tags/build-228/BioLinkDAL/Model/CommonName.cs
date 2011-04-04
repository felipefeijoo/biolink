using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class CommonName : GUIDObject {

        public int CommonNameID { get; set; }
        public int BiotaID { get; set; }
        public string Name { get; set; }
        public string RefCode { get; set; }
        public string RefPage { get; set; }
        public int? RefID { get; set; }
        public string Notes { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.CommonNameID; }
        }
    }

}
