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

    }

    class BinaryConvertingMapper : ConvertingMapper {
        public BinaryConvertingMapper(string columnName) : base(columnName, (x) => { 
            return ((System.Data.SqlTypes.SqlBinary)x).Value; }
        ) { }
    }

}
