using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {
    public class XMLIOMaterialID : BioLinkDataObject {

        public int MaterialID { get; set; }
        public Guid? MaterialGUID { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => MaterialID; }
        }
    }
}
