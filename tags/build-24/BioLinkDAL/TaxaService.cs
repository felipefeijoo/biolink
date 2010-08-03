using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class TaxaService : BioLinkService {

        public static string SPECIES_INQUIRENDA = "SI";
        public static string INCERTAE_SEDIS = "IS";

        private List<TaxonRank> _rankList;
        private byte[] _rankLock = new byte[] { };

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

        /// <summary>
        /// The rank list is cached because it rarely, if ever, changes.
        /// </summary>
        /// <returns></returns>
        public List<TaxonRank> GetTaxonRanks() {
            lock (_rankLock) {
                if (_rankList == null) {
                    _rankList = new List<TaxonRank>();
                    StoredProcReaderForEach("spBiotaDefRankGetAll", (reader) => {
                        _rankList.Add(TaxonMapper.MapTaxonRank(reader));
                    });
                    
                }
            }
            return _rankList;
        }

        public Dictionary<string, TaxonRank> GetTaxonRankMap() {
            return GetTaxonRanks().ToDictionary((rank) => { return RankKey(rank); });
        }

        public DataValidationResult ValidateTaxonMove(Taxon source, Taxon dest) {
            var map = GetTaxonRankMap();

            // Can only really validate if the ranks of the source and target are 'known'
            if (map.ContainsKey(RankKey(dest)) && map.ContainsKey(RankKey(source))) {
                TaxonRank destrank = map[RankKey(dest)];
                TaxonRank srcrank = map[RankKey(source)];
                if (!IsValidChild(srcrank, destrank)) {
                    return new DataValidationResult(false, String.Format("{0} is not a valid child of {1}", srcrank.LongName, destrank.LongName));
                }
            }

            return new DataValidationResult(true);
        }

        public TaxonRank GetTaxonRank(string elemType) {
            foreach (TaxonRank rank in GetTaxonRanks()) {
                if (rank.Code == elemType) {
                    return rank;
                }
            }
            return null;
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

        public bool IsValidChild(TaxonRank src, TaxonRank dest) {
            ISet<string> valid = SplitCSV(dest.ValidChildList);
            return valid.Contains(src.Code, StringComparer.OrdinalIgnoreCase);
        }

        private string RankKey(string kingdomCode, string rankCode) {
            return kingdomCode + "_" + rankCode;
        }

        private string RankKey(TaxonRank rank) {
            return RankKey(rank.KingdomCode, rank.Code);
        }

        private string RankKey(Taxon taxon) {
            return RankKey(taxon.KingdomCode, taxon.ElemType);
        }

        public List<TaxonRank> GetChildRanks(TaxonRank targetRank) {
            var map = GetTaxonRankMap();
            string[] valid = targetRank.ValidChildList.Split(',');
            List<TaxonRank> result = new List<TaxonRank>();
            foreach (string child in valid) {
                string elemType = child;
                if (child.StartsWith("'") && child.EndsWith("'")) {
                    elemType = child.Substring(1, child.Length - 2);
                }
                string key = RankKey(targetRank.KingdomCode, elemType);
                if (map.ContainsKey(key)) {
                    result.Add(map[key]);
                }
            }
            return result;
        }

        public string GetTaxonParentage(int taxaId) {
            string parentage = null;            
            StoredProcReaderFirst("spBiotaGetParentage", (reader) => {
                parentage = reader[0] as string;
            }, new SqlParameter("intBiotaID", taxaId));

            return parentage;
        }

        public List<Taxon> GetExpandFullTree(int taxonId) {
            List<Taxon> taxa = new List<Taxon>();
            StoredProcReaderForEach("spBiotaListFullTree", (reader) => {
                taxa.Add(TaxonMapper.MapTaxon(reader, new ConvertingMapper("NumChildren", (elem) => { return Int32.Parse(elem == null ? "-1" : elem.ToString()); })));
            }, new SqlParameter("intParentId", taxonId));

            return taxa;
        }

        public void MoveTaxon(int taxonId, int newParentId) {
            StoredProcUpdate("spBiotaMove", _P("intTaxaID", taxonId), _P("intNewParentID", newParentId));           
        }

        public void UpdateTaxon(Taxon taxon) {
            StoredProcUpdate("spBiotaUpdate", 
                _P("intBiotaID", taxon.TaxaID), 
                _P("vchrEpithet", taxon.Epithet),
                _P("vchrAuthor", taxon.Author),
                _P("vchrYearOfPub", taxon.YearOfPub),
                _P("bitChgComb", taxon.ChgComb),
                _P("chrElemType", taxon.ElemType),
                _P("bitUnplaced", taxon.Unplaced),
                _P("bitUnverified", taxon.Unverified),
                _P("vchrRank", taxon.Rank),
                _P("intOrder", taxon.Order),
                _P("chrKingdomCode", taxon.KingdomCode),
                _P("bitAvailableName", taxon.AvailableName),
                _P("bitLiteratureName", taxon.LiteratureName),
                _P("vchrAvailableNameStatus", taxon.NameStatus, ""));
        }

        public void MergeTaxon(int sourceId, int targetId, bool createNewIDRecord) {
            StoredProcUpdate("spBiotaPreDeleteMerge",
                _P("intRemovedBiotaID", sourceId),
                _P("intMergedWithBiotaID", targetId),
                _P("bitCreateNewIDRecord", createNewIDRecord));
        }

        public void DeleteTaxon(int taxonId) {
            StoredProcUpdate("spBiotaDelete", _P("intTaxaID", taxonId));
        }

    }

    public class DataValidationResult {

        public DataValidationResult(bool success, string message = null) {
            this.Success = success;
            this.Message = message;
        }

        public bool Success { get; private set; }
        public string Message { get; private set; }
    }
    
}
