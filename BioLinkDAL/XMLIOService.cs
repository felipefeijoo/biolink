using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class XMLIOService : BioLinkService {

        public XMLIOService(User user) : base(user) { }

        public void ExportXML(List<int> taxonIds, XMLIOExportOptions options, IProgressObserver progress, Func<bool> isCancelledCallback) {

            try {
                if (progress != null) {
                    progress.ProgressStart("Counting total taxa to export...");
                }

                var exporter = new XMLExporter(User, taxonIds, options, progress, isCancelledCallback);

                exporter.Export();

            } finally {
                if (progress != null) {
                    progress.ProgressEnd("Export complete.");
                }
            }

        }

    }

    class XMLExporter {

        private TextWriter _logWriter = null;

        private GUIDToIDCache _taxonList;
        private GUIDToIDCache _referenceList;
        private GUIDToIDCache _journalList;
        private GUIDToIDCache _multimediaList;
        private GUIDToIDCache _associateList;
        private GUIDToIDCache _unplacedTaxon;
        private GUIDToIDCache _regionList;
        private GUIDToIDCache _siteList;
        private GUIDToIDCache _siteVisitList;
        private GUIDToIDCache _materialList;
        private Func<bool> _isCancelled;

        private XMLExportObject _xmlDoc;
        private int _itemTotal;

        public XMLExporter(User user, List<int> taxonIds, XMLIOExportOptions options, IProgressObserver progress, Func<bool> isCancelledCallback) {
            this.User = user;
            this.TaxonIDs = taxonIds;
            this.Options = options;
            this.ProgressObserver = progress;
            _isCancelled = isCancelledCallback;

            if (options.KeepLogFile) {
                string logfile = SystemUtils.ChangeExtension(options.Filename, "log");
                _logWriter = new StreamWriter(logfile, false);                
            }

        }

        public void Export() {
            try {

                Init();

                ExportBiota();

            } finally {
                if (_logWriter != null) {
                    _logWriter.Close();
                    _logWriter.Dispose();
                }
            }
        }

        private void ExportBiota() {
            Log("Counting total taxa to export");
            var taxonService = new TaxaService(User);
            var taxonMap = new Dictionary<int, Taxon>();
            foreach (int taxonId in TaxonIDs) {
                var taxon = taxonService.GetTaxon(taxonId);
                taxonMap[taxonId] = taxon;
                Log("Counting children of Taxon '{0}' ({1})", taxon.TaxaFullName, taxonId);
                var itemCount = GetItemCount(taxonId);
                _itemTotal += itemCount;

                if (IsCancelled) {
                    return;
                }
            }

            Log("{0} items to export", _itemTotal);
            StartTime = DateTime.Now;
            var XMLTaxaNode = _xmlDoc.TaxaRoot;
            foreach (int taxonId in TaxonIDs) {
                if (IsCancelled) {
                    break;
                }

                var taxon = taxonMap[taxonId];

                if (Options.IncludeFullClassification) {
                    XMLTaxaNode = ImportParents(XMLTaxaNode, taxon);
                }

                if (XMLTaxaNode != null) {
                    AddTaxonElement(XMLTaxaNode, taxon, Options.ExportChildTaxa);
                }

            }
        }

        private XmlElement ImportParents(XmlElement parent, Taxon taxon) {

            var newParent = parent;

            var parentIds = taxon.Parentage.Split('\\').Select((s) => {
                return Int32.Parse(s);
            });

            var taxaService = new TaxaService(User);

            foreach (int parentId in parentIds) {                
                if (parentId == taxon.TaxaID.Value) {
                    break;
                } else {
                    var parentTaxon = taxaService.GetTaxon(parentId);
                    newParent = AddTaxonElement(newParent, parentTaxon, false);
                }
            }

            return parent;
        }

        private XmlElement AddTaxonElement(XmlElement parent, Taxon taxon, bool recurseChildren) {

            if (IsCancelled) {
                return null;
            }

            XmlElement taxonNode = null;

            var guid = _taxonList.GUIDForID(taxon.TaxaID.Value);
            if (guid != null) {
                if (recurseChildren) {
                    taxonNode = _xmlDoc.GetElementByGUID(guid, "TAXON");
                }

            } else {
                guid = taxon.GUID.Value.ToString();
                taxonNode = _xmlDoc.CreateNode(parent, "TAXON");
                taxonNode.AddAttribute("ID", guid);
                _taxonList[guid] = taxon.TaxaID.Value;
//                 taxonNode.AddNamedValue(


            }
                
            return taxonNode;
        }

        private int GetItemCount(int taxonId) {
            var service = new TaxaService(User);
            var stats = service.GetTaxonStatistics(taxonId);
            return stats.TotalItems + 1; // the taxon itself counts as one
        }

        private bool IsCancelled {
            get {
                if (_isCancelled != null) {
                    return _isCancelled();
                }

                return false;
            }
        }

        private void Init() {

            Log("Export XML started (User {0}, Database {1} on {2})", User.Username, User.ConnectionProfile.Database, User.ConnectionProfile.Server);
            Log("Destination file: {0}", Options.Filename);
            Log("Taxon IDS: {0}", TaxonIDs.Join(","));
            Log("Exporting child taxa: {0}", Options.ExportChildTaxa);
            Log("Exporting material: {0}", Options.ExportMaterial);
            Log("Exporting multimedia: {0}", Options.ExportMultimedia);
            Log("Exporting notes: {0}", Options.ExportNotes);
            Log("Exporting traits: {0}", Options.ExportTraits);
            Log("Including full classification: {0}", Options.IncludeFullClassification);

            _taxonList = new GUIDToIDCache();
            _referenceList = new GUIDToIDCache();
            _journalList = new GUIDToIDCache(); 
            _multimediaList = new GUIDToIDCache();
            _associateList = new GUIDToIDCache();
            _unplacedTaxon = new GUIDToIDCache();
            _regionList = new GUIDToIDCache();
            _unplacedTaxon = new GUIDToIDCache();
            _siteList = new GUIDToIDCache();
            _siteVisitList = new GUIDToIDCache();
            _materialList = new GUIDToIDCache();

            _xmlDoc = new XMLExportObject();
        }

        protected void Log(string format, params object[] args) {
            if (_logWriter == null) { 
                return; 
            }

            var message = format;
            if (args.Length > 0) {
                message = string.Format(format, args);
            }

            _logWriter.WriteLine(string.Format("[{0:d/M/yyyy HH:mm:ss}] {1}", DateTime.Now, message));
        }

        protected User User { get; private set; }
        protected XMLIOExportOptions Options { get; private set; }
        protected IProgressObserver ProgressObserver { get; private set; }
        protected List<int> TaxonIDs { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
    }

    class GUIDToIDCache : Dictionary<string, int> {

        public string GUIDForID(int id) {            
            foreach (string key in Keys) {
                if (this[key] == id) {
                    return key;
                }
            }
            return null;
        }
    }

}
