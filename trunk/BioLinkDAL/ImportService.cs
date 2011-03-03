using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class ImportService : BioLinkService {

        public ImportService(User user) : base(user) { }

        public int ImportRegion(string name, int parentId, string rank) {
            return StoredProcReturnVal<int>("spRegionImportGetID",
                _P("vchrRegionName", name),
                _P("intInsertUnderParent", parentId),
                _P("vchrRank", rank)
            );

        }

        public int ImportSite(Site site) {
            return StoredProcReturnVal<int>("spSiteImportGetID",
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

        public int ImportSiteVisit(SiteVisit siteVisit) {
            return StoredProcReturnVal<int>("spSiteVisitImportGetID",
                _P("vchrSiteVisitName", siteVisit.SiteVisitName),
                _P("intSiteID", siteVisit.SiteID),
                _P("vchrFieldNumber", siteVisit.FieldNumber),
                _P("vchrCollector", siteVisit.Collector),
                _P("tintDateType", siteVisit.DateType),
                _P("intDateStart", siteVisit.DateStart),
                _P("intDateEnd", siteVisit.DateEnd),
                _P("intTimeStart", siteVisit.TimeStart),
                _P("intTimeEnd", siteVisit.TimeEnd),
                _P("vchrCasualTime", siteVisit.CasualTime));
        }

        public int ImportTaxon(int parentID, string epithet, string author, string yearOfPub, bool changedCombination, string elemType, bool unplaced, string rank, string kingdom, int order, bool unverified, string availnamestatus) {
            return StoredProcReturnVal<int>("spBiotaImport",
                _P("intParentID", parentID),
                _P("vchrEpithet", epithet),
                _P("vchrAuthor", author),
                _P("vchrYearOfPub", yearOfPub),
                _P("bitChgComb", changedCombination),
                _P("chrElemType", elemType),
                _P("bitUnplaced", unplaced),
                _P("vchrRank", rank),
                _P("chrKingdomType", kingdom),
                _P("intOrder", order),
                _P("bitUnverified", unverified),
                _P("vchrAvailableNameStatus", availnamestatus)
            );
        }

        public void ImportTrait(string category, int intraCatID, string traitName, string traitValue, string comment = "") {
            StoredProcUpdate("spTraitImport",
                _P("vchrCategory", category),
                _P("intIntraCatID", intraCatID),
                _P("vchrTrait", traitName),
                _P("vchrValue", traitValue),
                _P("vchrComment", comment)
            );
        }

        public int ImportMaterial(Material m) {
            var matService = new MaterialService(User);
            var materialID = matService.InsertMaterial(m.SiteVisitID);
            matService.UpdateMaterial(m);
            return materialID;
        }

        public void ImportCommonName(int taxonID, string commonName) {
            StoredProcUpdate("spCommonNameImport",
                _P("intBiotaID", taxonID),
                _P("vchrCommonName", commonName)
            );
        }

    }
}
