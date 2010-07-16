using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class TaxaService : BioLinkService {

        public TaxaService(User user) : base(user) {
        }

        public List<Taxon> GetTopLevelTaxa() {
            List<Taxon> taxa = new List<Taxon>();
            StoredProcReaderForEach("spBiotaListTop", (reader) => {                
                taxa.Add(MapTaxon(reader));
            });

            return taxa;
        }

        private Taxon MapTaxon(SqlDataReader reader) {
            Taxon t = new Taxon();
            ReflectMap(t, reader);
            //t.TaxaID = reader["TaxaID"] as Nullable<int>;
            //t.TaxaParentID = reader["TaxaParentID"] as Nullable<int>;
            //t.TaxaFullName = reader["TaxaFullName"] as string;
            //t.NumChildren = reader["NumChildren"] as Nullable<int>;
            //t.Author = reader["Author"] as string;
            return t;
        }

        public List<Taxon> GetTaxaForParent(int taxonId) {
            List<Taxon> taxa = new List<Taxon>();            
            StoredProcReaderForEach("spBiotaList", (reader) => {
                taxa.Add(MapTaxon(reader));
            }, new SqlParameter("intParentId", taxonId));

            return taxa;
        }

    }
    
}
