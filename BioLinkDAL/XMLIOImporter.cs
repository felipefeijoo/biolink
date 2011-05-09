using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.IO;
using System.Xml;

namespace BioLink.Data {

    public class XMLIOImporter : XMLIOBase {

        private XmlDocument _xmlDoc;
        private GUIDToIDCache _guidToIDCache;

        private NameToIDCache _CategoryCache;
        private NameToIDCache _TraitItemTypeCache;
        private NameToIDCache _RefLinkTypeCache;    
        private NameToIDCache _DistRegionCache;    
        private NameToIDCache _KeywordItemTypeCache;    
        private NameToIDCache _MultimediaTypeCache;    
        private NameToIDCache _NoteTypeCache;    
        private NameToIDCache _StorageCache;
        private List<string> _AvailableNameList;


        public XMLIOImporter(string filename, IXMLImportProgressObserver observer) {
            this.Filename = filename;
            this.Observer = observer;
        }

        public void Import() {
            if (InitImport()) {
            }
        }

        private bool InitImport() {
            ProgressMessage(string.Format("Parsing {0}...", Filename));
            _xmlDoc = new XmlDocument();
            _xmlDoc.Load(Filename);

            ProgressMessage("Initialising import...");

            InitMappings();

            

            // Initialize Caches....    
            _guidToIDCache = new GUIDToIDCache();

            _CategoryCache = new NameToIDCache();  
            _TraitItemTypeCache = new NameToIDCache();
            _RefLinkTypeCache = new NameToIDCache();    
            _DistRegionCache = new NameToIDCache();    
            _KeywordItemTypeCache = new NameToIDCache();    
            _MultimediaTypeCache = new NameToIDCache();    
            _NoteTypeCache = new NameToIDCache();    
            _StorageCache = new NameToIDCache();    
            _AvailableNameList = new List<string>();

            return true;
        }

        protected void ProgressMessage(string message) {
            if (Observer != null) {
                Observer.ProgressMessage(message);
            }
        }

        public string Filename { get; private set; }
        protected IXMLImportProgressObserver Observer { get; private set; }
    }

}
