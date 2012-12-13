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
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using System.Text.RegularExpressions;
using BioLink.Client.Utilities;
using System.Data;

namespace BioLink.Data {

    public class ImportService : BioLinkService {

        private static Regex _UTMZoneRegex = new Regex(@"^(\d+)([A-HJ-NP-Za-hj-np-z]{1})$");
        private static Regex _UnitRangeSingleUnitsRegex = new Regex(@"^([\d\.]+)\s*([^\d]+)$");
        private static Regex _UnitRangeRegex = new Regex(@"^([\d\.]+)\s*[-]\s*([\d\.]+)\s*([^\d]*)$");
        private static Regex _TimeRegex = new Regex(@"^(\d\d)[:]*(\d\d)$");

        private List<FieldDescriptor> _importFieldsCache = null;

        public static List<FieldDescriptor> ImportFieldDescriptors = new List<FieldDescriptor>();

        public static List<FieldDescriptor> ImportReferencesDescriptors = new List<FieldDescriptor>();

        static ImportService() {

            #region Import Field Descriptors
            // Import field descriptors
            #region Region
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Region", Category = "Region", Description = "Region the locality falls in", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Country", Category = "Region", Description = "Country the locality falls in", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "State/Province", Category = "Region", Description = "State/Province the locality falls in", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "County", Category = "Region", Description = "County the locality falls in", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "ExistingRegionID", Category = "Region", Description = "Internal database id for an existing region (advanced)", Validate = IntegerValidator });
            #endregion
            #region Site
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Site Name", Category = "Site", Description = "Identifier (name or code) for this site/station assigned by the collector (need not be unique)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Locality", Category = "Site", Description = "Place name and optional offset where the material was found", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Distance from place", Category = "Site", Description = "Distance (with units) from named place (e.g. 10km) (used in conjunction with vchrLocal and vchrDirFromPlace).", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Direction from place", Category = "Site", Description = "Direction from named place (e.g. S) (used in conjunction with vchrLocal and vchrDistanceFromPlace).", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Informal locality", Category = "Site", Description = "Description based on a non-named place or based on features which are only meaningful in a local context (e.g. 'near the small hill just south of the river crossing')", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position area type", Category = "Site", Description = "Shape of the area defined by Coordinates (1 = Point/Circle, 2 = Line, 3 = Rectangle)", Validate = PositionAreaTypeValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Coordinate type", Category = "Site", Description = "Integer representing the coordinate data format, Latitude and longitude = 1, UTM = 2", Validate = CoordinateTypeValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Latitude", Category = "Site", Description = "Latitude (in decimal degrees, positive = N, negative = S) of point, N or W end of Line, NW corner of Rectangle", Validate = CoordinateValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Longitude", Category = "Site", Description = "Longitude (in decimal degrees, positive = E, negative = W) of point, N or W end of Line, NW corner of Rectangle", Validate = CoordinateValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Latitude 2", Category = "Site", Description = "Latitude (in decimal degrees, positive = N, negative = S) of S or E end of Line, SE corner of Rectangle", Validate = CoordinateValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Longitude 2", Category = "Site", Description = "Longitude (in decimal degrees, positive = E, negative = W) of S or E end of Line, SE corner of Rectangle", Validate = CoordinateValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position source", Category = "Site", Description = "Where or how the Latitude/Longitude coordinates were determined.  Can include sources such as map using label data, gazetteer using label data, GPS, collector, compiler.", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position error", Category = "Site", Description = "Error associated with Lat/Long in meters (1sec = 25m, 5sec = 100m, 10sec = 200m, 30sec = 500m, 1min = 1km, 5min = 5km, 10min = 10km, 1deg = 60km, 5deg = 300km, 10deg = 600km)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Generated by", Category = "Site", Description = "Name of person determining coordinates.", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Generated on", Category = "Site", Description = "Date the position data were generated", Validate= StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Original position", Category = "Site", Description = "...", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM ellipsoid", Category = "Site", Description = "Ellipsoid defining northings and eastings", Validate = EllipsoidIndexConverter });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM zone", Category = "Site", Description = "UTM zone and latitude band the coordinates are in (eg. 17T)", Validate = UTMZoneValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM source", Category = "Site", Description = "The projection used to convert between Latitude/Longitude and UTM/map grids.", Validate= StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map projection", Category = "Site", Description = "Map projection of map or GPS used when determining Coordinates", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map name", Category = "Site", Description = "Name of map used to determine Coordinates", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map version", Category = "Site", Description = "Version or date of map used to determine Coordinates", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation upper", Category = "Site", Description = "Capture or upper elevation (if elevation) or capture or upper water depth (if depth).", Validate = UnitRangeValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation lower", Category = "Site", Description = "Lower elevation or lower water depth.", Validate = UnitRangeValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation depth", Category = "Site", Description = "Water Depth if different from Capture Depth", Validate = UnitRangeValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation units", Category = "Site", Description = "Units used for elevation (i.e. m, ft.)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation source", Category = "Site", Description = "Where or how the elevation or depth was determined.  Can include sources such as map using label data, digital elevation model using label data, GPS, altimeter, collector, compiler, field estimate.", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation error", Category = "Site", Description = "Error associated with altitude/depth in meters", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological era", Category = "Site", Description = "Geologic Era", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological state", Category = "Site", Description = "...", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological plate", Category = "Site", Description = "Plate Tectonic Name", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological formation", Category = "Site", Description = "Lithostratigraphic Formation", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological member", Category = "Site", Description = "Lithostratigraphic Member", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological bed", Category = "Site", Description = "Lithostratigraphic Bed", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological name", Category = "Site", Description = "Stratigraphy Name", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological age bottom", Category = "Site", Description = "Stratigraphy Bottom Age", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological age top", Category = "Site", Description = "Stratigraphy Top Age", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological notes", Category = "Site", Description = "...", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Other", Category = "Site", Description = "Other user defined field", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Notes", Category = "Site", Description = "Other user defined text", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "ExistingSiteID", Category = "Site", Description = "Internal database id for an existing site (advanced)", Validate = IntegerValidator });
            #endregion
            #region Site Visit
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collector(s)", Category = "SiteVisit", Description = "Persons collecting the material", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Start Date", Category = "SiteVisit", Description = "Date the collection took place or began", Validate = BLDateValidator  });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Start Time", Category = "SiteVisit", Description = "Optional Time the collection took place or began", Validate = TimeValidator});
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "End Date", Category = "SiteVisit", Description = "Optional date the collection concluded", Validate = BLDateValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "End Time", Category = "SiteVisit", Description = "Optional time the collection concluded", Validate = TimeValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Visit Name", Category = "SiteVisit", Description = "Name given to site visit", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Field number", Category = "SiteVisit", Description = "Number assigned by collector(s) to material", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Casual Date", Category = "SiteVisit", Description = "A casual date for the site visit", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Other", Category = "SiteVisit", Description = "Other user defined field", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Notes", Category = "SiteVisit", Description = "Other user defined text", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "ExistingSiteVisitID", Category = "SiteVisit", Description = "Internal database id for an existing site visit (advanced)", Validate = IntegerValidator });
            #endregion
            #region Material
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Material name", Category = "Material", Description = "Descriptive name given to material", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Accession number", Category = "Material", Description = "An identification number assigned by the collection for this material.  This number will normally uniquely identify the material.", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Registration number", Category = "Material", Description = "Code assigned by collection owner", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collector number", Category = "Material", Description = "Code assigned by collector", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identified by", Category = "Material", Description = "Person providing identification", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identified on", Category = "Material", Description = "Date identification was made", Validate= BLDateValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification reference", Category = "Material", Description = "The publication in which this identification appears", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification reference page", Category = "Material", Description = "The page number in the publication in which this identification appears", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification method", Category = "Material", Description = "The method used to make the identification", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification accuracy", Category = "Material", Description = "Likelihood that ID is correct: (0) unchecked by any authority, (1) compared with other named specimens, (2) determined by authority based on existing classification or named material, (3) determined by authority during revision, (4) part of type series", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification name qualifier", Category = "Material", Description = "An indication that the cited name is uncertain.  Includes cf (compare with), near, incorrect (current name is incorrect but true name is unknown), ? (questionable)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification notes", Category = "Material", Description = "Assorted notes on this identification", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Institute", Category = "Material", Description = "Institute where material is held", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collection method", Category = "Material", Description = "Method used to collect sample or origin of sample (e.g. pitfall, Malaise trap, reared, cultivated, from wild)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Abundance", Category = "Material", Description = "Abundance or frequency of material", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Macrohabitat", Category = "Material", Description = "General description of habitat.  Can include vegetation, soil, landform, etc.", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Microhabitat", Category = "Material", Description = "Specific, small-scale habitat or situation of collection", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Source", Category = "Material", Description = "Source of this information (collection (specimen), electronic (specimen no longer available), literature (published record only), observation (unvouchered sighting), photograph (of existing specimen)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Special label", Category = "Material", Description = "Special text to appear on printed labels", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Original label", Category = "Material", Description = "Verbatim text of label(s) that originally appeared with specimen(s)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Create label", Category = "Material", Description = "Generate a 'original label' automatically", Validate = BooleanValidator});                
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Part name", Category = "Material", Description = "The name of the subpart.  This should describe the subpart type, for example wings, genitalia, skin, bark.", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Sample type", Category = "Material", Description = "Type of material in this sample (e.g. bulk sample, individual specimen, dissection, subpart name or type)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Number of specimens", Category = "Material", Description = "Number of individual specimens represented by the record.", Validate = IntegerValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Number of specimens qualifier", Category = "Material", Description = "Qualification of intNoSpecimens.  Possible values include exactly, about, at least, at most", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Life stage", Category = "Material", Description = "The life stage (e.g. larvae, immature, adult) or age (e.g. 20 days, 2 years) of the specimen(s)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Gender", Category = "Material", Description = "The gender or caste of the specimen(s)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Part registration number", Category = "Material", Description = "...", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Condition", Category = "Material", Description = "Condition of specimen (e.g. good, damaged, faded)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Storage site", Category = "Material", Description = "The location within collection where specimen(s) is stored", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Storage Method", Category = "Material", Description = "Location or type of storage used for specimen(s); can also indicate preparation method (e.g. on slide, pin, card, point, etc)", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Curation status", Category = "Material", Description = "Current curation status of this material", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Subpart Notes", Category = "Material", Description = "Notes for the material sub part", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Other", Category = "Material", Description = "Other user defined field", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Notes", Category = "Material", Description = "Other user defined text", Validate = StringValidator });            
            #endregion
            #region Taxon
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "KingdomType", Category = "Taxon", Description = "Defaults are P for Plantae, A for Animalia", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Author", Category = "Taxon", Description = "Author of the name", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Year", Category = "Taxon", Description = "Year of publication of the name" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Changed Combination", Category = "Taxon", Description = "Indicates that the species epithet is in other than the original combination", Validate = BooleanValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Verified", Category = "Taxon", Description = "Name needs checking, suspected of being no longer valid or is known to be unavailable, non zero = Verified, 0 = Unverified", Validate = BooleanValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Common Name", Category = "Taxon", Description = "Common name given to the taxa.", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Name Status", Category = "Taxon", Description = "The status of the available names.", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Other", Category = "Taxon", Description = "Other user defined field", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Notes", Category = "Taxon", Description = "Other user defined text", Validate = StringValidator });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "ExistingTaxonID", Category = "Taxon", Description = "Internal database id for an existing Taxon (advanced)", Validate = IntegerValidator });            
            #endregion
            // END Import Field Descriptors

            #endregion

            #region Import Reference Field Descriptors

            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "ReferenceCode", Description ="Code used to identify the reference", Validate = StringValidator});
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Author(s)", Description = "Authors of the reference", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Year of publication", Description = "The year in which the reference was published", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Article title", Description = "The title of the article", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Book title", Description = "The title of the book where the article was sourced", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Editor", Description = "The editor of the article", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Actual publication date", Description = "Date the article was actually published", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Journal name", Description = "The name of the journal from which the article was sourced", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Journal Abbreviated Name", Description = "Abbreviated name of the journal", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Part Number", Description = "Part Number of the journal", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Series Number", Description = "Series number of the journal", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Book Publisher", Description = "The publisher of the book", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Book Place of publication", Description = "Place where the book was published", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Volume", Description = "Volume number of the book", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Pages", Description = "Page numbers of the article", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Total pages", Description = "Total number of pages if reference is a book or journal section", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Edition", Description = "Edition number of the book", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "ISBN", Description = "ISBN number of the book", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "ISSN", Description = "ISSN number of the book", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Qualification", Description = "Any qualifier for the reference", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Qualification-date", Description = "Date qualifier for the reference", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Source", Description = "Where the referenced was sourced", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Possession", Description = "The form of the reference that I possess (e.g. Photocopy, orginal, etc..)", Validate = StringValidator });
            ImportReferencesDescriptors.Add(new FieldDescriptor { Category = "References", DisplayName = "Other", Description = "Any other field", Validate = StringValidator });

            #endregion
        }

        public ImportService(User user) : base(user) { }

        public List<FieldDescriptor> GetImportFields() {
            // Start off with the base list of static fields...
            var list = new List<FieldDescriptor>(ImportFieldDescriptors);

            var kingdomField = list.Find((fld) => {
                return fld.DisplayName == "KingdomType";
            });

            // The old code put the ranks after the kingdomtype field, but before the other taxon fields...
            int index = -1;
            if (kingdomField != null) {
                index = list.IndexOf(kingdomField) + 1;
            }

            // Add taxon ranks...            
            StoredProcReaderForEach("spBiotaRankList", (reader) => {
                string rank = reader[1].ToString();
                if (!string.IsNullOrWhiteSpace(rank)) {
                    var field = new FieldDescriptor { DisplayName = rank, Category = "Taxon", Description = "The taxonamic rank " + rank };
                    if (index > 0) {
                        list.Insert(index++, field);
                    } else {
                        list.Add(field);
                    }
                }
            });

            return list;
        }

        public List<FieldDescriptor> GetReferenceImportFields() {
            var list = new List<FieldDescriptor>(ImportReferencesDescriptors);
            return list;
        }

        public int ImportReference(ReferenceImport r) {
            var retval = ReturnParam("NewRefID");
            StoredProcUpdate("spReferenceImport",
                _P("vchrRefCode", r.RefCode),
                _P("vchrAuthor", r.Author),
                _P("vchrTitle", r.Title),
                _P("vchrBookTitle", r.BookTitle),
                _P("vchrEditor", r.Editor),
                _P("vchrRefType", r.RefType),
                _P("vchrYearOfPub", r.YearOfPub),
                _P("vchrActualDate", r.ActualDate),
                _P("vchrJournalAbbrevName", r.JournalAbbrevName),
                _P("vchrJournalFullName", r.JournalFullName),
                _P("vchrPartNo", r.PartNo),
                _P("vchrSeries", r.Series),
                _P("vchrPublisher", r.Publisher),
                _P("vchrPlace", r.Place),
                _P("vchrVolume", r.Volume),
                _P("vchrPages", r.Pages),
                _P("vchrTotalPages", r.TotalPages),
                _P("vchrPossess", r.Possess),
                _P("vchrSource", r.Source),
                _P("vchrEdition", r.Edition),
                _P("vchrISBN", r.ISBN),
                _P("vchrISSN", r.ISSN),
                _P("txtAbstract", r.Abstract),
                _P("txtDateQual", r.DateQual),                
                _P("intStartPage", r.StartPage),
                _P("intEndPage", r.EndPage),
                retval
            );

            return (int)retval.Value;

        }

        public int ImportRegion(string name, int parentId, string rank) {
            return StoredProcReturnVal("spRegionImportGetID",
                _P("vchrRegionName", name),
                _P("intInsertUnderParent", parentId),
                _P("vchrRank", rank)
            );

        }

        public int ImportSite(Site site) {
            return StoredProcReturnVal("spSiteImportGetID",
                _P("vchrSiteName", site.SiteName),
                _P("intPoliticalRegionID", site.PoliticalRegionID),
                _P("tintLocalType", site.LocalityType),
                _P("vchrLocal", site.Locality),
                _P("vchrDistanceFromPlace", site.DistanceFromPlace),
                _P("vchrDirFromPlace", site.DirFromPlace),
                _P("vchrInformalLocal", site.InformalLocal),
                _P("tintPosCoordinates", site.PosCoordinates),
                _P("tintPosAreaType", site.PosAreaType),
                _P("fltPosX1", site.PosX1),
                _P("fltPosY1", site.PosY1),
                _P("fltPosX2", site.PosX2),
                _P("fltPosY2", site.PosY2),
                _P("tintPosXYDisplayFormat", site.PosXYDisplayFormat),
                _P("vchrPosSource", site.PosSource),
                _P("vchrPosError", site.PosError),
                _P("vchrPosWho", site.PosWho),
                _P("vchrPosDate", site.PosDate),
                _P("vchrPosOriginal", site.PosOriginal),
                _P("vchrPosUTMSource", site.PosUTMSource),
                _P("vchrPosUTMMapProj", site.PosUTMMapProj),
                _P("vchrPosUTMMapName", site.PosUTMMapName),
                _P("vchrPosUTMMapVer", site.PosUTMMapVer),
                _P("tintElevType", site.ElevType),
                _P("fltElevUpper", site.ElevUpper),
                _P("fltElevLower", site.ElevLower),
                _P("fltElevDepth", site.ElevDepth),
                _P("vchrElevUnits", site.ElevUnits),
                _P("vchrElevSource", site.ElevSource),
                _P("vchrElevError", site.ElevError),
                _P("vchrGeoEra", site.GeoEra),
                _P("vchrGeoState", site.GeoState),
                _P("vchrGeoPlate", site.GeoPlate),
                _P("vchrGeoFormation", site.GeoFormation),
                _P("vchrGeoMember", site.GeoMember),
                _P("vchrGeoBed", site.GeoBed),
                _P("vchrGeoName", site.GeoName),
                _P("vchrGeoAgeBottom", site.GeoAgeBottom),
                _P("vchrGeoAgeTop", site.GeoAgeTop),
                _P("vchrGeoNotes", site.GeoNotes)
            );

        }

        public int ImportSiteVisit(SiteVisit siteVisit) {
            return StoredProcReturnVal("spSiteVisitImportGetID",
                _P("vchrSiteVisitName", siteVisit.SiteVisitName),
                _P("intSiteID", siteVisit.SiteID),
                _P("vchrFieldNumber", siteVisit.FieldNumber),
                _P("vchrCollector", siteVisit.Collector),
                _P("tintDateType", siteVisit.DateType),
                _P("intDateStart", siteVisit.DateStart),
                _P("intDateEnd", siteVisit.DateEnd),
                _P("intTimeStart", siteVisit.TimeStart),
                _P("intTimeEnd", siteVisit.TimeEnd),
                _P("vchrCasualTime", siteVisit.CasualTime));
        }

        public int ImportTaxon(int parentID, string epithet, string author, string yearOfPub, bool changedCombination, string elemType, bool unplaced, string rank, string kingdom, int order, bool unverified, string availnamestatus) {
            return StoredProcReturnVal("spBiotaImport",
                _P("intParentID", parentID),
                _P("vchrEpithet", epithet),
                _P("vchrAuthor", author),
                _P("vchrYearOfPub", yearOfPub),
                _P("bitChgComb", changedCombination),
                _P("chrElemType", elemType),
                _P("bitUnplaced", unplaced),
                _P("vchrRank", rank),
                _P("chrKingdomType", kingdom),
                _P("intOrder", order),
                _P("bitUnverified", unverified),
                _P("vchrAvailableNameStatus", availnamestatus)
            );
        }

        public void ImportNote(string category, int intraCatID, string noteName, string noteText, string comment = "") {
            var service = new SupportService(User);
            service.InsertNote(category, intraCatID, noteName, noteText, "", "", false, 0, "");
        }


        public void ImportTrait(string category, int intraCatID, string traitName, string traitValue, string comment = "") {
            StoredProcUpdate("spTraitImport",
                _P("vchrCategory", category),
                _P("intIntraCatID", intraCatID),
                _P("vchrTrait", traitName),
                _P("vchrValue", traitValue),
                _P("vchrComment", comment)
            );
        }

        public int ImportMaterial(Material m) {
            var matService = new MaterialService(User);
            m.MaterialID = matService.InsertMaterial(m.SiteVisitID);
            matService.UpdateMaterial(m);
            return m.MaterialID;
        }

        public void ImportCommonName(int taxonID, string commonName) {
            StoredProcUpdate("spCommonNameImport",
                _P("intBiotaID", taxonID),
                _P("vchrCommonName", commonName)
            );
        }

        public FieldDescriptor GetFieldDescriptorFromName(string name) {
            lock (this) {
                if (_importFieldsCache == null) {
                    _importFieldsCache = GetImportFields();
                }

                var cat = name.Substring(0, name.IndexOf("."));
                var fieldname = name.Substring(name.IndexOf(".") + 1);
                return _importFieldsCache.Find((fld) => {
                    return fld.Category.Equals(cat) && fld.DisplayName.Equals(fieldname);
                });
            }
        }

        public ConvertingValidatorResult ValidateImportValue(string targetField, string value, bool throwOnFailure = false) {

             var field = GetFieldDescriptorFromName(targetField);
            if (field != null) {
                return ValidateImportValue(field, value, throwOnFailure);
            } else {
                throw new Exception("Unrecognized target field name: " + targetField);
            }
        }

        public ConvertingValidatorResult ValidateImportValue(FieldDescriptor targetField, string value, bool throwOnFailure = false) {
            if (targetField.Validate == null || string.IsNullOrWhiteSpace(value)) {
                var result = new ConvertingValidatorResult(targetField, value);
                result.Success(value);
                return result;
            } else {
                var result = new ConvertingValidatorResult(targetField, value);
                targetField.Validate(value, result);
                if (throwOnFailure && !result.IsValid) {
                    throw new Exception(result.Message);
                }

                return result;
            }
        }


        #region Validators

        /// <summary>
        /// Default validator for fields - all values are accepted.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        static public bool StringValidator(string value, ConvertingValidatorResult result) {            
            return result.Success(value);            
        }

        public static bool PositionAreaTypeValidator(string value, ConvertingValidatorResult result) {            
            int i;
            if (Int32.TryParse(value, out i)) {
                if ( i >= 1 && i <= 3) {
                    return result.Success(i);
                } else {
                    return result.Fail("Value must be between 1 and 3 (1 = Point/Circle, 2 = Line, 3 = Rectangle)");
                }
            } else {
                return result.Fail("Value must be an integer");
            }
        }

        public static bool CoordinateTypeValidator(string value, ConvertingValidatorResult result) {            
            int i;
            if (Int32.TryParse(value, out i)) {
                if ( i >= 1 && i <= 3) {
                    return result.Success(i);
                } else {
                    return result.Fail("Value must be either 1 or 2 (1 = Latitude and longitude, 2 = UTM)");
                }
            } else {
                return result.Fail("Value must be an integer");
            }
        }

        public static bool CoordinateValidator(string value, ConvertingValidatorResult result) {

            double? coord = GeoUtils.ParseCoordinate(value);
            if (coord.HasValue) {
                return result.Success(coord.Value);
            }

            return result.Fail("Invalid coordinate format");
        }

        public static bool EllipsoidIndexConverter(string value, ConvertingValidatorResult result) {

            if (value.IsNumeric()) {
                int i;
                if (Int32.TryParse(value, out i)) {
                    if (i >= 0 && i < GeoUtils.ELLIPSOIDS.Length) {
                        return result.Success(i);
                    }
                }
            }

            var lngEllipsoid = GeoUtils.GetEllipsoidIndex(value);

            if (lngEllipsoid >= 0) {
                return result.Success(lngEllipsoid);
            }

            return result.Fail("Invalid ellipsoid value. Must be either an ellipsoid name, or and index between 0 and {1}.", GeoUtils.ELLIPSOIDS.Length - 1);
        }

        public static bool UTMZoneValidator(string value, ConvertingValidatorResult result) {
            if (_UTMZoneRegex.IsMatch(value)) {
                return result.Success(value);
            }

            return result.Fail("Invalid or incorrectly formatted UTM zone. Must include a UTM zone number and Latitude Band letter.");
        }

        public static bool UnitRangeValidator( string value, ConvertingValidatorResult result) {

            if (value.IsNumeric()) {
                return result.Success(new UnitRange(Double.Parse(value))); 
            }

            var matcher = _UnitRangeSingleUnitsRegex.Match(value);
            if (matcher.Success) {
                var upper = double.Parse(matcher.Groups[1].Value);
                var units = matcher.Groups[2].Value;
                return result.Success(new UnitRange(upper, units));
            }

            matcher = _UnitRangeRegex.Match(value);
            if (matcher.Success) {
                var lower = double.Parse(matcher.Groups[1].Value);
                var upper = double.Parse(matcher.Groups[2].Value);
                string units = null;
                if (matcher.Groups.Count > 2) {
                    units = matcher.Groups[3].Value;
                }
                return result.Success(new UnitRange(upper, lower, units));
            }

            return result.Fail("Invalid number or range. Must be either a number, number and units or a range of numbers with or without units (e.g. 18.2, 18.2 KM, 100-150, 100-150 metres)");
        }

        public static bool BLDateValidator(string value, ConvertingValidatorResult result) {
            String errMsg;

            if (DateUtils.IsValidBLDate(value, out errMsg)) {
                return result.Success(Int32.Parse(value.PadRight(8, '0')));
            } else {
                if (Microsoft.VisualBasic.Information.IsDate(value)) {
                    return result.Success(Int32.Parse(DateUtils.DateStrToBLDate(value)));
                }
            }

            return result.Fail("Invalid date format");
        }

        public static bool TimeValidator(string value, ConvertingValidatorResult result) {

            if (value.Length < 4) {
                value = value.PadLeft(4, '0');
            }

            var matcher = _TimeRegex.Match(value);
            if (matcher.Success) {
                var time = string.Format("{0}{1}", matcher.Groups[1].Value, matcher.Groups[2].Value);
                return result.Success(Int32.Parse(time));
            }

            return result.Fail("Invalid time format. Times must be either hhmm or hh:mm (24 hour time).");
        }

        public static bool IntegerValidator(string value, ConvertingValidatorResult result) {
            int i = 0;
            if (Int32.TryParse(value, out i)) {
                return result.Success(i);
            }
            return result.Fail("'{0}' is not an integer", value);
        }

        public static bool BooleanValidator(string value, ConvertingValidatorResult result) {
            bool b;
            if (Boolean.TryParse(value, out b)) {
                return result.Success(b);
            }

            if ("Y1".Contains(value[0])) {
                return result.Success(true);
            } else if ("N0".Contains(value[0])) {
                return result.Success(false);
            }

            return result.Fail("Must yield a boolean value (e.g. True, False, 0, 1, Y/N)");
        }


        #endregion

    }

    public class UnitRange {

        public UnitRange(double value) {
            RangeType = UnitRangeType.SingleValueNoUnits;
            Upper = value;
        }

        public UnitRange(double value, string units) {
            if (!string.IsNullOrEmpty(units)) {
                RangeType = UnitRangeType.SingleValueWithUnits;
                Upper = value;
                Units = units;
            } else {
                RangeType = UnitRangeType.SingleValueNoUnits;
                Upper = value;
            }
        }

        public UnitRange(double upper, double lower) {
            RangeType = UnitRangeType.RangeNoUnits;
            Upper = upper;
            Lower = lower;
        }

        public UnitRange(double upper, double lower, string units) {
            if (!string.IsNullOrEmpty(units)) {
                RangeType = UnitRangeType.RangeWithUnits;
                Upper = upper;
                Lower = lower;
                Units = units;
            } else {
                RangeType = UnitRangeType.RangeNoUnits;
                Upper = upper;
                Lower = lower;
            }
        }

        public double Upper { get; private set; }
        public double Lower { get; private set; }
        public string Units { get; private set; }
        public UnitRangeType RangeType { get; private set; }

    }

    public enum UnitRangeType {
        SingleValueNoUnits,
        SingleValueWithUnits,
        RangeNoUnits,
        RangeWithUnits
    }
}
