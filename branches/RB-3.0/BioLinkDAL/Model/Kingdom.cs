using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Kingdom : OwnedDataObject {

        public String KingdomCode { get; set; }
        public String KingdomName { get; set; }


        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return null; }
        }

        public override string ToString() {
            return KingdomName;
        }
    }
}
