using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class TaxonRankName : BioLinkDataObject {
        
        public string Code { get; set; }
        public string LongName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => 0; }
        }

    }
}
