using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class LookupResult : BioLinkDataObject {

        public int LookupObjectID { get; set; }
        public string Label { get; set; }
        public LookupType LookupType { get; set; }
        
        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => LookupObjectID; }
        }
    
    }
}
