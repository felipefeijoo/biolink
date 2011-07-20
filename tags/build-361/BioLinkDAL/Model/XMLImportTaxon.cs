using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportTaxon : XMLImportObject {

        public XMLImportTaxon() : base(null) { // Should actuall never be called! 
            throw new Exception("This ctor should not be called!");
        }

        public XMLImportTaxon(XmlElement node) : base(node) { }

        public string Rank { get; set; }
        public string Kingdom { get; set; }
        public string RankCategory { get; set; }

        public int BiotaID { get; set; }
        public int ParentID { get; set; }

        public string Epithet { get; set; }
        public string YearOfPub { get; set; }
        public string Author { get; set; }
        public string NameQualifier { get; set; }
        public string ElemType { get; set; }        
        public System.Nullable<int> Order { get; set; }
        public string Parentage { get; set; }
        public System.Nullable<bool> ChangedComb { get; set; }
        public System.Nullable<bool> Shadowed { get; set; }
        public System.Nullable<bool> Unplaced { get; set; }
        public System.Nullable<bool> Unverified { get; set; }
        public System.Nullable<bool> AvailableName { get; set; }
        public System.Nullable<bool> LiteratureName { get; set; }
        public string FullName { get; set; }
        public string KingdomCode { get; set; }
        public DateTime DateCreated { get; set; }
        public string WhoCreated { get; set; }
        public DateTime DateLastUpdated { get; set; }
        public string WhoLastUpdated { get; set; }
        public string DistQual { get; set; }
        public string AvailableNameStatus { get; set; }        
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

        [MappingInfo("GUID")]
        public Guid GUIDObj { get; set; }

    }

}
