using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.ComponentModel;

namespace BioLink.Data.Model {

    public class ConnectionProfile : OwnedDataObject {

        public String Name { get; set; }
        public String Server { get; set; }
        public String Database { get; set; }
        public Nullable<int> Timeout { get; set; }
        public bool IntegratedSecurity { get; set; }
        public String LastUser { get; set; }


        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return null; }
        }
    }

}
