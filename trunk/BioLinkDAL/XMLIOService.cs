using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.IO;
using System.Xml;
using BioLink.Data.Model;

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
                ret.Add((int)reader["TaxaID"]);
            }, _P("intParentID", parentId));
            return ret;
        }

        #region Import

        public int ImportMultimedia(string guid, byte[] imageData, string insertClause, string updateClause) {
            int mediaId = -1;
            StoredProcReaderFirst("spXMLImportMultimedia", (reader) => {
                if (!reader.IsDBNull(0)) {
                    var obj = reader[0];
                    if (obj != null) {
                        if (typeof(Int32).IsAssignableFrom(obj.GetType())) {
                            mediaId = (Int32)reader[0];
                        } else if (typeof(decimal).IsAssignableFrom(obj.GetType())) {
                            mediaId = (int)(decimal)reader[0];
                        } else {
                            Logger.Debug("Failed to import multimedia: {0}. Stored proc did not return a recognizable media id: {1}", obj.GetType());
                        }
                    }
                }
            }, _P("GUID", guid), _P("txtInsertClause", insertClause), _P("txtUpdateClause", updateClause));
            if (mediaId > 0) {
                var service = new SupportService(User);
                service.UpdateMultimediaBytes(mediaId, imageData);
            } else {
                Logger.Debug("Failed to import multimedia: {0}. Stored proc did not return a recognizable media id", guid);
            }
            return mediaId;
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
            return StoredProcReturnVal<int>("spXMLImportTraitTypeGet", _P("intCategoryID", lngCategoryID), _P("vchrTraitType", strTraitName));
        }

        public int GetTraitCategoryID(string category) {
            return StoredProcReturnVal<int>("spXMLImportCategoryGet", _P("vchrCategory", category));
        }

        public int NoteGetTypeID(int categoryID, string noteType) {
            return StoredProcReturnVal<int>("spXMLImportNoteTypeGet", _P("intCategoryID", categoryID), _P("vchrNoteType", noteType));
        }

        public void InsertNotes(List<XMLIONote> notes) {
            foreach (XMLIONote note in notes) {
                StoredProcUpdate("spXMLImportNote", _P("GUID", note.GUID), _P("txtInsertClause", note.InsertClause), _P("txtUpdateClause", note.UpdateClause));
            }
        }
    }

    class BinaryConvertingMapper : ConvertingMapper {
        public BinaryConvertingMapper(string columnName) : base(columnName, (x) => { 
            return ((System.Data.SqlTypes.SqlBinary)x).Value; }
        ) { }
    }

}
