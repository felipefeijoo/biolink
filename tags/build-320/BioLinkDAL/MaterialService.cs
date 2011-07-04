using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using System.Xml;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    public class MaterialService : BioLinkService {

        public MaterialService(User user)
            : base(user) {
        }

        #region Reports

        public DataMatrix GetMaterialForTrap(int trapID) {
            return StoredProcDataMatrix("spMaterialListForTrap", _P("intTrapID", trapID));
        }

        public DataMatrix GetTaxaForSites(bool includeLocations, string itemType, int itemID, int biotaID, string criteriaText) {

            var taxonService = new TaxaService(User);
            var rtf = new RTFReportBuilder();

            // Create the Header inforrmation
            rtf.AppendFullHeader();

            // Create the title information
            rtf.Append(@"\pard\fs36\b Taxa for Site/Region Report\b0\pard\par\fs24 ").Append(criteriaText);
            rtf.Append(@"\pard\par\fs24 Produced: ").AppendCurrentDate();
    
    
            // extract the parentage string from the database.
            // Loop through the recordset and build the report output.
            int lngLastBiotaID = -1;
            int lngLastRegionID = -1;
            int lngLastSiteID = -1;
            string strOrderRank = "";
            string strFamilyRank = "";

            StoredProcReaderForEach("spReportTaxaForSites", (reader) => {
                // If there is a change in taxa, print the header.
                int currentBiotaID = (int) reader["BiotaID"];
                if (lngLastBiotaID != currentBiotaID) {
                    lngLastBiotaID = currentBiotaID;
                    lngLastRegionID = -1;
                    lngLastSiteID = -1;
                    rtf.Par().Par().Append(@"\pard\sb20\fs28\b ");
                    rtf.Append(AsString(reader["BiotaFullName"])).Append(@"\b0");
                
                    // extract the family and order
                    strOrderRank = taxonService.GetBiotaRankElemType(currentBiotaID, "O");                    
                    strFamilyRank = taxonService.GetBiotaRankElemType(currentBiotaID, "F");
                    
                    if (!string.IsNullOrWhiteSpace(strOrderRank) && string.IsNullOrWhiteSpace(strFamilyRank)) {
                        rtf.Append("  [").Append(strOrderRank).Append("]");
                    } else if ((!string.IsNullOrWhiteSpace(strOrderRank) || (!string.IsNullOrWhiteSpace(strFamilyRank)))) {
                        rtf.Append("  [").Append(strOrderRank).Append(": ").Append(strFamilyRank).Append("]");
                    }
                }

                if (includeLocations) {
                    // Add the region group
                    int currentRegionID = (int) reader["RegionID"];
                    if (lngLastRegionID != currentRegionID) {
                        // Add the region
                        lngLastRegionID = currentRegionID;
                        rtf.Par().Append(@"\pard\sb10\fs20\li600 ");
                        rtf.Append(AsString(reader["FullRegion"]));
                    }

                    int currentSiteID = (int) reader["SiteID"];
    
                    if (lngLastSiteID != currentSiteID) {
                        lngLastSiteID = currentSiteID;
                        // Add the Site
                        rtf.Par().Append(@"\pard\sb10\fs20\li1200 ");
                        // Add the locality
                        int localType = (byte) reader["LocalType"];
                        switch (localType) {
                            case 0:
                                rtf.Append(AsString(reader["Local"]));
                                break;                            
                            case 1:                                
                                rtf.Append(AsString(reader["DistanceFromPlace"])).Append(" ");
                                rtf.Append(AsString(reader["DirFromPlace"])).Append(" of ").Append(AsString(reader["Local"]));
                                break;
                            default:
                                rtf.Append(AsString(reader["Local"]));
                                break;
                        }
                    
                       // Add the long and lat.
                        int areaType = (byte) reader["AreaType"];

                        double? lat = reader.Get<double?>("Lat");
                        double? lon = reader.Get<double?>("Long");
                        
                        double? lat2 = reader.Get<double?>("Lat2");
                        double? lon2 = reader.Get<double?>("Long2");
    
                        switch (areaType) {
                            case 1:   // Point

                                if (!lat.HasValue || !lon.HasValue) {                            
                                    rtf.Append("; No position data");
                                } else {
                                    rtf.Append("; {0}, {1}", GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude), GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude));
                                }
                                break;
                            case 2:  // Box
                                if (!lat.HasValue || !lon.HasValue || !lat2.HasValue || !lon2.HasValue) {                            
                                    rtf.Append("; No position data");
                                } else {
                                    rtf.Append("; Box: {0}, {1}; {2}, {3}", 
                                        GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude),
                                        GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude),
                                        GeoUtils.DecDegToDMS(lat2.Value, CoordinateType.Latitude),
                                        GeoUtils.DecDegToDMS(lon2.Value, CoordinateType.Longitude));                                    
                                }
                                break;
                            case 3: // Line
                                if (!lat.HasValue || !lon.HasValue || !lat2.HasValue || !lon2.HasValue) {                            
                                    rtf.Append("; No position data");
                                } else {
                                    rtf.Append("; Line: {0}, {1}; {2}, {3}", 
                                        GeoUtils.DecDegToDMS(lat.Value, CoordinateType.Latitude),
                                        GeoUtils.DecDegToDMS(lon.Value, CoordinateType.Longitude),
                                        GeoUtils.DecDegToDMS(lat2.Value, CoordinateType.Latitude),
                                        GeoUtils.DecDegToDMS(lon2.Value, CoordinateType.Longitude));                                    
                                }
                                break;
                            default:
                                // ignore
                                break;
                        }
                            
                    }
                }

            }, _P("vchrItemType", itemType), _P("intItemID", itemID), _P("intBiotaID", biotaID)); 
    

    
            rtf.Append(" }");

            return rtf.GetAsMatrix();
        }

        #endregion

        #region Site Explorer

        public List<SiteExplorerNode> GetTopLevelExplorerItems(SiteExplorerNodeType parentType = SiteExplorerNodeType.Region) {
            int parentId = 0;
            if (parentType == SiteExplorerNodeType.Unplaced) {
                parentId = -1;
            }
            return GetExplorerElementsForParent(parentId, parentType);
        }

        public List<SiteExplorerNode> GetExplorerElementsForParent(int parentID, SiteExplorerNodeType parentElemType) {
            return GetExplorerElementsForParent(parentID, parentElemType.ToString());
        }

        public List<SiteExplorerNode> GetExplorerElementsForParent(int parentID, string parentElemType) {
            var mapper = new GenericMapperBuilder<SiteExplorerNode>().build();
            return StoredProcToList<SiteExplorerNode>("spSiteExplorerList",
                mapper,
                _P("intParentID", parentID),
                _P("vchrParentType", parentElemType)
            );
        }

        public List<SiteExplorerNode> FindNodesByName(string searchTerm, string limitations) {

            searchTerm = EscapeSearchTerm(searchTerm);

            var mapper = new GenericMapperBuilder<SiteExplorerNode>().build();
            return StoredProcToList<SiteExplorerNode>("spSiteFindByName",
                mapper,
                _P("vchrLimitations", limitations),
                _P("vchrSiteToFind", searchTerm + "%")
            );
        }

        public List<SiteExplorerNode> GetSiteTemplates() {
            var mapper = new GenericMapperBuilder<SiteExplorerNode>().PostMapAction((n)=>{
                n.IsTemplate = true;
            }).build();
            return StoredProcToList("spSiteListTemplates", mapper);
        }

        public List<SiteExplorerNode> GetSiteVisitTemplates() {
            var mapper = new GenericMapperBuilder<SiteExplorerNode>().PostMapAction((n) => {
                n.IsTemplate = true;
            }).build();
            return StoredProcToList("spSiteVisitListTemplates", mapper);
        }

        public List<SiteExplorerNode> GetMaterialTemplates() {
            var mapper = new GenericMapperBuilder<SiteExplorerNode>().PostMapAction((n) => {
                n.IsTemplate = true;
            }).build();
            return StoredProcToList("spMaterialListTemplates", mapper);
        }



        #endregion

        #region Region

        public Region GetRegion(int regionID) {
            var mapper = new GenericMapperBuilder<Region>().build();
            Region result = null;
            StoredProcReaderFirst("spRegionGet", (reader) => {
                result = mapper.Map(reader);
            }, _P("intRegionID", regionID));
            return result;
        }

        public void UpdateRegion(int regionID, string name, string rank) {
            StoredProcUpdate("spRegionUpdate",
                _P("intRegionID", regionID),
                _P("vchrName", name),
                _P("vchrRank", rank)
            );
        }

        public void DeleteRegion(int regionID) {
            StoredProcUpdate("spRegionDelete", _P("intRegionID", regionID));
        }

        public void RenameRegion(int regionID, string name) {
            StoredProcUpdate("spRegionRename",
                _P("intRegionID", regionID),
                _P("vchrName", name)
            );
        }

        public int InsertRegion(string name, int parentID) {
            var retval = ReturnParam("NewRegionID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spRegionInsert",
                _P("vchrName", name),
                _P("intParentID", parentID),
                retval
            );
            return (int)retval.Value;
        }

        public List<RegionSearchResult> FindRegions(string searchTerm) {
            searchTerm = EscapeSearchTerm(searchTerm) + '%';
            var mapper = new GenericMapperBuilder<RegionSearchResult>().build();
            return StoredProcToList("spRegionFind", mapper, _P("vchrRegionToFind", searchTerm));
        }

        public void MoveRegion(int regionId, int newParentID) {
            StoredProcUpdate("spRegionMove", _P("intRegionID", regionId), _P("intNewParentID", newParentID));
        }

        #endregion

        #region Site Group

        public void RenameSiteGroup(int siteGroupID, string name) {
            StoredProcUpdate("spSiteGroupRename", _P("intSiteGroupID", siteGroupID), _P("vchrSiteGroupName", name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the new Site Group</param>
        /// <param name="parentType">The "type" of the parent: 1 = Region, 2 = Site Group</param>
        /// <param name="parentID">The parent ID</param>
        /// <param name="regionID">The region id underwhich this group is placed. Note that even if this is a child of another site group it still needs a region id</param>
        /// <returns></returns>
        public int InsertSiteGroup(string name, int parentType, int parentID, int regionID) {
            var retval = ReturnParam("NewRegionID");

            StoredProcUpdate("spSiteGroupInsert",
                _P("vchrName", name),
                _P("sintParentType", parentType),
                _P("intParentID", parentID),
                _P("intPoliticalRegionID", regionID),
                retval
            );

            return (int)retval.Value;
        }

        public void DeleteSiteGroup(int siteGroupID) {
            StoredProcUpdate("spSiteGroupDelete", _P("intSiteGroupID", siteGroupID));
        }

        public void MoveSiteGroup(int siteGroupID, int newParentType, int newParentID, int newRegionID) {
            StoredProcUpdate("spSiteGroupMove",
                _P("intSiteGroupID", siteGroupID),
                _P("sintParentType", newParentType),
                _P("intParentID", newParentID),
                _P("intPoliticalRegionID", newRegionID)
            );
        }

        public List<int> GetSiteGroupSiteIDList(int siteGroupID) {
            var results = new List<int>();
            StoredProcReaderForEach("spSiteGroupGetSiteIDList",
                (reader) => { results.Add(reader.GetInt32(0)); },
                _P("intSiteGroupID", siteGroupID)
            );

            return results;
        }

        public void MergeSiteGroup(int oldSiteGroupID, int newSiteGroupID) {
            StoredProcUpdate("spSiteGroupMerge", _P("intOldSiteGroupID", oldSiteGroupID), _P("intNewSiteGroupID", newSiteGroupID));
        }

        #endregion

        #region Site

        public Site GetSite(int siteID) {
            Site result = null;
            var mapper = new GenericMapperBuilder<Site>().Override(new TintToBoolConvertingMapper("tintTemplate")).build();
            StoredProcReaderFirst("spSiteGet", (reader) => {
                result = mapper.Map(reader);
            }, _P("intSiteID", siteID));
            return result;
        }

        public void RenameSite(int siteID, string name) {
            StoredProcUpdate("spSiteUpdateName",
                _P("intSiteID", siteID),
                _P("vchrSiteName", name)
            );
        }

        public void DeleteSite(int siteID) {
            StoredProcUpdate("spSiteDelete", _P("intSiteID", siteID));
        }

        public int InsertSite(int politicalRegion, int siteGroupId, int basedOnSiteId = -1) {
            var retval = ReturnParam("NewSiteID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spSiteInsert",
                _P("intPoliticalRegionID", politicalRegion),
                _P("intSiteGroupID", siteGroupId),
                _P("intBasedOnSiteID", basedOnSiteId),
                retval);
            return (int)retval.Value;
        }

        public void UpdateSite(Site site) {
            StoredProcUpdate("spSiteUpdate",
                _P("intSiteID", site.SiteID),
                _P("vchrSiteName", site.SiteName),
                _P("intPoliticalRegionID", site.PoliticalRegionID),
                _P("tintLocalType", site.LocalityType),
                _P("vchrLocal", site.Locality),
                _P("vchrDistanceFromPlace", site.DistanceFromPlace),
                _P("vchrDirFromPlace", site.DirFromPlace),
                _P("vchrInformalLocal", site.InformalLocal),
                _P("tintPosCoordinates", site.PosCoordinates),
                _P("tintPosAreaType", site.PosAreaType),
                _P("fltPosX1", site.PosX1),
                _P("fltPosY1", site.PosY1),
                _P("fltPosX2", site.PosX2),
                _P("fltPosY2", site.PosY2),
                _P("tintPosXYDisplayFormat", site.PosXYDisplayFormat),
                _P("vchrPosSource", site.PosSource),
                _P("vchrPosError", site.PosError),
                _P("vchrPosWho", site.PosWho),
                _P("vchrPosDate", site.PosDate),
                _P("vchrPosOriginal", site.PosOriginal),
                _P("vchrPosUTMSource", site.PosUTMSource),
                _P("vchrPosUTMMapProj", site.PosUTMMapProj),
                _P("vchrPosUTMMapName", site.PosUTMMapName),
                _P("vchrPosUTMMapVer", site.PosUTMMapVer),
                _P("tintElevType", site.ElevType),
                _P("fltElevUpper", site.ElevUpper),
                _P("fltElevLower", site.ElevLower),
                _P("fltElevDepth", site.ElevDepth),
                _P("vchrElevUnits", site.ElevUnits),
                _P("vchrElevSource", site.ElevSource),
                _P("vchrElevError", site.ElevError),
                _P("vchrGeoEra", site.GeoEra),
                _P("vchrGeoState", site.GeoState),
                _P("vchrGeoPlate", site.GeoPlate),
                _P("vchrGeoFormation", site.GeoFormation),
                _P("vchrGeoMember", site.GeoMember),
                _P("vchrGeoBed", site.GeoBed),
                _P("vchrGeoName", site.GeoName),
                _P("vchrGeoAgeBottom", site.GeoAgeBottom),
                _P("vchrGeoAgeTop", site.GeoAgeTop),
                _P("vchrGeoNotes", site.GeoNotes)
            );
        }

        public List<RDESite> GetRDESites(params int[] siteIds) {
            var mapper = new GenericMapperBuilder<RDESite>().Override(new IntToBoolConvertingMapper("Locked")).build();
            string siteIdList = siteIds.Join(",");
            return StoredProcToList("spSiteGetRDEFromIDList", mapper, _P("vchrSiteIDList", siteIdList));
        }

        public int InsertSiteTemplate() {
            var retval = ReturnParam("intNewSiteID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spSiteInsertTemplate", _P("vchrDummy", ""), retval);
            return (int)retval.Value;
        }

        public void MergeSite(int oldSiteID, int newSiteID) {
            StoredProcUpdate("spSiteMerge", _P("intOldSiteID", oldSiteID), _P("intNewSiteID", newSiteID));
        }

        public void MoveSite(int siteID, int politicalRegionID, int siteGroupID) {
            StoredProcUpdate("spSiteMove", _P("intSiteID", siteID), _P("intPoliticalRegionID", politicalRegionID), _P("intSiteGroupID", siteGroupID));
        }

        private void CreateNode(XmlDocument doc, string name, string value) {
            var newNode = doc.CreateElement(name);
            newNode.InnerText = value;
            doc.DocumentElement.AppendChild(newNode);
        }

        public List<SiteDifference> CompareSites(int siteAID, int siteBID) {
            string[] ignore = new string[] { "intSiteID", "intPoliticalRegionID", "intSiteGroupID", "vchrSiteName", "tintPosXYDisplayFormat", "vchrWhoCreated", "dtDateCreated", "vchrWhoLastUpdated", "dtDateLastUpdated", "intOrder", "tintTemplate", "GUID" };

            var docA = GetSiteXML(siteAID, ignore);
            var docB = GetSiteXML(siteBID, ignore);

            var list = new List<SiteDifference>();

            var processed = new List<String>();
            // initially, loop throught the base data, looking for differences.
            foreach (XmlElement elem in docA.DocumentElement.ChildNodes) {
                string baseVal = elem.InnerText;
                string otherVal = GetNodeText(docB, elem.Name);

                processed.Add(elem.Name);

                if (!baseVal.Equals(otherVal, StringComparison.InvariantCultureIgnoreCase)) {
                    list.Add(new SiteDifference(elem.Name, baseVal, otherVal));

                }
            }

            // Then loop through the Other fields. If we find any that have not been processed above,
            // assume it is a difference if the value is not blank. Note that thsi will occur with
            // traits, etc.
            foreach (XmlElement elem in docB.DocumentElement.ChildNodes) {
                if (!processed.Contains(elem.Name)) {
                    string otherVal = elem.InnerText;
                    if (!String.IsNullOrEmpty(otherVal)) {
                        list.Add(new SiteDifference(elem.Name, "", otherVal));
                    }
                }
            }

            return list;
        }

        private String GetNodeText(XmlDocument doc, string name) {
            string xpath = string.Format("//Site/{0}", name);
            XmlElement elem = doc.SelectSingleNode(xpath) as XmlElement;
            if (elem != null) {
                return elem.InnerText;
            }
            return "";
        }

        public XmlDocument GetSiteXML(int siteID, params string[] ignorelist) {
            var doc = new XmlDocument();
            var ignore = new List<String>(ignorelist);
            doc.AppendChild(doc.CreateElement("Site"));

            // Add the base items...
            StoredProcReaderFirst("spSiteGet", (reader) => {
                for (int i = 0; i < reader.FieldCount; ++i) {
                    string name = reader.GetName(i);
                    if (!ignorelist.Contains(name)) {
                        CreateNode(doc, name, AsString(reader[i]));
                    }
                }
            }, _P("intSiteID", siteID));

            // Traits...            
            StoredProcReaderForEach("spTraitList", (reader) => {
                CreateNode(doc, "Trait." + AsString(reader["Trait"]), AsString(reader["Value"]));
            }, _P("vchrCategory", "Site"), _P("vchrIntraCatID", siteID + ""));

            StoredProcReaderForEach("spNoteList", (reader) => {
                CreateNode(doc, "Note." + AsString(reader["NoteType"]), AsString(reader["Note"]));
            }, _P("vchrCategory", "Site"), _P("intIntraCatID", siteID));

            return doc;
        }

        #endregion

        #region Site Visit

        public void RenameSiteVisit(int siteVisitID, string name) {
            StoredProcUpdate("spSiteVisitUpdateName", _P("intSiteVisitID", siteVisitID), _P("vchrName", name));
        }

        public void DeleteSiteVisit(int siteVisitID) {
            StoredProcUpdate("spSiteVisitDelete", _P("intSiteVisitID", siteVisitID));
        }

        public int InsertSiteVisit(int parentID, int basedOnSiteVisitID = -1) {
            var retval = ReturnParam("NewSiteID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spSiteVisitInsert",
                _P("intParentID", parentID),
                _P("intBasedOnSiteVisitID", basedOnSiteVisitID),
                retval);

            return (int)retval.Value;
        }

        public void UpdateSiteVisit(SiteVisit siteVisit) {
            StoredProcUpdate("spSiteVisitUpdate",
                _P("intSiteVisitID", siteVisit.SiteVisitID),
                _P("vchrSiteVisitName", siteVisit.SiteVisitName),
                _P("vchrFieldNumber", siteVisit.FieldNumber),
                _P("vchrCollector", siteVisit.Collector),
                _P("tintDateType", siteVisit.DateType),
                _P("intDateStart", siteVisit.DateStart),
                _P("intDateEnd", siteVisit.DateEnd),
                _P("intTimeStart", siteVisit.TimeStart),
                _P("intTimeEnd", siteVisit.TimeEnd),
                _P("vchrCasualTime", siteVisit.CasualTime));
        }

        public SiteVisit GetSiteVisit(int siteVisitID) {
            var mapper = new GenericMapperBuilder<SiteVisit>().Override(new TintToBoolConvertingMapper("tintTemplate")).build();
            SiteVisit result = null;
            StoredProcReaderFirst("spSiteVisitGet", (reader) => {
                result = mapper.Map(reader);
            }, _P("intSiteVisitID", siteVisitID));

            return result;
        }

        public List<string> GetDistinctCollectors() {
            var list = new List<string>();
            StoredProcReaderForEach("spCollectorListDistinct", (reader) => {
                list.Add(reader[1] as string);
            });
            return list;
        }

        public void MergeSiteVisit(int oldSiteVisitID, int newSiteVisitID) {
            StoredProcUpdate("spSiteVisitMerge", _P("intOldSiteVisitID", oldSiteVisitID), _P("intNewSiteVisitID", newSiteVisitID));
        }

        public void MoveSiteVisit(int siteVisitID, int newParentID) {
            StoredProcUpdate("spSiteVisitMove", _P("intSiteVisitID", siteVisitID), _P("intNewParentID", newParentID));
        }

        public int InsertSiteVisitTemplate() {
            var retval = ReturnParam("intNewSiteVisitID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spSiteVisitInsertTemplate", _P("vchrDummy", ""), retval);
            return (int)retval.Value;
        }

        public List<RDESiteVisit> GetRDESiteVisits(int[] siteVisitIDs, RDEObjectType idType=RDEObjectType.SiteVisit) {
            string type = GetRDEObjectTypeStr(idType);
            var mapper = new GenericMapperBuilder<RDESiteVisit>().Override(new IntToBoolConvertingMapper("Locked")).build();
            var ids = siteVisitIDs.Join(",");
            return StoredProcToList("spSiteVisitGetRDEFromIDList", mapper, _P("vchrType", type), _P("txtIDList", ids));
        }

        #endregion

        #region Trap

        public void RenameTrap(int trapID, string name) {
            StoredProcUpdate("spTrapUpdateName", _P("intTrapID", trapID), _P("vchrTrapName", name));
        }

        public void DeleteTrap(int trapID) {
            StoredProcUpdate("spTrapDelete", _P("intTrapID", trapID));
        }

        public Trap GetTrap(int trapID) {
            Trap trap = null;
            var mapper = new GenericMapperBuilder<Trap>().build();
            StoredProcReaderFirst("spTrapGet", (reader) => {
                trap = mapper.Map(reader);
            }, _P("intTrapID", trapID));

            return trap;
        }

        public int InsertTrap(int siteID, string trapName) {
            var retval = ReturnParam("NewTrapID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spTrapInsert",
                _P("intSiteID", siteID),
                _P("vchrTrapName", trapName),
                retval);
            return (int)retval.Value;
        }

        public void UpdateTrap(Trap trap) {
            StoredProcUpdate("spTrapUpdate",
                _P("intTrapID", trap.TrapID),
                _P("vchrTrapName", trap.TrapName),
                _P("vchrTrapType", trap.TrapType),
                _P("vchrDescription", trap.Description)
            );
        }

        public void MergeTrap(int oldTrapID, int newTrapID) {
            StoredProcUpdate("spTrapMerge", _P("intOldTrapID", oldTrapID), _P("intNewTrapID", newTrapID));
        }

        public void MoveTrap(int trapID, int newSiteID) {
            StoredProcUpdate("spTrapMove", _P("intTrapID", trapID), _P("intNewSiteID", newSiteID));
        }

        #endregion

        #region Material

        public void RenameMaterial(int materialID, string name) {
            StoredProcUpdate("spMaterialUpdateName", _P("intMaterialID", materialID), _P("vchrName", name));
        }

        public int InsertMaterial(int siteVisitID, int basedOnMaterialID = -1) {
            var retval = ReturnParam("NewMaterialID", System.Data.SqlDbType.Int);

            StoredProcUpdate("spMaterialInsert",
                _P("intSiteVisitID", siteVisitID),
                _P("intBasedOnMaterialID", basedOnMaterialID),
                retval
            );

            return (int)retval.Value;

        }

        public void DeleteMaterial(int materialID) {
            StoredProcUpdate("spMaterialDelete", _P("intMaterialID", materialID));
        }

        public Material GetMaterial(int materialID) {
            var mapper = new GenericMapperBuilder<Material>().Override(new TintToBoolConvertingMapper("tintTemplate")).build();
            Material result = null;
            StoredProcReaderFirst("spMaterialGet", (reader) => {
                result = mapper.Map(reader);
            }, _P("intMaterialID", materialID));

            return result;
        }

        public void UpdateMaterial(Material material) {
            StoredProcUpdate("spMaterialUpdate",
                _P("intMaterialID", material.MaterialID),
                _P("vchrMaterialName", material.MaterialName),
                _P("intSiteVisitID", material.SiteVisitID),
                _P("vchrAccessionNo", material.AccessionNumber),
                _P("vchrRegNo", material.RegistrationNumber),
                _P("vchrCollectorNo", material.CollectorNumber),
                _P("intBiotaID", material.BiotaID),
                _P("vchrIDBy", material.IdentifiedBy),
                _P("vchrIDDate", material.IdentificationDate),
                _P("intIDRefID", material.IdentificationReferenceID),
                _P("vchrIDRefPage", material.IdentificationRefPage),
                _P("vchrIDMethod", material.IdentificationMethod),
                _P("vchrIDAccuracy", material.IdentificationAccuracy),
                _P("vchrIDNameQual", material.IdentificationNameQualification),
                _P("vchrIDNotes", material.IdentificationNotes),
                _P("vchrInstitution", material.Institution),
                _P("vchrCollectionMethod", material.CollectionMethod),
                _P("vchrAbundance", material.Abundance),
                _P("vchrMacroHabitat", material.MacroHabitat),
                _P("vchrMicroHabitat", material.MicroHabitat),
                _P("vchrSource", material.Source),
                _P("intAssociateOf", material.AssociateOf),
                _P("intTrapID", material.TrapID),
                _P("vchrSpecialLabel", material.SpecialLabel),
                _P("vchrOriginalLabel", material.OriginalLabel)
            );
        }

        public void MergeMaterial(int oldMaterialID, int newMaterialID) {
            StoredProcUpdate("spMaterialMerge", _P("intOldMaterialID", oldMaterialID), _P("intNewMaterialID", newMaterialID));
        }

        public void MoveMaterial(int materialID, int newSiteVisitID) {
            StoredProcUpdate("spMaterialMove", _P("intMaterialID", materialID), _P("intNewSiteVisitID", newSiteVisitID));
        }

        public string GetMaterialSummary(Material material) {

            string strHEADER = @"{\rtf1\ansi\deff0\deflang1033 {\fonttbl {\f0\fswiss\fcharset0 SYSTEM;}{\f1\froman\fcharset0 TIMES NEW ROMAN;}}";
            string strCOLOUR_TABLE = @"{\colortbl \red0\green0\blue0;\red255\green255\blue255;\red0\green0\blue255}";
            string strPRE_TEXT = @"\paperw11895 \margr0\margl0\ATXph0 \plain \fs20 \f1 ";

            var buf = new StringBuilder();

            buf.AppendFormat("{0}\n{1}\n{2}", strHEADER, strCOLOUR_TABLE, strPRE_TEXT);
            var site = GetSite(material.SiteID);

            buf.Append(@"\pard\fs24\b\ul Site \b0\ul0 ");

            buf.Append(FieldRTF(site.SiteName, "Name: ", 2));
            buf.Append(FieldRTF(site.PoliticalRegion, "Region: ", 2));
            buf.Append(FieldRTF(site.Locality, "Local: ", 2));
            buf.Append(FieldRTF(site.DistanceFromPlace, "Distance: ", 2));
            buf.Append(FieldRTF(site.DirFromPlace, "; Dir: ", 2, false));

            buf.Append(FieldRTF(site.InformalLocal, "Informal: ", 2));
            buf.Append(FieldRTF(site.PosX1, "Lat: ", 2));
            buf.Append(FieldRTF(site.PosY1, "Long: ", 2));
            buf.Append(FieldRTF(site.ElevUpper + " " + site.ElevUnits, "Elev. Upper: ", 1));
            buf.Append(FieldRTF(site.ElevLower + " " + site.ElevUnits, "Elev. Lower: ", 1));
            buf.Append(FieldRTF(site.ElevDepth + " " + site.ElevUnits, "Elev. Depth: ", 1));
            buf.Append(FieldRTF(site.WhoCreated + ", " + site.DateCreated, "Created: ", 1));
            buf.Append(FieldRTF(site.WhoLastUpdated + ", " + site.DateLastUpdated, "Last Updated: ", 1));

            var visit = GetSiteVisit(material.SiteVisitID);

            buf.Append(@"\pard\par\par\fs24\b\ul\cf0 Site Visit \b0\ul0 ");
            buf.Append(FieldRTF(visit.SiteVisitName, "Name: ", 2));
            buf.Append(FieldRTF(visit.Collector, "Collector: ", 1));
            buf.Append(FieldRTF(visit.DateStart, "Date Start: ", 1));
            buf.Append(FieldRTF(visit.DateEnd, "Date End: ", 1));
            buf.Append(FieldRTF(visit.CasualTime, "Casual Date: ", 1));
            buf.Append(FieldRTF(visit.WhoCreated + ", " + visit.DateCreated, "Created: ", 1));
            buf.Append(FieldRTF(visit.WhoLastUpdated + ", " + visit.DateLastUpdated, "Last Updated: ", 1));

            buf.Append(@"\pard\par }");

            return buf.ToString();
        }

        private string FieldRTF(object obj, string title, int tabs, bool parabefore = true) {

            if (obj == null) {
                return "";
            };

            string str = obj.ToString().Trim();

            if (String.IsNullOrEmpty(str)) {
                return "";
            }

            var buf = new StringBuilder();
            if (parabefore) {
                buf.Append(@"\pard\par\fi-7000\li7000\tx7000");
            }

            buf.AppendFormat(@"\fs20\cf3\b {0}\b0\cf2{1} {2}", title, @"\tab".Repeat(tabs), str);

            return buf.ToString();

        }

        public int InsertMaterialTemplate() {
            var retval = ReturnParam("intNewMaterialID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spMaterialInsertTemplate", _P("vchrDummy", ""), retval);
            return (int)retval.Value;
        }

        public List<RDEMaterial> GetRDEMaterial(int[] idlist, RDEObjectType idType=RDEObjectType.Material) {
            string type = GetRDEObjectTypeStr(idType);
            var mapper = new GenericMapperBuilder<RDEMaterial>().Override(new IntToBoolConvertingMapper("Locked")).build();            
            return StoredProcToList("spMaterialGetRDEFromIDList", mapper, _P("vchrType", type), _P("txtIDList", idlist.Join(",")));
        }

        private string GetRDEObjectTypeStr(RDEObjectType objType) {
            switch (objType) {
                case RDEObjectType.Material: return "m";
                case RDEObjectType.Site: return "s";
                case RDEObjectType.SiteVisit: return "v";
                default:
                    throw new Exception("Unhandled RDEObjectType: " + objType.ToString());
            }
        }

        #endregion

        #region Material Identification

        public List<MaterialIdentification> GetMaterialIdentification(int materialID) {
            var mapper = new GenericMapperBuilder<MaterialIdentification>().build();
            return StoredProcToList("spMaterialIDGet", mapper, _P("intMaterialID", materialID));
        }

        public void DeleteMaterialIdentification(int materialIdentID) {
            StoredProcUpdate("spMaterialIDDelete", _P("intMaterialIdentID", materialIdentID));
        }

        public int InsertMaterialIdentification(MaterialIdentification i) {
            var retval = ReturnParam("NewMaterialIdentID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spMaterialIDInsert",
                _P("intMaterialID", i.MaterialID),
                _P("vchrTaxa", i.Taxa),
                _P("vchrIDBy", i.IDBy),
                _P("vchrIDDate", i.IDDate),
                _P("vchrIDMethod", i.IDMethod),
                _P("intIDRefID", i.IDRefID),
                _P("vchrIDRefPage", i.IDRefPage),
                _P("vchrIDAccuracy", i.IDAccuracy),
                _P("vchrNameQual", i.NameQual),
                _P("txtNotes", i.IDNotes),
                retval
            );

            return (int)retval.Value;
        }

        public void UpdateMaterialIdentification(MaterialIdentification i) {
            StoredProcUpdate("spMaterialIDUpdate",
                _P("intMaterialIdentID", i.MaterialIdentID),
                _P("intMaterialID", i.MaterialID),
                _P("vchrTaxa", i.Taxa),
                _P("vchrIDBy", i.IDBy),
                _P("vchrIDDate", i.IDDate),
                _P("vchrIDMethod", i.IDMethod),
                _P("intIDRefID", i.IDRefID),
                _P("vchrIDRefPage", i.IDRefPage),
                _P("vchrIDAccuracy", i.IDAccuracy),
                _P("vchrNameQual", i.NameQual),
                _P("txtNotes", i.IDNotes)
            );

        }


        #endregion

        #region Material Part (subparts)

        public List<MaterialPart> GetMaterialParts(int materialID) {
            var mapper = new GenericMapperBuilder<MaterialPart>().build();
            return StoredProcToList("spMaterialPartGet", mapper, _P("intMaterialID", materialID));
        }

        public List<MaterialPart> GetMaterialParts(params int[] materialIds) {
            var mapper = new GenericMapperBuilder<MaterialPart>().build();
            var ids = materialIds.Join(",");
            return StoredProcToList("spMaterialPartGetRDEFromIDList", mapper, _P("txtMaterialIDList", ids));
        }

        public void DeleteMaterialPart(int materialPartID) {
            StoredProcUpdate("spMaterialPartDelete", _P("intMaterialPartID", materialPartID));
        }

        public int InsertMaterialPart(MaterialPart part) {
            var retval = ReturnParam("NewMaterialPartID", System.Data.SqlDbType.Int);

            StoredProcUpdate("spMaterialPartInsert",
                _P("intMaterialID", part.MaterialID),
                _P("vchrPartName", part.PartName),
                _P("vchrSampleType", part.SampleType),
                _P("intNoSpecimens", part.NoSpecimens),
                _P("vchrNoSpecimensQual", part.NoSpecimensQual),
                _P("vchrLifestage", part.Lifestage),
                _P("vchrGender", part.Gender),
                _P("vchrRegNo", part.RegNo),
                _P("vchrCondition", part.Condition),
                _P("vchrStorageSite", part.StorageSite),
                _P("vchrStorageMethod", part.StorageMethod),
                _P("vchrCurationStatus", part.CurationStatus),
                _P("txtNotes", part.Notes),
                retval
            );

            return (int)retval.Value;
        }

        public void UpdateMaterialPart(MaterialPart part) {
            StoredProcUpdate("spMaterialPartUpdate",
                _P("intMaterialPartID", part.MaterialPartID),
                _P("intMaterialID", part.MaterialID),
                _P("vchrPartName", part.PartName),
                _P("vchrSampleType", part.SampleType),
                _P("intNoSpecimens", part.NoSpecimens),
                _P("vchrNoSpecimensQual", part.NoSpecimensQual),
                _P("vchrLifestage", part.Lifestage),
                _P("vchrGender", part.Gender),
                _P("vchrRegNo", part.RegNo),
                _P("vchrCondition", part.Condition),
                _P("vchrStorageSite", part.StorageSite),
                _P("vchrStorageMethod", part.StorageMethod),
                _P("vchrCurationStatus", part.CurationStatus),
                _P("txtNotes", part.Notes)
            );
        }

        #endregion

        #region Material Curation Events

        public List<CurationEvent> GetCurationEvents(int materialID) {
            var mapper = new GenericMapperBuilder<CurationEvent>().build();
            return StoredProcToList("spCurationEventGet", mapper, _P("intMaterialID", materialID));
        }

        public int InsertCurationEvent(CurationEvent e) {
            var retval = ReturnParam("intNewID");
            StoredProcUpdate("spCurationEventInsert",
                _P("intMaterialID", e.MaterialID),
                _P("vchrSubpartName", e.SubpartName),
                _P("vchrWho", e.Who),
                _P("dtWhen", e.When),
                _P("vchrEventType", e.EventType),
                _P("txtEventDesc", e.EventDesc),
                retval
            );
            return (int)retval.Value;
        }

        public void UpdateCurationEvent(CurationEvent e) {
            StoredProcUpdate("spCurationEventUpdate",
                _P("intCurationEventID", e.CurationEventID),
                _P("intMaterialID", e.MaterialID),
                _P("vchrSubpartName", e.SubpartName),
                _P("vchrWho", e.Who),
                _P("dtWhen", e.When),
                _P("vchrEventType", e.EventType),
                _P("txtEventDesc", e.EventDesc)
            );
        }

        public void DeleteCurationEvent(int eventID) {
            StoredProcUpdate("spCurationEventDelete", _P("intCurationEventID", eventID));
        }

        #endregion
    }

    public enum RDEObjectType {
        Site, 
        SiteVisit,
        Material
    }

    public class SiteDifference {

        public SiteDifference() {
        }

        public SiteDifference(string item, string a, string b) {
            this.Item = item;
            this.A = a;
            this.B = b;
        }

        public String Item { get; set; }
        public String A { get; set; }
        public String B { get; set; }
    }
}
