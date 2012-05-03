/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.IO;
using System.Xml;
using BioLink.Data.Model;
using System.Data.Common;
using System.Data.SqlClient;

namespace BioLink.Data {

    public class XMLIOService : BioLinkService {

        public XMLIOService(User user) : base(user) { }

        public void ExportXML(List<int> taxonIds, XMLIOExportOptions options, IProgressObserver progress, Func<bool> isCancelledCallback) {

            try {
                if (progress != null) {
                    progress.ProgressStart("Counting total taxa to export...");
                }

                var exporter = new XMLIOExporter(User, taxonIds, options, progress, isCancelledCallback);

                exporter.Export();

            } finally {
                if (progress != null) {
                    progress.ProgressEnd("Export complete.");
                }
            }

        }

        public void ImportXML(string filename, IXMLImportProgressObserver observer, Func<bool> isCancelledCallback) {

        }

        public List<XMLIOMultimediaLink> GetExportMultimediaLinks(string category, int intraCatId) {
            var mapper = new GenericMapperBuilder<XMLIOMultimediaLink>().build();
            return StoredProcToList("spXMLExportMultimediaList", mapper, _P("vchrCategory", category), _P("intIntraCatID", intraCatId));
        }

        public XMLIOMultimedia GetMultimedia(int mediaId) {

            var mapper = new GenericMapperBuilder<XMLIOMultimedia>().build();
            XMLIOMultimedia ret = null;
            StoredProcReaderFirst("spXMLExportMultimediaGet", (reader) => {
                ret = mapper.Map(reader);
            }, _P("intMultimediaID", mediaId));

            return ret;
        }

        public XMLIOMaterial GetMaterial(int materialId) {
            var mapper = new GenericMapperBuilder<XMLIOMaterial>().Override(new ByteToBoolConvertingMapper("tintTemplate")).build();
            return StoredProcGetOne("spExportMaterialGet", mapper, _P("intMaterialID", materialId));
        }

        public XMLIORegion GetRegion(int regionId) {
            var mapper = new GenericMapperBuilder<XMLIORegion>().build();
            return StoredProcGetOne("spExportRegionGet", mapper, _P("intRegionID", regionId));                
        }

        public XMLIOSite GetSite(int siteId) {
            var mapper = new GenericMapperBuilder<XMLIOSite>().Override(new ByteToBoolConvertingMapper("tintTemplate")).build();
            return StoredProcGetOne("spExportSiteGet", mapper, _P("intSiteID", siteId));
        }

        public XMLIOSiteVisit GetSiteVisit(int siteVisitId) {
            var mapper = new GenericMapperBuilder<XMLIOSiteVisit>().Override(new ByteToBoolConvertingMapper("tintTemplate")).build();
            return StoredProcGetOne("spExportSiteVisitGet", mapper, _P("intVisitID", siteVisitId));
        }

        public List<StorageLocation> GetStorageLocations(int taxonId) {
            var mapper = new GenericMapperBuilder<StorageLocation>().build();
            return StoredProcToList("spBiotaLocationGet", mapper, _P("intBiotaID", taxonId));
        }

        public List<XMLIOMaterialID> GetMaterialForTaxon(int biotaId) {
            var mapper = new GenericMapperBuilder<XMLIOMaterialID>().build();
            return StoredProcToList("spMaterialIDListForTaxon", mapper, _P("intBiotaID", biotaId));
        }

        public List<int> GetTaxaIdsForParent(int parentId) {
            var ret = new List<int>();
            StoredProcReaderForEach("spBiotaList", (reader) => {
                ret.Add(reader.GetIdentityValue());
            }, _P("intParentID", parentId));
            return ret;
        }

        #region Import

        public bool ImportMultimedia(XMLImportMultimedia media) {
            media.ID = -1;
            StoredProcReaderFirst("spXMLImportMultimedia", (reader) => {
                media.ID = reader.GetIdentityValue();
            }, _P("GUID", media.GUID), _P("txtInsertClause", media.InsertClause), _P("txtUpdateClause", media.UpdateClause));
            if (media.ID > 0) {
                var service = new SupportService(User);
                service.UpdateMultimediaBytes(media.ID, media.ImageData);
            } else {
                Logger.Debug("Failed to import multimedia: {0}. Stored proc did not return a recognizable media id", media.GUID);
            }
            return media.ID >= 0;
        }

        public void InsertTraits(IEnumerable<XMLIOTrait> traits) {
            foreach (XMLIOTrait trait in traits) {
                StoredProcUpdate("spXMLImportTrait",
                    _P("intCategoryID", trait.CategoryID),
                    _P("intTraitTypeID", trait.TraitTypeID),
                    _P("intIntraCatID", trait.IntraCatID),
                    _P("vchrValue", trait.Value));
            }
        }

        #endregion


        public int GetTraitTypeID(int lngCategoryID, string strTraitName) {            
            int typeId = 0;
            StoredProcReaderFirst("spXMLImportTraitTypeGet", (reader) => {
                typeId = (int) reader[0];
            }, _P("intCategoryID", lngCategoryID), _P("vchrTraitType", strTraitName));
            return typeId;
        }

        public string GetTraitCategoryName(int catId) {
            string name = "";
            SQLReaderForEach("SELECT vchrCategory FROM tblTraitCategory WHERE intTraitCategoryID = @catId", (reader) => {
                name = reader[0] as string;
            }, _P("@catId", catId));
            return name;
        }

        public int GetTraitCategoryID(string category) {
            int catId = 0;
            StoredProcReaderFirst("spXMLImportCategoryGet", (reader) => {
                catId = (int)reader[0];
            }, _P("vchrCategory", category));
            return catId;
        }

        public int NoteGetTypeID(int categoryID, string noteType) {
            int typeId = 0;
            StoredProcReaderFirst("spXMLImportNoteTypeGet", (reader) => {
                typeId = (int)reader[0];
            }, _P("intCategoryID", categoryID), _P("vchrNoteType", noteType));
            return typeId;
        }

        public void InsertNotes(List<XMLImportNote> notes) {
            foreach (XMLImportNote note in notes) {
                StoredProcUpdate("spXMLImportNote", _P("GUID", note.GUID), _P("txtInsertClause", note.InsertClause), _P("txtUpdateClause", note.UpdateClause));
            }
        }

        public bool ImportJournal(XMLImportJournal journal) {
            return ImportObject(journal, "spXMLImportJournal", _P("GUID", journal.GUID), _P("vchrFullName", journal.FullName));
        }

        public bool ImportReference(XMLImportReference reference) {
            return ImportObject(reference, "spXMLImportReference",
                _P("GUID", reference.GUID),
                _P("vchrRefCode", reference.Code),
                _P("vchrAuthor", reference.Author),
                _P("vchrYearOfPub", reference.Year));
        }

        public bool ImportMultimediaLink(List<XMLImportMultimediaLink> items) {
            bool ok = true;
            foreach (XMLImportMultimediaLink item in items) {
                ImportObject(item, "spXMLImportMultimediaLink", _P("GUID", item.GUID));
                if (item.ID < 0) {

                    ok = false;
                } else {
                    if (!string.IsNullOrWhiteSpace(item.Caption)) {
                        try {
                            XMLImportMultimediaLink item1 = item;
                            Command((con, cmd) => {
                                cmd.CommandText = "UPDATE tblMultimediaLink SET vchrCaption = @vchrCaption WHERE intMultimediaLinkID = @linkid";
                                cmd.Parameters.Add(_P("@vchrCaption", item1.Caption));
                                cmd.Parameters.Add(_P("@linkid", item1.ID));
                                cmd.ExecuteNonQuery();
                            });
                        } catch (Exception ex) {
                            Logger.Debug("Failed to insert caption for multimedia link id {0}. Probably due to the size of the caption exceeding the size of the table column. Will trim and try again. {1}.", item.ID, ex.Message);
                            try {
                                // first strip off any rtf...
                                var caption = RTFUtils.StripMarkup(item.Caption);
                                if (caption.Length > 255) {
                                    caption = caption.Truncate(255);
                                }
                                XMLImportMultimediaLink item1 = item;
                                Command((con, cmd) => {
                                    cmd.CommandText = "UPDATE tblMultimediaLink SET vchrCaption = @vchrCaption WHERE intMultimediaLinkID = @linkid";
                                    cmd.Parameters.Add(_P("@vchrCaption", caption));
                                    cmd.Parameters.Add(_P("@linkid", item1.ID));
                                    cmd.ExecuteNonQuery();
                                });
                            } catch (Exception ex2) {
                                Logger.Debug("Failed to insert shortened caption for multimedia link id {0} again!. Probably due to the size of the caption exceeding the size of the table column. Giving up! {1}. ", item.ID, ex2.Message);
                            }
                        }
                    }
                }
            }
            return ok;
        }

        protected bool ImportObject(XMLImportObject obj, string storedProc, params DbParameter[] @params) {
            obj.ID = -1;            
            Array.Resize(ref @params, @params.Length + 2);
            @params[@params.Length-2] = _P("txtInsertClause", obj.InsertClause);
            @params[@params.Length-1] = _P("txtUpdateClause", obj.UpdateClause);

            StoredProcReaderFirst(storedProc, (reader) => {
                obj.ID = reader.GetIdentityValue();
            }, @params);

            return obj.ID >= 0;
        }


        public int GetMultimediaTypeID(int lngCategoryID, string strMMType) {
            int result = -1;
            StoredProcReaderFirst("spXMLImportMultimediaTypeGet", (reader) => {
                result = reader.GetIdentityValue();
            }, _P("intCategoryID", lngCategoryID), _P("vchrMultimediaType", strMMType));
            return result;
        }

        public bool FindTaxon(string GUID, string FullName, string Epithet, int ParentID, out int lngTaxonID) {
            lngTaxonID = -1;
            int temp = 0;
            StoredProcReaderFirst("spXMLImportBiotaFind", (reader) => {
                temp = reader.GetIdentityValue();
            }, _P("GUID", GUID), _P("vchrFullName", FullName), _P("vchrEpithet", Epithet), _P("intParentID", ParentID));
            lngTaxonID = temp;
            return lngTaxonID >= 0;
        }


        public bool UpdateTaxon(XMLImportTaxon taxon) {
            string strRankCode;
            string strKingdomCode;
            bool bRankAdded;
            bool bKingdomAdded;

            if (!GetRankCodeFromName(taxon.Rank, out strRankCode, out bRankAdded)) {
                Logger.Debug("Failed to get rank code for taxon: RankName={0}, TaxonGUID={1}", taxon.Rank, taxon.GUID);
                return false;                
            }
    
            if (!GetKingdomCodeFromName(taxon.Kingdom, out strKingdomCode, out bKingdomAdded)) {
                Logger.Debug("Failed to get kingdom code for taxon: KingdomName={0}, TaxonGUID={1}", taxon.Kingdom, taxon.GUID);
            }
    
            var UpdateStr = taxon.UpdateClause + ", chrElemType='" + strRankCode + "'";

            var mapper = new GenericMapperBuilder<XMLImportTaxon>().build();
            bool succeeded = false;
            StoredProcReaderFirst("spXMLImportBiotaUpdate", (reader) => {
                mapper.Map(reader, taxon);
                succeeded = true;
            }, _P("vchrBiotaID", taxon.ID), _P("txtUpdateSetClause", UpdateStr));
   
            if (!succeeded) {
                // ErrorMsg = "[BIXMLIOServer.TaxonUpdate()] Failed to update Biota details! (TaxonID=" & TaxonID & ",UpdateStr='" & UpdateStr & "') - " & user.LastError
                return false;
            } else {
                return true;
            }
        }

        private static NameCodeCache _RankCache = new NameCodeCache();
        private static NameCodeCache _KingdomCache = new NameCodeCache();

        private bool GetRankCodeFromName(string RankName , out string RankCode, out bool Added ) {
    
            // Check the rank cache...
            var objRank = _RankCache.FindByName(RankName);
            if (objRank != null) {
                RankCode = objRank.Code;
                Added = false;
                return true;
            }

            RankCode = null;
            Added = false;

            NameCodeItem item = null;
            StoredProcReaderFirst("spXMLImportBiotaDefRankResolve", (reader) => {
                item = new NameCodeItem { Name = RankName, Code = reader["RankCode"] as string, IsExisting = (bool)reader["Added"] };
                _RankCache.Add(RankName, item);
            }, _P("vchrFullRank", RankName));

            if (item != null) {
                RankCode = item.Code;
                Added = !item.IsExisting;
            }
            return item != null;
        }
        
        private bool GetKingdomCodeFromName(string KingdomName, out string KingdomCode, out bool Added) {
    
            // Check the Kingdom cache...
            var objKingdom = _KingdomCache.FindByName(KingdomName);
            if (objKingdom != null) {
                KingdomCode = objKingdom.Code;
                Added =false;
                return true;
            }
    
            NameCodeItem item = null;

            KingdomCode = null;
            Added = false;
    
            StoredProcReaderFirst("spXMLImportBiotaDefKingdomResolve", (reader) => {
                item = new NameCodeItem { Name = KingdomName, Code=reader["KingdomCode"] as string, IsExisting = (bool) reader["Added"] };
            }, _P("vchrFullKingdom", KingdomName));

            if (item != null) {
                KingdomCode = item.Code;
                Added = !item.IsExisting;
            }

            return item != null;

        }

        public bool ImportCommonNames(int TaxonID, List<XMLImportCommonName> list) {
            foreach (XMLImportCommonName name in list) {
                ImportObject(name, "spXMLImportCommonName",
                    _P("GUID", name.GUID),
                    _P("vchrCommonName", name.CommonName),
                    _P("intBiotaID", TaxonID));               
            }
            return true;
        }

        public int GetRefLinkTypeID(int lngCategoryID, string strRefLinkType) {
            int id = -1;
            StoredProcReaderFirst("spXMLImportRefLinkTypeGet", (reader) => {
                id = reader.GetIdentityValue();
            }, _P("intCategoryID", lngCategoryID), _P("vchrRefLinkType", strRefLinkType));
            return id;
        }

        public bool ImportReferenceLinks(List<XMLImportRefLink> list) {
            foreach (XMLImportRefLink link in list) {
                ImportObject(link, "spXMLImportRefLink", _P("GUID", link.GUID));
            }
            return true;
        }

        public int InsertDistributionRegion(string strRegion, int lngParentID) {
            int id = -1;
            StoredProcReaderFirst("spXMLImportDistributionRegion", (reader) => {
                id = reader.GetIdentityValue();
            }, _P("intParentID", lngParentID), _P("vchrRegion", strRegion));
            return id;
        }

        public bool ImportTaxonDistribution(List<XMLImportDistribution> list) {
            foreach (XMLImportDistribution dist in list) {
                ImportObject(dist, "spXMLImportBiotaDistribution",
                    _P("GUID", dist.GUID),
                    _P("intBiotaID", dist.TaxonID),
                    _P("intDistributionRegionID", dist.RegionID));
            }
            return true;
        }

        public int ImportStorageLocation(string Location, int ParentID) {
            int id = -1;
            StoredProcReaderFirst("spXMLImportStorageLocation", (reader) => {
                id = reader.GetIdentityValue();
            }, _P("intParentID", ParentID), _P("vchrLocation", Location));
            return id;
        }

        public bool ImportTaxonStorage(List<XMLImportStorageLocation> list) {
            foreach (XMLImportStorageLocation loc in list) {
                ImportObject(loc, "spXMLImportBiotaStorageLocation", _P("GUID", loc.GUID), _P("intBiotaID", loc.TaxonID), _P("intBiotaStorageID", loc.LocationID));
            }
            return true;
        }

        public bool ImportPoliticalRegion(XMLImportRegion region) {
            return ImportObject(region, "spXMLImportPoliticalRegion", _P("GUID", region.GUID), _P("intParentID", region.ParentID), _P("vchrRegion", region.RegionName));            
        }

        public bool ImportSite(XMLImportSite site) {
            return ImportObject(site, "spXMLImportSite", _P("GUID", site.GUID), _P("tintLocalType", site.LocalityType), _P("vchrLocal", site.Locality), _P("fltPosY1", site.Y1), _P("fltPosX1", site.X1));
        }

        public bool ImportTrap(XMLImportTrap trap) {
            return ImportObject(trap, "spXMLImportTrap", _P("GUID", trap.GUID), _P("intSiteID", trap.SiteID), _P("vchrTrapName", trap.TrapName));
        }

        public bool ImportSiteVisit(XMLImportSiteVisit visit) {
            return ImportObject(visit, "spXMLImportSiteVisit", _P("GUID", visit.GUID), _P("intSiteID", visit.SiteID), _P("vchrCollector", visit.Collector), _P("intDateStart", visit.StartDate));
        }

        public bool ImportMaterial(XMLImportMaterial material) {
            return ImportObject(material, "spXMLImportMaterial", _P("GUID", material.GUID));
        }

        public bool InsertMaterialIdentification(List<XMLImportIDHistory> list) {
            foreach (XMLImportIDHistory item in list) {
                if (!ImportObject(item, "spXMLImportMaterialIdentification", _P("GUID", item.GUID))) {
                    return false;
                }
            }
            return true;
        }

        public bool InsertMaterialSubparts(List<XMLImportSubPart> list) {
            foreach (XMLImportSubPart part in list) {
                if (!ImportObject(part, "spXMLImportMaterialPart", _P("GUID", part.GUID))) {
                    return false;
                }
            }
            return true;
        }

        public bool InsertMaterialEvents(List<XMLImportEvent> list) {
            foreach (XMLImportEvent evt in list) {
                if (!ImportObject(evt, "spXMLImportCurationEvent", _P("GUID", evt.GUID))) {
                    return false;
                }
            }
            return true;
        }

        public bool ImportTaxonSAN(XMLImportSAN san, List<XMLImportSANType> list) {
            ImportObject(san, "spXMLImportSAN", _P("GUID", san.GUID), _P("intBiotaID", san.BiotaID));
            foreach (XMLImportSANType sanType in list) {
                ImportObject(sanType, "spXMLImportSANTypeData", _P("GUID", sanType.GUID));
            }            
            return true;
        }

        public bool ImportTaxonGAN(XMLImportGAN gan, List<XMLImportGANIncludedSpecies> species) {
            ImportObject(gan, "spXMLImportGAN", _P("GUID", gan.GUID), _P("intBiotaID", gan.TaxonID));
            foreach (XMLImportGANIncludedSpecies item in species) {
                ImportObject(item, "spXMLImportGANIncludedSpecies", _P("GUID", item.GUID));
            }
            return true;
        }

        public bool ImportTaxonALN(XMLImportALN aln) {
            return ImportObject(aln, "spXMLImportALN", _P("GUID", aln.GUID), _P("intBiotaID", aln.TaxonID));
        }

        public bool ImportAssociate(XMLImportAssociate assoc) {
            return ImportObject(assoc, "spXMLImportAssociate",
                _P("GUID", assoc.GUID),
                _P("intFromCatID", assoc.FromCatID),
                _P("intFromIntraCatID", assoc.FromIntraCatID),
                _P("intToCatID", assoc.ToCatID),
                _P("intToIntraCatID", assoc.ToIntraCatID),
                _P("txtAssocDescription", assoc.AssocDescription),
                _P("vchrRelationFromTo", assoc.RelationFromTo),
                _P("vchrRelationToFrom", assoc.RelationToFrom));
        }
    }

    class NameCodeCache : Dictionary<string, NameCodeItem> {

        public NameCodeItem Add(string name, string code, bool existing) {
            var item = new NameCodeItem { Name = name, Code = code, IsExisting = existing };
            this[code] = item;
            return item;
        }

        public NameCodeItem FindByName(string name) {

            foreach (NameCodeItem item in this.Values) {
                if (item.Name.Equals(name)) {
                    return item;
                }
            }
            return null;
        }

        public NameCodeItem FindByCode(string code) {
            if (ContainsKey(code)) {
                return this[code];
            }
            return null;
        }

    }

    class NameCodeItem {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsExisting { get; set; }
    }

    class BinaryConvertingMapper : ConvertingMapper {
        public BinaryConvertingMapper(string columnName) : base(columnName, (x) => { 
            return ((System.Data.SqlTypes.SqlBinary)x).Value; }
        ) { }
    }

}
