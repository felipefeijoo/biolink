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

        public string NameQualifier { get; set; }
        public bool? Shadowed { get; set; }
        public int? ImportedWithProjectID { get; set; }
        public string ParentKingdom { get; set; }
        public string ParentPhylum { get; set; }
        public string ParentClass { get; set; }
        public string ParentOrder { get; set; }
        public string ParentFamily { get; set; }
        public string ParentGenus { get; set; }
        public string ParentSpecies { get; set; }
        public string ParentSubspecies { get; set; }
        public string RankLong { get; set; }
        public string KingdomLong { get; set; }
        public string RankCategory { get; set; }

    }

    public class TaxonSearchResult : Taxon {
        public string CommonName { get; set; }
    }

}
