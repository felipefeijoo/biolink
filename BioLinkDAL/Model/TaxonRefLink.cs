using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class TaxonRefLink : BioLinkDataObject {

        public int RefLinkID { get; set; }
        public int RefID { get; set; }
        public string RefLink { get; set; }
        public int BiotaID { get; set; }
        public string FullName { get; set; }
        public string RefPage { get; set; }
        public string RefQual { get; set; }
        [MappingInfo("bitUseInReport")]
        public bool UseInReports { get; set; }

        public int Changes { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.RefLinkID; }
        }
    }
}
