using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Data {

    public abstract class XMLIOBase {

        protected Func<bool> _isCancelled;
        private TextWriter _logWriter = null;

        private FieldToNameMappings _taxonMappings;
        private FieldToNameMappings _taxonALNMappings;
        private FieldToNameMappings _taxonGANMappings;
        private FieldToNameMappings _taxonSANMappings;
        private FieldToNameMappings _taxonSANTypeDataMappings;
        private FieldToNameMappings _commonNameMappings;
        private FieldToNameMappings _referenceMappings;
        private FieldToNameMappings _multimediaMappings;
        private FieldToNameMappings _journalMappings;
        private FieldToNameMappings _refLinkMappings;
        private FieldToNameMappings _distributionMappings;
        private FieldToNameMappings _multimediaLinkMappings;
        private FieldToNameMappings _notesMappings;
        private FieldToNameMappings _keywordMappings;
        private FieldToNameMappings _storageLocationMappings;
        private FieldToNameMappings _materialMappings;
        private FieldToNameMappings _IDHistoryMappings;
        private FieldToNameMappings _subPartMappings;
        private FieldToNameMappings _associateMappings;
        private FieldToNameMappings _curationEventMappings;
        private FieldToNameMappings _siteVisitMappings;
        private FieldToNameMappings _siteMappings;
        private FieldToNameMappings _regionMappings;
        private FieldToNameMappings _trapMappings;
        private static string[] _coordType = new string[] { "None", "Latitude Longitude", "Eastings Northings" };
        private static string[] _localType = new string[] { "Nearest Place", "Locality" };
        private static string[] _geomType = new string[] { "Point", "Line", "Bounding Box" };
        private static string[] _elevType = new string[] { "Not Specified", "Elevation", "Depth" };

        protected string IntTypeToStr(int index, string[] array) {
            if (index >= 0 && index < array.Length) {
                return array[index];
            }
            return "";
        }

        protected int StrToIntType(string str, string[] array) {
            for (int i = 0; i < array.Length; ++i) {
                if (str.Equals(array[i], StringComparison.CurrentCultureIgnoreCase)) {
                    return i;
                }
            }
            return -1;
        }

        protected int GetCoordinateType(string str) {
            return StrToIntType(str, _coordType);
        }

        protected int GetLocalityType(string str) {
            return StrToIntType(str, _localType);
        }

        protected int GetGeometryType(string str) {
            return StrToIntType(str, _geomType);
        }

        protected int GetElevationType(string str) {
            return StrToIntType(str, _elevType);
        }

        protected string ElevationTypeStr(int p) {
            return IntTypeToStr(p, _elevType);
        }

        protected string GeometryTypeStr(int p) {
            return IntTypeToStr(p, _geomType);
        }

        protected string CoordinateTypeStr(int p) {
            return IntTypeToStr(p, _coordType);
        }

        protected string LocalityTypeStr(int localType) {
            return IntTypeToStr(localType, _localType);
        }

        protected XMLIOBase(User user) {
            this.User = user;
            this.TaxaService = new TaxaService(User);
            this.SupportService = new SupportService(User);
            this.XMLIOService = new XMLIOService(User);
            this.MaterialService = new MaterialService(User);
        }

        protected void InitLog(string logFile) {
            _logWriter = new StreamWriter(logFile, false);
        }

        protected void FinalizeLog() {
            if (_logWriter != null) {
                _logWriter.Close();
                _logWriter.Dispose();
            }
        }

        protected bool IsCancelled {
            get {
                if (_isCancelled != null) {
                    return _isCancelled();
                }

                return false;
            }
        }


        protected void InitMappings() {

            Log("Initializing field mappings...");

            // Taxon
            _taxonMappings = new FieldToNameMappings();

            _taxonMappings.Add("NAME", "vchrFullName");
            _taxonMappings.Add("EPITHET", "vchrEpithet");
            _taxonMappings.Add("PUBLICATIONYEAR", "vchrYearOfPub");
            _taxonMappings.Add("AUTHOR", "vchrAuthor");
            // _taxonMappings.Add("NAMEQUALIFIER", "vchrNameQualifier"); ???
            _taxonMappings.Add("NAMESTATUS", "vchrAvailableNameStatus");
            _taxonMappings.Add("ELEMENTTYPE", "chrElemType");
            _taxonMappings.Add("RANK", "RankLong", false);
            _taxonMappings.Add("KINGDOM", "KingdomLong", false);
            _taxonMappings.Add("ORDER", "intOrder");
            _taxonMappings.Add("ISCHANGEDCOMBINATION", "bitChangedComb");
            _taxonMappings.Add("ISUNPLACED", "bitUnplaced");
            _taxonMappings.Add("ISUNVERIFIED", "bitUnverified");
            _taxonMappings.Add("ISAVAILABLENAME", "bitAvailableName");
            _taxonMappings.Add("ISLITERATURENAME", "bitLiteratureName");
            _taxonMappings.Add("DISTRIBUTIONQUALIFICATION", "txtDistQual");
            _taxonMappings.Add("DATECREATED", "dtDateCreated");
            _taxonMappings.Add("WHOCREATED", "vchrWhoCreated");
            // Taxon Literature Available Name (ALN)
            _taxonALNMappings = new FieldToNameMappings();
            _taxonALNMappings.Add("REFID", "intRefID", false);
            _taxonALNMappings.Add("REFPAGE", "vchrRefPage");
            _taxonALNMappings.Add("REFQUAL", "txtRefQual");            
            // Taxon Genus Available Name (GAN)
            _taxonGANMappings = new FieldToNameMappings();
            _taxonGANMappings.Add("REFID", "intRefID", false);
            _taxonGANMappings.Add("REFPAGE", "vchrRefPage");
            _taxonGANMappings.Add("REFQUAL", "txtRefQual");
            _taxonGANMappings.Add("DESIGNATION", "sintDesignation", false);
            _taxonGANMappings.Add("FIXATIONMETHOD", "vchrTSFixationMethod");
            _taxonGANMappings.Add("TYPESPECIES", "vchrTypeSpecies");
            // Taxon Species Available Name (SAN)
            _taxonSANMappings = new FieldToNameMappings();
            _taxonSANMappings.Add("REFID", "intRefID", false);
            _taxonSANMappings.Add("REFPAGE", "vchrRefPage");
            _taxonSANMappings.Add("REFQUAL", "txtRefQual");
            _taxonSANMappings.Add("PRIMARYTYPE", "vchrPrimaryType");
            _taxonSANMappings.Add("PRIMARYTYPEPROBABLE", "bitPrimaryTypeProbable");
            _taxonSANMappings.Add("SECONDARYTYPE", "vchrSecondaryType");
            _taxonSANMappings.Add("SECONDARYTYPEPROBABLE", "bitSecondaryTypeProbable");
            // Taxon Species Available Name Type Data (SAN Type)
            _taxonSANTypeDataMappings = new FieldToNameMappings();
            _taxonSANTypeDataMappings.Add("TYPE", "vchrType");
            _taxonSANTypeDataMappings.Add("INSTITUTION", "vchrMuseum");
            _taxonSANTypeDataMappings.Add("ACCESSIONNUMBER", "vchrAccessionNum");
            _taxonSANTypeDataMappings.Add("MATERIAL", "vchrMaterial");
            _taxonSANTypeDataMappings.Add("LOCALITY", "vchrLocality");
            _taxonSANTypeDataMappings.Add("IDCONFIRMED", "bitIDConfirmed");
            _taxonSANTypeDataMappings.Add("MATERIALID", "intMaterialID", false);
            // Common Names
            _commonNameMappings = new FieldToNameMappings();
            _commonNameMappings.Add("NAME", "vchrCommonName");
            _commonNameMappings.Add("REFID", "intRefID");
            _commonNameMappings.Add("REFPAGE", "vchrRefPage");
            _commonNameMappings.Add("NOTES", "txtNotes");
            _commonNameMappings.Add("REFCODE", "vchrRefCode", false);
            // Reference
            _referenceMappings = new FieldToNameMappings();
            _referenceMappings.Add("REFCODE", "vchrRefCode");
            _referenceMappings.Add("AUTHOR", "vchrAuthor");
            _referenceMappings.Add("TITLE", "vchrTitle");
            _referenceMappings.Add("BOOKTITLE", "vchrBookTitle");
            _referenceMappings.Add("EDITOR", "vchrEditor");
            _referenceMappings.Add("REFTYPE", "vchrRefType");
            _referenceMappings.Add("PUBLICATIONYEAR", "vchrYearOfPub");
            _referenceMappings.Add("ACTUALDATE", "vchrActualDate");
            _referenceMappings.Add("PARTNUMBER", "vchrPartNo");
            _referenceMappings.Add("SERIES", "vchrSeries");
            _referenceMappings.Add("PUBLISHER", "vchrPublisher");
            _referenceMappings.Add("PLACE", "vchrPlace");
            _referenceMappings.Add("VOLUME", "vchrVolume");
            _referenceMappings.Add("PAGES", "vchrPages");
            _referenceMappings.Add("TOTALPAGES", "vchrTotalPages");
            _referenceMappings.Add("POSSESS", "vchrPossess");
            _referenceMappings.Add("SOURCE", "vchrSource");
            _referenceMappings.Add("EDITION", "vchrEdition");
            _referenceMappings.Add("ISBN", "vchrISBN");
            _referenceMappings.Add("ISSN", "vchrISSN");
            _referenceMappings.Add("ABSTRACT", "txtAbstract");
            _referenceMappings.Add("FULLTEXT", "txtFullText");
            _referenceMappings.Add("STARTPAGE", "intStartPage");
            _referenceMappings.Add("ENDPAGE", "intEndPage");
            _referenceMappings.Add("JOURNALNAME", "vchrJournalName", false);
            _referenceMappings.Add("JOURNALID", "intJournalID");
            _referenceMappings.Add("DATECREATED", "dtDateCreated");
            _referenceMappings.Add("WHOCREATED", "vchrWhoCreated");
            // Multimedia Item
            _multimediaMappings = new FieldToNameMappings();
            _multimediaMappings.Add("NAME", "vchrName");
            _multimediaMappings.Add("EXTENSION", "vchrFileExtension");
            _multimediaMappings.Add("NUMBER", "vchrNumber");
            _multimediaMappings.Add("ARTIST", "vchrArtist");
            _multimediaMappings.Add("DATERECORDED", "vchrDateRecorded");
            _multimediaMappings.Add("OWNER", "vchrOwner");
            _multimediaMappings.Add("COPYRIGHT", "txtCopyright");
            _multimediaMappings.Add("SIZEINBYTES", "intSizeInBytes");
            _multimediaMappings.Add("DATECREATED", "dtDateCreated");
            _multimediaMappings.Add("WHOCREATED", "vchrWhoCreated");
            // Journal Items
            _journalMappings = new FieldToNameMappings();
            _journalMappings.Add("ABBREVNAME", "vchrAbbrevName");
            _journalMappings.Add("ALTABBREVNAME", "vchrAbbrevName2");
            _journalMappings.Add("ALIAS", "vchrAlias");
            _journalMappings.Add("FULLNAME", "vchrFullName");
            _journalMappings.Add("JOURNALNOTES", "txtNotes");
            _journalMappings.Add("DATECREATED", "dtDateCreated");
            _journalMappings.Add("WHOCREATED", "vchrWhoCreated");
            // RefLinks
            _refLinkMappings = new FieldToNameMappings();
            _refLinkMappings.Add("REFID", "intRefID");
            _refLinkMappings.Add("REFLINKTYPE", "RefLink", false);
            _refLinkMappings.Add("REFPAGE", "vchrRefPage");
            _refLinkMappings.Add("REFQUAL", "txtRefQual");
            _refLinkMappings.Add("ORDER", "intOrder");
            _refLinkMappings.Add("USEINREPORTS", "bitUseInReport");
            // Taxon distribution
            _distributionMappings = new FieldToNameMappings();
            _distributionMappings.Add("INTRODUCED", "bitIntroduced");
            _distributionMappings.Add("UNCERTAIN", "bitUncertain");
            _distributionMappings.Add("THROUGHOUTREGION", "bitThroughoutRegion");
            _distributionMappings.Add("QUALIFICATION", "txtQual");
            _distributionMappings.Add("FULLPATH", "txtDistRegionFullPath", false);
            // Multimedia Link
            _multimediaLinkMappings = new FieldToNameMappings();
            _multimediaLinkMappings.Add("MULTIMEDIAID", "intMultimediaID", false);
            _multimediaLinkMappings.Add("MULTIMEDIATYPE", "MultimediaType", false);
            _multimediaLinkMappings.Add("CAPTION", "vchrCaption", false);
            _multimediaLinkMappings.Add("USEINREPORTS", "bitUseInReport");
            // Notes Mappings
            _notesMappings = new FieldToNameMappings();
            _notesMappings.Add("NOTETYPE", "NoteType", false);
            _notesMappings.Add("NOTE", "txtNote");
            _notesMappings.Add("AUTHOR", "vchrAuthor");
            _notesMappings.Add("COMMENTS", "txtComments");
            _notesMappings.Add("USEINREPORTS", "bitUseInReports");
            _notesMappings.Add("REFID", "RefID", false);
            _notesMappings.Add("REFPAGES", "vchrRefPages");
            // Keyword Mappings
            _keywordMappings = new FieldToNameMappings();
            _keywordMappings.Add("TYPE", "Keyword", false);
            _keywordMappings.Add("VALUE", "vchrValue");
            _keywordMappings.Add("USEINREPORTS", "bitUseInReport");
            _keywordMappings.Add("QUALIFICATION", "txtValueQual");
            // Storage Location Mappings
            _storageLocationMappings = new FieldToNameMappings();
            _storageLocationMappings.Add("STORAGELOCATION", "StorageLocation", false);
            _storageLocationMappings.Add("FULLPATH", "StoragePath", false);
            _storageLocationMappings.Add("NOTES", "txtNotes");
            // Material Mappings
            _materialMappings = new FieldToNameMappings();
            _materialMappings.Add("NAME", "vchrMaterialName");
            _materialMappings.Add("ACCESIONNUMBER", "vchrAccessionNo");
            _materialMappings.Add("REGISTRATIONNUMBER", "vchrRegNo");
            _materialMappings.Add("COLLECTORNUMBER", "vchrCollectorNo");
            _materialMappings.Add("IDENTIFIEDBY", "vchrIDBy");
            _materialMappings.Add("TAXONID", "intBiotaID");
            _materialMappings.Add("IDENTIFIEDDATE", "dtIDDate");
            _materialMappings.Add("REFID", "intIDRefID");
            _materialMappings.Add("TRAPID", "intTrapID");
            _materialMappings.Add("REFPAGE", "vchrIDRefPage");
            _materialMappings.Add("IDENTIFICATIONMETHOD", "vchrIDMethod");
            _materialMappings.Add("IDENTIFICATIONACCURACY", "vchrIDAccuracy");
            _materialMappings.Add("NAMEQUALIFICATION", "vchrIDNameQual");
            _materialMappings.Add("IDENTIFICATIONNOTES", "vchrIDNotes");
            _materialMappings.Add("INSTITUTION", "vchrInstitution");
            _materialMappings.Add("COLLECTIONMETHOD", "vchrCollectionMethod");
            _materialMappings.Add("ABUNDANCE", "vchrAbundance");
            _materialMappings.Add("MACROHABITAT", "vchrMacroHabitat");
            _materialMappings.Add("MICROHABITAT", "vchrMicroHabitat");
            _materialMappings.Add("SOURCE", "vchrSource");
            _materialMappings.Add("SPECIALLABEL", "vchrSpecialLabel");
            _materialMappings.Add("ORIGINALLABEL", "vchrOriginalLabel");
            _materialMappings.Add("DATECREATED", "dtDateCreated");
            _materialMappings.Add("WHOCREATED", "vchrWhoCreated");
            // IDHistory Mappings
            _IDHistoryMappings = new FieldToNameMappings();
            _IDHistoryMappings.Add("TAXON", "vchrTaxa");
            _IDHistoryMappings.Add("IDENTIFIEDBY", "vchr);IDBy");
            _IDHistoryMappings.Add("DATEIDENTIFIED", "dtIDDate");
            _IDHistoryMappings.Add("REFID", "intIDRefID");
            _IDHistoryMappings.Add("REFPAGE", "vchrIDRefPage");
            _IDHistoryMappings.Add("METHOD", "vchrIDMethod");
            _IDHistoryMappings.Add("ACCURACY", "vchrIDAccuracy");
            _IDHistoryMappings.Add("NAMEQUALIFICATION", "vchrNameQual");
            _IDHistoryMappings.Add("NOTES", "txtIDNotes");
            // Subpart Mappings
            _subPartMappings = new FieldToNameMappings();
            _subPartMappings.Add("NAME", "vchrPartName");
            _subPartMappings.Add("SAMPLETYPE", "vchrSampleType");
            _subPartMappings.Add("NUMBEROFSPECIMENS", "intNoSpecimens");
            _subPartMappings.Add("NUMBEROFSPECIMENSQUALIFCATION", "vchrNoSpecimensQual");
            _subPartMappings.Add("LIFESTAGE", "vchrLifeStage");
            _subPartMappings.Add("GENDER", "vchrGender");
            _subPartMappings.Add("REGISTRATIONNUMBER", "vchrRegNo");
            _subPartMappings.Add("CONDITION", "vchrCondition");
            _subPartMappings.Add("STORAGESITE", "vchrStorageSite");
            _subPartMappings.Add("STORAGEMETHOD", "vchrStorageMethod");
            _subPartMappings.Add("CURATIONSTATUS", "vchrCurationStatus");
            _subPartMappings.Add("NUMBEROFUNITS", "vchrNoOfUnits");
            _subPartMappings.Add("NOTES", "txtNotes");
            // Associate Mappings
            _associateMappings = new FieldToNameMappings();
            _associateMappings.Add("FROMCATEGORYID", "intFromCatID");
            _associateMappings.Add("FROMINTRACATID", "intFromIntraCatID");
            _associateMappings.Add("TOCATEGORYID", "intToCatID");
            _associateMappings.Add("TOINTRACATID", "intToIntraCatID");
            _associateMappings.Add("ASSOCDESCRIPTION", "txtAssocDescription");
            _associateMappings.Add("RELATIONFROMTO", "vchrRelationFromTo");
            _associateMappings.Add("RELATIONTOFROM", "vchrRelationToFrom");
            _associateMappings.Add("REGIONID", "intPoliticalRegionID");
            _associateMappings.Add("SOURCE", "vchrSource");
            _associateMappings.Add("REFID", "intRefID");
            _associateMappings.Add("REFPAGE", "vchrRefPage");
            _associateMappings.Add("UNCERTAIN", "bitUncertain");
            _associateMappings.Add("NOTES", "txtNotes");
            // Curation Event mappings
            _curationEventMappings = new FieldToNameMappings();
            _curationEventMappings.Add("SUBPARTNAME", "vchrSubpartName");
            _curationEventMappings.Add("WHO", "vchrWho");
            _curationEventMappings.Add("DATE", "dtWhen");
            _curationEventMappings.Add("EVENTTYPE", "vchrEventType");
            _curationEventMappings.Add("DESCRIPTION", "txtEventDesc");
            // SiteVisit Mappings
            _siteVisitMappings = new FieldToNameMappings();
            _siteVisitMappings.Add("NAME", "vchrSiteVisitName");
            _siteVisitMappings.Add("FIELDNUMBER", "vchrFieldNumber");
            _siteVisitMappings.Add("COLLECTOR", "vchrCollector");
            _siteVisitMappings.Add("DATESTART", "intDateStart", false);
            _siteVisitMappings.Add("DATEEND", "intDateEnd", false);
            _siteVisitMappings.Add("TIMESTART", "intTimeStart", false);
            _siteVisitMappings.Add("TIMEEND", "intTimeEnd", false);
            _siteVisitMappings.Add("CASUALTIME", "vchrCasualTime");
            _siteVisitMappings.Add("DATECREATED", "dtDateCreated");
            _siteVisitMappings.Add("WHOCREATED", "vchrWhoCreated");
            // Site Mappings
            _siteMappings = new FieldToNameMappings();
            _siteMappings.Add("NAME", "vchrSiteName");
            _siteMappings.Add("LOCALITYTYPE", "tintLocalType", false);
            _siteMappings.Add("LOCALITY", "vchrLocal");
            _siteMappings.Add("DISTANCEFROMPLACE", "vchrDistanceFromPlace");
            _siteMappings.Add("DIRECTIONFROMPLACE", "vchrDirFromPlace");
            _siteMappings.Add("INFORMALLOCALITY", "vchrInformalLocal");
            _siteMappings.Add("COORDINATETYPE", "tintPosCoordinates", false);
            _siteMappings.Add("POSITIONGEOMETRY", "tintPosAreaType", false);
            _siteMappings.Add("X1", "fltPosX1");
            _siteMappings.Add("Y1", "fltPosY1");
            _siteMappings.Add("X2", "fltPosX2");
            _siteMappings.Add("Y2", "fltPosY2");
            _siteMappings.Add("POSITIONSOURCE", "vchrPosSource");
            _siteMappings.Add("POSITIONERROR", "vchrPosError");
            _siteMappings.Add("POSITIONWHO", "vchrPosWho");
            _siteMappings.Add("POSITIONDATE", "vchrPosDate");
            _siteMappings.Add("POSITIONORIGINAL", "vchrPosOriginal");
            _siteMappings.Add("POSITIONUTMSOURCE", "vchrPosUTMSource");
            _siteMappings.Add("POSITIONUTMPROJECTION", "vchrPosUTMMapProj");
            _siteMappings.Add("POSITIONUTMMAPNAME", "vchrPosUTMMapName");
            _siteMappings.Add("POSITIONUTMMAPVERSION", "vchrPosUTMMapVer");
            _siteMappings.Add("ELEVATIONTYPE", "tintElevType", false);
            _siteMappings.Add("ELEVATIONUPPER", "fltElevUpper");
            _siteMappings.Add("ELEVATIONLOWER", "fltElevLower");
            _siteMappings.Add("ELEVATIONDEPTH", "fltElevDepth");
            _siteMappings.Add("ELEVATIONUNITS", "vchrElevUnits");
            _siteMappings.Add("ELEVATIONSOURCE", "vchrElevSource");
            _siteMappings.Add("ELEVATIONERROR", "vchrElevError");
            _siteMappings.Add("GEOLOGYERA", "vchrGeoEra");
            _siteMappings.Add("GEOLOGYSTATE", "vchrGeoState");
            _siteMappings.Add("GEOLOGYPLATE", "vchrGeoPlate");
            _siteMappings.Add("GEOLOGYFORMATION", "vchrGeoFormation");
            _siteMappings.Add("GEOLOGYMEMBER", "vchrGeoMember");
            _siteMappings.Add("GEOLOGYBED", "vchrGeoBed");
            _siteMappings.Add("GEOLOGYNAME", "vchrGeoName");
            _siteMappings.Add("GEOLOGYAGEBOTTOM", "vchrGeoAgeBottom");
            _siteMappings.Add("GEOLOGYAGETOP", "vchrGeoAgeTop");
            _siteMappings.Add("GEOLOGYNOTES", "vchrGeoNotes");
            _siteMappings.Add("DATECREATED", "dtDateCreated");
            _siteMappings.Add("WHOCREATED", "vchrWhoCreated");
            // Region Mappings
            _regionMappings = new FieldToNameMappings();
            _regionMappings.Add("NAME", "vchrName");
            _regionMappings.Add("RANK", "vchrRank");
            _regionMappings.Add("DATECREATED", "dtDateCreated");
            _regionMappings.Add("WHOCREATED", "vchrWhoCreated");
            // Trap Mappings
            _trapMappings = new FieldToNameMappings();
            _trapMappings.Add("NAME", "vchrTrapName");
            _trapMappings.Add("TYPE", "vchrTrapType");
            _trapMappings.Add("DESCRIPTION", "vchrDescription");
            _trapMappings.Add("DATECREATED", "dtDateCreated");
            _trapMappings.Add("WHOCREATED", "vchrWhoCreated");

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
            _logWriter.Flush();
        }

        protected FieldToNameMappings GetCollectionForCategory(string category) {
            switch (category.ToLower()) {
                case "taxon":
                    return _taxonMappings;
                case "aln":
                    return _taxonALNMappings;
                case "gan":
                    return _taxonGANMappings;
                case "san":
                    return _taxonSANMappings;
                case "santype":
                    return _taxonSANTypeDataMappings;
                case "commonname":
                    return _commonNameMappings;
                case "reference":
                    return _referenceMappings;
                case "multimediaitem":
                    return _multimediaMappings;
                case "journal":
                    return _journalMappings;
                case "multimedialink":
                    return _multimediaLinkMappings;
                case "referencelink":
                    return _refLinkMappings;
                case "distributionitem":
                    return _distributionMappings;
                case "noteitem":
                    return _notesMappings;
                case "keyworditem":
                    return _keywordMappings;
                case "storagelocation":
                    return _storageLocationMappings;
                case "material":
                    return _materialMappings;
                case "identification":
                    return _IDHistoryMappings;
                case "subpart":
                    return _subPartMappings;
                case "associate":
                    return _associateMappings;
                case "sitevisit":
                    return _siteVisitMappings;
                case "site":
                    return _siteMappings;
                case "region":
                    return _regionMappings;
                case "trap":
                    return _trapMappings;
                case "curationevent":
                    return _curationEventMappings;
                default:
                    return null;
            }

        }

        public bool LookupFieldName(string ObjectType, string XMLName, out string FieldName) {    
            var pCollection = GetCollectionForCategory(ObjectType);    
            if (pCollection == null) {
                FieldName = "";
                return false;
            }

            return pCollection.XMLNameToFieldName(XMLName, out FieldName, true);
        }

        protected User User { get; set; }
        protected TaxaService TaxaService { get; private set; }
        protected SupportService SupportService { get; private set; }
        protected MaterialService MaterialService { get; private set; }
        protected XMLIOService XMLIOService { get; private set; }

    }

    public class XMLIOExporter : XMLIOBase {

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

        private XMLExportObject _xmlDoc;
        private int _itemTotal;
        private int _itemCount;

        public XMLIOExporter(User user, List<int> taxonIds, XMLIOExportOptions options, IProgressObserver progress, Func<bool> isCancelledCallback) : base(user) {
            this.TaxonIDs = taxonIds;
            this.Options = options;
            this.ProgressObserver = progress;
            _isCancelled = isCancelledCallback;

            if (options.KeepLogFile) {
                string logfile = SystemUtils.ChangeExtension(options.Filename, "log");
                InitLog(logfile);
            }

        }

        public void Export() {
            try {

                Init();

                ExportBiota();

                if (ProgressObserver != null) {
                    ProgressObserver.ProgressMessage("Saving XML...");
                }
                _xmlDoc.Save(Options.Filename);

            } finally {
                FinalizeLog();
            }
        }

        private void ExportBiota() {
            Log("Counting total taxa to export");

            var taxonMap = new Dictionary<int, Taxon>();
            foreach (int taxonId in TaxonIDs) {
                var taxon = TaxaService.GetTaxon(taxonId);
                taxonMap[taxonId] = taxon;
                Log("Counting children of Taxon '{0}' ({1})", taxon.TaxaFullName, taxonId);
                var itemCount = GetItemCount(taxonId);
                _itemTotal += itemCount;

                if (IsCancelled) {
                    return;
                }
            }

            var logMsg = string.Format("{0} items to export", _itemTotal);
            Log(logMsg);
            if (ProgressObserver != null) {
                ProgressObserver.ProgressMessage(logMsg);
            }
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
                    AddTaxonElement(XMLTaxaNode, taxon, Options.ExportChildTaxa, Options.ExportMaterial);
                }
            }

            if (!IsCancelled) {
                ExportUnplaced();
            }

            FinishExport();
        }

        private void FinishExport() {

            WriteMetaData();

            if (Options.KeepLogFile) {
                if (IsCancelled) {
                    Log("Export Cancelled !");
                } else {
                    Log("Export Finished.");
                }

                Log(_taxonList.Count + " Taxon items added");
                Log(_regionList.Count + " Regions added");
                Log(_siteList.Count + " Sites added");
                Log(_siteVisitList.Count + " SiteVisits added");
                Log(_materialList.Count + " Material items added");
                Log(_referenceList.Count + " Reference items added");
                Log(_journalList.Count + " Journals added");
                Log(_multimediaList.Count + " Multimedia items added");
                Log(_associateList.Count + " Associate items added");

            }

        }

        private void WriteMetaData() {
            var XMLMeta = _xmlDoc.MetaRoot;

            var now = DateTime.Now;
    
            _xmlDoc.CreateNode(XMLMeta, "DATECREATED").InnerText = string.Format("{0:dd MM yyyy}", now);
            _xmlDoc.CreateNode(XMLMeta, "TIMECREATED").InnerText = string.Format("{0:hh:mm}", now);
            _xmlDoc.CreateNode(XMLMeta, "WHOCREATED").InnerText = User.Username;
            _xmlDoc.CreateNode(XMLMeta, "DATABASE").InnerText = User.ConnectionProfile.Database;
            _xmlDoc.CreateNode(XMLMeta, "DATABASESERVER").InnerText = User.ConnectionProfile.Server;    
            _xmlDoc.CreateNode(XMLMeta, "TAXONCOUNT").InnerText = CStr(_taxonList.Count);
            _xmlDoc.CreateNode(XMLMeta, "MATERIALCOUNT").InnerText = CStr(_materialList.Count);
            _xmlDoc.CreateNode(XMLMeta, "SITEVISITCOUNT").InnerText = CStr(_siteVisitList.Count);
            _xmlDoc.CreateNode(XMLMeta, "SITECOUNT").InnerText = CStr(_siteList.Count);
            _xmlDoc.CreateNode(XMLMeta, "REFERENCECOUNT").InnerText = CStr(_referenceList.Count);
            _xmlDoc.CreateNode(XMLMeta, "REGIONCOUNT").InnerText = CStr(_regionList.Count);
            _xmlDoc.CreateNode(XMLMeta, "JOURNALCOUNT").InnerText = CStr(_journalList.Count);
            _xmlDoc.CreateNode(XMLMeta, "MULTIMEDIACOUNT").InnerText = CStr(_multimediaList.Count);
            _xmlDoc.CreateNode(XMLMeta, "ASSOCIATECOUNT").InnerText = CStr(_associateList.Count);
        }

        private string CStr(int i) {
            return i.ToString();
        }

        private void ExportUnplaced() {
            if (_unplacedTaxon.Count > 0) {
                Log("Exporting 'Unplaced Taxon' (Associates)...");

                if (ProgressObserver != null) {
                    ProgressObserver.ProgressMessage("Exporting Unplaced Taxon...");
                }

                var XMLUnplaced = _xmlDoc.UnplacedTaxaRoot;
                foreach (KeyValuePair<string, int> kvp in _unplacedTaxon) {
                    var lngTaxonID = kvp.Value;
                    var strGUID = kvp.Key;
                    // Do not add material for unplaced items...
                    var taxon = TaxaService.GetTaxon(lngTaxonID);
                    AddTaxonElement(XMLUnplaced, taxon, false, false);
                }
            }
        }

        private XmlElement ImportParents(XmlElement parent, Taxon taxon) {

            var newParent = parent;

            var strParentIds = taxon.Parentage.Split('\\');
            var parentIds = new List<int>();
            foreach (string s in strParentIds) {
                if (!string.IsNullOrEmpty(s)) {
                    parentIds.Add(Int32.Parse(s));
                }
            }

            foreach (int parentId in parentIds) {
                if (parentId == taxon.TaxaID.Value) {
                    break;
                } else {
                    var parentTaxon = TaxaService.GetTaxon(parentId);
                    newParent = AddTaxonElement(newParent, parentTaxon, false, false);
                }
            }

            return newParent;
        }

        private void ProgressTick() {
            _itemCount++;
            var percent = ((double)_itemCount / (double)_itemTotal) * 100.0;
            if (ProgressObserver != null) {
                ProgressObserver.ProgressMessage("Exporting data...", percent);
            }
        }

        private XmlElement AddTaxonElement(XmlElement parent, Taxon taxon, bool recurseChildren, bool addMaterial) {

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

                ProgressTick();

                guid = taxon.GUID.Value.ToString();
                taxonNode = _xmlDoc.CreateNode(parent, "TAXON");
                taxonNode.AddAttribute("ID", guid);
                _taxonList[guid] = taxon.TaxaID.Value;

                CreateNamedNode(taxonNode, "vchrFullName", taxon.TaxaFullName);
                CreateNamedNode(taxonNode, "vchrEpithet", taxon.Epithet);
                CreateNamedNode(taxonNode, "vchrYearOfPub", taxon.YearOfPub);
                CreateNamedNode(taxonNode, "vchrAuthor", taxon.Author);
                CreateNamedNode(taxonNode, "vchrAvailableNameStatus", taxon.NameStatus);

                // Rank and Kingom are special!
                var taxaService = new TaxaService(User);
                var rank = taxaService.GetTaxonRank(taxon.ElemType);
                var kingdomLong = string.IsNullOrWhiteSpace(taxon.KingdomCode) ? "" : taxaService.GetKingdomName(taxon.KingdomCode);

                CreateNamedNode(taxonNode, "RankLong", rank == null ? "" : rank.LongName);
                CreateNamedNode(taxonNode, "KingdomLong", kingdomLong);
                CreateNamedNode(taxonNode, "intOrder", taxon.Order);
                CreateNamedNode(taxonNode, "bitChangedComb", taxon.ChgComb);
                CreateNamedNode(taxonNode, "bitUnplaced", taxon.Unplaced);
                CreateNamedNode(taxonNode, "bitUnverified", taxon.Unverified);
                CreateNamedNode(taxonNode, "bitAvailableName", taxon.AvailableName);
                CreateNamedNode(taxonNode, "bitLiteratureName", taxon.LiteratureName);
                if (taxon.AvailableName.ValueOrFalse() || taxon.LiteratureName.ValueOrFalse()) {
                    AddAvailableNameData(taxonNode, taxon, rank);
                }

                CreateNamedNode(taxonNode, "txtDistQual", taxon.DistQual);
                CreateNamedNode(taxonNode, "dtDateCreated", FormatDate(taxon.DateCreated));
                CreateNamedNode(taxonNode, "vchrWhoCreated", taxon.WhoCreated);

                // CommonNames
                AddCommonNames(taxonNode, taxon);
                // References
                AddReferences(taxonNode, taxon.TaxaID.Value, "Taxon");
                // Distribution
                AddTaxonDistribution(taxonNode, taxon);
                // Multimedia
                AddMultimedia(taxonNode, taxon.TaxaID.Value, "Taxon");
                // Morphology
                //AddMorphology TaxonNode, TaxonID, "Taxon"
                // Associates
                AddTaxonAssociates(taxonNode, taxon);
                // Traits
                AddTraits(taxonNode, taxon.TaxaID.Value, "Taxon");
                // Keywords
                //AddKeywords TaxonNode, TaxonID, "Taxon"
                // Notes
                AddNotes(taxonNode, taxon.TaxaID.Value, "Taxon");
                // Storage Locations
                AddStorageLocations(taxonNode, taxon);

                // Material
                if (addMaterial) {
                    AddMaterialForTaxon(taxonNode, taxon);
                }

                // If an associate has placed this taxon in the UnplacedList, remove it (it is //placed// now!)
                if (_unplacedTaxon.ContainsValue(taxon.TaxaID.Value)) {
                    // RemoveFromUnplacedList TaxonID
                }
            }

            if (recurseChildren && !IsCancelled) {

                var childIds = XMLIOService.GetTaxaIdsForParent(taxon.TaxaID.Value);

                foreach (int childId in childIds) {
                    if (IsCancelled) {
                        break;
                    }

                    var child = TaxaService.GetTaxon(childId);
                    AddTaxonElement(taxonNode, child, true, addMaterial);
                }
            }

            return taxonNode;
        }

        private void AddMaterialForTaxon(XmlElement ParentNode, Taxon taxon) {
            Log("Adding Material for Taxon {0}", taxon.TaxaID.Value);

            var materialIds = XMLIOService.GetMaterialForTaxon(taxon.TaxaID.Value);
            var XMLMaterialRoot = _xmlDoc.CreateNode(ParentNode, "MATERIAL");
            foreach (XMLIOMaterialID materialId in materialIds) {
                if (IsCancelled) {
                    break;
                }

                var XMLMaterial = _xmlDoc.CreateNode(XMLMaterialRoot, "MATERIALITEM");
                var strMaterialGUID = materialId.MaterialGUID;
                var guid = AddMaterialItem(materialId.MaterialID);
                if (guid != null) {
                    XMLMaterial.AddAttribute("ID", guid.ToString());
                }
            }
        }

        private void AddStorageLocations(XmlElement ParentNode, Taxon taxon) {
            Log("Adding Storage Locations (TaxonID={0})", taxon.TaxaID.Value);
            var locations = XMLIOService.GetStorageLocations(taxon.TaxaID.Value);
            var XMLLocationParent = _xmlDoc.CreateNode(ParentNode, "STORAGELOCATIONS");
            foreach (StorageLocation location in locations) {
                var XMLLocation = _xmlDoc.CreateNode(XMLLocationParent, "STORAGELOCATION");
                XMLLocation.AddAttribute("ID", location.GUID.ToString());
                XMLLocation.AddAttribute("PATHSEPARATOR", "\\");
                CreateNamedNode(XMLLocation, "StorageLocation", location.Location);
                CreateNamedNode(XMLLocation, "StoragePath", location.StoragePath);
                CreateNamedNode(XMLLocation, "txtNotes", ExpandNotes(location.Notes));
            }
        }

        private void AddTaxonAssociates(XmlElement ParentNode, Taxon taxon) {
            var associates = SupportService.GetAssociates("Taxon", taxon.TaxaID.Value);
            Log("Adding Taxon Associates (TaxonID={0})", taxon.TaxaID.Value);
            var XMLAssoc = _xmlDoc.CreateNode(ParentNode, "ASSOCIATES");
            AddAssociates(XMLAssoc, associates);
        }

        private void AddAssociates(XmlElement XMLAssoc, List<Associate> associates) {
            foreach (Associate assoc in associates) {
                if (IsCancelled) {
                    break;
                }
                var XMLAssocItem = _xmlDoc.CreateNode(XMLAssoc, "ASSOCIATEITEM");
                var assocGuid = assoc.AssocGUID.ToString();
                XMLAssocItem.AddAttribute("ID", assocGuid);

                // also add to the general associates area if not already there
                var lngAssociateRecID = assoc.AssociateID;
                var guid = _associateList.GUIDForID(assoc.AssociateID);
                if (string.IsNullOrWhiteSpace(guid)) {
                    Log("Adding Associate (AssociateID={0})", assoc.AssociateID);
                    var XMLAssocParent = _xmlDoc.AssociateRoot;
                    var XMLAssocNode = _xmlDoc.CreateNode(XMLAssocParent, "ASSOCIATE");
                    XMLAssocNode.AddAttribute("ID", assoc.AssocGUID.ToString());

                    _associateList.Add(assocGuid, lngAssociateRecID);

                    int lngFromCatID, lngToCatID, lngFromID, lngToID;
                    string strFromCategory;
                    string strToCategory;

                    if (assoc.Direction.Equals("FromTo", StringComparison.CurrentCultureIgnoreCase)) {
                        lngFromCatID = assoc.FromCatID;
                        lngToCatID = assoc.ToCatID.GetValueOrDefault(-1);
                        lngFromID = assoc.FromIntraCatID;
                        lngToID = assoc.ToIntraCatID.GetValueOrDefault(-1);
                        strFromCategory = assoc.FromCategory;
                        strToCategory = assoc.ToCategory;
                    } else {
                        lngFromCatID = assoc.ToCatID.GetValueOrDefault(-1);
                        lngToCatID = assoc.FromCatID;
                        lngFromID = assoc.ToIntraCatID.GetValueOrDefault(-1);   // GetRowProperty(vAssoc, i, "intToIntraCatID", -1)
                        lngToID = assoc.FromIntraCatID; // GetRowProperty(vAssoc, i, "intFromIntraCatID", -1)
                        strFromCategory = assoc.ToCategory; // GetRowProperty(vAssoc, i, "ToCategory", "")
                        strToCategory = assoc.FromCategory; //GetRowProperty(vAssoc, i, "FromCategory", "")
                    }
                    CreateNamedNode(XMLAssocNode, "FROMCATEGORYID", lngFromCatID);

                    switch (strFromCategory) {
                        case "Taxon":
                            CreateNamedNode(XMLAssocNode, "FROMINTRACATID", AddToUnplacedList(lngFromID));
                            break;
                        case "Material":
                            if (AddMaterialItem(lngFromID) != null) {
                                CreateNamedNode(XMLAssocNode, "FROMINTRACATID", assocGuid.ToString());
                            }
                            break;
                        default:
                            CreateNamedNode(XMLAssocNode, "ASSOCDESCRIPTION", assoc.AssocDescription);
                            break;
                    }

                    CreateNamedNode(XMLAssocNode, "TOCATEGORYID", lngToCatID);

                    switch (strToCategory) {
                        case "Taxon":
                            CreateNamedNode(XMLAssocNode, "TOINTRACATID", AddToUnplacedList(lngToID));
                            break;
                        case "Material":
                            if (AddMaterialItem(lngToID) != null) {
                                CreateNamedNode(XMLAssocNode, "TOINTRACATID", assocGuid.ToString());
                            }
                            break;
                        default:
                            CreateNamedNode(XMLAssocNode, "ASSOCDESCRIPTION", assoc.AssocDescription);
                            break;
                    }

                    CreateNamedNode(XMLAssocNode, "RELATIONFROMTO", assoc.RelationFromTo);
                    CreateNamedNode(XMLAssocNode, "RELATIONTOFROM", assoc.RelationToFrom);
                    var lngRegionID = assoc.PoliticalRegionID;
                    var strRegionGUID = assoc.RegionGUID.ToString();
                    if (lngRegionID > 0) {
                        AddRegionItem(lngRegionID, strRegionGUID);
                    }

                    CreateNamedNode(XMLAssocNode, "REGIONID", strRegionGUID);
                    CreateNamedNode(XMLAssocNode, "SOURCE", assoc.Source);
                    CreateNamedNode(XMLAssocNode, "REFID", AddReferenceItem(assoc.RefID));
                    CreateNamedNode(XMLAssocNode, "REFPAGE", assoc.RefPage);
                    CreateNamedNode(XMLAssocNode, "UNCERTAIN", assoc.Uncertain);
                    CreateNamedNode(XMLAssocNode, "NOTES", assoc.Notes);
                }
            }
        }

        private XmlElement AddRegionItem(int RegionID, string strRegionGUID) {
            var RegionGUID = _regionList.GUIDForID(RegionID);
            if (RegionGUID != null) {
                var XMLRegionNode = _xmlDoc.GetElementByGUID(RegionGUID, "REGION");
                if (XMLRegionNode != null) {
                    return XMLRegionNode;
                }
            }

            Log("Adding Region (RegionID={0})", RegionID);

            var region = XMLIOService.GetRegion(RegionID);
            if (region != null) {

                var lngParentID = region.ParentID;

                var strParentGUID = region.ParentGUID;

                XmlElement XMLRegionParent = null;
                if (lngParentID > 0) {
                    XMLRegionParent = AddRegionItem(lngParentID, strParentGUID.ToString());
                } else {
                    XMLRegionParent = _xmlDoc.MaterialRoot;
                }

                var XMLRegionNode = _xmlDoc.CreateNode(XMLRegionParent, "REGION");
                RegionGUID = region.GUID.ToString();
                XMLRegionNode.AddAttribute("ID", RegionGUID);

                CreateNamedNode(XMLRegionNode, "vchrName", region.Name);
                CreateNamedNode(XMLRegionNode, "vchrRank", region.Rank);
                CreateNamedNode(XMLRegionNode, "dtDateCreated", FormatDate(region.DateCreated));
                CreateNamedNode(XMLRegionNode, "vchrWhoCreated", region.WhoCreated);

                AddTraits(XMLRegionNode, RegionID, "Region");

                _regionList.Add(RegionGUID, RegionID);

                return XMLRegionNode;
            }
            return null;
        }

        private XmlElement AddMaterialItem(int MaterialID) {

            if (MaterialID <= 0) {
                return null;
            }

            // Search to see if this material is already in the Document...
            var guid = _materialList.GUIDForID(MaterialID);
            if (!string.IsNullOrWhiteSpace(guid)) {
                var existing = _xmlDoc.GetElementByGUID(guid, "MATERIAL");
                if (existing != null) {
                    return existing;
                }
            }

            Log("Adding Material (MaterialID={0})", MaterialID);
            var material = XMLIOService.GetMaterial(MaterialID);

            if (material == null) {
                Log("Failed to retrieve material (ID={0})", MaterialID);
                return null;
            }

            ProgressTick();

            var XMLMaterialRoot = AddSiteVisitItem(material.SiteVisitID, material.SiteVisitGUID.ToString());

            XmlElement XMLMaterial = _xmlDoc.CreateNode(XMLMaterialRoot, "MATERIAL");
            var MaterialGUID = material.GUID.ToString();
            XMLMaterial.AddAttribute("ID", MaterialGUID.ToString());


            CreateNamedNode(XMLMaterial, "vchrMaterialName", material.MaterialName);
            CreateNamedNode(XMLMaterial, "vchrAccessionNo", material.AccessionNumber);
            CreateNamedNode(XMLMaterial, "vchrRegNo", material.RegistrationNumber);
            CreateNamedNode(XMLMaterial, "vchrCollectorNo", material.CollectorNumber);
            CreateNamedNode(XMLMaterial, "vchrIDBy", material.IdentifiedBy);
            CreateNamedNode(XMLMaterial, "intBiotaID", AddToUnplacedList(material.BiotaID));
            CreateNamedNode(XMLMaterial, "dtIDDate", FormatDate(material.IdentificationDate));
            CreateNamedNode(XMLMaterial, "intIDRefID", AddReferenceItem(material.IdentificationReferenceID));
            CreateNamedNode(XMLMaterial, "vchrIDRefPage", material.IdentificationRefPage);
            CreateNamedNode(XMLMaterial, "vchrIDMethod", material.IdentificationMethod);
            CreateNamedNode(XMLMaterial, "vchrIDAccuracy", material.IdentificationAccuracy);
            CreateNamedNode(XMLMaterial, "vchrIDNameQual", material.IdentificationNameQualification);
            CreateNamedNode(XMLMaterial, "vchrIDNotes", ExpandNotes(material.IdentificationNotes));
            CreateNamedNode(XMLMaterial, "vchrInstitution", material.Institution);
            CreateNamedNode(XMLMaterial, "vchrCollectionMethod", material.CollectionMethod);
            CreateNamedNode(XMLMaterial, "vchrAbundance", material.Abundance);
            CreateNamedNode(XMLMaterial, "vchrMacroHabitat", material.MacroHabitat);
            CreateNamedNode(XMLMaterial, "vchrMicroHabitat", material.MicroHabitat);
            CreateNamedNode(XMLMaterial, "vchrSource", material.Source);
            CreateNamedNode(XMLMaterial, "vchrSpecialLabel", material.SpecialLabel);
            CreateNamedNode(XMLMaterial, "vchrOriginalLabel", material.OriginalLabel);

            _materialList.Add(MaterialGUID, MaterialID);

            var lngTrapID = material.TrapID;
            var strTrapGUID = "";
            if (lngTrapID > 0) {
                strTrapGUID = material.TrapGUID.ToString();
                if (AddTrapItem(lngTrapID, material.SiteGUID.ToString(), strTrapGUID) == null) {
                    Log("Failed to add trap (TrapID={0})", lngTrapID);
                    strTrapGUID = "";
                }
            }


            CreateNamedNode(XMLMaterial, "intTrapID", strTrapGUID);
            _xmlDoc.CreateNode(XMLMaterial, "DATECREATED").InnerText = FormatDate(material.DateCreated);
            _xmlDoc.CreateNode(XMLMaterial, "WHOCREATED").InnerText = material.WhoCreated;

            // ID History
            var idhistory = MaterialService.GetMaterialIdentification(MaterialID);
            var XMLNodeParent = _xmlDoc.CreateNode(XMLMaterial, "IDENTIFICATIONHISTORY");
            foreach (MaterialIdentification id in idhistory) {
                var XMLNode = _xmlDoc.CreateNode(XMLNodeParent, "IDENTIFICATION");
                XMLNode.AddAttribute("ID", id.GUID.ToString());
                CreateNamedNode(XMLNode, "vchrTaxa", id.Taxa);
                CreateNamedNode(XMLNode, "vchrIDBy", id.IDBy);
                CreateNamedNode(XMLNode, "dtIDDate", FormatDate(id.IDDate));
                CreateNamedNode(XMLNode, "intIDRefID", AddReferenceItem(id.IDRefID));
                CreateNamedNode(XMLNode, "vchrIDRefPage", id.IDRefPage);
                CreateNamedNode(XMLNode, "vchrIDMethod", id.IDMethod);
                CreateNamedNode(XMLNode, "vchrIDAccuracy", id.IDMethod);
                CreateNamedNode(XMLNode, "vchrNameQual", id.NameQual);
                CreateNamedNode(XMLNode, "txtIDNotes", ExpandNotes(id.IDNotes));
            }

            // Subparts
            var subparts = MaterialService.GetMaterialParts(MaterialID);
            XMLNodeParent = _xmlDoc.CreateNode(XMLMaterial, "SUBPARTS");
            foreach (MaterialPart part in subparts) {
                var XMLNode = _xmlDoc.CreateNode(XMLNodeParent, "SUBPART");
                XMLNode.AddAttribute("ID", part.GUID.ToString());
                CreateNamedNode(XMLNode, "vchrPartName", part.PartName);
                CreateNamedNode(XMLNode, "vchrSampleType", part.SampleType);
                CreateNamedNode(XMLNode, "intNoSpecimens", part.NoSpecimens);
                CreateNamedNode(XMLNode, "vchrNoSpecimensQual", part.NoSpecimensQual);
                CreateNamedNode(XMLNode, "vchrLifeStage", part.Lifestage);
                CreateNamedNode(XMLNode, "vchrGender", part.Gender);
                CreateNamedNode(XMLNode, "vchrRegNo", part.RegNo);
                CreateNamedNode(XMLNode, "vchrCondition", part.Condition);
                CreateNamedNode(XMLNode, "vchrStorageSite", part.StorageSite);
                CreateNamedNode(XMLNode, "vchrStorageMethod", part.StorageMethod);
                CreateNamedNode(XMLNode, "vchrCurationStatus", part.CurationStatus);
                CreateNamedNode(XMLNode, "vchrNoOfUnits", part.NoOfUnits);
                CreateNamedNode(XMLNode, "txtNotes", ExpandNotes(part.Notes));
            }

            // Associates
            var associates = SupportService.GetAssociates("Material", MaterialID);
            XMLNodeParent = _xmlDoc.CreateNode(XMLMaterial, "ASSOCIATES");
            AddAssociates(XMLNodeParent, associates);

            // Curation Events
            var events = MaterialService.GetCurationEvents(MaterialID);
            XMLNodeParent = _xmlDoc.CreateNode(XMLMaterial, "CURATIONEVENTS");
            foreach (CurationEvent e in events) {
                var XMLNode = _xmlDoc.CreateNode(XMLNodeParent, "CURATIONEVENT");
                XMLNode.AddAttribute("ID", e.GUID.ToString());
                CreateNamedNode(XMLNode, "vchrSubpartName", e.SubpartName);
                CreateNamedNode(XMLNode, "vchrWho", e.Who);
                CreateNamedNode(XMLNode, "dtWhen", FormatDate(e.When));
                CreateNamedNode(XMLNode, "vchrEventType", e.EventType);
                CreateNamedNode(XMLNode, "txtEventDesc", e.EventDesc);
            }

            AddTraits(XMLMaterial, MaterialID, "Material");
            AddNotes(XMLMaterial, MaterialID, "Material");
            AddMultimedia(XMLMaterial, MaterialID, "Material");

            return XMLMaterial;
        }

        private XmlElement AddTrapItem(int TrapID, string SiteGUID, string TrapGUID) {
            if (!string.IsNullOrWhiteSpace(TrapGUID)) {
                var existing = _xmlDoc.GetElementByGUID(TrapGUID, "TRAP");
                if (existing != null) {
                    return existing;
                }
            }

            Log("Adding Trap (TrapID={0})", TrapID);

            var trap = MaterialService.GetTrap(TrapID);

            if (trap == null) {
                Log("Failed to get trap details (TrapID={0})", TrapID);
                return null;
            }


            var XMLParent = _xmlDoc.GetElementByGUID(SiteGUID, "SITE");
            if (XMLParent == null) {
                Log("Failed to locate Site (SiteGUID {0}). AddTrap (TrapID={1}) failed!", SiteGUID, TrapID);
                return null;
            }

            var strTrapGUID = trap.GUID.ToString();
            if (!string.IsNullOrWhiteSpace(TrapGUID)) {
                if (!TrapGUID.Equals(strTrapGUID)) {
                    Log("GUID's are different for trap ! - Material Record thinks trap id is {0}, Trap's GUID is {1}! - AddTrap failed!", TrapGUID, trap.GUID.ToString());
                    return null;
                }
            }

            var XMLTrap = _xmlDoc.CreateNode(XMLParent, "TRAP", strTrapGUID);

            CreateNamedNode(XMLTrap, "vchrTrapName", trap.TrapName);
            CreateNamedNode(XMLTrap, "vchrTrapType", trap.TrapType);
            CreateNamedNode(XMLTrap, "vchrDescription", trap.Description);
            CreateNamedNode(XMLTrap, "dtDateCreated", FormatDate(trap.DateCreated));
            CreateNamedNode(XMLTrap, "vchrWhoCreated", trap.WhoCreated);

            AddTraits(XMLTrap, TrapID, "Trap");
            AddNotes(XMLTrap, TrapID, "Trap");

            return XMLTrap;
        }

        private XmlElement AddSiteVisitItem(int SiteVisitID, string SiteVisitGUID) {

            var guid = _siteVisitList.GUIDForID(SiteVisitID);
            if (!string.IsNullOrWhiteSpace(guid)) {

                var existing = _xmlDoc.GetElementByGUID(guid, "SITEVISIT");
                if (existing != null) {
                    return existing;
                }
            }

            Log("Adding SiteVisit (SiteVisitID={0})", SiteVisitID);

            var visit = XMLIOService.GetSiteVisit(SiteVisitID);
            if (visit == null) {
                Log("Failed to extract site visit details! (SiteVisitID={0})", SiteVisitID);
                return null;
            }

            var XMLSiteNode = AddSiteItem(visit.SiteID, visit.SiteGUID.ToString());

            var XMLVisitNode = _xmlDoc.CreateNode(XMLSiteNode, "SITEVISIT", visit.GUID.ToString());

            CreateNamedNode(XMLVisitNode, "vchrSiteVisitName", visit.SiteVisitName);
            CreateNamedNode(XMLVisitNode, "vchrFieldNumber", visit.FieldNumber);
            CreateNamedNode(XMLVisitNode, "vchrCollector", visit.Collector);

            CreateNamedNode(XMLVisitNode, "intDateStart", FormatBLDate(visit.DateStart));
            CreateNamedNode(XMLVisitNode, "intDateEnd", FormatBLDate(visit.DateEnd));
            CreateNamedNode(XMLVisitNode, "intTimeStart", FormatBLTime(visit.TimeStart));
            CreateNamedNode(XMLVisitNode, "intTimeEnd", FormatBLTime(visit.TimeEnd));
            CreateNamedNode(XMLVisitNode, "vchrCasualTime", visit.CasualTime);
            CreateNamedNode(XMLVisitNode, "dtDateCreated", FormatDate(visit.DateCreated));
            CreateNamedNode(XMLVisitNode, "vchrWhoCreated", visit.WhoCreated);

            AddTraits(XMLVisitNode, SiteVisitID, "SiteVisit");
            AddNotes(XMLVisitNode, SiteVisitID, "SiteVisit");

            _siteVisitList.Add(SiteVisitGUID, SiteVisitID);

            return XMLVisitNode;
        }

        private string FormatBLTime(int? BLTime) {
            if (!BLTime.HasValue || BLTime.Value < 0) {
                return "";
            }

            var Hour = BLTime.Value / 100;
            var Minute = BLTime - (Hour * 100);
            return string.Format("{0:00}:{1:00}", Hour, Minute);
        }

        private string FormatBLDate(int? bldate) {
            if (bldate.HasValue) {
                return DateUtils.BLDateToStr(bldate.Value);
            }
            return "";
        }

        private XmlElement AddSiteItem(int SiteID, string SiteGUID) {
            var guid = _siteList.GUIDForID(SiteID);
            if (guid != null) {
                var existing = _xmlDoc.GetElementByGUID(guid, "SITE");
                if (existing != null) {
                    return existing;
                }
            }

            Log("Adding Site (SiteID={0})", SiteID);

            var site = XMLIOService.GetSite(SiteID);

            if (site == null) {
                Log("Failed to extract site data for site ID {0}!", SiteID);
                return null;
            }

            var intRegionID = site.PoliticalRegionID;
            XmlElement XMLRegionNode = null;
            XmlElement XMLSiteNode = null;
            if (intRegionID >= 0) {
                XMLRegionNode = AddRegionItem(intRegionID, site.RegionGUID.ToString());
            }

            if (XMLRegionNode == null) { // No political region, so add it directly under the Material Root
                XMLSiteNode = _xmlDoc.CreateNode(_xmlDoc.MaterialRoot, "SITE");
            } else {
                XMLSiteNode = _xmlDoc.CreateNode(XMLRegionNode, "SITE", site.GUID.ToString());
            }

            CreateNamedNode(XMLSiteNode, "vchrSiteName", site.SiteName);
            CreateNamedNode(XMLSiteNode, "tintLocalType", LocalityTypeStr(site.LocalityType));
            CreateNamedNode(XMLSiteNode, "vchrLocal", site.Locality);
            CreateNamedNode(XMLSiteNode, "vchrDistanceFromPlace", site.DistanceFromPlace);
            CreateNamedNode(XMLSiteNode, "vchrDirFromPlace", site.DirFromPlace);
            CreateNamedNode(XMLSiteNode, "vchrInformalLocal", site.InformalLocal);
            CreateNamedNode(XMLSiteNode, "tintPosCoordinates", CoordinateTypeStr(site.PosCoordinates));
            CreateNamedNode(XMLSiteNode, "tintPosAreaType", GeometryTypeStr(site.PosAreaType));
            CreateNamedNode(XMLSiteNode, "fltPosX1", site.PosX1);
            CreateNamedNode(XMLSiteNode, "fltPosY1", site.PosY1);
            CreateNamedNode(XMLSiteNode, "fltPosX2", site.PosX2);
            CreateNamedNode(XMLSiteNode, "fltPosY2", site.PosY2);
            CreateNamedNode(XMLSiteNode, "vchrPosSource", site.PosSource);
            CreateNamedNode(XMLSiteNode, "vchrPosError", site.PosError);
            CreateNamedNode(XMLSiteNode, "vchrPosWho", site.PosWho);
            CreateNamedNode(XMLSiteNode, "vchrPosDate", site.PosDate);
            CreateNamedNode(XMLSiteNode, "vchrPosOriginal", site.PosOriginal);
            CreateNamedNode(XMLSiteNode, "vchrPosUTMSource", site.PosUTMSource);
            CreateNamedNode(XMLSiteNode, "vchrPosUTMMapProj", site.PosUTMMapProj);
            CreateNamedNode(XMLSiteNode, "vchrPosUTMMapName", site.PosUTMMapName);
            CreateNamedNode(XMLSiteNode, "vchrPosUTMMapVer", site.PosUTMMapVer);
            CreateNamedNode(XMLSiteNode, "tintElevType", ElevationTypeStr(site.ElevType));
            CreateNamedNode(XMLSiteNode, "fltElevUpper", site.ElevUpper);
            CreateNamedNode(XMLSiteNode, "fltElevLower", site.ElevLower);
            CreateNamedNode(XMLSiteNode, "fltElevDepth", site.ElevDepth);
            CreateNamedNode(XMLSiteNode, "vchrElevUnits", site.ElevUnits);
            CreateNamedNode(XMLSiteNode, "vchrElevSource", site.ElevSource);
            CreateNamedNode(XMLSiteNode, "vchrElevError", site.ElevError);
            CreateNamedNode(XMLSiteNode, "vchrGeoEra", site.GeoEra);
            CreateNamedNode(XMLSiteNode, "vchrGeoState", site.GeoState);
            CreateNamedNode(XMLSiteNode, "vchrGeoPlate", site.GeoPlate);
            CreateNamedNode(XMLSiteNode, "vchrGeoFormation", site.GeoFormation);
            CreateNamedNode(XMLSiteNode, "vchrGeoMember", site.GeoMember);
            CreateNamedNode(XMLSiteNode, "vchrGeoBed", site.GeoBed);
            CreateNamedNode(XMLSiteNode, "vchrGeoName", site.GeoName);
            CreateNamedNode(XMLSiteNode, "vchrGeoAgeBottom", site.GeoAgeBottom);
            CreateNamedNode(XMLSiteNode, "vchrGeoAgeTop", site.GeoAgeTop);
            CreateNamedNode(XMLSiteNode, "vchrGeoNotes", ExpandNotes(site.GeoNotes));
            CreateNamedNode(XMLSiteNode, "dtDateCreated", FormatDate(site.DateCreated));
            CreateNamedNode(XMLSiteNode, "vchrWhoCreated", site.WhoCreated);

            AddTraits(XMLSiteNode, SiteID, "Site");
            AddNotes(XMLSiteNode, SiteID, "Site");
            AddMultimedia(XMLSiteNode, SiteID, "Site");

            _siteList.Add(SiteGUID, SiteID);

            return XMLSiteNode;
        }

        private string AddToUnplacedList(int TaxonID) {
            if (TaxonID <= 0) {
                return null;
            }

            var guid = _taxonList.GUIDForID(TaxonID);

            if (guid == null) {
                var taxon = TaxaService.GetTaxon(TaxonID);
                if (taxon != null) {
                    guid = taxon.GUID.ToString();
                    _unplacedTaxon.Add(guid, TaxonID);
                } else {
                    Log("Failed to extract Taxon Details for TaxonID {0}", TaxonID);
                }
            }

            return guid;
        }

        private void AddTaxonDistribution(XmlElement ParentNode, Taxon taxon) {

            var dist = TaxaService.GetDistribution(taxon.TaxaID);
            var strDesc = TaxaService.GetDistributionQualification(taxon.TaxaID.Value);

            Log("Adding Taxon Distribution (TaxonID={0})", taxon.TaxaID.Value);
            var XMLDist = _xmlDoc.CreateNode(ParentNode, "DISTRIBUTION");
            _xmlDoc.CreateNode(XMLDist, "DESCRIPTION").InnerText = strDesc;

            foreach (TaxonDistribution d in dist) {
                if (IsCancelled) {
                    break;
                }

                var XMLDistItem = _xmlDoc.CreateNode(XMLDist, "DISTRIBUTIONITEM");
                XMLDistItem.AddAttribute("ID", d.GUID.ToString());
                CreateNamedNode(XMLDistItem, "bitIntroduced", d.Introduced);
                CreateNamedNode(XMLDistItem, "bitUncertain", d.Uncertain);
                CreateNamedNode(XMLDistItem, "bitThroughoutRegion", d.ThroughoutRegion);
                CreateNamedNode(XMLDistItem, "txtQual", d.Qual);
                var XMLNode = CreateNamedNode(XMLDistItem, "txtDistRegionFullPath", d.DistRegionFullPath);
                XMLNode.AddAttribute("PATHSEPARATOR", "\\");
            }
        }

        private void AddCommonNames(XmlElement taxonNode, Taxon taxon) {
            Log("Adding Commmon Names (TaxonID={0})", taxon.TaxaID.Value);

            var commonNames = TaxaService.GetCommonNames(taxon.TaxaID.Value);

            var XMLNameList = _xmlDoc.CreateNode(taxonNode, "COMMONNAMELIST");
            foreach (CommonName name in commonNames) {
                var XMLName = _xmlDoc.CreateNode(XMLNameList, "COMMONNAME");
                XMLName.AddAttribute("ID", name.GUID.ToString());
                CreateNamedNode(XMLName, "vchrCommonName", name.Name);
                CreateNamedNode(XMLName, "intRefID", AddReferenceItem(name.RefID));
                CreateNamedNode(XMLName, "vchrRefPage", name.RefPage);
                CreateNamedNode(XMLName, "txtNotes", ExpandNotes(name.Notes));
                CreateNamedNode(XMLName, "vchrRefCode", name.RefCode);
            }
        }

        private void AddReferences(XmlElement ParentNode, int itemId, string category) {
            var refLinks = SupportService.GetReferenceLinks(category, itemId);
            Log("Adding References ({0}, ID={1})", category, itemId);
            var XMLRefParent = _xmlDoc.CreateNode(ParentNode, "REFERENCES");

            foreach (RefLink link in refLinks) {
                if (IsCancelled) {
                    break;
                }

                var lngRefID = link.RefID;
                var strRefGUID = AddReferenceItem(lngRefID);
                if (!string.IsNullOrWhiteSpace(strRefGUID)) {
                    var XMLRef = _xmlDoc.CreateNode(XMLRefParent, "REFERENCELINK");
                    XMLRef.AddAttribute("ID", link.GUID.ToString());

                    CreateNamedNode(XMLRef, "intRefID", strRefGUID);
                    CreateNamedNode(XMLRef, "RefLink", link.RefLinkID);
                    CreateNamedNode(XMLRef, "vchrRefPage", link.RefPage);
                    CreateNamedNode(XMLRef, "txtRefQual", ExpandNotes(link.RefQual));
                    CreateNamedNode(XMLRef, "intOrder", link.Order);
                    CreateNamedNode(XMLRef, "bitUseInReport", link.UseInReport);
                } else {
                    Log("Failed to locate Reference Item {0} for {1}. ID={2} -  RefLink not exported.", lngRefID, category, itemId);
                }
            }
        }

        private void AddAvailableNameData(XmlElement taxonNode, Taxon taxon, TaxonRank rank) {
            Log("Exporting Available name data (TaxonID={0}) Rank Category='{1}'", taxon.TaxaID.Value, rank.Category);

            if (taxon.AvailableName.ValueOrFalse()) {
                switch (rank.Category.ToLower()) {
                    case "s":        // Species Available Name
                        AddSANData(taxonNode, taxon);
                        break;
                    case "g":        // Genus Available Name
                        AddGANData(taxonNode, taxon);
                        break;
                    case "h":
                    case "f": // Literature Available name
                        AddALNData(taxonNode, taxon);
                        break;
                    default:
                        Log("Unknown Rank Category (TaxonID={0}), RankCategory='{1}'). No Available Name Data exported.", taxon.TaxaID.Value, rank.Category);
                        break;
                }
            } else if (taxon.LiteratureName.ValueOrFalse()) {
                AddALNData(taxonNode, taxon);
            }
        }

        private void AddALNData(XmlElement taxonNode, Taxon taxon) {
            Log("Literature Available Name (TaxonID={0})", taxon.TaxaID.Value);
            var aln = TaxaService.GetAvailableName(taxon.TaxaID.Value);

            if (aln != null) {
                var XMLNode = _xmlDoc.CreateNode(taxonNode, "ALN");
                XMLNode.AddAttribute("ID", aln.GUID.Value.ToString());
                CreateNamedNode(XMLNode, "intRefID", AddReferenceItem(aln.RefID));
                CreateNamedNode(XMLNode, "vchrRefPage", aln.RefPage);
                CreateNamedNode(XMLNode, "txtRefQual", aln.RefQual);
                CreateNamedNode(XMLNode, "vchrRefCode", aln.RefCode);
            }
        }

        private string AddReferenceItem(int? refId) {
            if (refId.HasValue && refId.Value > 0) {
                var guid = _referenceList.GUIDForID(refId.Value);
                if (string.IsNullOrEmpty(guid)) {
                    var r = SupportService.GetReference(refId.Value);
                    if (r != null) {
                        Log("Adding Reference (RefID={0})", refId);
                        var XMLRefParent = _xmlDoc.ReferenceRoot;
                        var XMLRefNode = _xmlDoc.CreateNode(XMLRefParent, "REFERENCE");
                        guid = r.GUID.ToString();
                        XMLRefNode.AddAttribute("ID", guid);
                        CreateNamedNode(XMLRefNode, "vchrRefCode", r.RefCode);
                        CreateNamedNode(XMLRefNode, "vchrAuthor", r.Author);
                        CreateNamedNode(XMLRefNode, "vchrTitle", r.Title);
                        CreateNamedNode(XMLRefNode, "vchrBookTitle", r.BookTitle);
                        CreateNamedNode(XMLRefNode, "vchrEditor", r.Editor);
                        CreateNamedNode(XMLRefNode, "vchrRefType", r.RefType);
                        CreateNamedNode(XMLRefNode, "vchrYearOfPub", r.YearOfPub);
                        CreateNamedNode(XMLRefNode, "vchrActualDate", r.ActualDate);
                        CreateNamedNode(XMLRefNode, "vchrPartNo", r.PartNo);
                        CreateNamedNode(XMLRefNode, "vchrSeries", r.Series);
                        CreateNamedNode(XMLRefNode, "vchrPublisher", r.Publisher);
                        CreateNamedNode(XMLRefNode, "vchrPlace", r.Place);
                        CreateNamedNode(XMLRefNode, "vchrVolume", r.Volume);
                        CreateNamedNode(XMLRefNode, "vchrPages", r.Pages);
                        CreateNamedNode(XMLRefNode, "vchrTotalPages", r.TotalPages);
                        CreateNamedNode(XMLRefNode, "vchrPossess", r.Possess);
                        CreateNamedNode(XMLRefNode, "vchrSource", r.Source);
                        CreateNamedNode(XMLRefNode, "vchrEdition", r.Edition);
                        CreateNamedNode(XMLRefNode, "vchrISBN", r.ISBN);
                        CreateNamedNode(XMLRefNode, "vchrISSN", r.ISSN);
                        CreateNamedNode(XMLRefNode, "txtAbstract", r.Abstract);
                        CreateNamedNode(XMLRefNode, "txtFullText", r.FullText);
                        CreateNamedNode(XMLRefNode, "intStartPage", r.StartPage);
                        CreateNamedNode(XMLRefNode, "intEndPage", r.EndPage);
                        CreateNamedNode(XMLRefNode, "vchrJournalName", r.JournalName);
                        CreateNamedNode(XMLRefNode, "intJournalID", AddJournal(r.JournalID.HasValue ? r.JournalID.Value : -1));
                        CreateNamedNode(XMLRefNode, "dtDateCreated", FormatDate(r.DateCreated));
                        CreateNamedNode(XMLRefNode, "vchrWhoCreated", r.WhoCreated);

                        AddTraits(XMLRefNode, refId.Value, "Reference");

                        AddNotes(XMLRefNode, refId.Value, "Reference");

                        // AddKeywords(XMLRefNode, refId.Value, "Reference");

                        AddMultimedia(XMLRefNode, refId.Value, "Reference");

                        _referenceList.Add(guid, refId.Value);

                    } else {
                        Log("Failed to extract reference detail for ref id {0}", refId);
                    }
                }
                return guid;
            } else {
                return "";
            }
        }

        private string AddJournal(int journalId) {
            if (journalId < 0) {
                return "";
            }

            var guid = _journalList.GUIDForID(journalId);
            if (string.IsNullOrEmpty(guid)) {
                var journal = SupportService.GetJournal(journalId);
                if (journal != null) {
                    Log("Adding Journal Item (JournalID={0})", journalId);
                    var XMLJournal = _xmlDoc.CreateNode(_xmlDoc.JournalRoot, "JOURNAL");
                    guid = journal.GUID.ToString();
                    XMLJournal.AddAttribute("ID", guid);
                    CreateNamedNode(XMLJournal, "vchrAbbrevName", journal.AbbrevName);
                    CreateNamedNode(XMLJournal, "vchrAbbrevName2", journal.AbbrevName2);
                    CreateNamedNode(XMLJournal, "vchrAlias", journal.Alias);
                    CreateNamedNode(XMLJournal, "vchrFullName", journal.FullName);
                    CreateNamedNode(XMLJournal, "txtNotes", ExpandNotes(journal.Notes));
                    CreateNamedNode(XMLJournal, "dtDateCreated", FormatDate(journal.DateCreated));
                    CreateNamedNode(XMLJournal, "vchrWhoCreated", journal.WhoCreated);

                    AddTraits(XMLJournal, journalId, "Journal");
                    AddNotes(XMLJournal, journalId, "Journal");
                    _journalList.Add(guid, journalId);
                } else {
                    Log("Failed to retrieve journal details (Journal ID={0}).", journalId);
                }
            }

            return guid;
        }

        private void AddMultimedia(XmlElement ParentNode, int itemId, string category) {

            if (!Options.ExportMultimedia) {
                return;
            }


            var links = XMLIOService.GetExportMultimediaLinks(category, itemId);

            Log("Adding Multimedia ({0}, ID={1})", category, itemId);
            var XMLMM = _xmlDoc.CreateNode(ParentNode, "MULTIMEDIA");
            foreach (XMLIOMultimediaLink link in links) {
                if (IsCancelled) {
                    break;
                }
                var XMLMMNode = _xmlDoc.CreateNode(XMLMM, "MULTIMEDIALINK");
                XMLMMNode.AddAttribute("ID", link.GUID.ToString());
                CreateNamedNode(XMLMMNode, "intMultimediaID", AddMultimediaItem(link.MultimediaID));
                CreateNamedNode(XMLMMNode, "MultimediaType", link.MultimediaType);
                CreateNamedNode(XMLMMNode, "vchrCaption", link.Caption);
                CreateNamedNode(XMLMMNode, "bitUseInReport", link.UseInReport);
            }
        }

        private string AddMultimediaItem(int MediaID) {
            if (MediaID < 0) {
                return "";
            }

            var guid = _multimediaList.GUIDForID(MediaID);
            if (string.IsNullOrEmpty(guid)) {
                var mm = XMLIOService.GetMultimedia(MediaID);
                if (mm != null) {
                    Log("Adding Multimedia Item (MediaID={0})", MediaID);
                    var XMLMM = _xmlDoc.CreateNode(_xmlDoc.MultimediaRoot, "MULTIMEDIAITEM");
                    guid = mm.GUID.ToString();
                    XMLMM.AddAttribute("ID", guid);
                    XMLMM.AddAttribute("ENCODING", "BASE64");
                    var strFilename = mm.Name;
                    var strExtension = mm.FileExtension;
                    CreateNamedNode(XMLMM, "vchrName", strFilename);
                    CreateNamedNode(XMLMM, "vchrNumber", mm.Number);
                    CreateNamedNode(XMLMM, "vchrArtist", mm.Artist);
                    CreateNamedNode(XMLMM, "vchrDateRecorded", mm.DateRecorded);
                    CreateNamedNode(XMLMM, "vchrOwner", mm.Owner);
                    CreateNamedNode(XMLMM, "vchrFileExtension", strExtension);
                    CreateNamedNode(XMLMM, "intSizeInBytes", mm.SizeInBytes);
                    CreateNamedNode(XMLMM, "txtCopyright", mm.Copyright);
                    CreateNamedNode(XMLMM, "dtDateCreated", FormatDate(mm.DateCreated));
                    CreateNamedNode(XMLMM, "vchrWhoCreated", mm.WhoCreated);

                    Log("Base64 Encoding image data for multimedia {0}", MediaID);
                    var XMLData = _xmlDoc.XMLDocument.CreateCDataSection(Convert.ToBase64String(mm.Multimedia));
                    XMLMM.AppendChild(XMLData);

                    //using (var imgStream = new MemoryStream(mm.Multimedia)) {
                    //    using (var outputStream = new MemoryStream()) {
                    //        try {

                    //            UUCodec.UUEncode(imgStream, outputStream);
                    //            outputStream.Position = 0;
                    //            var reader = new StreamReader(outputStream);
                    //            var imageData = reader.ReadToEnd();

                    //            if (imageData.Contains("]]>")) {
                    //                imageData = imageData.Replace("]]>", "]]&gt;");
                    //            }
                    //            var XMLData = _xmlDoc.XMLDocument.CreateCDataSection(imageData);
                    //            XMLMM.AppendChild(XMLData);
                    //        } catch (Exception ex) {
                    //            Log("Exception encoding Multimedia Item {0}: {1}", MediaID, ex.Message);
                    //        }
                    //    }
                    //}

                    AddTraits(XMLMM, MediaID, "Multimedia");
                    AddNotes(XMLMM, MediaID, "Multimedia");

                    _multimediaList.Add(guid, MediaID);
                } else {
                    Log("Failed to export multimedia for item {0} - no media found.", MediaID);
                }
            }
            return guid;
        }

        private void AddKeywords(XmlElement ParentNode, int itemId, string category) {
            throw new NotImplementedException();
        }

        private void AddNotes(XmlElement ParentNode, int itemId, string category) {

            if (!Options.ExportNotes) {
                return;
            }

            var notes = SupportService.GetNotes(category, itemId);

            Log("Adding Notes ({0}, ID={1})", category, itemId);
            var XMLNoteParent = _xmlDoc.CreateNode(ParentNode, "NOTES");
            foreach (Note note in notes) {
                if (IsCancelled) {
                    break;
                }
                var XMLNote = _xmlDoc.CreateNode(XMLNoteParent, "NOTEITEM");
                XMLNote.AddAttribute("ID", note.GUID.ToString());
                CreateNamedNode(XMLNote, "NoteType", note.NoteType);
                CreateNamedNode(XMLNote, "txtNote", ExpandNotes(note.NoteRTF));
                CreateNamedNode(XMLNote, "vchrAuthor", note.Author);
                CreateNamedNode(XMLNote, "txtComments", note.Comments);
                CreateNamedNode(XMLNote, "bitUseInReports", note.UseInReports);
                CreateNamedNode(XMLNote, "RefID", AddReferenceItem(note.RefID));
                CreateNamedNode(XMLNote, "vchrRefPages", note.RefPages);
            }
        }

        private string ExpandNotes(string notes) {

            if (string.IsNullOrEmpty(notes)) {
                return "";
            }

            var lngPos = notes.IndexOf("#");
            if (lngPos < 0) {
                return notes;
            }

            var bFinished = false;
            var strRemainder = notes.Substring(lngPos);
            while (!bFinished) {
                lngPos = strRemainder.IndexOf("#");
                if (lngPos < 0) {
                    bFinished = true;
                } else {
                    var strBuf = strRemainder.Substring(0, lngPos);
                    var refId = ValidateRefCode(strBuf);
                    if (refId.HasValue) {
                        AddReferenceItem(refId);
                    }
                    strRemainder = strRemainder.Substring(lngPos + 1);
                }
            }

            return notes;
        }

        private int? ValidateRefCode(string code) {
            if (code.Length > 50) {
                return null;
            }

            var lngPos = code.IndexOf(":");
            if (lngPos < 0) {
                return null;
            }
            var strCode = code.Substring(0, lngPos);

            return SupportService.GetReferenceIDFromRefCode(strCode);
        }

        private void AddTraits(XmlElement ParentNode, int itemId, string category) {
            if (!Options.ExportTraits) {
                return;
            }

            var traits = SupportService.GetTraits(category, itemId);
            Log("Adding Traits ({0}, ID={1}", category, itemId);

            foreach (Trait t in traits) {
                if (IsCancelled) {
                    break;
                }
                var alias = t.Name;
                var mangled = MangleName(alias);
                var XMLTraitItem = _xmlDoc.CreateNode(ParentNode, mangled);
                XMLTraitItem.AddAttribute("ALIAS", alias);
                XMLTraitItem.InnerText = t.Value;
            }
        }

        private string MangleName(string name) {
            var strReplace = " <>&:/\\" + (char)34;
            var sb = new StringBuilder();
            foreach (char ch in name) {
                if (strReplace.IndexOf(ch) >= 0) {
                    sb.Append("_");
                } else {
                    sb.Append(ch);
                }
            }
            return sb.ToString().ToUpper();
        }

        private void AddGANData(XmlElement taxonNode, Taxon taxon) {
            Log("Genus Available Name (TaxonID={0})", taxon.TaxaID.Value);
            var gan = TaxaService.GetGenusAvailableName(taxon.TaxaID.Value);
            if (gan != null) {
                var XMLNode = _xmlDoc.CreateNode(taxonNode, "GAN");
                XMLNode.AddAttribute("ID", gan.GUID.ToString());
                CreateNamedNode(XMLNode, "intRefID", AddReferenceItem(gan.RefID));
                CreateNamedNode(XMLNode, "vchrRefPage", gan.RefPage);
                CreateNamedNode(XMLNode, "txtRefQual", gan.RefQual);
                CreateNamedNode(XMLNode, "sintDesignation", DesignationStr(gan.Designation));
                CreateNamedNode(XMLNode, "vchrTSFixationMethod", gan.TSFixationMethod);
                CreateNamedNode(XMLNode, "vchrTypeSpecies", gan.TypeSpecies);
                CreateNamedNode(XMLNode, "vchrRefCode", gan.RefCode);

                var isData = TaxaService.GetGenusAvailableNameIncludedSpecies(taxon.TaxaID.Value);

                foreach (GANIncludedSpecies ganis in isData) {
                    var XMLISNode = _xmlDoc.CreateNode(XMLNode, "INCLUDEDSPECIES");
                    XMLISNode.SetAttribute("ID", ganis.GUID.ToString());
                }
            } else {
                Log("No Genus Available Name data for taxon {0} (ID {1})", taxon.TaxaFullName, taxon.TaxaID.Value);
            }
        }

        private string DesignationStr(int Designation) {

            switch (Designation) {
                case 0:
                    return "Designated (Type Species)";
                case 1:
                    return "None Required";
                case 2:
                    return "Not Designated (With Included Species)";
                case 3:
                    return "Not Designated (Without Included Species)";
                default:
                    return "Unknown Designation";
            }
        }

        private void AddSANData(XmlElement taxonNode, Taxon taxon) {
            Log("Species Available Name (TaxonID={0})", taxon.TaxaID.Value);

            var san = TaxaService.GetSpeciesAvailableName(taxon.TaxaID.Value);

            if (san != null) {
                var typeData = TaxaService.GetSANTypeData(taxon.TaxaID.Value);
                // var typeDataTypes = TaxaService.GetSANTypeDataTypes(taxon.TaxaID.Value);

                var XMLNode = _xmlDoc.CreateNode(taxonNode, "SAN");

                XMLNode.AddAttribute("ID", san.GUID.ToString());
                CreateNamedNode(XMLNode, "intRefID", AddReferenceItem(san.RefID));
                CreateNamedNode(XMLNode, "vchrRefPage", san.RefPage);
                CreateNamedNode(XMLNode, "txtRefQual", san.RefQual);
                CreateNamedNode(XMLNode, "vchrPrimaryType", san.PrimaryType);
                CreateNamedNode(XMLNode, "vchrSecondaryType", san.SecondaryType);
                CreateNamedNode(XMLNode, "bitPrimaryTypeProbable", san.PrimaryTypeProbable);
                CreateNamedNode(XMLNode, "bitSecondaryTypeProbable", san.SecondaryTypeProbable);
                CreateNamedNode(XMLNode, "vchrRefCode", san.RefCode);

                foreach (SANTypeData type in typeData) {
                    var XMLTypeNode = _xmlDoc.CreateNode(XMLNode, "SANTYPE");

                    XMLTypeNode.AddAttribute("ID", type.GUID.ToString());
                    CreateNamedNode(XMLTypeNode, "vchrType", type.Type);
                    CreateNamedNode(XMLTypeNode, "vchrMuseum", type.Museum);
                    CreateNamedNode(XMLTypeNode, "vchrAccessionNum", type.AccessionNumber);
                    CreateNamedNode(XMLTypeNode, "vchrMaterial", type.MaterialName);
                    CreateNamedNode(XMLTypeNode, "vchrLocality", type.Locality);
                    CreateNamedNode(XMLTypeNode, "bitIDConfirmed", type.IDConfirmed);
                    if (Options.ExportMaterial) { // if I am exporting material then                
                        var lngMaterialID = type.MaterialID;
                        var strMaterialGUID = "";
                        if (lngMaterialID > 0) {
                            AddMaterialItem(lngMaterialID.Value);
                        }
                        CreateNamedNode(XMLTypeNode, "intMaterialID", strMaterialGUID);
                    }
                }
            } else {
                Log("No Species Available Name data found for {0} (ID {1})", taxon.TaxaFullName, taxon.TaxaID.Value);
            }
        }

        private string FormatDate(DateTime? dt) {
            if (dt.HasValue) {
                if (dt.Value.Year == 1 && dt.Value.Month == 1 && dt.Value.Day == 1) {
                    // This default date is out of range, and can't be imported. Not ideal, but set it to the export date instead.
                    dt = DateTime.Now;
                }
                return string.Format("{0:yyyy-MM-dd}", dt.Value);
            }
            return "";
        }

        private XmlElement CreateNamedNode(XmlElement parent, string fieldname, string value) {
            var strXMLTag = LookupXMLName(parent.Name, fieldname);
            return parent.AddNamedValue(strXMLTag, value);
        }

        private XmlElement CreateNamedNode(XmlElement parent, string fieldname, int? value) {
            var strXMLTag = LookupXMLName(parent.Name, fieldname);
            return parent.AddNamedValue(strXMLTag, value.HasValue ? value.Value + "" : "");
        }

        private XmlElement CreateNamedNode(XmlElement parent, string fieldname, double? value) {
            var strXMLTag = LookupXMLName(parent.Name, fieldname);
            return parent.AddNamedValue(strXMLTag, value.HasValue ? value.Value + "" : "");
        }

        private XmlElement CreateNamedNode(XmlElement parent, string fieldname, bool? value) {
            var strXMLTag = LookupXMLName(parent.Name, fieldname);
            return parent.AddNamedValue(strXMLTag, value.ValueOrFalse() ? "1" : "0");
        }

        private string LookupXMLName(string category, string field) {
            var collection = GetCollectionForCategory(category);
            if (collection != null) {
                if (!collection.ContainsKey(field)) {
                    Log("XML Name not found!");
                    return field;
                }
                return collection[field].XMLName;
            }

            throw new Exception("Unrecognized category: " + category);
        }

        private int GetItemCount(int taxonId) {

            var stats = TaxaService.GetTaxonStatistics(taxonId);
            return stats.TotalItems + 1; // the taxon itself counts as one
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

            InitMappings();

            _xmlDoc = new XMLExportObject();
        }

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

        internal int IDfromGUID(string guid) {
            if (ContainsKey(guid)) {
                return this[guid];
            }

            return -1;
        }
    }

    class NameToIDCache : Dictionary<string, int> {
        public bool NameInCache(string name, out int id) {
            return TryGetValue(name, out id);
        }

        public bool IdInCache(int id, out string name) {
            if (ContainsValue(id)) {
                foreach (string key in Keys) {
                    if (this[key] == id) {
                        name = key;
                        return true;
                    }
                }
            }
            name = null;
            return false;
        }

    }

    public class FieldToNameMappings : Dictionary<string, NameMapping> {

        public void Add(string name, string field, bool isInsertable = true) {
            var mapping = new NameMapping { XMLName = name, FieldName = field, IsInsertable = isInsertable };
            Add(field, mapping);
        }


        internal bool XMLNameToFieldName(string XMLName, out string FieldName, bool FailIfNotInsertable = true) {
            foreach (NameMapping m in this.Values) {
                if (m.XMLName.Equals(XMLName, StringComparison.CurrentCultureIgnoreCase)) {
                    if (FailIfNotInsertable) {
                        if (m.IsInsertable) {
                            FieldName = m.FieldName;
                            return true;
                        } else {
                            FieldName = "";
                            return false;
                        }
                    } else {
                        FieldName = m.FieldName;
                            return true;
                    }
                } 
            }
            FieldName = "";
            return false;
        }
    }

    public class NameMapping {
        public string XMLName { get; set; }
        public string FieldName { get; set; }
        public bool IsInsertable { get; set; }
    }
}
