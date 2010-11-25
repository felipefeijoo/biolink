using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class MaterialService : BioLinkService {

        public MaterialService(User user)
            : base(user) {
        }

        #region Site Explorer

        public List<SiteExplorerNode> GetTopLevelExplorerItems() {
            return GetExplorerElementsForParent(0, SiteExplorerNodeType.Region);
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
            return (int) retval.Value;
        }

        public List<RegionSearchResult> FindRegions(string searchTerm) {
            searchTerm = searchTerm.Replace('*', '%') + '%';
            var mapper = new GenericMapperBuilder<RegionSearchResult>().build();
            return StoredProcToList("spRegionFind", mapper, _P("vchrRegionToFind", searchTerm));
        }

        #endregion

        #region Site Group

        public void RenameSiteGroup(int siteGroupID, string name) {
            StoredProcUpdate("spSiteGroupRename", _P("intSiteGroupID",siteGroupID), _P("vchrSiteGroupName", name));
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

        public void MergeSiteGroups(int sourceID, int targetID) {
            StoredProcUpdate("spSiteGroupMerge",
                _P("intOldSiteGroupID", sourceID),
                _P("intNewSiteGroupID", targetID)
            );
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

        #endregion

        #region Site

        public Site GetSite(int siteID) {
            Site result = null;
            var mapper = new GenericMapperBuilder<Site>().build();
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

        #endregion

        #region Site Visit

        public void RenameSiteVisit(int siteVisitID, string name) {
            StoredProcUpdate("spSiteVisitUpdateName", _P("intSiteVisitID", siteVisitID), _P("vchrName", name));
        }

        #endregion

        #region Trap

        public void RenameTrap(int trapID, string name) {
            StoredProcUpdate("spTrapUpdateName", _P("intTrapID", trapID), _P("vchrTrapName", name));
        }

        #endregion

        #region Material

        public void RenameMaterial(int materialID, string name) {
            StoredProcUpdate("spMaterialUpdateName", _P("intMaterialID", materialID), _P("vchrName", name));
        }

        #endregion

    }
}
