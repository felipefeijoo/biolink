using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public interface ITaxon {

        System.Nullable<int> TaxaID { get; set; }

        System.Nullable<int> TaxaParentID { get; set; }

        string Epithet { get; set; }

        string TaxaFullName { get; set; }

        string YearOfPub { get; set; }

        string Author { get; set; }

        string ElemType { get; set; }

        string KingdomCode { get; set; }

        System.Nullable<bool> Unplaced { get; set; }

        System.Nullable<int> Order { get; set; }

        string Rank { get; set; }

        System.Nullable<bool> ChgComb { get; set; }

        System.Nullable<bool> Unverified { get; set; }

        System.Nullable<bool> AvailableName { get; set; }

        System.Nullable<bool> LiteratureName { get; set; }

        string NameStatus { get; set; }

        System.Nullable<int> NumChildren { get; set; }

    }

    public class Taxon : BiolinkDataObject, ITaxon {

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

        public System.Nullable<int> NumChildren { get; set; }

    }

    public class TaxonSearchResult : Taxon {
        public string CommonName { get; set; }
    }

}
