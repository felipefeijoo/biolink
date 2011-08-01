using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Permission : BioLinkDataObject {

        public int GroupID { get; set; }
        public int PermissionID { get; set; }
        public int Mask1 { get; set; }
        public int Mask2 { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.PermissionID; }
        }
    }
}
