using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class BiotaPermission : BioLinkDataObject {

        public int PermMask1 { get; set; }
        public int PermMask2 { get; set; }
        public int NumOwners { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return null; }
        }

    }
}
