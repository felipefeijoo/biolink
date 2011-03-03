using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace BioLink.Client.Tools {

    class ImportProcessor {

        private Regex _TraitRegex = new Regex("^(.*)[.]Other$");

        public const int JUST_LOCALITY = 0;
        public const int OFFSET_LOCALITY = 1;
        public const int POINT_POSITION = 1;
        public const int LINE_POSITION = 2;
        public const int BOX_POSITION = 3;
        public const int ALTITUDE_ELEVATION = 1;
        public const int DEPTH_ELEVATION = 2;
        public const int UTM_COORDINATE = 2;
        public const int LATLON_COORDINATE = 1;
        public const int FIXED_DATE = 1;
        public const int CASUAL_DATE = 2;

        private List<TaxonRankName> _ranks;

        private Dictionary<string, int> _fieldIndex = new Dictionary<string, int>();

        private CachedRegion _lastRegion;
        private CachedSite _lastSite;
        private CachedSiteVisit _lastSiteVisit;
        private TaxonCache _taxonCache = new TaxonCache();

        public ImportProcessor(TabularDataImporter importer, IEnumerable<ImportFieldMapping> mappings, IProgressObserver progress, Action<ImportStatusLevel, string> logFunc) {
            this.Importer = importer;
            this.Mappings = mappings;
            this.Progress = progress;
            this.LogFunc = logFunc;
            this.User = PluginManager.Instance.User;
            this.Service = new ImportService(User);
        }

        public void Import() {

            if (Progress != null) {
                Progress.ProgressStart("Initialising...");
            }

            if (!InitImport()) {
                return;
            }

            ProgressMsg("Importing rows - Stage 1", 0);
            if (!DoStage1()) {
                return;
            }

            CreateColumnIndexes();

            LogMsg("Importing rows - Stage 2", 10);
            int rowCount = 0;

            Cancelled = false;

            var connection = User.GetConnection();

            int lastPercent = 0;

            while (RowSource.MoveNext() && !Cancelled) {
                rowCount++;
                var dblPercent = (double)((double)rowCount / (double)RowSource.RowCount) * 90;
                int percent = ((int)dblPercent) + 10 ;

                if (percent != lastPercent) {
                    var message = string.Format("Importing rows - Stage 2 ({0} of {1})", rowCount, RowSource.RowCount);
                    ProgressMsg(message, percent);
                    lastPercent = percent;
                }

                ImportCurrentRow(connection);
            }

            ProgressMsg("Importing rows - Stage 2 Complete", 100);

            connection.Close();
        }

        private void ImportCurrentRow(System.Data.SqlClient.SqlConnection connection) {

            int regionNumber = -1;
            int siteNumber = -1;
            int siteVisitNumber = -1;
            int taxonNumber = -1;
            int materialNumber = -1;
            int materialPartNumber = -1;

            try {
                User.BeginTransaction(connection);

                var level = GetLevel(Mappings);

                switch (level) {
                    case ImportLevel.Region:
                        regionNumber = GetRegionNumber();
                        break;
                    case ImportLevel.Site:
                        regionNumber = GetRegionNumber();
                        siteNumber = GetSiteNumber(regionNumber);
                        InsertTraits("Site", siteNumber);
                        break;
                    case ImportLevel.Visit:
                        regionNumber = GetRegionNumber();
                        siteNumber = GetSiteNumber(regionNumber);
                        InsertTraits("Site", siteNumber);
                        siteVisitNumber = GetSiteVisitNumber(siteNumber);
                        InsertTraits("SiteVisit", siteVisitNumber);
                        break;
                    case ImportLevel.MaterialWithTaxa:
                        regionNumber = GetRegionNumber();
                        siteNumber = GetSiteNumber(regionNumber);
                        InsertTraits("Site", siteNumber);
                        siteVisitNumber = GetSiteVisitNumber(siteNumber);
                        InsertTraits("SiteVisit", siteVisitNumber);
                        taxonNumber = GetTaxonNumber();
                        InsertTraits("Taxon", taxonNumber);
                        InsertCommonName(taxonNumber);
                        materialNumber = AddMaterial(siteVisitNumber, taxonNumber);
                        InsertTraits("Material", materialNumber);
                        materialPartNumber = AddMaterialPart(materialNumber);
                        break;
                    case ImportLevel.MaterialWithoutTaxa:
                        regionNumber = GetRegionNumber();
                        siteNumber = GetSiteNumber(regionNumber);
                        InsertTraits("Site", siteNumber);
                        siteVisitNumber = GetSiteVisitNumber(siteNumber);
                        InsertTraits("SiteVisit", siteVisitNumber);
                        InsertCommonName(taxonNumber);
                        materialNumber = AddMaterial(siteVisitNumber, -1);
                        InsertTraits("Material", materialNumber);
                        materialPartNumber = AddMaterialPart(materialNumber);
                        break;
                    case ImportLevel.TaxaOnly:
                        taxonNumber = GetTaxonNumber();
                        InsertCommonName(taxonNumber);
                        InsertTraits("Taxon", taxonNumber);
                        break;
                }


                // If we get here we can commit the transacton....
                User.CommitTransaction();
                // TODO: If the import is successful we can remove the source row from the staging database...or we could simply mark the row as successfully imported, and do a purge at the end...
            } catch (Exception ex) {
                Error("Error: {0}", ex.Message);
                // Roll back the transaction....
                User.RollbackTransaction();
                //  Mark the import row as failed
                RowSource.CopyToErrorTable(ex.Message);
            } 

        }

        private void InsertTraits(string category, int id) {
            foreach (ImportFieldMapping mapping in Mappings) {
                var match = _TraitRegex.Match(mapping.TargetColumn);
                if (match.Success) {
                    var candiateCategory = match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(candiateCategory) && candiateCategory.Equals(category, StringComparison.CurrentCultureIgnoreCase)) {
                        var value = Get(mapping.SourceColumn);
                        if (!string.IsNullOrEmpty(value)) {
                            Service.ImportTrait(category, id, mapping.SourceColumn, value);
                        }
                    }
                }
            }
        }

        private string Get(string field, string def = "") {
            var key = field.ToLower();
            if (_fieldIndex.ContainsKey(key)) {
                var index = _fieldIndex[key];
                var mapping = Mappings.ElementAt(index);
                Object value = null;
                if (!mapping.IsFixed) {
                    value = RowSource[index];
                } else {
                    value = mapping.DefaultValue;
                }

                var defaultValue = string.IsNullOrWhiteSpace(mapping.DefaultValue) ? def : mapping.DefaultValue;

                return value == null ? defaultValue : value.ToString();
            }

            return def;
        }

        private int GetRegionNumber() {

            var politicalRegion = Get("Region.Region", "[Imported Data]");
            var strCountry = Get("Region.Country");
            var strState = Get("Region.State/Province");
            var strCounty = Get("Region.County");

            if (string.IsNullOrWhiteSpace(politicalRegion)) {
                politicalRegion = "[Imported Data]";
            }

            if (_lastRegion != null && _lastRegion.Equals(politicalRegion, strCountry, strState, strCounty)) {
                return _lastRegion.RegionID;
            } else {
                var lngRegionID = Service.ImportRegion(politicalRegion, 0, "Region");
                if (!string.IsNullOrWhiteSpace(strCountry)) {
                    lngRegionID = Service.ImportRegion(strCountry, lngRegionID, "Country");
                }
                if (!string.IsNullOrWhiteSpace(strState)) {
                    lngRegionID = Service.ImportRegion(strState, lngRegionID, "State/Province");
                }
                if (!string.IsNullOrWhiteSpace(strCounty)) {
                    lngRegionID = Service.ImportRegion(strCounty, lngRegionID, "County");
                }

                _lastRegion = new CachedRegion { RegionID = lngRegionID, PoliticalRegion = politicalRegion, Country = strCountry, State = strState, County = strCounty };

                return lngRegionID;
            }
        }

        private Regex _DegDecMRegex = new Regex(@"^(\d+).*?([\d.]+)'*\s*(N|S|E|W|n|e|s|w)\s*$");
        private Regex _DMSRegex = new Regex(@"^(\d+)[^\d]+(\d+)\s*""\s*(\d+)\s*'\s*(N|S|E|W|n|e|s|w)\s*$");

        private double? GetCoordinate(string field, double? @default) {

            var str = Get(field);
            if (string.IsNullOrWhiteSpace(str)) {
                return @default;
            }

            // First try decimal degrees...
            if (str.IsNumeric()) {
                return Double.Parse(str);
            }

            // next try Degrees*minutes'Seconds", where * can be anything

            var matcher = _DMSRegex.Match(str);
            if (matcher.Success) {
                int degrees = Int32.Parse(matcher.Groups[1].Value);
                int minutes = Int32.Parse(matcher.Groups[2].Value);
                int seconds = Int32.Parse(matcher.Groups[3].Value);
                string dir = matcher.Groups[4].Value;
                return GeoUtils.DMSToDecDeg(degrees, minutes, seconds, dir);
            }

            matcher = _DegDecMRegex.Match(str);
            if (matcher.Success) {
                int degrees = Int32.Parse(matcher.Groups[1].Value);
                double minutes = Double.Parse(matcher.Groups[2].Value);
                string dir = matcher.Groups[3].Value;

                return GeoUtils.DDecMDirToDecDeg(degrees, minutes, dir);
            }

            return @default;
        }

        private int GetSiteNumber(int regionId) {

            var strLocal = Get("Site.Locality");
            var strOffSetDis = Get("Site.Distance from place");
            var strOffsetDir = Get("Site.Direction from place");
            var strInformal = Get("Site.Informal Locality");

            double? x1 = GetCoordinate("Site.Longitude", null);
            double? y1 = GetCoordinate("Site.Latitude", null);
            double? x2 = GetCoordinate("Site.Longitude 2", null);
            double? y2 = GetCoordinate("Site.Latitude 2", null);

            var iPosAreaType = GetPositionType();
            int iLocalType = 0; // TODO? Can we workout what this is supposed to be?
            var iElevationType = GetElevationType();

            int iCoordinateType;

            GetSiteCoordinates(out iCoordinateType, ref x1, ref y1, ref x2, ref y2);

            var strName = Get("Site.Site Name");
            if (string.IsNullOrWhiteSpace(strName)) {
                strName = strLocal;
            }

            if (_lastSite != null && _lastSite.Equals(strName, strLocal, strOffSetDis, strOffsetDir, strInformal, iCoordinateType, x1, y1, x2, y2)) {
                return _lastSite.SiteID;
            } else {
                // Create the Site object from the source material....
                var site = new Site { 
                    SiteName = strName, 
                    PoliticalRegionID = regionId, 
                    LocalityType = iLocalType, 
                    Locality = strLocal, 
                    DistanceFromPlace = strOffSetDis, 
                    DirFromPlace = strOffsetDir,
                    InformalLocal = Get("Site.Informal locality"),
                    PosCoordinates = iCoordinateType,
                    PosAreaType = iPosAreaType,
                    PosX1 = x1,
                    PosY1 = y1,
                    PosX2 = x2,
                    PosY2 = y2,
                    PosXYDisplayFormat = 1,
                    PosSource = Get("Site.Position source"),
                    PosError = Get("Site.Position error"),
                    PosWho = Get("Site.Generated by"),
                    PosDate = Get("Site.Generated on"),
                    PosOriginal = Get("Site.Original position"),
                    PosUTMSource = Get("Site.UTM source"),
                    PosUTMMapProj = Get("Site.UTM map projection"),
                    PosUTMMapName = Get("Site.UTM map name"),
                    PosUTMMapVer = Get("Site.UTM map version"),
                    ElevType= iElevationType,
                    ElevUpper = GetDouble("Site.Elevation upper"),
                    ElevLower = GetDouble("Site.Elevation lower"),
                    ElevDepth= GetDouble("Site.Elevation depth"),
                    ElevUnits = Get("Site.Elevation units"),
                    ElevSource = Get("Site.Elevation source"),
                    ElevError = Get("Site.Elevation error"),
                    GeoEra = Get("Site.Geological era"),
                    GeoState = Get("Site.Geological state"),
                    GeoPlate = Get("Site.Geological plate"),
                    GeoFormation = Get("Site.Geological formation"),
                    GeoMember = Get("Site.Geological member"),
                    GeoBed = Get("Site.Geological bed"),
                    GeoName = Get("Site.Geological name"),
                    GeoAgeBottom = Get("Site.Geological age bottom"),
                    GeoAgeTop = Get("Site.Geological age top"),
                    GeoNotes = Get("Site.Geological notes")
                };
                var siteID = Service.ImportSite(site);
                _lastSite = new CachedSite { Name = site.SiteName, Locality = site.Locality, OffsetDistance = site.DistanceFromPlace, OffsetDirection = site.DirFromPlace, InformalLocality = site.InformalLocal, X1 = site.PosX1, Y1 = site.PosY1, X2 = site.PosX2, Y2 = site.PosY2, LocalityType = iLocalType, SiteID = siteID };
                return siteID;
            }
        }

        protected void Error(string format, params object[] args) {
            if (LogFunc != null) {
                if (args.Length == 0) {
                    LogFunc(ImportStatusLevel.Error, format);
                } else {
                    LogFunc(ImportStatusLevel.Error, string.Format(format, args));
                }
            }            
        }

        //private void Warning(string format, params object[] args) {
        //    if (LogFunc != null) {
        //        if (args.Length == 0) {
        //            LogFunc(ImportStatusLevel.Warning, format);
        //        } else {
        //            LogFunc(ImportStatusLevel.Warning, string.Format(format, args));
        //        }
        //    }
        //}

        private bool? GetBool(string field, bool? @default = null) {
            var str = Get(field);
            if (!string.IsNullOrWhiteSpace(str)) {
                bool val = false;
                if (Boolean.TryParse(str, out val)) {
                    return val;
                }
                throw new Exception(string.Format("Expected a boolean value for field {0}, got {1}", field, str));
            }
            return @default;
        }

        private int? GetInt(string field, int? @default = null) {
            var str = Get(field);
            if (!string.IsNullOrWhiteSpace(str)) {
                int val = 0;
                if (Int32.TryParse(str, out val)) {
                    return val;
                }
                throw new Exception(string.Format("Expected an int value for field {0}, got {1}", field, str));
            }
            return @default;
        }

        private double? GetDouble(string field, double? @default = null) {
            var str = Get(field);
            if (!string.IsNullOrWhiteSpace(str)) {
                double val = 0;
                if (double.TryParse(str, out val)) {
                    return val;
                }
                throw new Exception(string.Format("Expected a double value for field {0}, got {1}", field, str));
            }
            return @default;
        }

        private bool GetSiteCoordinates(out int iCoordinateType, ref double? x1, ref double? y1, ref double? x2, ref double? y2) {

            var bRet = false;
            iCoordinateType = 0;

            iCoordinateType = Int32.Parse(Get("Site.Coordinate type", "-1"));
            if (iCoordinateType < 0) {
                iCoordinateType = GuessCoordinateType();
            }

            bool bBothPoints = GetPositionType() > 1;
            switch (iCoordinateType) {
                case 0:
                    x1 = null;
                    y1 = null;
                    x2 = null;
                    x2 = null;
                    bRet = true;
                    break;
                case 1:
                    bRet = ValidateLatLong(bBothPoints, out x1, out y1, out x2, out y2);
                    break;
                case 2:
                    bRet = ValidateUTM(bBothPoints, out x1, out y1, out x2, out y2);
                    break;
                default:
                    throw new Exception("Unrecognized coordinate type: " + iCoordinateType);
            }

            return bRet;

        }

        private int GuessCoordinateType() {
            var X = Get("Site.Longitude", null);
            var Y = Get("Site.Latitude", null);
            if (X == null || Y == null) {
                return 0;
            } else {
                if (Get("Site.UTM zone number") != "" && Get("Site.UTM ellipsoid") != "") {
                    return UTM_COORDINATE;
                } else {
                    
                    if (X.IsNumeric() && Y.IsNumeric()) {
                        var dblX = double.Parse(X);
                        var dblY = double.Parse(Y);
                        if ((dblX < -180 || dblX > 180) || (dblY < -90 || dblY > 90)) {
                            throw new Exception("No UTM Zone and/or Ellipsoid provided, but X and Y coordinates outside Lat./Long. range.");
                        } else {
                            return LATLON_COORDINATE;
                        }
                    } else {
                        return LATLON_COORDINATE;
                    }
                }
            }
        }

        private bool ValidateLatLong(bool twoPoints, out double? x1, out double? y1, out double? x2, out double? y2) {
            var X = GetCoordinate("Site.Longitude", null);
            var Y = GetCoordinate("Site.Latitude", null);

            x1 = null;
            x2 = null;
            y1 = null;
            y2 = null;

            if (X != null && X.HasValue && Y != null && Y.HasValue) {
                x1 = X.Value;
                y1 = Y.Value;
                if (twoPoints) {
                    x2 = GetCoordinate("Site.Longitude 2", null);
                    y2 = GetCoordinate("Site.Latitude 2", null);
                }
            } else {
                double dblX, dblY;
                string message;
                var ok = GeoUtils.DMSStrToDecDeg(Get("Site.Longitude"), CoordinateType.Longitude, out dblX, out message);
                ok &= GeoUtils.DMSStrToDecDeg(Get("Site.Latitude"), CoordinateType.Latitude, out dblY, out message);
                if (ok) {
                    x1 = dblX;
                    y1 = dblY;
                    if (twoPoints) {
                        ok = GeoUtils.DMSStrToDecDeg(Get("Site.Longitude 2"), CoordinateType.Longitude, out dblX, out message);
                        ok &= GeoUtils.DMSStrToDecDeg(Get("Site.Latitude 2"), CoordinateType.Latitude, out dblY, out message);
                        if (ok) {
                            x2 = dblX;
                            y2 = dblY;
                        }
                    }
                }
                return ok;
            }

            return true;        
        }

        private bool ValidateUTM(bool both, out double? x1, out double? y1, out double? x2, out double? y2) {
            x1 = null;
            y1 = null;
            x2 = null;
            y2 = null;
    
            var vEasting = GetDouble("Site.Longitude", null);
            var vNorthing = GetDouble("Site.Latitude", null);

            if (vEasting == null || !vEasting.HasValue || vNorthing == null || !vEasting.HasValue) {
                throw new Exception("Easting and/or Northing data is not numeric!");
            }

            var strZone = Get("Site.UTM zone number", "");
            if (string.IsNullOrWhiteSpace(strZone)) {
                throw new Exception("A zone letter must be provided for UTM coordinates");
            }
    
            var ellipsoidName = Get("Site.UTM ellipsoid");
            
            if (ellipsoidName.IsNumeric()) {
            }
            var lngEllipsoid = GeoUtils.GetEllipsoidIndex(ellipsoidName);

            if (lngEllipsoid < 0) {
                throw new Exception("Unrecognized ellipsoid name: " + ellipsoidName);
            }

            double dblX, dblY;
            GeoUtils.UTMToLatLong(GeoUtils.ELLIPSOIDS[lngEllipsoid], vEasting.Value, vNorthing.Value, strZone, out dblX, out dblY);
            x1 = dblX;
            y1 = dblY;
            if (both) {
                vEasting = GetDouble("Site.Longitude 2", null);
                vNorthing = GetDouble("Site.Latitude 2", null);

                if (vEasting == null || !vEasting.HasValue || vNorthing == null || !vEasting.HasValue) {
                    throw new Exception("Second Easting and/or Northing data is not numeric!");
                }
                GeoUtils.UTMToLatLong(GeoUtils.ELLIPSOIDS[lngEllipsoid], vEasting.Value, vNorthing.Value, strZone, out dblX, out dblY);
                x2 = dblX;
                y2 = dblY;
            }

            return true;
        }

        public int GetElevationType() {
            int elevType = ALTITUDE_ELEVATION;
            if (Get("Site.Elevation depth", "") == "") {
                var upper = Get("Site.Elevation upper");
                if (!String.IsNullOrWhiteSpace(upper)) {
                    double result;
                    if (double.TryParse(upper, out result)) {
                        if (result > 0) {
                            elevType = ALTITUDE_ELEVATION;
                        } else {
                            elevType = DEPTH_ELEVATION;
                        }
                    }
                }
            } else {
                elevType = DEPTH_ELEVATION;
            }

            return elevType;
        }

        private int GetPositionType() {
            var iPosType = Int32.Parse(Get("Site.Position area type", "-1"));
            if (iPosType != -1) {
                if (iPosType != POINT_POSITION && iPosType != LINE_POSITION && iPosType != BOX_POSITION) {
                    throw new Exception("Unrecognised coordinate type: " + iPosType);
                }
            } else {
                if (Get("Site.Latitude 2", null) == null) {
                    iPosType = POINT_POSITION;
                } else {
                    iPosType = LINE_POSITION;
                }
            }

            return iPosType;
        }

        private int GetSiteVisitNumber(int siteId) {
            var iDateType = FIXED_DATE;
            var strSiteVisitName = Get("SiteVisit.Visit Name");
            var strCollector = Get("SiteVisit.Collector(s)");
            var strDateStart = Get("SiteVisit.Start Date");
            if (string.IsNullOrWhiteSpace(strDateStart)) {
                strDateStart = "0";
            }
            var strDateEnd = Get("SiteVisit.End Date");
            if (string.IsNullOrWhiteSpace(strDateEnd)) {
                strDateEnd = "0";
            }

            var timeStart = GetInt("SiteVisit.Start Time", null);
            var timeEnd = GetInt("SiteVisit.End Time");
            var strCasualDate = Get("SiteVisit.Casual time");
            var strFieldNumber = Get("SiteVisit.Field number");

            if (strDateStart != "0") {
                string errMsg;
                if (DateUtils.IsValidBLDate(strDateStart, out errMsg)) {
                    strDateStart.PadRight(8, '0');
                } else {
                    if (Information.IsDate(strDateStart)) {
                        strDateStart = DateUtils.DateStrToBLDate(strDateStart);
                        iDateType = FIXED_DATE;
                    } else {
                        iDateType = CASUAL_DATE;
                    }
                }
            }

            if (strDateEnd != "0") {
                string errMsg;
                if (DateUtils.IsValidBLDate(strDateEnd, out errMsg)) {
                    strDateEnd.PadRight(8, '0');
                } else {
                    if (Information.IsDate(strDateEnd)) {
                        strDateEnd = DateUtils.DateStrToBLDate(strDateEnd);
                        iDateType = FIXED_DATE;
                    } else {
                        if (iDateType == FIXED_DATE) {
                            throw new Exception("Invalid end date value - start date is in fixed format, end date is not");
                        }                        
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(strSiteVisitName)) {
                if (strDateStart != "0") {
                    if (iDateType == FIXED_DATE) {
                        strSiteVisitName = strCollector + ", " + DateUtils.BLDateToStr(Int32.Parse(strDateStart));
                    } else {
                        strSiteVisitName = strCollector + ", " + strDateStart;
                    }
                } else {
                    if (strDateEnd != "0") {
                        if (iDateType == FIXED_DATE) {
                            strSiteVisitName = strCollector + ", " + DateUtils.BLDateToStr(Int32.Parse(strDateEnd));
                        } else {
                            strSiteVisitName = strCollector + ", " + strDateEnd;
                        }
                    } else {
                        strSiteVisitName = strCollector;
                    }
                }
            }

            if (_lastSiteVisit != null && _lastSiteVisit.Equals(siteId, strSiteVisitName, strCollector, strDateStart, strDateEnd, timeStart, timeEnd, strFieldNumber)) {
                return _lastSiteVisit.SiteVisitID;
            } else {
                int? dateStart = null;
                int? dateEnd = null;
                if (strDateStart.IsNumeric()) {
                    dateStart = Int32.Parse(strDateStart);
                }
                if (strDateEnd.IsNumeric()) {
                    dateEnd = Int32.Parse(strDateEnd);
                }

                var visit = new SiteVisit {
                    SiteVisitName = strSiteVisitName,
                    SiteID = siteId,
                    FieldNumber = strFieldNumber,
                    Collector = strCollector,
                    DateType = iDateType,
                    DateStart = dateStart,
                    DateEnd = dateEnd,
                    TimeStart = timeStart,
                    TimeEnd = timeEnd,
                    CasualTime = strCasualDate
                };

                var siteVisitID = Service.ImportSiteVisit(visit);
                _lastSiteVisit = new CachedSiteVisit { SiteVisitID = siteVisitID, SiteID = siteId, Collector = strCollector, FieldNumber = strFieldNumber, SiteVisitName = strSiteVisitName, DateEnd = strDateEnd, DateStart = strDateEnd, TimeEnd = timeEnd, TimeStart = timeStart };
                return siteVisitID;
            }
        }

        private string BuildDate(string date) {

            if (date.Length < 0) {
                date = date.PadRight(8, '0');
            }

            if (string.IsNullOrWhiteSpace(date)) {
                return "";
            }

            if (date.IsNumeric()) {
                return DateUtils.BLDateToStr(Int32.Parse(date));
            }

            return "";
        }

        private int GetTaxonNumber() {

            var strLowestRank = LowestTaxonLevel();

            var taxon = BuildTaxonCacheRecord();
            int elementID = -1;
            if (!_taxonCache.FindInCache(taxon, out elementID)) {
                elementID = 0;
                foreach (TaxonRankName name in _ranks) {
                    elementID = AddElementID(elementID, name, strLowestRank);
                }
                taxon.TaxonID = elementID;
                _taxonCache.AddToCache(taxon);
            }

            return elementID;
        }

        private int AddElementID(int parentID, TaxonRankName rank, string lowestRank) {
            var strEpithet = Get("Taxon." + rank.LongName);
            int elementID = 0;

            if (!string.IsNullOrWhiteSpace(strEpithet)) {
                string strAuthor, strYear, strKingdomType, strNameStatus;
                bool unverified, changeCombination;
                if (!GetAuthority(lowestRank, rank, out strAuthor, out strYear, out changeCombination, out unverified, out strKingdomType, out strNameStatus)) {
                    return -1;
                }

                if (rank.LongName == "Kingdom") {
                    parentID = 0;
                }

                elementID = Service.ImportTaxon(parentID, strEpithet, strAuthor, strYear, changeCombination, rank.Code, false, "", strKingdomType, 0, unverified, strNameStatus);

            } else {
                elementID = parentID;
            }

            return elementID;
        }

        private bool GetAuthority(string level, TaxonRankName rank, out string strAuthor, out string strYear, out bool changeCombination, out bool unverified, out string strKingdomType, out string strNameStatus) {
            if (level == rank.LongName) {
                unverified = GetBool("Taxon.Verified", true).Value;
                strAuthor = Get("Taxon.Author");
                strYear = Get("Taxon.Year");
                changeCombination = GetBool("Taxon.Changed Combination", false).Value;
                strKingdomType = Get("Taxon.KingdomType");
                strNameStatus = Get("Taxon.Name Status");
            } else {
                unverified = false;
                strAuthor = null;
                strYear = null;
                changeCombination = false;
                strKingdomType = null;
                strNameStatus = null;
            }

            return true;
        }

        private CachedTaxon BuildTaxonCacheRecord() {
            var list = new List<TaxonRankValue>();
            foreach (TaxonRankName rank in _ranks) {
                list.Add(new TaxonRankValue(rank, Get("Taxon." + rank.LongName)));
            }
            return new CachedTaxon(list);
        }

        private bool DataHasRank(String rankname) {
            var candidate = Get("Taxon." + rankname);
            return !string.IsNullOrWhiteSpace(candidate);
        }

        private String LowestTaxonLevel() {

            for (int i = _ranks.Count - 1; i >= 0; --i) {
                var rank = _ranks[i];
                if (DataHasRank(rank.LongName)) {
                    return rank.LongName;
                }
            }

            return null;
        }

        private void InsertCommonName(int taxonId) {
            var sCommonName = Get("Taxon.Common Name");
            if (!string.IsNullOrWhiteSpace(sCommonName)) {
                Service.ImportCommonName(taxonId, sCommonName);
            }
        }

        private int AddMaterial(int siteVisitId, int taxonId) {
            var sparcService = new MaterialService(User);

            var sMaterialName = Get("Material.Material name");
            var sAccessionNumber = Get("Material.Accession number");
            var sRegistrationNumber = Get("Material.Registration number");
            var sCollectorNumber = Get("Material.Collector number");
            var sIDBy = Get("Material.Identified by");
            var sIDOn = Get("Material.Identified on");

            if (!string.IsNullOrWhiteSpace(sIDOn)) {
                string message;
                if (DateUtils.IsValidBLDate(sIDOn, out message)) {
                    sIDOn = string.Format("{0:dd MMM yyyy}", DateUtils.MakeCompatibleBLDate(Int32.Parse(sIDOn)));
                } else {
                    if (!Information.IsDate(sIDOn)) {
                        throw new Exception("'Identified on' value (" + sIDOn + ") is not a valid date time! " + message);
                    }                    
                }
            }

            var sIDRef = Get("Material.Identification reference");
            var sIDRefPage = Get("Material.Identification reference page");
            var sIDMethod = Get("Material.Identification method");
            var sIDAccuracy = Get("Material.Identification accuracy");
            var sIDNameQualifier = Get("Material.Name qualifier");
            var sIDNotes = Get("Material.Identification notes");
            var sInstitution = Get("Material.Institute");
            var sCollectMethod = Get("Material.Collection method");
            var sAbundance = Get("Material.Abundance");
            var sMacroHabitat = Get("Material.Macrohabitat");
            var sMicrohabitat = Get("Material.Microhabitat");
            var sSource = Get("Material.Source");
            var sSpecialLabel = Get("Material.Special label");
    
            var bCopyLabel = GetBool("bOriginalLabel", false);
            var sDateStart = Get("SiteVisit.Start Date", "0");

            string sOriginalLabel = "";

            if (bCopyLabel.ValueOrFalse()) {
                if (sDateStart != "0") {
                    string message;
                    if (DateUtils.IsValidBLDate(sDateStart, out message)) {
                        sDateStart = BuildDate(sDateStart);
                    }
                }
                sOriginalLabel = "Import derived: " + Get("Site.Locality") + "; " + Get("SiteVisit.Collector(s)") + "; " + sDateStart;
            } else {
                sOriginalLabel = Get("Material.Original label");
            }

            if (string.IsNullOrWhiteSpace(sMaterialName)) {
                if (!string.IsNullOrWhiteSpace(sInstitution)) {
                    if (!string.IsNullOrWhiteSpace(sAccessionNumber)) {
                        sMaterialName = string.Format("{0}:{1}", sInstitution, sAccessionNumber);
                    } else {
                        if (!string.IsNullOrWhiteSpace(sRegistrationNumber)) {
                            sMaterialName = string.Format("{0}:{1}", sInstitution, sRegistrationNumber);
                        }
                    }
                } else {
                    if (!string.IsNullOrWhiteSpace(sAccessionNumber)) {
                        sMaterialName = sAccessionNumber;
                    } else {
                        if (!string.IsNullOrWhiteSpace(sRegistrationNumber)) {
                            sMaterialName = sRegistrationNumber;
                        }
                    }
                }
            }
            var material = new Material {
                MaterialName = sMaterialName,
                SiteVisitID = siteVisitId,
                AccessionNumber = sAccessionNumber,
                RegistrationNumber = sRegistrationNumber,
                CollectorNumber = sCollectorNumber,
                BiotaID = taxonId,
                IdentifiedBy = sIDBy,
                IdentificationDate = (string.IsNullOrWhiteSpace(sIDOn) ? null : new DateTime?(DateAndTime.DateValue(sIDOn))),
                IdentificationReferenceID = (!string.IsNullOrWhiteSpace(sIDRef) && sIDRef.IsNumeric()) ? Int32.Parse(sIDRef) : 0,
                IdentificationRefPage = sIDRefPage,
                IdentificationMethod = sIDMethod,
                IdentificationAccuracy = sIDAccuracy,
                IdentificationNameQualification = sIDNameQualifier,
                IdentificationNotes = sIDNotes,
                Institution = sInstitution,
                CollectionMethod = sCollectMethod,
                Abundance = sAbundance,
                MacroHabitat = sMacroHabitat,
                MicroHabitat = sMicrohabitat,
                Source = sSource,
                SpecialLabel = sSpecialLabel,
                OriginalLabel = sOriginalLabel
            };

            return Service.ImportMaterial(material);
        }

        private int AddMaterialPart(int materialId) {

            var numberOfSpecimens = GetInt("Material.Number of specimens");
            if (numberOfSpecimens == 0) {
                numberOfSpecimens = 1;
            }

            var part = new MaterialPart {
                MaterialID = materialId,
                PartName = Get("Material.Part name"), 
                SampleType = Get("Material.Sample type"),
                NoSpecimens = numberOfSpecimens,
                NoSpecimensQual = Get("Material.Number of specimens qualifier"), 
                Lifestage = Get("Material.Life stage"),
                Gender =  Get("Material.Gender"), 
                RegNo = Get("Material.Part registration number"),
                Condition = Get("Material.Condition"), 
                StorageSite = Get("Material.Storage site"),
                StorageMethod = Get("Material.Storage method"), 
                CurationStatus = Get("Material.Curation status"),
                Notes = Get("Material.Notes")
            };

            var matService = new MaterialService(User);

            return matService.InsertMaterialPart(part);
        }

        private ImportLevel GetLevel(IEnumerable<ImportFieldMapping> mappings) {
            bool bRegion = false;
            bool bSite = false;
            bool bVisit = false;
            bool bMaterial = false;
            bool bTaxa = false;

            foreach (ImportFieldMapping mapping in mappings) {
                if (!string.IsNullOrWhiteSpace(mapping.TargetColumn)) {
                    var category = mapping.TargetColumn.Substring(0, mapping.TargetColumn.IndexOf('.'));
                    switch (category) {
                        case "Region":
                            bRegion = true;
                            break;
                        case "Site":
                            bSite = true;
                            break;
                        case "SiteVisit":
                            bVisit = true;
                            break;
                        case "Material":
                            bMaterial = true;
                            break;
                        case "Taxon":
                            bTaxa = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!bRegion && !bSite && !bVisit && !bMaterial && !bTaxa) {
                throw new Exception("Row has no recognisable data mapped!");
            }

            if (bMaterial) {
                if (bTaxa) {
                    return ImportLevel.MaterialWithTaxa;
                } else {
                    return ImportLevel.MaterialWithoutTaxa;
                }
            }

            if (bTaxa) {
                if (bRegion || bSite || bVisit) {
                    return ImportLevel.MaterialWithTaxa;
                } else {
                    return ImportLevel.TaxaOnly;
                }
            }

            ImportLevel ret = ImportLevel.Error;

            if (bRegion) {
                ret = ImportLevel.Region;
            }

            if (bSite) {
                ret = ImportLevel.Site;
            }

            if (bVisit) {
                ret = ImportLevel.Visit;
            }

            return ret;
        }

        private bool DoStage1() {

            LogMsg("Stage 1 - preparing staging database");
            LogMsg("Stage 1 - Preprocessing rows...");
            this.RowSource = Importer.CreateRowSource();
            LogMsg("Stage 1 Complete, {0} rows staged for import.", RowSource.RowCount);

            return true;
        }

        private bool InitImport() {
            LogMsg("Initializing...");

            LogMsg("Caching rank data...");
            var taxonService = new TaxaService(User);
            _ranks = taxonService.GetOrderedRanks();
            LogMsg("Initialisation complete");

            return true;
        }

        private void CreateColumnIndexes() {

            LogMsg("Caching column mappings...");

            _fieldIndex.Clear();

            foreach (ImportFieldMapping mapping in Mappings) {

                string target = mapping.TargetColumn.ToLower();
                int index = -1;

                for (int i = 0; i < RowSource.RowCount - 1; ++i) {
                    if (RowSource.ColumnName(i).Equals(mapping.SourceColumn, StringComparison.CurrentCultureIgnoreCase)) {
                        index = i;
                        break;
                    }
                }
                if (index >= 0) {
                    _fieldIndex[target] = index;
                    if (!string.IsNullOrWhiteSpace(mapping.TargetColumn)) {
                        LogMsg("{0} mapped to {1} (index {2})", mapping.TargetColumn, mapping.SourceColumn, index);
                    } else {
                        LogMsg("Source column {1} (index {2}) is not mapped to a target column.", mapping.TargetColumn, mapping.SourceColumn, index);
                    }
                }
            }

        }

        private void ProgressMsg(string message, double? percent = null) {
            if (Progress != null) {
                Progress.ProgressMessage(message, percent);
            }
        }

        private void LogMsg(String format, params object[] args) {
            if (LogFunc != null) {
                if (args.Length > 0) {
                    LogFunc(ImportStatusLevel.Info, string.Format(format, args));
                } else {
                    LogFunc(ImportStatusLevel.Info, format);
                }
            }
        }

        protected ImportService Service { get; private set; }

        protected IProgressObserver Progress { get; private set; }

        protected Action<ImportStatusLevel, string> LogFunc { get; private set; }

        protected TabularDataImporter Importer { get; private set; }

        protected IEnumerable<ImportFieldMapping> Mappings { get; private set; }

        protected ImportRowSource RowSource { get; private set; }

        protected User User { get; private set; }

        public bool Cancelled { get; set; }

    }

    enum ImportLevel {
        Error,
        Region,
        Site,
        Visit,
        MaterialWithTaxa,
        MaterialWithoutTaxa,
        TaxaOnly
    }

}
