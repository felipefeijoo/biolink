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

        public DataMatrix GetTaxonTypes(int taxonId) {
            return StoredProcDataMatrix("spBiotaListTypes", _P("BiotaID", taxonId));
        }

        public DataMatrix GetAssociatesForTaxa(int regionID, params int[] taxonIds) {
            var strTaxonIDS = taxonIds.Join(",");
            return StoredProcDataMatrix("spAssociatesListForTaxon", _P("intPoliticalRegionID", regionID), _P("vchrBiotaID", strTaxonIDS));
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

        public DataMatrix TaxaForSites(int siteOrRegionID, int taxonID, string itemType, string criteriaDisplayText, bool includeLocations) {

            string strHEADER = @"{\rtf1\ansi\deff0\deflang1033 {\fonttbl {\f0\fswiss\fcharset0 SYSTEM;}{\f1\froman\fcharset0 TIMES NEW ROMAN;}}";
            string strCOLOUR_TABLE = @"{\colortbl \red0\green0\blue0}";
            string strPRE_TEXT = @"\paperw11895 \margr0\margl0\ATXph0 \plain \fs20 \f1 ";
            string strPARA = @"\par ";
            string vbCRLF = "\n";

            StringBuilder sb = new StringBuilder(strHEADER);
            sb.Append(vbCRLF).Append(strCOLOUR_TABLE).Append(vbCRLF).Append(strPRE_TEXT);
            sb.Append(@"\pard\fs36\b Taxa for Site/Region Report\b0\pard\par\fs24 ");
            sb.Append(criteriaDisplayText);
            sb.AppendFormat(@"\pard\par\fs24 Produced: {0:f}", DateTime.Now);

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
                    sb.Append(strPARA).Append(strPARA).Append(@"\pard\sb20\fs28\b ");
                    sb.Append(reader["BiotaFullName"]).Append(@"\b0");
                    // extract the family and order
                    string orderRank = GetBiotaRankElemType(lngLastBiotaID, "O");
                    string familyRank = GetBiotaRankElemType(lngLastBiotaID, "F");

                    if (!string.IsNullOrWhiteSpace(orderRank) && string.IsNullOrWhiteSpace(familyRank)) {
                        sb.Append("  [").Append(orderRank).Append("]");
                    } else if (!string.IsNullOrWhiteSpace(orderRank) && !string.IsNullOrWhiteSpace(familyRank)) {
                        sb.Append("  [").Append(orderRank).Append(": ").Append(familyRank).Append("]");
                    }
                }

                if (includeLocations) {
                    // Add the region group
                    int regionID = (Int32)reader["RegionID"];

                    if (lngLastRegionID != regionID) {
                        // Add the region
                        lngLastRegionID = regionID;
                        sb.Append(strPARA).Append(@"\pard\sb10\fs20\li600\b ").Append(reader["FullRegion"]).Append(@"\b0 ");
                    }

                    int siteID = (Int32)reader["SiteID"];
                    if (lngLastSiteID != siteID) {
                        lngLastSiteID = siteID;
                        // Add the Site
                        sb.Append(strPARA).Append(@"\pard\sb10\fs20\li1200 ");
                        // Add the locality
                        byte localType = (byte) reader["LocalType"];
                        switch (localType) {
                            case 0:
                                sb.Append(reader["Local"] as string);
                                break;
                            case 1:
                                sb.Append(reader["DistanceFromPlace"]).Append(" ").Append(reader["DirFromPlace"]).Append(" of ").Append(reader["Local"]);
                                break;
                            default:
                                sb.Append(reader["Local"] as string);
                                break;
                        }

                        // Add the long and lat.

                        byte areaType = (byte) reader["AreaType"];
                        double? lat = reader["Lat"] as double?;
                        double? lon = reader["Long"] as double?;
                        double? lat2 = reader["Lat2"] as double?;
                        double? lon2 = reader["Long2"] as double?;

                        if (!lat.HasValue || !lon.HasValue) {
                            sb.Append("; No position data");
                        } else {
                            switch (areaType) {
                                case 1:
                                    sb.Append("; ");
                                    sb.Append(GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude));
                                    sb.Append(", ");
                                    sb.Append(GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude));
                                    break;
                                case 2:
                                    sb.Append("; Box: ");
                                    sb.Append(GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude));
                                    sb.Append(", ");
                                    sb.Append(GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude));
                                    sb.Append("; ");
                                    if (!lat2.HasValue || !lon2.HasValue) {
                                        sb.Append("; No position data for second coordinate");
                                    } else {
                                        sb.Append(GeoUtils.DecDegToDMS(lat2.Value, CoordinateType.Latitude));
                                        sb.Append(", ");
                                        sb.Append(GeoUtils.DecDegToDMS(lon2.Value, CoordinateType.Longitude));
                                    }
                                    break;
                                case 3:
                                    sb.Append("; Line: ");
                                    sb.Append(GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude));
                                    sb.Append(", ");
                                    sb.Append(GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude));
                                    sb.Append("; ");
                                    if (!lat2.HasValue || !lon2.HasValue) {
                                        sb.Append("; No position data for second coordinate");
                                    } else {
                                        sb.Append(GeoUtils.DecDegToDMS(lat2.Value, CoordinateType.Latitude));
                                        sb.Append(", ");
                                        sb.Append(GeoUtils.DecDegToDMS(lon2.Value, CoordinateType.Longitude));
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
                sb.Append("No results.");
            }
    
   
            sb.Append("}");

            DataMatrix m = new DataMatrix();
            m.Columns.Add(new MatrixColumn { Name="RTF" });
            m.AddRow()[0] = sb.ToString();
            return m;
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
