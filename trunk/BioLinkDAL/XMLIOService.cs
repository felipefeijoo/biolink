using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.IO;
using System.Xml;
using BioLink.Data.Model;
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
                ret.Add((int)reader["TaxaID"]);
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
            return StoredProcReturnVal<int>("spXMLImportTraitTypeGet", _P("intCategoryID", lngCategoryID), _P("vchrTraitType", strTraitName));
        }

        public int GetTraitCategoryID(string category) {
            return StoredProcReturnVal<int>("spXMLImportCategoryGet", _P("vchrCategory", category));
        }

        public int NoteGetTypeID(int categoryID, string noteType) {
            return StoredProcReturnVal<int>("spXMLImportNoteTypeGet", _P("intCategoryID", categoryID), _P("vchrNoteType", noteType));
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
                }
            }
            return ok;
        }

        protected bool ImportObject(XMLImportObject obj, string storedProc, params SqlParameter[] @params) {
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
                result = (int)reader[0];
            }, _P("intCategoryID", lngCategoryID), _P("vchrMultimediaType", strMMType));
            return result;
        }
    }

    class BinaryConvertingMapper : ConvertingMapper {
        public BinaryConvertingMapper(string columnName) : base(columnName, (x) => { 
            return ((System.Data.SqlTypes.SqlBinary)x).Value; }
        ) { }
    }

    public static class ReaderExtensions {

        public static int GetIdentityValue(this SqlDataReader reader, int ordinal = 0, int @default = -1) {            
            if (ordinal >= 0) {
                if (!reader.IsDBNull(ordinal)) {
                    var obj = reader[ordinal];
                    if (obj != null) {
                        if (typeof(Int32).IsAssignableFrom(obj.GetType())) {
                            return (Int32)reader[0];
                        } else if (typeof(decimal).IsAssignableFrom(obj.GetType())) {
                            return (int)(decimal)reader[0];
                        }
                    }
                }                
            }
            return @default;
        }
    }

}
