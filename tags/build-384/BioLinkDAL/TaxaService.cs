using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Text;

namespace BioLink.Data {

    public class TaxaService : BioLinkService {

        public static string SPECIES_INQUIRENDA = "SI";
        public static string INCERTAE_SEDIS = "IS";

        private static List<TaxonRank> _rankList;
        private static byte[] _rankLock = new byte[] { };

        public TaxaService(User user) : base(user) {
        }

        public List<Taxon> GetTopLevelTaxa() {
            List<Taxon> taxa = new List<Taxon>();
            StoredProcReaderForEach("spBiotaListTop", (reader) => {                
                taxa.Add(TaxonMapper.MapTaxon(reader));
            });

            return taxa;
        }

        public List<int> GetChildrenIds(int parentId) {
            var result = new List<int>();
            StoredProcReaderForEach("spBiotaGetChildrenID", (reader) => {
                result.Add((int) reader[0]);
            }, _P("intParentID", parentId));

            return result;
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

            searchTerm = EscapeSearchTerm(searchTerm, true); /// * are wildcards, replace them with the SQL wildcard

            StoredProcReaderForEach("spBiotaFind", (reader) => {
                taxa.Add(TaxonMapper.MapTaxonSearchResult(reader));
            }, new SqlParameter("vchrLimitations", ""), new SqlParameter("vchrTaxaToFind", searchTerm));

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

        public List<TaxonRankName> GetOrderedRanks() {
            var mapper = new GenericMapperBuilder<TaxonRankName>().build();
            return StoredProcToList<TaxonRankName>("spBiotaRankList", mapper);
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

        private GenericMapper<Taxon> BuildTaxonMapper() {
            return new GenericMapperBuilder<Taxon>().Map("intBiotaID", "TaxaID").Map("intParentID", "TaxaParentID").Map("bitChangedComb", "ChgComb").Map("vchrAvailableNameStatus", "NameStatus").Map("vchrFullName", "TaxaFullName").build();
        }

        public Taxon GetTaxon(int taxonId) {
            Taxon t = null;
            var mapper = BuildTaxonMapper();
            StoredProcReaderFirst("spBiotaGet", (reader) => {
                t = mapper.Map(reader);
            }, _P("intBiotaID", taxonId));

            return t;
        }

        public TaxonStatistics GetTaxonStatistics(int taxonId) {
            var map = new Dictionary<string, int>();
            int total = 0;
            StoredProcReaderForEach("spBiotaStatistics", (reader) => {
                int count = (int)reader["Count"];
                map[reader["Category"] as string] = count;
                total += count;

            }, _P("intBiotaID", taxonId));

            var stats = new TaxonStatistics();
            
            stats.Sites = (map.ContainsKey("Sites") ? map["Sites"] : 0);
            stats.SiteVisits = (map.ContainsKey("Site Visits") ? map["Site Visits"] : 0);
            stats.Material = (map.ContainsKey("Material") ? map["Material"] : 0);
            stats.Specimens = (map.ContainsKey("Specimens") ? map["Specimens"] : 0);
            stats.Multimedia = (map.ContainsKey("Multimedia Items") ? map["Multimedia Items"] : 0);
            stats.TypeSpecimens = (map.ContainsKey("Type Specimens") ? map["Type Specimens"] : 0);
            stats.Notes = (map.ContainsKey("Notes") ? map["Notes"] : 0);
            stats.References = (map.ContainsKey("References") ? map["References"] : 0);

            stats.TotalItems = total;

            return stats;
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
            return StoredProcDataMatrix("spBiotaStatistics", null, _P("intBiotaId", taxonId));
        }

        public DataMatrix GetMaterialForTaxon(int taxonId) {
            return StoredProcDataMatrix("spMaterialListForTaxon", null, _P("intBiotaId", taxonId));
        }

        public DataMatrix GetTaxonTypes(int taxonId) {
            return StoredProcDataMatrix("spBiotaListTypes", null, _P("BiotaID", taxonId));
        }

        public DataMatrix GetAssociatesForTaxa(int regionID, params int[] taxonIds) {
            var strTaxonIDS = taxonIds.Join(",");
            return StoredProcDataMatrix("spAssociatesListForTaxon", null, _P("intPoliticalRegionID", regionID), _P("vchrBiotaID", strTaxonIDS));
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

        public string GetKingdomName(string kingdomCode) {
            switch (kingdomCode.ToLower()) {
                case "a":
                    return "Animalia";
                case "p":
                    return "Plantae";
                default:
                    throw new Exception("Unrecognized kingdom code: " + kingdomCode);
            }
        }

        #region Names

        public void InsertOrUpdateAvailableName(AvailableName name) {
            StoredProcUpdate("spALNInsertUpdate",
                _P("intBiotaID", name.BiotaID),
                _P("intRefID", name.RefID),
                _P("vchrRefPage", name.RefPage),
                _P("vchrAvailableNameStatus", name.AvailableNameStatus),
                _P("txtRefQual", name.RefQual)
            );
        }

        public SpeciesAvailableName GetSpeciesAvailableName(int taxonId) {
            var mapper = new GenericMapperBuilder<SpeciesAvailableName>().build();
            SpeciesAvailableName result = null;
            StoredProcReaderFirst("spSANGet", (reader)=> {
                result = mapper.Map(reader);
            }, _P("intBiotaID", taxonId));
            return result;
        }

        public void InsertOrUpdateSpeciesAvailableName(SpeciesAvailableName name) {
            StoredProcUpdate("spSANInsertUpdate",
                _P("intBiotaID", name.BiotaID),
                _P("intRefID", name.RefID),
                _P("vchrRefPage", name.RefPage),
                _P("vchrAvailableNameStatus", name.AvailableNameStatus),
                _P("txtRefQual", name.RefQual),
                _P("vchrPrimaryType", name.PrimaryType),
                _P("vchrSecondaryType", name.SecondaryType),
                _P("bitPrimaryTypeProbable", name.PrimaryTypeProbable),
                _P("bitSecondaryTypeProbable", name.SecondaryTypeProbable));
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

        public List<SANTypeData> GetSANTypeData(int taxonId) {
            var mapper = new GenericMapperBuilder<SANTypeData>().Map("vchrAccessionNum", "AccessionNumber").build();
            return StoredProcToList("spSANTypeDataGet", mapper, _P("intBiotaID", taxonId));
        }

        public void DeleteSANTypeData(int sanTypeDataID) {
            StoredProcUpdate("spSANTypeDataDelete", _P("intSANTypeDataID", sanTypeDataID));
        }

        public void InsertSANTypeData(SANTypeData data) {
            var retval = ReturnParam("RetVal", System.Data.SqlDbType.Int);
            StoredProcUpdate("spSANTypeDataInsert",
                _P("intBiotaID", data.BiotaID),
                _P("vchrType", data.Type),
                _P("vchrMuseum", data.Museum),
                _P("vchrAccessionNum", data.AccessionNumber),
                _P("vchrMaterial", data.Material),
                _P("vchrLocality", data.Locality),
                _P("intMaterialID", data.MaterialID, DBNull.Value),
                _P("bitIDConfirmed", data.IDConfirmed),
                retval);

            data.SANTypeDataID = (int) retval.Value;
        }

        public void UpdateSANTypeData(SANTypeData data) {
            StoredProcUpdate("spSANTypeDataUpdate",
                _P("intSANTypeDataID", data.SANTypeDataID),
                _P("intBiotaID", data.BiotaID),
                _P("vchrType", data.Type),
                _P("vchrMuseum", data.Museum),
                _P("vchrAccessionNum", data.AccessionNumber),
                _P("vchrMaterial", data.Material),
                _P("vchrLocality", data.Locality),
                _P("intMaterialID", data.MaterialID, DBNull.Value),
                _P("bitIDConfirmed", data.IDConfirmed));
        }

        public List<SANTypeDataType> GetSANTypeDataTypes(int taxonId) {
            var list = new List<SANTypeDataType>();
            StoredProcReaderForEach("spSANTypeDataTypesGet", (reader) => {
                var t = new SANTypeDataType();
                t.PrimaryType = reader[0] as string;
                string st = reader[1] as string;
                if (!string.IsNullOrEmpty(st)) {
                    t.SecondaryTypes = st.Split(',');
                }
                list.Add(t);
            }, _P("intBiotaID", taxonId));
            return list;
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

            StoredProcUpdate("spCommonNameInsert",                
                _P("intBiotaID", commonName.BiotaID),
                _P("vchrCommonName", commonName.Name),
                _P("intRefID", commonName.RefID, DBNull.Value),
                _P("vchrRefPage", commonName.RefPage),
                _P("txtNotes", commonName.Notes),
                retval
            );

            commonName.CommonNameID = (int)retval.Value;
        }

        public void DeleteCommonName(int commonNameID) {
            StoredProcUpdate("spCommonNameDelete", _P("intCommonNameID", commonNameID));
        }

        #endregion

        #region Distribution

        public List<TaxonDistribution> GetDistribution(int? TaxonID) {
            var mapper = new GenericMapperBuilder<TaxonDistribution>().build();
            return StoredProcToList("spBiotaDistGet", mapper, _P("intBiotaID", TaxonID.Value));
        }

        public string GetDistributionQualification(int taxonId) {
            string result = null;

            StoredProcReaderFirst("spBiotaGetDistQual", (reader) => {
                result = reader[0] as string;
            }, _P("intBiotaID", taxonId));

            return result;
        }

        public void UpdateDistributionQualification(int? TaxonID, string distQual) {
            StoredProcUpdate("spBiotaUpdateDistQual", _P("intBiotaID", TaxonID.Value), _P("txtDistQual", distQual));
        }

        public void DeleteAllBiotaDistribution(int? TaxonID) {
            StoredProcUpdate("spBiotaDistDeleteAll", _P("intBiotaID", TaxonID.Value));
        }

        public int InsertBiotaDist(int? TaxonID, TaxonDistribution dist) {
            var retval = ReturnParam("identity", System.Data.SqlDbType.Int);

            StoredProcUpdate("spBiotaDistInsert",
                _P("intBiotaID", TaxonID.Value),
                _P("txtRegionFullPath", dist.DistRegionFullPath),
                _P("bitIntroduced", dist.Introduced),
                _P("bitUncertain", dist.Uncertain),
                _P("bitThroughoutRegion", dist.ThroughoutRegion),
                _P("txtQual", dist.Qual),
                retval
            );

            return (int)retval.Value;

        }

        #endregion

        public string GetBiotaRankElemType(int taxonID, string elemType) {
            string result = null;
            StoredProcReaderFirst("spReportMBiotaParentage", (reader) => {
                result = reader[0] as string;
            }, _P("intBiotaID", taxonID), _P("vchrFormat", "rankonly"), _P("vchrElemType", elemType), _P("vchrSeparator", ""));
            return result;
        }

        public List<ChecklistData> GetChecklistData(int taxonID, int level, bool allChildren, bool userDefinedOrder, bool verifiedOnly, List<TaxonRankName> selectedRanks) {

            var list = new List<ChecklistData>();

            if (level == 0) {
                StoredProcReaderFirst("spReportCheckList", (reader) => {
                    string rankCode = (AsString(reader["RankCode"])).Trim();
                    if (selectedRanks.Find((name) => { return name.Code == rankCode; }) != null) {
                        list.Add(new ChecklistData {
                            BiotaID = reader.Get<int>("intBiotaID"),
                            IndentLevel = 0,
                            DisplayName = reader.Get("vchrFullName", ""),
                            Author = reader.Get("vchrAuthor", ""),
                            Type = reader.Get("chrElemType", ""),
                            Rank = reader.Get("vchrRank", ""),
                            Verified = !reader.Get<bool>("bitUnVerified"),
                            Unplaced = reader.Get<bool>("bitUnplaced"),
                            AvailableName = reader.Get<bool>("bitAvailableName"),
                            LiteratureName = reader.Get<bool>("bitLiteratureName"),
                            RankCategory = reader.Get("chrCategory", "")
                        });
                        }
                }, _P("intBiotaID", taxonID), _P("bitLevel", false), _P("bitUserOrder", userDefinedOrder), _P("bitVerifiedOnly", false));
            }

            StoredProcReaderForEach("spReportCheckList", (reader) => {
                string rankCode = (AsString(reader["RankCode"])).Trim();
                if (selectedRanks.Find((name) => { return name.Code == rankCode; }) != null) {
                    list.Add(new ChecklistData {
                        BiotaID = reader.Get<int>("intBiotaID"),
                        IndentLevel = level + 1,
                        DisplayName = reader.Get("vchrFullName", ""),
                        Author = reader.Get("vchrAuthor", ""),
                        Type = reader.Get("chrElemType", ""),
                        Rank = reader.Get("vchrRank", ""),
                        Verified = !reader.Get<bool>("bitUnVerified"),
                        Unplaced = reader.Get<bool>("bitUnplaced"),
                        AvailableName = reader.Get<bool>("bitAvailableName"),
                        LiteratureName = reader.Get<bool>("bitLiteratureName"),
                        RankCategory = reader.Get("chrCategory","")
                    });
                }
                
                if (allChildren) {
                    var temp = GetChecklistData((int)reader["intBiotaID"], level + 1, allChildren, userDefinedOrder, verifiedOnly, selectedRanks);
                    list.AddRange(temp);
                }

            }, _P("intBiotaID", taxonID), _P("bitLevel", true), _P("bitUserOrder", userDefinedOrder), _P("bitVerifiedOnly", verifiedOnly));


            return list;
        }

        public DataMatrix ChecklistReport(int taxonID, string criteriaDisplayText, ChecklistReportExtent extent, bool availableNames, bool literatureNames, ChecklistReportRankDepth? depth, bool userDefinedOrder, bool verifiedOnly, List<TaxonRankName> selectedRanks) {

            var b = new RTFReportBuilder();

            var data = GetChecklistData(taxonID, 0, extent == ChecklistReportExtent.FullHierarchy, userDefinedOrder, verifiedOnly, selectedRanks);

            // Process the list, generating the RTF...
            // Create the Header inforrmation
            b.AppendFullHeader();
            // Create the title information
            b.Append(@"\pard\fs36\b Taxon Checklist Report\b0\pard\par\fs22 ").Append(criteriaDisplayText).Append(@"\pard\par\fs16 Generated: ");
            b.AppendCurrentDate().Par().Par();
  
            int i = 0;
            foreach (ChecklistData item in data) {
                ++i;
                if (!availableNames && item.AvailableName || !literatureNames && item.LiteratureName) {
                    // ignore this one
                } else {
                    b.Append(@"\par\pard\fs20\li{0} {1}", item.IndentLevel * 300, FormatChecklistRow(item, i, depth));
                }
            }

            b.Append("}");

            return b.GetAsMatrix();
        }

        private string FormatChecklistRow(ChecklistData item, int i, ChecklistReportRankDepth? depth) {
            string result = "";
            switch (item.Type.ToLower()) {
                case "is":
                    result = "incertae sedis ";
                    break;
                case "si":
                    result = "Species inquirenda ";
                    break;
                default:
                    switch (item.RankCategory.ToLower()) {
                        case "g":
                        case "s":
                            if (item.Verified) {
                                result = string.Format(@"\i {0} \i0 ", UnItalicize(item.DisplayName, "subsp.|var.|sub.var.|form |sub.form "));
                            } else {
                                result = item.DisplayName;
                            }
                            break;
                        default:
                            result = item.DisplayName;
                            break;
                    }

                    // Add the rank if necessary...
                    if (depth.HasValue) {
                        switch (item.RankCategory.ToLower()) {
                            case "h":
                            case "f":
                                result = item.Rank + " " + result;
                                break;
                            case "g":
                                if (depth == ChecklistReportRankDepth.Subgenus) {
                                    result = item.Rank + " " + result;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }
                
            // Unitalicise the author/year component if applicable.
            if (!string.IsNullOrWhiteSpace(item.Author)) {
                int index = result.IndexOf(item.Author);
                if (index >= 0) {
                    result = result.Substring(0, index - 1) + "\\i0 " + result.Substring(index - 1);
                }
            }

            return result;                
        }

        protected string UnItalicize(string pstrBase ,string pstrSubstrList ) {
        //'
        //' Unitalicize any of the sub strings that appear in the base.
        //'

            string strWorking = pstrBase;
            string[] strSubstr = pstrSubstrList.Split('|');
            foreach (string s in strSubstr) {
                int lngInstrPos = strWorking.IndexOf(s);
                if (lngInstrPos >= 0) {
                    strWorking =  strWorking.Substring(0, lngInstrPos - 1) + "\\i0 " + s + "\\i " +strWorking.Substring(lngInstrPos + s.Length);
                }
            }
    
            return strWorking;
        }
            

        public DataMatrix TaxaForSites(int siteOrRegionID, int taxonID, string itemType, string criteriaDisplayText, bool includeLocations) {

            var rtf = new RTFReportBuilder();
            rtf.AppendFullHeader();
            rtf.ReportHeading("Taxa for Site/Region Report");            
            rtf.Append(criteriaDisplayText);
            rtf.Append(@"\pard\par\fs24 Produced: ").AppendCurrentDate();

            int lngLastBiotaID = -1;
            int lngLastRegionID = -1;
            int lngLastSiteID = -1;

            bool hasResults = false;
            StoredProcReaderForEach("spReportTaxaForSites", (reader) => {
                hasResults = true;
                // If there is a change in taxa, print the header.
                int biotaID = (Int32) reader["BiotaID"];
                if (lngLastBiotaID != biotaID) {
                    lngLastBiotaID = biotaID;
                    lngLastRegionID = -1;
                    lngLastSiteID = -1;
                    rtf.Par().Par().Append(@"\pard\sb20\fs28\b ");
                    rtf.Append(AsString(reader["BiotaFullName"])).Append(@"\b0");
                    // extract the family and order
                    string orderRank = GetBiotaRankElemType(lngLastBiotaID, "O");
                    string familyRank = GetBiotaRankElemType(lngLastBiotaID, "F");

                    if (!string.IsNullOrWhiteSpace(orderRank) && string.IsNullOrWhiteSpace(familyRank)) {
                        rtf.Append("  [").Append(orderRank).Append("]");
                    } else if (!string.IsNullOrWhiteSpace(orderRank) && !string.IsNullOrWhiteSpace(familyRank)) {
                        rtf.Append("  [").Append(orderRank).Append(": ").Append(familyRank).Append("]");
                    }
                }

                if (includeLocations) {
                    // Add the region group
                    int regionID = (Int32)reader["RegionID"];

                    if (lngLastRegionID != regionID) {
                        // Add the region
                        lngLastRegionID = regionID;
                        rtf.Par().Append(@"\pard\sb10\fs20\li600\b ").Append(AsString(reader["FullRegion"])).Append(@"\b0 ");
                    }

                    int siteID = (Int32)reader["SiteID"];
                    if (lngLastSiteID != siteID) {
                        lngLastSiteID = siteID;
                        // Add the Site
                        rtf.Par().Append(@"\pard\sb10\fs20\li1200 ");
                        // Add the locality
                        byte localType = (byte) reader["LocalType"];
                        switch (localType) {
                            case 0:
                                rtf.Append(reader["Local"] as string);
                                break;
                            case 1:
                                rtf.Append(reader["DistanceFromPlace"]).Append(" ").Append(reader["DirFromPlace"]).Append(" of ").Append(reader["Local"]);
                                break;
                            default:
                                rtf.Append(reader["Local"] as string);
                                break;
                        }

                        // Add the long and lat.

                        byte areaType = (byte) reader["AreaType"];
                        double? lat = reader["Lat"] as double?;
                        double? lon = reader["Long"] as double?;
                        double? lat2 = reader["Lat2"] as double?;
                        double? lon2 = reader["Long2"] as double?;

                        if (!lat.HasValue || !lon.HasValue) {
                            rtf.Append("; No position data");
                        } else {
                            switch (areaType) {
                                case 1:
                                    rtf.Append("; ");
                                    rtf.Append(GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude));
                                    rtf.Append(", ");
                                    rtf.Append(GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude));
                                    break;
                                case 2:
                                    rtf.Append("; Box: ");
                                    rtf.Append(GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude));
                                    rtf.Append(", ");
                                    rtf.Append(GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude));
                                    rtf.Append("; ");
                                    if (!lat2.HasValue || !lon2.HasValue) {
                                        rtf.Append("; No position data for second coordinate");
                                    } else {
                                        rtf.Append(GeoUtils.DecDegToDMS(lat2.Value, CoordinateType.Latitude));
                                        rtf.Append(", ");
                                        rtf.Append(GeoUtils.DecDegToDMS(lon2.Value, CoordinateType.Longitude));
                                    }
                                    break;
                                case 3:
                                    rtf.Append("; Line: ");
                                    rtf.Append(GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude));
                                    rtf.Append(", ");
                                    rtf.Append(GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude));
                                    rtf.Append("; ");
                                    if (!lat2.HasValue || !lon2.HasValue) {
                                        rtf.Append("; No position data for second coordinate");
                                    } else {
                                        rtf.Append(GeoUtils.DecDegToDMS(lat2.Value, CoordinateType.Latitude));
                                        rtf.Append(", ");
                                        rtf.Append(GeoUtils.DecDegToDMS(lon2.Value, CoordinateType.Longitude));
                                    }

                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                }                
            }, _P("vchrItemType", itemType), _P("intItemID", siteOrRegionID), _P("intBiotaID", taxonID));

            if (!hasResults) {
                rtf.Par().Par().Append("No results.");
            }
    
   
            rtf.Append("}");

            return rtf.GetAsMatrix();
        }

        public DataMatrix TaxaForDistributionRegionReport(int regionId, int taxonId) {

            var region = new SupportService(User).GetDistributionRegion(regionId);
            if (region == null) {
                throw new Exception("Could not retrieve region: " + regionId);
            }

            Taxon taxon = null;
            if (taxonId > 0) {
                taxon = GetTaxon(taxonId);
                if (taxon == null) {
                    throw new Exception("Could not retrieve taxon: " + taxonId);
                }
            }

            var rtf = new RTFReportBuilder();
            rtf.AppendFullHeader();
            rtf.ReportHeading("Taxa for Distribution Region Report");
            rtf.Append("Region: {0}", region.DistRegionName);
            if (taxon != null) {
                rtf.Append(" Taxon: {0}", taxon.TaxaFullName);
            }

            rtf.Append(@"\pard\par\fs24 Produced: ").AppendCurrentDate();

            int rowCount = 0;
            int lastRegionId = -1;
            int lastTaxonId = -1;
            StoredProcReaderForEach("spReportTaxaForDistRegion", (reader) => {
                rowCount++;
                int currentRegionId = (int) reader["DistRegionID"];
                if (lastRegionId != currentRegionId) {
                    lastRegionId = currentRegionId;
                    lastTaxonId = -1;
                    var regionName = reader["DistRegion"] as string;
                    regionName = regionName.Replace('\\', ':');
                    rtf.Par().Par().Append(@"\pard\sb20\fs24\b ").Append(regionName).Append(@"\b0 ");
                }

                var currentTaxonId = (int)reader["BiotaID"];
                if (lastTaxonId != currentTaxonId) {
                    lastTaxonId = currentTaxonId;
                    rtf.Par().Append(@"\pard\sb10\fs20\li600 ").Append(reader["Biota"].ToString());
                }

            }, _P("intDistributionRegionID", regionId), _P("intBiotaID", taxonId));

            if (rowCount == 0) {
                rtf.Par().Append("No results...");
            }

            rtf.Append(" }");

            return rtf.GetAsMatrix();
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

    public enum ChecklistReportExtent {
        FullHierarchy, NextLevelOnly
    }

    public enum ChecklistReportRankDepth {
        Family,
        Subgenus
    }
    
}
