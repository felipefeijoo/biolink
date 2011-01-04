using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {
    public class AvailableName : GUIDObject {

        public int BiotaID { get; set; }
        public int? RefID { get; set; }
        public string RefPage { get; set; }
        public string RefQual { get; set; }
        public string RefCode { get; set; }
        public string AvailableNameStatus { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return null; }
        }
    }
}
