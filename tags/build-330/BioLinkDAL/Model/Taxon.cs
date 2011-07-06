using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Taxon : OwnedDataObject {

        public System.Nullable<int> TaxaID { get; set; }

        public System.Nullable<int> TaxaParentID { get; set; }

        public string Epithet { get; set; }

        public string TaxaFullName { get; set; }

        public string YearOfPub { get; set; }

        public string Author { get; set; }
        
        public string ElemType { get; set; }

        public string KingdomCode { get; set; }

        public System.Nullable<bool> Unplaced { get; set; }

        public System.Nullable<int> Order { get; set; }

        public string Rank { get; set; }

        public System.Nullable<bool> ChgComb { get; set; }

        public System.Nullable<bool> Unverified { get; set; }

        public System.Nullable<bool> AvailableName { get; set; }

        public System.Nullable<bool> LiteratureName { get; set; }

        public string NameStatus { get; set; }

        public int NumChildren { get; set; }

        public string Parentage { get; set; }

        public string DistQual { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.TaxaID.Value; }
        }

    }

    public class TaxonSearchResult : Taxon {
        public string CommonName { get; set; }
    }

}
