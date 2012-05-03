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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using Microsoft.VisualBasic;

namespace BioLink.Client.Tools {

    public class MaterialImportProcessor : ImportProcessor {

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
        private CachedRegion _lastRegion;
        private CachedSite _lastSite;
        private CachedSiteVisit _lastSiteVisit;
        private TaxonCache _taxonCache = new TaxonCache();

        protected override void InitImportImpl() {
            LogMsg("Caching rank data...");
            var taxonService = new TaxaService(User);
            _ranks = taxonService.GetOrderedRanks();
        }

        protected override void ImportRowImpl(int rowId, System.Data.Common.DbConnection connection) {

            int regionNumber = -1;
            int siteNumber = -1;
            int siteVisitNumber = -1;
            int taxonNumber = -1;
            int materialNumber = -1;
            int materialPartNumber = -1;

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

        private int GetSiteNumber(int regionId) {

            var strLocal = Get("Site.Locality");
            var strOffSetDis = Get("Site.Distance from place");
            var strOffsetDir = Get("Site.Direction from place");
            var strInformal = Get("Site.Informal Locality");

            double? x1 = GetConvert<double?>("Site.Longitude", null);
            double? y1 = GetConvert<double?>("Site.Latitude", null);
            double? x2 = GetConvert<double?>("Site.Longitude 2", null);
            double? y2 = GetConvert<double?>("Site.Latitude 2", null);

            var iPosAreaType = GetPositionType();
            int iLocalType = 0; // TODO? Can we workout what this is supposed to be?
            var iElevationType = GetElevationType();

            int iCoordinateType;

            GetSiteCoordinates(out iCoordinateType, ref x1, ref y1, ref x2, ref y2);

            var strName = Get("Site.Site Name");
            if (string.IsNullOrWhiteSpace(strName)) {
                strName = strLocal;
            }

            double? elevUpper = null;
            double? elevLower = null;
            double? elevDepth = null;
            string elevUnits = null;

            var elevResult = GetConvert<UnitRange>("Site.Elevation upper");
            if (elevResult != null) {
                switch (elevResult.RangeType) {
                    case UnitRangeType.SingleValueNoUnits:
                        elevUpper = elevResult.Upper;
                        break;
                    case UnitRangeType.SingleValueWithUnits:
                        elevUpper = elevResult.Upper;
                        elevUnits = elevResult.Units;
                        break;
                    case UnitRangeType.RangeNoUnits:
                        elevUpper = Math.Max(elevResult.Upper, elevResult.Lower);
                        elevLower = Math.Min(elevResult.Upper, elevResult.Lower); ;
                        break;
                    case UnitRangeType.RangeWithUnits:
                        elevUpper = Math.Max(elevResult.Upper, elevResult.Lower);
                        elevLower = Math.Min(elevResult.Upper, elevResult.Lower); ;
                        elevUnits = elevResult.Units;
                        break;
                }
            }

            elevResult = GetConvert<UnitRange>("Site.Elevation lower");
            if (elevResult != null) {
                switch (elevResult.RangeType) {
                    case UnitRangeType.SingleValueNoUnits:
                        elevLower = elevResult.Upper;
                        break;
                    case UnitRangeType.SingleValueWithUnits:
                        elevLower = elevResult.Upper;
                        elevUnits = elevResult.Units;
                        break;
                    case UnitRangeType.RangeNoUnits:
                        elevUpper = Math.Max(elevResult.Upper, elevResult.Lower);
                        elevLower = Math.Min(elevResult.Upper, elevResult.Lower); ;
                        break;
                    case UnitRangeType.RangeWithUnits:
                        elevUpper = Math.Max(elevResult.Upper, elevResult.Lower);
                        elevLower = Math.Min(elevResult.Upper, elevResult.Lower); ;
                        elevUnits = elevResult.Units;
                        break;
                }
            }

            var depthResult = GetConvert<UnitRange>("Site.Elevation depth");
            if (depthResult != null) {
                switch (depthResult.RangeType) {
                    case UnitRangeType.SingleValueNoUnits:
                        elevDepth = depthResult.Upper;
                        break;
                    case UnitRangeType.SingleValueWithUnits:
                        elevDepth = depthResult.Upper;
                        elevUnits = depthResult.Units;
                        break;
                    case UnitRangeType.RangeNoUnits:
                        elevUpper = Math.Min(depthResult.Upper, depthResult.Lower);
                        elevLower = Math.Max(depthResult.Upper, depthResult.Lower);
                        break;
                    case UnitRangeType.RangeWithUnits:
                        elevUpper = Math.Min(depthResult.Upper, depthResult.Lower);
                        elevLower = Math.Max(depthResult.Upper, depthResult.Lower);
                        elevUnits = depthResult.Units;
                        break;
                }
            }

            // An explicit units mapping will override an implicit one inferred from a range, but if not, keep the inferred one, if any
            elevUnits = Get("Site.Elevation units", elevUnits);

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
                    ElevType = iElevationType,
                    ElevUpper = elevUpper,
                    ElevLower = elevLower,
                    ElevDepth = elevDepth,
                    ElevUnits = elevUnits,
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
            var X = GetConvert<double?>("Site.Longitude", null);
            var Y = GetConvert<double?>("Site.Latitude", null);

            x1 = null;
            x2 = null;
            y1 = null;
            y2 = null;

            if (X != null && X.HasValue && Y != null && Y.HasValue) {
                x1 = X.Value;
                y1 = Y.Value;
                if (twoPoints) {
                    x2 = GetConvert<double?>("Site.Longitude 2", null);
                    y2 = GetConvert<double?>("Site.Latitude 2", null);
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

            var strZone = Get("Site.UTM zone", "");
            if (string.IsNullOrWhiteSpace(strZone)) {
                throw new Exception("A grid zone must be provided for UTM coordinates (UTM zone + latitude band letter)");
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

            var strCasualDate = Get("SiteVisit.Casual time");

            int? startDate = GetConvert<int?>("SiteVisit.Start Date", null, false);
            if (!startDate.HasValue) {
                iDateType = CASUAL_DATE;
            } else {
                strCasualDate = Get("SiteVisit.Start Date", strCasualDate);
            }

            int? endDate = GetConvert<int?>("SiteVisit.End Date", null);
            if (!endDate.HasValue) {
                var sEndDate = Get("SiteVisit.End Date");
                if (!string.IsNullOrWhiteSpace(sEndDate)) {
                    if (iDateType == FIXED_DATE) {
                        throw new Exception("Invalid end date value - start date is in fixed format, end date is not");
                    } else {
                        if (string.IsNullOrWhiteSpace(strCasualDate)) {
                            strCasualDate = sEndDate;
                        }
                    }
                }
            }

            var timeStart = GetConvert<int?>("SiteVisit.Start Time", null);
            var timeEnd = GetConvert<int?>("SiteVisit.End Time");

            var strFieldNumber = Get("SiteVisit.Field number");

            if (string.IsNullOrWhiteSpace(strSiteVisitName)) {
                if (startDate.HasValue && startDate.Value > 0) {
                    if (iDateType == FIXED_DATE) {
                        strSiteVisitName = strCollector + ", " + DateUtils.BLDateToStr(startDate.Value);
                    } else {
                        strSiteVisitName = strCollector + ", " + strCasualDate;
                    }
                } else {
                    if (endDate.HasValue && endDate.Value > 0) {
                        if (iDateType == FIXED_DATE) {
                            strSiteVisitName = strCollector + ", " + DateUtils.BLDateToStr(endDate.Value);
                        } else {
                            strSiteVisitName = strCollector + ", " + strCasualDate;
                        }
                    } else {
                        strSiteVisitName = strCollector;
                    }
                }
            }

            if (_lastSiteVisit != null && _lastSiteVisit.Equals(siteId, strSiteVisitName, strCollector, startDate, endDate, timeStart, timeEnd, strFieldNumber, strCasualDate)) {
                return _lastSiteVisit.SiteVisitID;
            } else {

                var visit = new SiteVisit {
                    SiteVisitName = strSiteVisitName,
                    SiteID = siteId,
                    FieldNumber = strFieldNumber,
                    Collector = strCollector,
                    DateType = iDateType,
                    DateStart = startDate,
                    DateEnd = endDate,
                    TimeStart = timeStart,
                    TimeEnd = timeEnd,
                    CasualTime = strCasualDate
                };

                var siteVisitID = Service.ImportSiteVisit(visit);
                _lastSiteVisit = new CachedSiteVisit { SiteVisitID = siteVisitID, SiteID = siteId, Collector = strCollector, FieldNumber = strFieldNumber, SiteVisitName = strSiteVisitName, DateEnd = endDate, DateStart = startDate, TimeEnd = timeEnd, TimeStart = timeStart, CasualDate = strCasualDate };
                return siteVisitID;
            }
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
                unverified = GetConvert<bool>("Taxon.Verified", true);
                strAuthor = Get("Taxon.Author");
                strYear = Get("Taxon.Year");
                changeCombination = GetConvert<bool>("Taxon.Changed Combination", false);
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

            var bCreateLabel = GetConvert<bool>("Material.Create Label", false);
            var sDateStart = Get("SiteVisit.Start Date", "0");

            string sOriginalLabel = "";

            if (bCreateLabel) {
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

            var numberOfSpecimens = GetConvert<int?>("Material.Number of specimens");
            if (!numberOfSpecimens.HasValue || numberOfSpecimens.Value == 0) {
                numberOfSpecimens = 1;
            }

            var part = new MaterialPart {
                MaterialID = materialId,
                PartName = Get("Material.Part name"),
                SampleType = Get("Material.Sample type"),
                NoSpecimens = numberOfSpecimens,
                NoSpecimensQual = Get("Material.Number of specimens qualifier"),
                Lifestage = Get("Material.Life stage"),
                Gender = Get("Material.Gender"),
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
