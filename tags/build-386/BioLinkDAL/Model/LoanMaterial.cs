using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class LoanMaterial : GUIDObject {
        public int LoanMaterialID { get; set; }
        public int LoanID { get; set; }
        public int MaterialID { get; set; }
        public string NumSpecimens { get; set; }
        public string TaxonName { get; set; }
        public string MaterialDescription { get; set; }	
        public DateTime? DateAdded { get; set; }
        public DateTime? DateReturned { get; set; }
        public bool? Returned { get; set; }

        public string MaterialName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => LoanMaterialID; }
        }
    }
}
