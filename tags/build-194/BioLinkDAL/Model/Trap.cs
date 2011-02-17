using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {
    public class Trap : OwnedDataObject {

        public int TrapID { get; set; }
        public int SiteID { get; set; }
        public string TrapName { get; set; }
        public string TrapType { get; set; }
        public string Description { get; set; }
        public string SiteName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.TrapID; }
        }
    }
}
