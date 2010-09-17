using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

        public Taxon GetTaxon(int taxonId) {
            Taxon t = null;
            var mapper = new GenericMapperBuilder<Taxon>().Map("intBiotaID", "TaxaID").Map("intParentID","TaxaParentID").Map("bitChangedComb","ChgComb").Map("vchrAvailableNameStatus", "NameStatus").Map("vchrFullName","TaxaFullName").build();
            StoredProcReaderFirst("spBiotaGet", (reader) => {
                t = mapper.Map(reader);
            }, _P("intBiotaID", taxonId));

            return t;
        }

        public void UpdateTaxon(Taxon taxon) {
            StoredProcUpdate("spBiotaUpdate", 
                _P("intBiotaID", taxon.TaxaID), 
                _P("vchrEpithet", taxon.Epithet, ""),
                _P("vchrAuthor", taxon.Author, ""),
                _P("vchrYearOfPub", taxon.YearOfPub, ""),
                _P("bitChgComb", taxon.ChgComb, 0),
                _P("chrElemType", taxon.ElemType, ""),
                _P("bitUnplaced", taxon.Unplaced, 0),
                _P("bitUnverified", taxon.Unverified, 0),
                _P("vchrRank", taxon.Rank, ""),
                _P("intOrder", taxon.Order, 0),
                _P("chrKingdomCode", taxon.KingdomCode, "A"),
                _P("bitAvailableName", taxon.AvailableName, ""),
                _P("bitLiteratureName", taxon.LiteratureName, ""),
                _P("vchrAvailableNameStatus", taxon.NameStatus, ""));
        }

        public void InsertTaxon(Taxon taxon) {
            SqlParameter retval = ReturnParam("newTaxonId", System.Data.SqlDbType.Int);
            StoredProcUpdate("spBiotaInsert",
                _P("intParentID", taxon.TaxaParentID),
                _P("vchrEpithet", taxon.Epithet),
                _P("vchrAuthor", taxon.Author, ""),
                _P("vchrYearOfPub", taxon.YearOfPub, ""),
                _P("bitChgComb", taxon.ChgComb, 0),
                _P("chrElemType", taxon.ElemType, ""),
                _P("bitUnplaced", taxon.Unplaced, 0),
                _P("vchrRank", taxon.Rank, ""),
                _P("intOrder", taxon.Order, 0),
                _P("bitUnverified", taxon.Unverified, 0),                                
                _P("chrKingdomCode", taxon.KingdomCode, "A"),
                _P("bitAvailableName", taxon.AvailableName, 0),
                _P("bitLiteratureName", taxon.LiteratureName, ""),
                _P("vchrAvailableNameStatus", taxon.NameStatus, ""),
                retval);

            if (retval.Value != null) {
                taxon.TaxaID = (Int32) retval.Value;
            }            
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


        public TaxonRank GetRankByOrder(int order) {

            List<TaxonRank> ranks = GetTaxonRanks();

            foreach (TaxonRank rank in ranks) {
                if (rank.Order == order) {
                    if (rank.Code != "HO") {
                        return rank;
                    }
                }
            }

            return ranks[0];
        }

        public DataMatrix GetStatistics(int taxonId) {
            return StoredProcDataMatrix("spBiotaStatistics", _P("intBiotaId", taxonId));
        }

        public DataMatrix GetMaterialForTaxon(int taxonId) {
            return StoredProcDataMatrix("spMaterialListForTaxon", _P("intBiotaId", taxonId));
        }

        public List<Kingdom> GetKingdomList() {
            var mapper = new GenericMapper<Kingdom>();
            return StoredProcToList("spBiotaDefKingdomList", mapper);
        }

        public AvailableName GetAvailableName(int taxonId) {
            var mapper = new GenericMapperBuilder<AvailableName>().build();
            AvailableName result = null;
            StoredProcReaderFirst("spALNGet", (reader) => {
                result = mapper.Map(reader);
            }, _P("intBiotaID", taxonId));
            return result;
        }

        public void InsertOrUpdateAvailableName(AvailableName name) {
            StoredProcUpdate("spALNInsertUpdate",
                _P("intBiotaID", name.BiotaID),
                _P("intRefID", name.RefID),
                _P("vchrRefPage", name.RefPage),
                _P("vchrAvailableNameStatus", name.AvailableNameStatus),
                _P("txtRefQual", name.RefQual)
            );
        }

        public GenusAvailableName GetGenusAvailableName(int taxonId) {
            var mapper = new GenericMapperBuilder<GenusAvailableName>().build();
            GenusAvailableName result = null;
            StoredProcReaderFirst("spGANGet", (reader) => {
                result = mapper.Map(reader);
            }, _P("intBiotaID", taxonId));
            return result;
        }

        public void InsertOrUpdateGenusAvailableName(GenusAvailableName name) {
            StoredProcUpdate("spGANInsertUpdate",
                _P("intBiotaID", name.BiotaID),
                _P("intRefID", name.RefID),
                _P("vchrRefPage", name.RefPage),
                _P("vchrAvailableNameStatus", name.AvailableNameStatus),
                _P("txtRefQual", name.RefQual),
                _P("sintDesignation", name.Designation),
                _P("vchrTypeSpecies", name.TypeSpecies),
                _P("vchrTSFixationMethod", name.TSFixationMethod)
            );
        }

        public List<GANIncludedSpecies> GetGenusAvailableNameIncludedSpecies(int taxonId) {
            var mapper = new GenericMapperBuilder<GANIncludedSpecies>().build();
            return StoredProcToList("spGANIncludedSpeciesGet", mapper, _P("intBiotaID", taxonId));
        }

        public void DeleteGANIncludedSpecies(int GANISID) {
            StoredProcUpdate("spGANIncludedSpeciesDelete", _P("intGANISID", GANISID));
        }

        public void UpdateGANIncludedSpecies(GANIncludedSpecies model) {
            StoredProcUpdate("spGANIncludedSpeciesUpdate", 
                _P("intGANISID", model.GANISID),
                _P("intBiotaID", model.BiotaID),
                _P("vchrIncludedSpecies", model.IncludedSpecies)
            );
        }

        public void InsertGANIncludedSpecies(GANIncludedSpecies model) {
            var retval = ReturnParam("RetVal", System.Data.SqlDbType.Int);
            StoredProcUpdate("spGANIncludedSpeciesInsert",
                _P("intBiotaID", model.BiotaID),
                _P("vchrIncludedSpecies", model.IncludedSpecies),
                retval
            );
            model.GANISID = (int) retval.Value;
        }

        public List<CommonName> GetCommonNames(int taxonId) {
            var mapper = new GenericMapperBuilder<CommonName>().Map("CommonName", "Name").build();
            return StoredProcToList("spCommonNameGet", mapper, _P("intBiotaID", taxonId));
        }

        public void UpdateCommonName(CommonName commonName) {
            StoredProcUpdate("spCommonNameUpdate",
                _P("intCommonNameID", commonName.CommonNameID),
                _P("intBiotaID", commonName.BiotaID),
                _P("vchrCommonName", commonName.Name),
                _P("intRefID", commonName.RefID, DBNull.Value),
                _P("vchrRefPage", commonName.RefPage),
                _P("txtNotes", commonName.Notes)
            );
        }

        public void InsertCommonName(CommonName commonName) {

            var retval = ReturnParam("RetVal", System.Data.SqlDbType.Int);

            StoredProcUpdate("spCommonNameUpdate",                
                _P("intBiotaID", commonName.BiotaID),
                _P("vchrCommonName", commonName.Name),
                _P("intRefID", commonName.RefID, DBNull.Value),
                _P("vchrRefPage", commonName.RefPage),
                _P("txtNotes", commonName.Notes),
                retval
            );

            commonName.CommonNameID = (int)retval.Value;
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
