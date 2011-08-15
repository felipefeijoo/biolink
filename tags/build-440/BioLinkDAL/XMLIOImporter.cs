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
        private Dictionary<string, AvailableNameInfo> _AvailableNameList;

        private Dictionary<ImportItemType, XMLImportProgressItem> _progressItems;

        private int _totalItems;


        public XMLIOImporter(User user, string filename, IXMLImportProgressObserver observer)
            : base(user) {
            this.Filename = filename;
            this.Observer = observer;
        }

        public void Import() {
            if (InitImport()) {

                ImportMultimedia();

                ImportJournals();

                ImportReferences();

                // Import Taxon...
                var XMLTaxonRoot = GetTaxaRoot();

                if (XMLTaxonRoot == null) {
                    Log("Failed to locate the TAXON Root node in XML !");
                } else {
                    // Add main taxon nodes...
                    if (AddTaxonChildren(XMLTaxonRoot, -1)) {
                        // Add Unplaced Taxon
                        Log("Adding Unplaced Taxon");
                        var XMLUnplacedRoot = XMLTaxonRoot.SelectSingleNode("UNPLACEDTAXA") as XmlElement;
                        if (XMLUnplacedRoot == null) {
                            Log("No unplaced taxa in file.");
                        } else {
                            AddTaxonChildren(XMLUnplacedRoot, -1);
                        }
                    }
                }

                var XMLMaterialRoot = _xmlDoc.SelectSingleNode("//*/DATA/MATERIAL") as XmlElement;
                if (XMLMaterialRoot == null) {
                    Log("Failed to locate Material Root in XML ! No specimen data imported.");
                } else {
                    // Political Regions
                    ImportPoliticalRegions(XMLMaterialRoot, 0);        
                }

                // Taxon Available Names (SAN records often point to material records, so do it after material has been imported !)
                foreach (AvailableNameInfo AvailableNameItem in _AvailableNameList.Values) {
                    ImportAvailableNameData(AvailableNameItem.XMLNode as XmlElement, AvailableNameItem.TaxonID, AvailableNameItem.IsAvailableName, AvailableNameItem.IsLiteratureName, AvailableNameItem.RankCategory);
                }
        
                // Associates...
                ImportAssociates();

                if (Observer != null) {
                    Observer.ImportCompleted();
                }
            }
        }

        private bool ImportAssociates() {
    
            Log("Importing Associates");
            var XMLAssocParent = _xmlDoc.SelectSingleNode("//*/DATA/ASSOCIATES");    
            
            if (XMLAssocParent == null) {
                Log("Failed to get ASSOCIATES node from XML File (No Associates Imported)");
                return false;
            }
    
            var NodeList = XMLAssocParent.SelectNodes("ASSOCIATE");
    
            foreach (XmlElement XMLAssoc in NodeList) {
                var assoc = new XMLImportAssociate(XMLAssoc) { FromCatID = GetNodeValue<int>(XMLAssoc, "FROMCATEGORYID") };

                assoc.FromIntraCatID = _guidToIDCache.IDfromGUID(GetNodeValue<string>(XMLAssoc, "FROMINTRACATID"));
                assoc.ToCatID = GetNodeValue<int>(XMLAssoc, "TOCATEGORYID");
                assoc.ToIntraCatID = _guidToIDCache.IDfromGUID(GetNodeValue<string>(XMLAssoc, "TOINTRACATID"));
                assoc.AssocDescription = GetNodeValue<string>(XMLAssoc, "ASSOCDESCRIPTION");
                assoc.RelationFromTo = GetNodeValue<string>(XMLAssoc, "RELATIONFROMTO");
                assoc.RelationToFrom = GetNodeValue<string>(XMLAssoc, "RELATIONTOFROM");
                
                GenerateUpdateString(XMLAssoc, "ASSOCIATE", assoc);
                                       
                if (XMLIOService.ImportAssociate(assoc)) {
                    
                    if (assoc.ID < 0) {
                        Log("Failed to get AssociateID from Associate Array (Not set in server?) (ID=" + assoc.GUID + ")");
                    } else {
                        _guidToIDCache.Add(assoc.GUID, assoc.ID);
                        ProgressTick( ImportItemType.Associate);
                    }
                } else {
                    Log("Failed to import associate (ID=" + assoc.GUID + ")");
                }                
            }

            return true;        
        }

        private bool ImportAvailableNameData(XmlElement TaxonNode, int TaxonID , bool isAvailableName, bool isLiteratureName, string RankCategory) {
    
            if (isAvailableName) {
                // Could be SAN, GAN or ALN depending on Rank Category...
                switch (RankCategory.ToLower()) {
                    case "s":        // SAN
                        ImportSANData(TaxonNode, TaxonID);
                        break;
                    case "g":        // GAN
                        ImportGANData(TaxonNode, TaxonID);
                        break;
                    case "f":        // ALN
                        ImportALNData(TaxonNode, TaxonID);
                        break;
                    default:
                        Log("Unknown rank category (TaxonID=" + TaxonID + ", RankCategory ='" + RankCategory + "')");
                        break;
                }
            }
    
            if (isLiteratureName) {
                // Add ALN data
                ImportALNData(TaxonNode, TaxonID);
            }

            return true;
        }

        private bool ImportALNData(XmlElement TaxonNode, int TaxonID) {
    
            var XMLALN = TaxonNode.SelectSingleNode("ALN") as XmlElement;
            if (XMLALN == null) {
                Log("The Importer deterimed that there should be Available Literature Name (ALN) data for this Taxon (TaxonID=" + TaxonID + "), but no ALN node could be located !");
                return false;
            }

            var aln = new XMLImportALN(XMLALN) { TaxonID = TaxonID };

            var strRefID = GetNodeValue(XMLALN, "REFID", "");
            if (string.IsNullOrEmpty(strRefID )) {
                strRefID = "NULL";
            } else {
                strRefID = _guidToIDCache.IDfromGUID(strRefID) + "";
            }
        
    
            GenerateUpdateString(XMLALN, "ALN", aln, 
                "intBiotaID=" + TaxonID + ", intRefID=" + strRefID, 
                "intBiotaID, intRefID", 
                TaxonID + ", " + strRefID);
                
            if (XMLIOService.ImportTaxonALN(aln)) {
                Log("ALN Data imported for TaxonID=" + TaxonID);
                return true;
            } else {
                Log("Failed to import ALN Data for TaxonID=" + TaxonID);
            }

            return false;        
        }

        public int Designation(string Desig) {
        
            if (string.IsNullOrEmpty(Desig)) {
                return -1;
            }

            switch(Desig.ToLower()) {
                case "designated (type species)":
                    return 0;                    
                case "none required":
                    return 1;
                case "not designated (with included species)":
                    return 2;
                case "not designated (without included species)":
                    return 3;
                default:
                    return -1;
            }         
        }

        private bool ImportGANData(XmlElement TaxonNode, int TaxonID) {

    
            var XMLGAN = TaxonNode.SelectSingleNode("GAN") as XmlElement;
            if (XMLGAN == null) {
                Log("The Importer determined that there should be Genus Available Name (GAN) data for this Taxon (TaxonID=" + TaxonID + "), but no GAN node could be located !");                
                return false;
            }
            
            var strRefID = GetNodeValue<string>(XMLGAN, "REFID", "");
            if (string.IsNullOrEmpty(strRefID)) {
                strRefID = "NULL";
            } else {
                strRefID = _guidToIDCache.IDfromGUID(strRefID) + "";
            }
    
            var intDesignation = Designation(GetNodeValue<string>(XMLGAN, "DESIGNATION", ""));

            var gan = new XMLImportGAN(XMLGAN) { TaxonID = TaxonID };
        
            GenerateUpdateString(XMLGAN, "GAN", gan, 
                "intBiotaID=" + TaxonID + ", intRefID=" + strRefID + ", sintDesignation=" + intDesignation, 
                "intBiotaID, intRefID, sintDesignation", 
                TaxonID + ", " + strRefID + ", " + intDesignation);
                    
    
            var species = new List<XMLImportGANIncludedSpecies>();
            if (intDesignation == 1 || intDesignation == 2) {
                var NodeList = XMLGAN.SelectNodes("INCLUDEDSPECIES");                
                foreach (XmlElement XMLIS in NodeList) {                
                    var item = new XMLImportGANIncludedSpecies(XMLIS);
                    var strIncSpecies = XMLIS.InnerText;
                    
                    item.InsertClause = "(intBiotaID, vchrIncludedSpecies) VALUES (" + TaxonID + ", '" + strIncSpecies + "')";
                    item.UpdateClause = "intBiotaID=" + TaxonID + ", vchrIncludedSpecies='" + strIncSpecies + "'";
                    
                    species.Add(item);
                }
            }
        
            if (XMLIOService.ImportTaxonGAN(gan, species)) {
                Log("GAN Data imported for TaxonID=" + TaxonID);
                return true;
            } else {
                Log("Failed to import GAN Data for TaxonID=" + TaxonID);
            }

            return false;
        
        }

        private bool ImportSANData(XmlElement TaxonNode, int TaxonID) {
    
            var XMLSAN = TaxonNode.SelectSingleNode("SAN") as XmlElement;
            if (XMLSAN == null) {
                Log("The Importer determined that there should be Species Available Name (SAN) data for this Taxon (TaxonID=" + TaxonID + "), but no SAN node could be located !");
            }


            var san = new XMLImportSAN(XMLSAN) { BiotaID = TaxonID };

            var strRefID = GetNodeValue<string>(XMLSAN, "REFID", "");
            if (string.IsNullOrWhiteSpace(strRefID )) {
                strRefID = "NULL";
            } else {
                strRefID = _guidToIDCache.IDfromGUID(strRefID) + "";
            }
                
            GenerateUpdateString(XMLSAN, "SAN", san, "intBiotaID=" + TaxonID + ", intRefID=" + strRefID, "intBiotaID, intRefID", TaxonID + ", " + strRefID);
            
            var NodeList = XMLSAN.SelectNodes("SANTYPE");
            var list = new List<XMLImportSANType>();
            foreach(XmlElement XMLType in NodeList) {
                var strMaterialID = GetNodeValue<string>(XMLType, "MATERIALID", "");
                if (string.IsNullOrWhiteSpace(strMaterialID)) {
                    strMaterialID = "NULL";
                } else {
                    strMaterialID = _guidToIDCache.IDfromGUID(strMaterialID) + "";
                    if (strMaterialID == "-1") {
                        strMaterialID = "NULL";
                    }
                }

                var sanType = new XMLImportSANType(XMLType);
                GenerateUpdateString(XMLType, "SANTYPE", sanType, "intBiotaID=" + TaxonID + ", intMaterialID=" + strMaterialID, "intBiotaID, intMaterialID", TaxonID + ", " + strMaterialID);
                list.Add(sanType);                    
            }
        
            if (XMLIOService.ImportTaxonSAN(san, list)) {
                Log("SAN Data imported for TaxonID=" + TaxonID);
            } else {
                Log("Failed to import SAN Data for TaxonID=" + TaxonID);
            }

            return true;
        
        }

        private bool ImportPoliticalRegions(XmlElement xmlParentNode, int ParentID ) {
    
            Log("Importing Political Regions...");
    
            if (xmlParentNode == null) {             
                Log("[ImportPoliticalRegions()] Parent Node is nothing! - Internal Error.");
                return false;
            }
    
            var NodeList = xmlParentNode.SelectNodes("REGION");

            foreach (XmlElement XMLRegion in NodeList) {

                var region = new XMLImportRegion(XMLRegion) { ParentID=ParentID, RegionName = GetNodeValue<string>(XMLRegion, "NAME", "ERR") };
        
                GenerateUpdateString(XMLRegion, "Region", region, "intParentID=" + ParentID, "intParentID", ParentID + "");
            
                // Insert region into database and retrieve RegionID
        
                if (!XMLIOService.ImportPoliticalRegion(region)) {
                    Log("Error importing region !");
                } else {

                    Log("Region '" + region.RegionName + "' GUID=" + region.GUID + " Added (RegionID=" + region.ID + ")");
                    _guidToIDCache.Add(region.GUID, region.ID);
                    // Add traits for this region
                    ImportTraits(XMLRegion, "Region", region.ID);
                    ProgressTick(ImportItemType.Region);
            
                    ImportSites(XMLRegion, region.ID);
                    //
                    ImportPoliticalRegions(XMLRegion, region.ID);
                }                
            }
    
            return true;
        }

        private bool ImportSites(XmlElement XMLRegion, int RegionID ) {
    
            if (XMLRegion == null) {
                Log("Region node is nothing ! - Internal Error !");
                return false;
            }
    
            Log("Checking for sites in RegionID=" + RegionID);
    
            var SiteList = XMLRegion.SelectNodes("SITE");
            if (SiteList.Count > 0) {
                Log("Found " + SiteList.Count + " sites to import (RegionID=" + RegionID + ")");
        
                foreach (XmlElement XMLSite in SiteList) {                    
                    var lngLocalityType = GetLocalityType(GetNodeValue<string>(XMLSite, "LOCALITYTYPE", "locality"));
                    var lngCoordType = GetCoordinateType(GetNodeValue<string>(XMLSite, "COORDINATETYPE", "none"));
                    var lngGeometry = GetGeometryType(GetNodeValue<string>(XMLSite, "POSITIONGEOMETRY", "point"));
                    var lngElevationType = GetElevationType(GetNodeValue<string>(XMLSite, "ELEVATIONTYPE", "not specified"));

                    var site = new XMLImportSite(XMLSite) { LocalityType = lngLocalityType, Locality = GetNodeValue<string>(XMLSite, "LOCALITY"), X1=GetNodeValue<double>(XMLSite, "X1"), Y1=GetNodeValue<double>(XMLSite, "Y1")};

                    GenerateUpdateString(XMLSite, "Site", site, 
                        "intPoliticalRegionID=" + RegionID + ", tintLocalType=" + lngLocalityType + ", tintPosCoordinates=" + lngCoordType + ", tintPosAreaType=" + lngGeometry + ", tintElevType=" + lngElevationType, 
                        "intPoliticalRegionID, tintLocalType, tintPosCoordinates, tintPosAreaType, tintElevType", 
                        RegionID + ", " + lngLocalityType + ", " + lngCoordType + ", " + lngGeometry + ", " + lngElevationType);
                        
                    XMLIOService.ImportSite(site);
                    if (site.ID < 0) {
                        Log("Failed to import Site " + site.GUID + " !\nUpdateClause: {0}\n Insert Clause: {1} ", site.UpdateClause, site.InsertClause);
                    } else {
                        Log("Site " + site.GUID + " imported (SiteID=" + site.ID + ")");
                        ImportTraits(XMLSite, "Site", site.ID);
                        ImportNotes(XMLSite, "Site", site.ID);
                        ImportMultimediaLinks(XMLSite, "Site", site.ID);
                
                        // Traps....
                        ImportTraps(XMLSite, site.ID);
                        // Visits...
                        ImportSiteVisits(XMLSite, site.ID);

                        ProgressTick(ImportItemType.Site);
                    }
                }
            }
            return true;
        }

        private int DateStrToInt(string date) {
            if (date == null) {
                return 0;
            }
            date = DateUtils.DateStrToBLDate(date);            
            return date == null ? 0 : int.Parse(date);
        }

        private bool ImportSiteVisits(XmlElement XMLSite, int SiteID) {

            if (XMLSite == null) {
                Log("Site Node is nothing! - Internal Error !");
                return false;
            }
    
            Log("Checking for Site Visits in SiteID=" + SiteID);
            var XMLSiteVisitList = XMLSite.SelectNodes("SITEVISIT");
            if (XMLSiteVisitList.Count > 0) {

                
                Log("Found " + XMLSiteVisitList.Count + " Site Visits to import (SiteID=" + SiteID + ")");

                foreach (XmlElement XMLSiteVisit in XMLSiteVisitList) {
                    
                    var lngDateStart = DateStrToInt(GetNodeValue<string>(XMLSiteVisit, "DATESTART"));
                    var lngDateEnd = DateStrToInt(GetNodeValue<string>(XMLSiteVisit, "DATEEND"));
                    var lngTimeStart = DateUtils.StrToBLTime(GetNodeValue<string>(XMLSiteVisit, "TIMESTART"));
                    var lngTimeEnd = DateUtils.StrToBLTime(GetNodeValue<string>(XMLSiteVisit, "TIMEEND"));

                    var visit = new XMLImportSiteVisit(XMLSiteVisit) { Collector = GetNodeValue<string>(XMLSiteVisit, "COLLECTOR"), SiteID = SiteID, StartDate = lngDateStart };
            
                    GenerateUpdateString(XMLSiteVisit, "SiteVisit", visit, 
                    "intSiteID=" + SiteID + ", intDateStart=" + lngDateStart + ", intDateEnd=" + lngDateEnd + ", intTimeStart=" + lngTimeStart + ", intTimeEnd=" + lngTimeEnd, 
                    "intSiteID, intDateStart, intDateEnd, intTimeStart, intTimeEnd", 
                    SiteID + ", " + lngDateStart + ", " + lngDateEnd + ", " + lngTimeStart + ", " + lngTimeEnd);
                        
                    XMLIOService.ImportSiteVisit(visit);
                    if (visit.ID < 0) {
                        Log("Failed to insert SiteVisit " + visit.GUID + "!\nUpdateClause: {0}\nInsertClause: {1}", visit.UpdateClause, visit.InsertClause);
                        return false;
                    } else {
                        Log("SiteVisit " + visit.GUID + " imported (SiteVisitID=" + visit.ID + ")");
                        _guidToIDCache.Add(visit.GUID, visit.ID);
                        // Traits
                        ImportTraits(XMLSiteVisit, "SiteVisit", visit.ID);
                        // Notes
                        ImportNotes(XMLSiteVisit, "SiteVisit", visit.ID);
                        // Do all material for this visit...
                        ImportMaterial(XMLSiteVisit, visit.ID);
                        // Update Status
                        ProgressTick(ImportItemType.SiteVisit);
                    }
                }
            }
            return true;
        }

        private bool ImportMaterial(XmlElement XMLVisit, int SiteVisitID) {
        
            if (XMLVisit == null) {
                Log("Site Visit Node is nothing! - Internal Error !");
                return false;
            }
    
            Log("Checking for Material for SiteVisitID=" + SiteVisitID);
            var XMLMaterialList = XMLVisit.SelectNodes("MATERIAL");
            if (XMLMaterialList.Count > 0) {
                Log("Found " + XMLMaterialList.Count + " pieces of material to import (SiteVisitID=" + SiteVisitID + ")");
        
                foreach (XmlElement XMLMaterial in XMLMaterialList) {
                    var material = new XMLImportMaterial(XMLMaterial);
            
                    GenerateUpdateString(XMLMaterial, "Material", material, "intSiteVisitID=" + SiteVisitID, "intSiteVisitID", SiteVisitID + "");
                            
                    XMLIOService.ImportMaterial(material);
                    if (material.ID > 0) {
                        Log("Material " + material.GUID + " imported (MaterialID=" + material.ID + ")");
                        _guidToIDCache.Add(material.GUID, material.ID);
                        // ID History
                        ImportMaterialIDHistory(XMLMaterial, material.ID);
                        // Subparts
                        ImportMaterialSubParts(XMLMaterial, material.ID);
                        // Events
                        ImportMaterialEvents(XMLMaterial, material.ID);
                        // Traits
                        ImportTraits(XMLMaterial, "Material", material.ID);
                        // Notes
                        ImportNotes(XMLMaterial, "Material", material.ID);
                        // Multimedia links
                        ImportMultimediaLinks(XMLMaterial, "Material", material.ID);

                        ProgressTick(ImportItemType.Material);
                    } else {
                        Log("Failed to import Material (GUID=" + material.GUID + ")!\nUpdateClause: {0}\nInsertClause: {1}", material.UpdateClause, material.InsertClause);
                    }
            
                }
            }

            return true;
        }

        private bool ImportMaterialEvents(XmlElement XMLMaterial, int MaterialID) {
    
            if (XMLMaterial == null) {
                Log("[ImportMaterialEvents()] Material Node is nothing ! Internal Error !");
                return false;
            }
    
            Log("Checking for Event items (MaterialID=" + MaterialID + ")");
            var NodeList = XMLMaterial.SelectNodes("CURATIONEVENTS/CURATIONEVENT");
    
            if (NodeList.Count == 0) {
                Log("No Curation Events found (MaterialID=" + MaterialID + ")");
                return false;
            }
        
            var list = new List<XMLImportEvent>();
            
            foreach (XmlElement XMLID in NodeList) {
                var evt = new XMLImportEvent(XMLID);                
                GenerateUpdateString(XMLID, "curationevent", evt, "intMaterialID=" + MaterialID, "intMaterialID", MaterialID + "");
                list.Add(evt);

            }
    
            if (list.Count > 0) {
                if (!XMLIOService.InsertMaterialEvents(list)) {
                    Log("Failed to import Events for MaterialID=" + MaterialID + "!");
                    return false;
                }
            }
    
            return true;
        }

        private bool ImportMaterialSubParts(XmlElement XMLMaterial, int MaterialID) {
    
            if (XMLMaterial == null) {
                Log("[ImportMaterialSubParts()] Material Node is nothing ! Internal Error !");
                return false;
            }
    
            Log("Checking for Subpart items (MaterialID=" + MaterialID + ")");
            var NodeList = XMLMaterial.SelectNodes("SUBPARTS/SUBPART");
    
            if (NodeList.Count == 0) {
                Log("No Sub Parts found (MaterialID=" + MaterialID + ")");
                return false;
            }
    
            var list = new List<XMLImportSubPart>();
            
            foreach (XmlElement XMLID in NodeList) {
                var subpart = new XMLImportSubPart(XMLID);                
                GenerateUpdateString(XMLID, "Subpart", subpart, "intMaterialID=" + MaterialID, "intMaterialID", MaterialID + "");
                list.Add(subpart);        
            }
    
            if (list.Count > 0) {
                if (!XMLIOService.InsertMaterialSubparts(list)) {
                    Log("Failed to import Subparts for MaterialID=" + MaterialID + " !");
                }
            }

            return true;
        }


        private bool ImportMaterialIDHistory(XmlElement XMLMaterial, int MaterialID) {
    
            if (XMLMaterial == null) {
                Log("[ImportMaterialHistory()] Material Node is nothing ! Internal Error !");
                return false;
            }
    
            Log("Checking for Identification History items (MaterialID=" + MaterialID + ")");
            var NodeList = XMLMaterial.SelectNodes("IDENTIFICATIONHISTORY/IDENTIFICATION");
    
            if (NodeList.Count == 0) {
                Log("No ID History items found (MaterialID=" + MaterialID + ")");
                return false;
            }

            var list = new List<XMLImportIDHistory>();
        
            foreach (XmlElement XMLID in NodeList) {
                var hist =new XMLImportIDHistory(XMLID);                
                GenerateUpdateString(XMLID, "Identification", hist, "intMaterialID=" + MaterialID, "intMaterialID", MaterialID + "");        
                list.Add(hist);
            }
    
            if (list.Count > 0) {
                if (! XMLIOService.InsertMaterialIdentification(list)) {
                    Log("Failed to import ID History for MaterialID=" + MaterialID + " !");
                }
            }

            return true;    
        }

        private bool ImportTraps(XmlElement XMLSite, int SiteID ) {
    
            if (XMLSite == null) {
                Log("Site Node is nothing! - Internal Error !");
                return false;
            }
    
            Log("Checking for Traps in SiteID=" + SiteID);
            var XMLTrapList = XMLSite.SelectNodes("TRAP");
            if (XMLTrapList.Count > 0) {
                Log("Found " + XMLTrapList.Count + " Traps to import (SiteID=" + SiteID + ")");
        
        
                foreach (XmlElement XMLTrap in XMLTrapList) {
                    var trap = new XMLImportTrap(XMLTrap) { SiteID = SiteID, TrapName=GetNodeValue<string>(XMLTrap, "NAME") };
                    GenerateUpdateString(XMLTrap, "Trap", trap, "intSiteID=" + SiteID, "intSiteID", SiteID + "");
                                
                    XMLIOService.ImportTrap(trap);
            
                    if (trap.ID > 0) {
                        Log("Trap " + trap.GUID + " imported (TrapID=" + trap.ID + ")");
                        _guidToIDCache.Add(trap.GUID, trap.ID);
                        // Traits
                        ImportTraits(XMLTrap, "Trap", trap.ID);
                        // Notes
                        ImportNotes(XMLTrap, "Trap", trap.ID);

                        // ProgressTick(ImportItemType.Trap);
                        
                    } else {
                        Log("Failed to import trap " + trap.GUID + "!\n UpdateClause: {0}\nInsertClause: {1}", trap.UpdateClause, trap.InsertClause);
                        return false;
                    }                    
                }
            }

            return true;
        }

        private bool AddTaxonChildren(XmlElement ParentNode, int ParentID) {

            var ChildList = ParentNode.SelectNodes("TAXON");

            foreach (XmlElement XMLChild in ChildList) {
                if (IsCancelled) {
                    return false;
                }

                var strFullName = GetNodeValue(XMLChild, "NAME", "ERR");
                var strGUID = XMLChild.GetAttributeValue("ID", "");
                Log("Adding taxon '" + strFullName + "' " + strGUID);
                var strEpithet = GetNodeValue(XMLChild, "EPITHET", "");
                int lngTaxonID;
                if (!XMLIOService.FindTaxon(strGUID, strFullName, strEpithet, ParentID, out lngTaxonID)) {
                    Log("Failed to get TaxonID for '" + strFullName + "' - GUID=" + strGUID + ".");
                    return false;
                }

                var bIsAvailableName = GetNodeValue<bool>(XMLChild, "ISAVAILABLENAME", false);
                var bIsLiteratureName = GetNodeValue<bool>(XMLChild, "ISLITERATURENAME", false);

                var taxon = new XMLImportTaxon(XMLChild) { Rank = GetNodeValue(XMLChild, "RANK", ""), Kingdom = GetNodeValue(XMLChild, "KINGDOM", ""), ID = lngTaxonID, AvailableName=bIsAvailableName, LiteratureName = bIsLiteratureName };
                GenerateUpdateString(XMLChild, "Taxon", taxon);
                Log("Updating Taxon '" + strFullName + "' (TaxonID=" + taxon.ID + ",Rank=" + taxon.Rank + ",Kingdom=" + taxon.Kingdom + ")...");

                if (!XMLIOService.UpdateTaxon(taxon)) {
                    Log("Taxon update failed! {0}", taxon.GUID);
                    return false;
                }

                if (bIsAvailableName || bIsLiteratureName) {
                    var ANItem = new AvailableNameInfo { IsAvailableName = bIsAvailableName, IsLiteratureName = bIsLiteratureName, RankCategory = taxon.RankCategory, TaxonID = taxon.ID, XMLNode = XMLChild };
                    _AvailableNameList.Add(strGUID, ANItem);
                }

                ImportCommonNames(XMLChild, lngTaxonID);
                ImportRefLinks(XMLChild, "Taxon", lngTaxonID);
                ImportDistribution(XMLChild, lngTaxonID);
                ImportTraits(XMLChild, "Taxon", lngTaxonID);
                ImportNotes(XMLChild, "Taxon", lngTaxonID);
                // ImportKeyWords XMLChild, "Taxon", lngTaxonID
                ImportMultimediaLinks(XMLChild, "Taxon", lngTaxonID);
                ImportStorageLocations(XMLChild, lngTaxonID);

                //        ' Add this GUID to the cache...
                _guidToIDCache.Add(strGUID, lngTaxonID);
                ProgressTick(ImportItemType.Taxon);
                AddTaxonChildren(XMLChild, lngTaxonID);
            }

            return true;
        }

        private bool ImportStorageLocations(XmlElement TaxonNode, int TaxonID) {
            Log("Importing Taxon Storage Locations for TaxonID=" + TaxonID);
            var XMLDistNode = TaxonNode.SelectSingleNode("STORAGELOCATIONS");
            if (XMLDistNode == null) {
                Log("Failed to locate a STORAGELOCATIONS node in Taxon node (TaxonID=" + TaxonID + ") !");
                return false;
            }

            var list = new List<XMLImportStorageLocation>();

            var ItemList = XMLDistNode.SelectNodes("STORAGELOCATION");
            foreach (XmlElement XMLStorageLocationNode in ItemList) {
                var strDelimiter = XMLStorageLocationNode.GetAttributeValue("PATHSEPARATOR", "\\");
                var strPath = GetNodeValue<string>(XMLStorageLocationNode, "FULLPATH");
                if (!string.IsNullOrEmpty(strPath)) {
                    var lngStorageLocationID = ImportStorageLocation(strPath, strDelimiter);
                    if (lngStorageLocationID < 0) {
                        Log("Failed to map/add StorageLocation Path to StorageLocationID ! - " + strPath);
                    } else {
                        var loc = new XMLImportStorageLocation(XMLStorageLocationNode) { TaxonID = TaxonID, LocationID = lngStorageLocationID };
                        GenerateUpdateString(XMLStorageLocationNode, "StorageLocation", loc,
                            "intBiotaID=" + TaxonID + ", intBiotaStorageID=" + lngStorageLocationID,
                            "intBiotaID, intBiotaStorageID",
                            TaxonID + ", " + lngStorageLocationID);

                        list.Add(loc);
                    }
                }
            }

            if (list.Count > 0) {
                if (!XMLIOService.ImportTaxonStorage(list)) {
                    Log("Failed to insert distribution items for TaxonID=" + TaxonID + "!");
                }
            }
            return true;
        }

        private bool ImportDistribution(XmlElement TaxonNode, int TaxonID) {

            Log("Importing Distribution Regions for TaxonID=" + TaxonID);
            var XMLDistNode = TaxonNode.SelectSingleNode("DISTRIBUTION");
            if (XMLDistNode == null) {
                Log("Failed to locate a DISTRIBUTION node in Taxon node !");
                return false;
            }


            var list = new List<XMLImportDistribution>();

            var ItemList = XMLDistNode.SelectNodes("DISTRIBUTIONITEM");
            foreach (XmlElement XMLRegionNode in ItemList) {

                var strDelimiter = XMLRegionNode.GetAttributeValue("PATHSEPARATOR", "\\");
                var strPath = GetNodeValue<string>(XMLRegionNode, "FULLPATH");
                var lngRegionID = ImportDistributionRegion(strPath, strDelimiter);
                if (lngRegionID < 0) {
                    Log("Failed to map/add Region Path to RegionID ! - " + strPath);
                } else {
                    var dist = new XMLImportDistribution(XMLRegionNode) { TaxonID = TaxonID, RegionID = lngRegionID };
                    GenerateUpdateString(XMLRegionNode, "DistributionItem", dist, "intBiotaID=" + TaxonID + ", intDistributionRegionID=" + lngRegionID, "intBiotaID, intDistributionRegionID", TaxonID + ", " + lngRegionID);
                    list.Add(dist);

                }
            }

            if (list.Count > 0) {
                if (!XMLIOService.ImportTaxonDistribution(list)) {
                    Log("Failed to insert distribution items for TaxonID=" + TaxonID + " !");
                }
            }

            return true;
        }

        private int ImportStorageLocation(string Path, string Delimiter) {

            int lngLocationID;
            if (_StorageCache.NameInCache(Path, out lngLocationID)) {
                return lngLocationID;
            }

            var lngIdx = Path.LastIndexOf(Delimiter);
            string strLocation;
            int lngParentID;

            if (lngIdx > 0) {
                var strSubPath = Path.Substring(0, lngIdx);
                strLocation = Path.Substring(lngIdx + 1);
                lngParentID = ImportStorageLocation(strSubPath, Delimiter);
                if (lngParentID < 0) {
                    return -1;
                }
            } else { // If no delimiter found, then it is a root node.
                strLocation = Path;
                lngParentID = 0;         // A ParentID of 0 means its a root node...
            }

            lngLocationID = XMLIOService.ImportStorageLocation(strLocation, lngParentID);

            if (lngLocationID < 0) {
                Log("Failed to add storage location '" + strLocation + "'");
            } else {
                _StorageCache.Add(Path, lngLocationID);
            }
            return lngLocationID;

        }


        private int ImportDistributionRegion(string Path, string Delimiter) {

            int lngRegionID;
            if (_DistRegionCache.NameInCache(Path, out lngRegionID)) {
                return lngRegionID;
            }

            var lngIdx = Path.LastIndexOf(Delimiter);

            string strRegion;
            int lngParentID;

            if (lngIdx > 0) {
                var strSubPath = Path.Substring(0, lngIdx);
                strRegion = Path.Substring(lngIdx + 1);
                lngParentID = ImportDistributionRegion(strSubPath, Delimiter);
                if (lngParentID < 0) {
                    return -1;
                }
            } else {                     // If no delimiter found, then it is a root node.
                strRegion = Path;
                lngParentID = 0;         // A ParentID of 0 means its a root node...
            }

            lngRegionID = XMLIOService.InsertDistributionRegion(strRegion, lngParentID);

            if (lngRegionID < 0) {
                Log("Failed to add region '" + strRegion + "'");
            } else {
                _DistRegionCache.Add(Path, lngRegionID);
            }

            return lngRegionID;
        }

        private bool ImportRefLinks(XmlElement ParentNode, string ItemType, int ItemID) {

            Log("Adding references for " + ItemType + "ID=" + ItemID);
            var XMLRefListNode = ParentNode.SelectSingleNode("REFERENCES");
            if (XMLRefListNode == null) {
                Log("Failed to locate REFERENCES node for " + ItemType + " (ID=" + ItemID + ")");
                return false;
            }

            var NodeList = XMLRefListNode.SelectNodes("REFERENCELINK");
            var list = new List<XMLImportRefLink>();
            int lngCategoryID;
            if (!GetCategoryID(ItemType, out lngCategoryID)) {
                Log("Failed to get category id for item type {0}", ItemType);
                return false;
            }

            foreach (XmlElement XMLRefLink in NodeList) {

                var strRefLinkType = GetNodeValue(XMLRefLink, "REFLINKTYPE", "");
                int lngRefLinkTypeID;
                if (!_RefLinkTypeCache.NameInCache(strRefLinkType, out lngRefLinkTypeID)) {

                    lngRefLinkTypeID = XMLIOService.GetRefLinkTypeID(lngCategoryID, strRefLinkType);
                    if (lngRefLinkTypeID < 0) {
                        Log("Failed to RefLinkTypeID for '" + strRefLinkType + "'");
                        return false;
                    } else {
                        _RefLinkTypeCache.Add(strRefLinkType, lngRefLinkTypeID);
                    }
                }

                var link = new XMLImportRefLink(XMLRefLink);

                GenerateUpdateString(XMLRefLink, "REFERENCELINK", link, "intRefLinkTypeID=" + lngRefLinkTypeID + ", intCatID=" + lngCategoryID + ", intIntraCatID=" + ItemID, "intRefLinkTypeID, intCatID, intIntraCatID", lngRefLinkTypeID + ", " + lngCategoryID + ", " + ItemID);
                list.Add(link);


            }

            if (list.Count > 0) {
                if (!XMLIOService.ImportReferenceLinks(list)) {
                    Log("Failed to add RefLinks for Item {0} {1}", ItemType, ItemID);
                }
            }

            return true;
        }

        public XmlElement GetTaxaRoot() {
            return _xmlDoc.SelectSingleNode("//*/DATA/TAXA") as XmlElement;
        }

        private bool ImportCommonNames(XmlElement TaxonNode, int TaxonID) {

            var XMLNameParent = TaxonNode.SelectSingleNode("COMMONNAMELIST");

            if (XMLNameParent == null) {
                Log("Failed to locate COMMONNAME node in TAXON (ID=" + TaxonID + ")");
                return false;
            }

            var NodeList = XMLNameParent.SelectNodes("COMMONNAME");
            var list = new List<XMLImportCommonName>();
            foreach (XmlElement XMLCN in NodeList) {
                var cn = new XMLImportCommonName(XMLCN) { CommonName = GetNodeValue<string>(XMLCN, "NAME") };
                GenerateUpdateString(XMLCN, "CommonName", cn, "intBiotaID=" + TaxonID, "intBiotaID", TaxonID + "");
                list.Add(cn);
            }

            if (list.Count > 0) {
                if (!XMLIOService.ImportCommonNames(TaxonID, list)) {
                    Log("Failed to add Common Names for TaxonID " + TaxonID + " !");
                    return false;
                }
            }
            return true;
        }


        private void ImportReferences() {
            var XMLReferenceParent = _xmlDoc.SelectSingleNode("//*/DATA/REFERENCES");

            Log("Importing References");
            if (XMLReferenceParent == null) {
                Log("Failed to get REFERENCES node from XML File (No References Imported)");
                return;
            }

            var NodeList = XMLReferenceParent.SelectNodes("REFERENCE");

            foreach (XmlElement XMLReference in NodeList) {                
                var reference = new XMLImportReference(XMLReference) { Code = GetNodeValue<string>(XMLReference, "REFCODE"), Author = GetNodeValue<string>(XMLReference, "AUTHOR"), Year = GetNodeValue<string>(XMLReference, "YEAR") };
                GenerateUpdateString(XMLReference, "REFERENCE", reference);
                if (XMLIOService.ImportReference(reference)) {
                    var lngRefID = reference.ID;
                    if (lngRefID < 0) {
                        Log("Failed to get ReferenceID from Reference Array (Not set in server?) (ID=" + reference.GUID + ")");
                    } else {
                        _guidToIDCache.Add(reference.GUID, lngRefID);
                        ImportTraits(XMLReference, "Reference", lngRefID);
                        ImportNotes(XMLReference, "Reference", lngRefID);
                        // ImportKeyWords XMLReference, "Reference", lngRefID
                        ImportMultimediaLinks(XMLReference, "Reference", lngRefID);
                        ProgressTick(ImportItemType.Reference);
                    }
                } else {
                    Log("Failed to import reference (ID=" + reference.GUID + ")");
                }

            }
        }

        private bool ImportMultimediaLinks(XmlNode XMLParent, string ItemType, int ItemID) {

            Log("Importing Multimedia links for " + ItemType + "ID=" + ItemID);

            var XMLMMParent = XMLParent.SelectSingleNode("MULTIMEDIA");

            if (XMLMMParent == null) {
                Log("Failed to locate MULTIMEDIA node for item (" + ItemType + "ID=" + ItemID + ")");
                return false;
            }

            int lngCategoryID;
            if (!GetCategoryID(ItemType, out lngCategoryID)) {
                Log("Failed get get category id for item type {0}", ItemType);
                return false;
            }


            var MultimediaList = XMLMMParent.SelectNodes("MULTIMEDIALINK");
            if (MultimediaList.Count == 0) {
                Log("No MULTIMEDIALINK nodes found in MULTIMEDIA node for " + ItemType + "ID=" + ItemID + ". No links imported.");
                return false;
            }

            var list = new List<XMLImportMultimediaLink>();

            foreach (XmlElement XMLNode in MultimediaList) {

                var bOK = true;
                var strMMGUID = GetNodeValue(XMLNode, "MULTIMEDIAID", "ERR");
                var lngMultimediaID = _guidToIDCache.IDfromGUID(strMMGUID);
                if (lngMultimediaID < 0) {
                    Log("Failed to locate Multimedia item " + strMMGUID + " in cache. It either failed to import or was not present in source file.");
                    bOK = false;
                }

                var strMMType = GetNodeValue(XMLNode, "MULTIMEDIATYPE", "ERR");
                int lngMultimediaTypeID;
                if (!_MultimediaTypeCache.NameInCache(strMMType, out lngMultimediaTypeID)) {
                    lngMultimediaTypeID = XMLIOService.GetMultimediaTypeID(lngCategoryID, strMMType);
                    if (lngMultimediaTypeID < 0) {
                        Log("Failed to get MultimediaTypeID for " + strMMGUID + " Type = '" + strMMType + "'");
                    } else {
                        _MultimediaTypeCache.Add(strMMType, lngMultimediaTypeID);
                    }
                }

                if (bOK) {                    
                    var mml = new XMLImportMultimediaLink(XMLNode) { Caption=GetNodeValue<string>(XMLNode, "CAPTION") };
                    GenerateUpdateString(XMLNode, "MultimediaLink", mml,
                        "intMultimediaTypeID=" + lngMultimediaTypeID + ", intCatID=" + lngCategoryID + ", intIntraCatID=" + ItemID + ", intMultimediaID=" + lngMultimediaID,
                        "intMultimediaTypeID, intCatID, intIntraCatID, intMultimediaID",
                        lngMultimediaTypeID + ", " + lngCategoryID + ", " + ItemID + ", " + lngMultimediaID);

                    list.Add(mml);
                }
            }

            if (list.Count > 0) {
                // Call the server and add the links to this record. !
                if (XMLIOService.ImportMultimediaLink(list)) {
                    //For i = 0 To lngRowCount - 1
                    //    If GetRowProperty(vData, i, "Status", BL_OK) <> BL_OK Then
                    //        Elog "Error inserting multimedia link " & GetRowProperty(vData, i, "GUID", "ERR") & " (" & ItemType & "ID=" & ItemID & ") ! - " & GetRowProperty(vData, i, "ErrorMsg", "")
                    //    End If
                    //Next i
                } else {
                    Log("Failed to insert Multimedia Links for " + ItemType + "ID=" + ItemID + " !");
                }
            }

            return true;
        }


        private void ImportJournals() {

            var XMLJournalParent = _xmlDoc.SelectSingleNode("//*/DATA/JOURNALS");

            Log("Importing Journals");
            if (XMLJournalParent == null) {
                Log("Failed to get JOURNALS node from XML File (No Journals Imported)");
                return;
            }

            var list = new List<XMLImportJournal>();

            var NodeList = XMLJournalParent.SelectNodes("JOURNAL");

            foreach (XmlElement XMLJournal in NodeList) {

                var journal = new XMLImportJournal(XMLJournal) { FullName = GetNodeValue<string>(XMLJournal, "FULLNAME") };
                GenerateUpdateString(XMLJournal, "Journal", journal);

                if (XMLIOService.ImportJournal(journal)) {
                    _guidToIDCache.Add(journal.GUID, journal.ID);
                    // var XMLJournal = GetNodeByGUID(strGUID, "JOURNAL")
                    ImportTraits(XMLJournal, "Journal", journal.ID);
                    ImportNotes(XMLJournal, "Journal", journal.ID);
                    ProgressTick(ImportItemType.Journal);
                } else {
                    Log("Failed to extract journal id from Journal Data (ID=" + journal.GUID + ")");
                }
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
                        var mm = new XMLImportMultimedia(XMLMM) { ImageData = imageData };

                        GenerateUpdateString(XMLMM, "MULTIMEDIAITEM", mm);
                        if (XMLIOService.ImportMultimedia(mm)) {
                            _guidToIDCache.Add(strGUID, mm.ID);
                            lngRowCount = lngRowCount + 1;

                            ImportTraits(XMLMM, "Multimedia", mm.ID);
                            ImportNotes(XMLMM, "Multimedia", mm.ID);
                            ProgressTick(ImportItemType.Multimedia);
                        }


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

            var notes = new List<XMLImportNote>();

            foreach (XmlElement XMLNote in NoteList) {
                //var strNoteGUID = XMLNote.GetAttributeValue("ID", "ERR");
                var strNoteType = GetNodeValue(XMLNote, "NOTETYPE", "ERR");
                int lngNoteTypeID = 0;
                var cacheKey = lngCategoryID + "_" + strNoteType;
                if (!_NoteTypeCache.NameInCache(cacheKey, out lngNoteTypeID)) {
                    lngNoteTypeID = XMLIOService.NoteGetTypeID(lngCategoryID, strNoteType);
                    if (lngNoteTypeID < 0) {
                        Log("Failed to get NoteTypeID for Note !");
                        return;
                    } else {
                        _NoteTypeCache.Add(cacheKey, lngNoteTypeID);
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

                var note = new XMLImportNote(XMLNote);
                GenerateUpdateString(XMLNote, "NoteItem", note,
                    "intNoteTypeID=" + lngNoteTypeID + ", intCatID=" + lngCategoryID + ", intIntraCatID=" + ItemID + ", intRefID=" + strRefID,
                    "intNoteTypeID, intCatID, intIntraCatID, intRefID",
                    lngNoteTypeID + ", " + lngCategoryID + ", " + ItemID + ", " + strRefID);

                notes.Add(note);
            }

            if (notes.Count > 0) {
                XMLIOService.InsertNotes(notes);
            }
        }

        private bool GetCategoryID(string ItemType, out int CategoryID) {


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

        private void ProgressTick(ImportItemType itemType) {
            var item = _progressItems[itemType];
            var count = item.Completed += 1;
            if (Observer != null) {
                Observer.ProgressTick(item.Name, count);
            }
        }

        private void GenerateUpdateString(XmlElement XMLNode, string ObjectType, XMLImportObject transferObject, string UpdateExtra = "", string InsertExtraField = "", string InsertExtraValue = "") {
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
                        var strValue = XMLElement.InnerText.Replace("'", "''").Trim();
                        if (!CheckForObjectReference(ObjectType, strFieldName, ref strValue)) {

                            if (strFieldName.StartsWith("vchr") || strFieldName.StartsWith("chr") || strFieldName.StartsWith("txt") || strFieldName.StartsWith("dt")) {
                                strValue = string.Format("'{0}'", strValue);
                            } else if (strFieldName.StartsWith("bit")) {
                                strValue = BoolToBit(strValue);
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
                strValues = strValues.Substring(0, strValues.Length - 2);
            }

            transferObject.UpdateClause = strUpdate;
            transferObject.InsertClause = "(" + strFields + ") VALUES (" + strValues + ")";
        }

        private string BoolToBit(string boolStr) {
            if (string.IsNullOrEmpty(boolStr)) {
                return "0";
            }

            if (boolStr.Equals("1") || boolStr.Equals("0")) {
                return boolStr;
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
                        case "intPoliticalRegionID":
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
            _AvailableNameList = new Dictionary<string, AvailableNameInfo>();

            _progressItems = new Dictionary<ImportItemType, XMLImportProgressItem>();
            _progressItems[ImportItemType.Taxon] = new XMLImportProgressItem { Name = "Taxon", Total = GetNodeValue(_xmlDoc, "//*/META/TAXONCOUNT", 0) };
            _progressItems[ImportItemType.Material] = new XMLImportProgressItem { Name = "Material", Total = GetNodeValue(_xmlDoc, "//*/META/MATERIALCOUNT", 0) };
            _progressItems[ImportItemType.SiteVisit] = new XMLImportProgressItem { Name = "SiteVisit", Total = GetNodeValue(_xmlDoc, "//*/META/SITEVISITCOUNT", 0) };
            _progressItems[ImportItemType.Site] = new XMLImportProgressItem { Name = "Site", Total = GetNodeValue(_xmlDoc, "//*/META/SITECOUNT", 0) };
            _progressItems[ImportItemType.Region] = new XMLImportProgressItem { Name = "Region", Total = GetNodeValue(_xmlDoc, "//*/META/REGIONCOUNT", 0) };
            _progressItems[ImportItemType.Journal] = new XMLImportProgressItem { Name = "Journal", Total = GetNodeValue(_xmlDoc, "//*/META/JOURNALCOUNT", 0) };
            _progressItems[ImportItemType.Reference] = new XMLImportProgressItem { Name = "Reference", Total = GetNodeValue(_xmlDoc, "//*/META/REFERENCECOUNT", 0) };
            _progressItems[ImportItemType.Associate] = new XMLImportProgressItem { Name = "Associate", Total = GetNodeValue(_xmlDoc, "//*/META/ASSOCIATECOUNT", 0) };
            _progressItems[ImportItemType.Multimedia] = new XMLImportProgressItem { Name = "Multimedia", Total = GetNodeValue(_xmlDoc, "//*/META/MULTIMEDIACOUNT", 0) };


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

                var str = node.InnerText;

                if (string.IsNullOrWhiteSpace(str)) {
                    return @default;
                }

                if (typeof(int).IsAssignableFrom(typeof(T))) {
                    return (T)(object)int.Parse(str);
                } else if (typeof(double).IsAssignableFrom(typeof(T))) {
                    return (T)(object)double.Parse(str);
                } else if (typeof(bool).IsAssignableFrom(typeof(T))) {
                    if (str.IsInteger()) {
                        int i = int.Parse(str);
                        return (T)(object)(i != 0);
                    } else {
                        return (T)(object)bool.Parse(str);
                    }
                }

                return (T)(object)str;
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

    class AvailableNameInfo {
        public int TaxonID { get; set; }
        public XmlNode XMLNode { get; set; }
        public bool IsAvailableName { get; set; }
        public bool IsLiteratureName { get; set; }
        public string RankCategory { get; set; }
    }

    public enum ImportItemType { 
        Taxon, Material, SiteVisit, Site, Region, Journal, Reference, Associate, Multimedia
    }

}
