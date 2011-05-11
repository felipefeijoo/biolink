using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.IO;
using System.Xml;
using BioLink.Data.Model;

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

        private Dictionary<string, XMLImportProgressItem> _progressItems;

        private int _totalItems;


        public XMLIOImporter(User user, string filename, IXMLImportProgressObserver observer)
            : base(user) {
            this.Filename = filename;
            this.Observer = observer;
        }

        public void Import() {
            if (InitImport()) {

                ImportMultimedia();
            }
        }

        private void ImportMultimedia() {


            var XMLMMParent = _xmlDoc.SelectSingleNode("//*/DATA/MULTIMEDIALIST");

            Log("Importing Multimedia");
            if (XMLMMParent == null) {
                Log("Failed to get MULTIMEDIALIST node from XML File (No Multimedia Imported)");
                return;
            }

            var lngRowCount = 0;

            var service = new SupportService(User);

            foreach (XmlElement XMLMM in XMLMMParent.ChildNodes) {

                var strGUID = XMLMM.GetAttributeValue("ID", "ERR");

                var strExtension = GetNodeValue(XMLMM, "EXTENSION", "bin");
                var XMLData = XMLMM.GetCData();


                if (XMLData == null) {
                    Log("Failed to locate CDATA section in MultimediaItem '{0}' - Item not imported", strGUID);
                } else {
                    var imageData = Convert.FromBase64String(XMLData);
                    if (imageData.Length > 0) {
                        string strUpdate = "", strInsert = "";
                        GenerateUpdateString(XMLMM, "MULTIMEDIAITEM", out strUpdate, out strInsert);
                        var lngMediaID = XMLIOService.ImportMultimedia(strGUID, imageData, strInsert, strUpdate);
                        _guidToIDCache.Add(strGUID, lngMediaID);
                        lngRowCount = lngRowCount + 1;

                        ImportTraits(XMLMM, "Multimedia", lngMediaID);
                        ImportNotes(XMLMM, "Multimedia", lngMediaID);
                        ProgressTick("Multimedia");
                        

                    } else {
                        Log("Failed to decode image data for Multimedia Item {0}. Multimedia not imported!", strGUID);

                    }
                }
            }

            Log("{0} Multimedia items imported.", lngRowCount);
        }

        private void ImportNotes(XmlElement XMLParent, string ItemType, int ItemID) {
            Log("Importing Notes for " + ItemType + "ID=" + ItemID);
    
            var XMLNoteParent = XMLParent.SelectSingleNode("NOTES") as XmlElement;
    
            if (XMLNoteParent == null) {
                Log("Failed to locate NOTES node for " + ItemType + "ID=" + ItemID + ". No notes imported.");
                return;
            }
    
            var NoteList = XMLNoteParent.SelectNodes("NOTEITEM");
    
            if (NoteList.Count <= 0) {
                Log("No Note items found in NOTE tag (" + ItemType + "ID=" + ItemID + ". No notes imported.");
                return;
            }
    
            int lngCategoryID = 0;
            if (!GetCategoryID(ItemType, out lngCategoryID)) {
                return;
            }

            var notes = new List<XMLIONote>();
    
            foreach (XmlElement XMLNote in NoteList) {
                var strNoteGUID = XMLNote.GetAttributeValue("ID", "ERR");
                var strNoteType = GetNodeValue(XMLNote, "NOTETYPE", "ERR");
                int lngNoteTypeID = 0;
                if (!_NoteTypeCache.NameInCache(lngCategoryID + "_" + strNoteType, out lngNoteTypeID)) {
                    lngNoteTypeID = XMLIOService.NoteGetTypeID(lngCategoryID, strNoteType);
                    if (lngNoteTypeID < 0) {
                        Log("Failed to get NoteTypeID for Note !");
                        return;
                    } else {
                        _NoteTypeCache.Add(lngCategoryID + " " + strNoteType, lngNoteTypeID);
                    }
                }
        
                var strRefID = GetNodeValue(XMLNote, "REFID", "");
                if (string.IsNullOrEmpty(strRefID)) {
                    strRefID = "NULL";
                } else {
                    var lngRefID = _guidToIDCache.IDfromGUID(strRefID);
                    if (lngRefID < 0) {
                        Log("Failed to locate reference GUID '" + strRefID + "' in GUID cache. Either the reference failed to import, or is not present in source XML file. Reference link not imported.");
                        strRefID = "NULL";
                    } else {
                        strRefID = lngRefID + "";
                    }
                }
        
                string strUpdate, strInsert;
                GenerateUpdateString(XMLNote, "NoteItem", out strUpdate, out strInsert, 
                    "intNoteTypeID=" + lngNoteTypeID + ", intCatID=" + lngCategoryID + ", intIntraCatID=" + ItemID + ", intRefID=" + strRefID, 
                    "intNoteTypeID, intCatID, intIntraCatID, intRefID", 
                    lngNoteTypeID + ", " + lngCategoryID + ", " + ItemID + ", " + strRefID);
            
                var note = new XMLIONote { GUID = strNoteGUID, InsertClause = strInsert, UpdateClause = strUpdate };                    
                notes.Add(note);
            }
    
            if (notes.Count > 0) {
                XMLIOService.InsertNotes(notes);
            }
        }

        private bool GetCategoryID(string ItemType , out int CategoryID ) {

        
            // Attempt to get the CategoryID from cache
            if (_CategoryCache.NameInCache(ItemType, out CategoryID)) {
                return true;
            } else {
                CategoryID = XMLIOService.GetTraitCategoryID(ItemType);
                if (CategoryID < 0) {
                    Log("Failed to get CategoryID for '" + ItemType + "' !");
                    return false;
                } else {
                    _CategoryCache.Add(ItemType, CategoryID);
                    return true;
                }
            }
        }

        private void ImportTraits(XmlElement XMLNode, string ItemType, int ItemID) {

            Log("Importing traits for " + ItemType + "ID=" + ItemID);
    
            var pCollection = GetCollectionForCategory(XMLNode.Name);
    
            if (pCollection == null) {
                Log("Failed to get field mapping collection for item '" + ItemType + "'");
                return;
            }
        
            int lngCategoryID;
            if (!GetCategoryID(ItemType, out lngCategoryID)) {
                return;
            }
           
            var traits = new List<XMLIOTrait>();
            string strFieldName;
            foreach (XmlNode xmlNode in XMLNode.ChildNodes) {
                if (xmlNode is XmlElement) {
                    var XMLChild = xmlNode as XmlElement;
                    if (!pCollection.XMLNameToFieldName(XMLChild.Name, out strFieldName)) {
                        if (XMLChild.HasChildNodes) {
                            if (XMLChild.ChildNodes.Count == 1) {
                                if (XMLChild.ChildNodes[0] is XmlText) {
                                    // Get ItemTypeID from cache or add it...
                                    var strTraitName = XMLChild.GetAttributeValue("ALIAS", XMLChild.Name);
                                    int lngItemTypeID;
                                    if (!_TraitItemTypeCache.NameInCache(lngCategoryID + "_" + strTraitName, out lngItemTypeID)) {
                                        lngItemTypeID = XMLIOService.GetTraitTypeID(lngCategoryID, strTraitName);
                                        if (lngItemTypeID < 0) {
                                            Log("Failed to get ItemTypeID for Trait !");
                                            return;
                                        } else {
                                            _TraitItemTypeCache.Add(lngCategoryID + "_" + strTraitName, lngItemTypeID);
                                        }
                                    }

                                    var trait = new XMLIOTrait { CategoryID = lngCategoryID, TraitTypeID = lngItemTypeID, IntraCatID = ItemID, Value = XMLChild.InnerText };
                                    traits.Add(trait);
                                }
                            }
                        }
                    }
                }

                if (traits.Count > 0) {
                    XMLIOService.InsertTraits(traits);
                }
            }
        }

        private void ProgressTick(string item) {
            var count = _progressItems["Multimedia"].Completed++;
            if (Observer != null) {
                Observer.ProgressTick(item, count);
            }
        }

        private void GenerateUpdateString(XmlElement XMLNode, string ObjectType, out string UpdateString, out string InsertString, string UpdateExtra = "", string InsertExtraField = "", string InsertExtraValue = "") {
            var guid = XMLNode.GetAttributeValue("ID", "ERR");
            var strUpdate = string.Format("GUID='{0}', ", guid);
            if (!string.IsNullOrWhiteSpace(UpdateExtra)) {
                    strUpdate = strUpdate + UpdateExtra + ", ";
            }
            var strFields = "GUID, ";
            var strValues = "'" + guid + "', ";
            if (!string.IsNullOrWhiteSpace(InsertExtraField) && !string.IsNullOrWhiteSpace(InsertExtraValue)) {
                strFields = strFields + InsertExtraField + ", ";
                strValues = strValues + InsertExtraValue + ", ";
            }

            string strFieldName = "";
            foreach (XmlNode XMLChild in XMLNode.ChildNodes) {
                if (XMLChild is XmlElement) {
                    var XMLElement = XMLChild as XmlElement;
                    if (LookupFieldName(ObjectType, XMLElement.Name, out strFieldName)) {
                        var strValue =  XMLElement.InnerText.Replace("'", "''");
                        if (!CheckForObjectReference(ObjectType, strFieldName, ref strValue)) {

                            if (strFieldName.StartsWith("vchr") || strFieldName.StartsWith("chr") || strFieldName.StartsWith("txt") || strFieldName.StartsWith("dt")) {
                                strValue = string.Format("'{0}'", strValue); 
                            } else {
                                strValue = BoolToBit(XMLChild.InnerText);
                            }

                        }
                        // If it is numeric (i.e. not quoted), and value is empty then force to NULL
                        if (string.IsNullOrWhiteSpace(strValue)) {
                            strValue = "NULL";
                        }

                        strUpdate = strUpdate + strFieldName + " = " + strValue + ", ";
                        strFields = strFields + strFieldName + ", ";
                        strValues = strValues + strValue + ", ";
                    }
                }
            }
    
            if (strUpdate.Length > 2) {
                strUpdate = strUpdate.Substring(0, strUpdate.Length - 2);
            }

            if (strFields.Length > 2) {
                strFields = strFields.Substring(0, strFields.Length - 2);
            }

            if (strValues.Length > 2) {
                strValues = strValues.Substring(0, strValues.Length -2 );
            }

            UpdateString = strUpdate;
            InsertString = "(" + strFields + ") VALUES (" + strValues + ")";
        }

        private string BoolToBit(string boolStr) {
            if (string.IsNullOrEmpty(boolStr)) {
                return "0";
            }
            return boolStr.ToLower() == "true" ? "1" : "0";
            
        }

        private bool CheckForObjectReference(string ObjectType, string FieldName, ref string Value) {

            int lngID;
            bool bDoCheck = false;

            switch (ObjectType.ToLower()) {
                case "reference":
                    if (FieldName.Equals("intJournalID")) {
                        bDoCheck = true;
                    }
                    break;
                case "associate":
                    switch (FieldName) {

                        case "intFromIntraCatID":
                        case "intToIntraCatID":
                        case "intRefID":
                            bDoCheck = true;
                            break;
                    }
                    break;
                case "commonname":
                    if (FieldName.Equals("intRefID")) {
                        bDoCheck = true;
                    }
                    break;
                case "referencelink":
                    if (FieldName.Equals("intRefID")) {
                        bDoCheck = true;
                    }
                    break;
                case "material":
                    if (FieldName == "intBiotaID") {
                        bDoCheck = true;
                    }

                    if (FieldName == "intIDRefID") {
                        bDoCheck = true;
                    }

                    if (FieldName == "intTrapID") {
                        bDoCheck = true;
                    }
                    break;

                case "identification":
                    if (FieldName == "intIDRefID") {
                        bDoCheck = true;
                    }
                    break;

                default:
                    bDoCheck = false;
                    break;
            }

            if (bDoCheck) {
                if (string.IsNullOrEmpty(Value)) {
                    Value = "NULL";
                } else {
                    lngID = _guidToIDCache.IDfromGUID(Value);
                    if (lngID < 0) {
                        Log("Failed to locate cached " + ObjectType + " item (Fieldname ='" + FieldName + "', ID=" + Value + "). This may be because of an earlier import failure ?");
                        Value = "NULL";
                    } else {
                        Value = lngID + "";
                    }
                }
            }

            return bDoCheck;
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

            _progressItems = new Dictionary<string, XMLImportProgressItem>();
            _progressItems["Taxon"] = new XMLImportProgressItem { Name = "Taxon", Total = GetNodeValue(_xmlDoc, "//*/META/TAXONCOUNT", 0) };
            _progressItems["Material"] = new XMLImportProgressItem { Name = "Material", Total = GetNodeValue(_xmlDoc, "//*/META/MATERIALCOUNT", 0) };
            _progressItems["SiteVisit"] = new XMLImportProgressItem { Name = "SiteVisit", Total = GetNodeValue(_xmlDoc, "//*/META/SITEVISITCOUNT", 0) };
            _progressItems["Site"] = new XMLImportProgressItem { Name = "Site", Total = GetNodeValue(_xmlDoc, "//*/META/SITECOUNT", 0) };
            _progressItems["Region"] = new XMLImportProgressItem { Name = "Region", Total = GetNodeValue(_xmlDoc, "//*/META/REGIONCOUNT", 0) };
            _progressItems["Journal"] = new XMLImportProgressItem { Name = "Journal", Total = GetNodeValue(_xmlDoc, "//*/META/JOURNALCOUNT", 0) };
            _progressItems["Reference"] = new XMLImportProgressItem { Name = "Reference", Total = GetNodeValue(_xmlDoc, "//*/META/REFERENCECOUNT", 0) };
            _progressItems["Associate"] = new XMLImportProgressItem { Name = "Associate", Total = GetNodeValue(_xmlDoc, "//*/META/ASSOCIATECOUNT", 0) };
            _progressItems["Multimedia"] = new XMLImportProgressItem { Name = "Multimedia", Total = GetNodeValue(_xmlDoc, "//*/META/MULTIMEDIACOUNT", 0) };


            _totalItems = _progressItems.Sum((item) => {
                return item.Value.Total;
            });

            Log("{0} items to import.", _totalItems);

            if (Observer != null) {
                Observer.ImportStarted("Importing", new List<XMLImportProgressItem>(_progressItems.Values));
            }

            return true;
        }

        private T GetNodeValue<T>(XmlNode root, string xpath, T @default = default(T)) {
            var node = root.SelectSingleNode(xpath);
            if (node != null) {
                if (typeof(int).IsAssignableFrom(typeof(T))) {
                    return (T)(object)int.Parse(node.InnerText);
                } else if (typeof(double).IsAssignableFrom(typeof(T))) {
                    return (T)(object)double.Parse(node.InnerText);
                }

                return (T)(object)node.InnerText;
            }

            return @default;
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
