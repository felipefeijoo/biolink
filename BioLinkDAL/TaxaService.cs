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
                taxa.Add(TaxonMapper.MapTaxon(reader));
            });

            return taxa;
        }

        public List<Taxon> GetTaxaForParent(int taxonId) {
            List<Taxon> taxa = new List<Taxon>();            
            StoredProcReaderForEach("spBiotaList", (reader) => {
                taxa.Add(TaxonMapper.MapTaxon(reader));
            }, new SqlParameter("intParentId", taxonId));

            return taxa;
        }

        public List<TaxonSearchResult> FindTaxa(string searchTerm) {
            List<TaxonSearchResult> taxa = new List<TaxonSearchResult>();
            StoredProcReaderForEach("spBiotaFind", (reader) => {
                taxa.Add(TaxonMapper.MapTaxonSearchResult(reader));
            }, new SqlParameter("vchrLimitations", ""), new SqlParameter("vchrTaxaToFind", searchTerm + "%"));

            return taxa;
        }

        public List<TaxonRank> GetTaxonRanks() {
            List<TaxonRank> ranks = new List<TaxonRank>();
            StoredProcReaderForEach("spBiotaDefRankGetAll", (reader) => {
                ranks.Add(TaxonMapper.MapTaxonRank(reader));
            });

            return ranks;
        }

        public ValidationResult ValidateTaxonMove(Taxon source, Taxon dest) {
            List<TaxonRank> ranks = GetTaxonRanks();
            Dictionary<string, TaxonRank> map = ranks.ToDictionary((rank) => { return rank.KingdomCode + "_" + rank.Code; });
            if (map.ContainsKey(dest.KingdomCode + "_" + dest.ElemType)) {
                TaxonRank destrank = map[dest.KingdomCode + "_" + dest.ElemType];
                TaxonRank srcrank = map[source.KingdomCode + "_" + source.ElemType];
                if (!IsValidChild(srcrank, destrank)) {
                    return new ValidationResult(false, String.Format("{0} is not a valid child of {1}", srcrank.LongName, destrank.LongName));
                }
            }

            return new ValidationResult(true);
        }

        private HashSet<string> SplitCSV(string list) {
            String[] items = list.Split(',');
            HashSet<string> set = new HashSet<string>();
            foreach (string item in items) {
                if (item.StartsWith("'") && item.EndsWith("'")) {
                    set.Add(item.Substring(1, item.Length - 2));
                } else {
                    set.Add(item);
                }
            }            
            
            return set;
        }

        private bool IsValidChild(TaxonRank src, TaxonRank dest) {
            ISet<string> valid = SplitCSV(dest.ValidChildList);
            return valid.Contains(src.Code, StringComparer.OrdinalIgnoreCase);
        }

    }

    public class ValidationResult {

        public ValidationResult(bool success, string message = null) {
            this.Success = success;
            this.Message = message;
        }

        public bool Success { get; private set; }
        public string Message { get; private set; }
    }
    
}
