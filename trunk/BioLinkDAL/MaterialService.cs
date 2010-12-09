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
            var mapper = new GenericMapperBuilder<SiteExplorerNode>().build();
            return StoredProcToList<SiteExplorerNode>("spSiteFindByName",
                mapper,
                _P("vchrLimitations", limitations),
                _P("vchrSiteToFind", searchTerm + "%")
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
    }
}
