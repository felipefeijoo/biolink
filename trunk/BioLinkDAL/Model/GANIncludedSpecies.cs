using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class GANIncludedSpecies : GUIDObject {

        public int GANISID { get; set; }
        public int BiotaID { get; set; }
        public string IncludedSpecies { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.GANISID; }
        }
    }
}
