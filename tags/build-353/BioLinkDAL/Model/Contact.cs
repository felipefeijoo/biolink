using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Contact : GUIDObject {

        public int ContactID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string GivenName { get; set; }
        public string PostalAddress { get; set; }
        public string StreetAddress { get; set; }
        public string Institution { get; set; }
        public string JobTitle { get; set; }
        public string WorkPh { get; set; }
        public string WorkFax { get; set; }
        public string HomePh { get; set; }
        public string EMail { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => ContactID; }
        }
    }

}
