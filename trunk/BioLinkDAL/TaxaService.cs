using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;


namespace BioLink.Data {

    public class TaxaService : BioLinkService {

        public TaxaService(User user) : base(user) {
        }

        public List<Taxon> GetTopLevelTaxa() {
            List<Taxon> taxa = new List<Taxon>();
            StoredProcReaderForEach("spBiotaListTop", (reader) => {
                Taxon t = new Taxon();
                t.TaxaID = reader["TaxaID"] as Nullable<int>;
                t.TaxaParentID = reader["TaxaParentID"] as Nullable<int>;
                t.TaxaFullName = reader["TaxaFullName"] as string;
                t.NumChildren = reader["NumChildren"] as Nullable<int>;
                taxa.Add(t);
            });

            return taxa;
        }

    }

    public abstract class BiolinkDataObject {
    }

    public class Taxon : BiolinkDataObject {

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
}
