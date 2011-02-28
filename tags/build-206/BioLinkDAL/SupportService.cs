﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using BioLink.Data.Model;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    public class SupportService : BioLinkService {

        public static Dictionary<string, RefTypeMapping> RefTypeMap = new Dictionary<string, RefTypeMapping>();
        public static List<FieldDescriptor> FieldDescriptors = new List<FieldDescriptor>();
        public static Dictionary<string, string> TableAliases = new Dictionary<string, string>();
        public static Dictionary<string, string> TableTraitKeyFields = new Dictionary<string, string>();
        public static List<FieldDescriptor> ImportFieldDescriptors = new List<FieldDescriptor>();

        #region Static Initializer
        static SupportService() {
            RefTypeMap["J"] = new RefTypeMapping("J", "Journal");
            RefTypeMap["JS"] = new RefTypeMapping("JS", "Journal Section");
            RefTypeMap["B"] = new RefTypeMapping("B", "Book");
            RefTypeMap["BS"] = new RefTypeMapping("BS", "Book Section");
            RefTypeMap["M"] = new RefTypeMapping("M", "Miscellaneous");
            RefTypeMap["U"] = new RefTypeMapping("U", "Internet URL");

            // TABLE ALIASES
            TableAliases["tblPoliticalRegion"] = "R";
            TableAliases["tblSiteGroup"] = "SG";
            TableAliases["tblSite"] = "S";
            TableAliases["tblSiteVisit"] = "SV";
            TableAliases["tblMaterial"] = "M";
            TableAliases["tblMaterialPart"] = "MP";
            TableAliases["tblMaterialAssoc"] = "MA";
            TableAliases["tblBiota"] = "B";
            TableAliases["tblBiotaDefRank"] = "DBF";
            TableAliases["tblCommonName"] = "CN";
            TableAliases["tblBiotaDistribution"] = "BD";
            TableAliases["vwAssociateText"] = "AT";
            TableAliases["tblBiotaLocation"] = "BL";
            TableAliases["tblBiotaStorage"] = "BS";
            TableAliases["tblDistributionRegion"] = "DR";

            // Trait table key fields...
            TableTraitKeyFields["tblPoliticalRegion"] = "intPoliticalRegionID";
            TableTraitKeyFields["tblSiteGroup"] = "intSiteGroupID";
            TableTraitKeyFields["tblSite"] = "intSiteID";
            TableTraitKeyFields["tblSiteVisit"] = "intSiteVisitID";
            TableTraitKeyFields["tblMaterial"] = "intMaterialID";
            TableTraitKeyFields["tblMaterialPart"] = "intMaterialID";
            TableTraitKeyFields["tblBiota"] = "intBiotaID";
            TableTraitKeyFields["tblBiotaDefRank"] = "chrCode";
            TableTraitKeyFields["tblCommonName"] = "intCommonNameID";
            TableTraitKeyFields["tblBiotaDistribution"] = "intBiotaDistID";
            TableTraitKeyFields["vwAssociateText"] = "intAssociateID";
            TableTraitKeyFields["tblBiotaStorage"] = "intBiotaStorageID";
            TableTraitKeyFields["tblBiotaLocation"] = "intBiotaLocationID";
            TableTraitKeyFields["tblDistributionRegion"] = "intDistributionRegionID";

            // START FIELD DESCRIPTORS
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Biota Identifier", FieldName="intBiotaID", TableName="tblBiota", Category="Nomenclature", Description="Internal Database Indentifier for the Taxon", Format="", UseInRDE=true, DataType="ObjectID" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Full Name", FieldName="vchrFullName", TableName="tblBiota", Category="Nomenclature", Description="Full Name of a taxon including Author and Year", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Name", FieldName="vchrEpithet", TableName="tblBiota", Category="Nomenclature", Description="Name/Epithet of the taxon", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Author", FieldName="vchrAuthor", TableName="tblBiota", Category="Nomenclature", Description="Author", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Year", FieldName="vchrYearOfPub", TableName="tblBiota", Category="Nomenclature", Description="Year", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Changed Combination", FieldName="bitChangedComb", TableName="tblBiota", Category="Nomenclature", Description="Changed Combination", Format="", UseInRDE=true, DataType="Boolean" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Common Name", FieldName="vchrCommonName", TableName="tblCommonName", Category="Nomenclature", Description="Taxon's common name", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Rank", FieldName="vchrLongName", TableName="tblBiotaDefRank", Category="Nomenclature", Description="Element rank in full", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Rank Code", FieldName="chrElemType", TableName="tblBiota", Category="Nomenclature", Description="Element rank code", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Is Available Name", FieldName="bitAvailableName", TableName="tblBiota", Category="Nomenclature", Description="Taxon is an available name", Format="", UseInRDE=true, DataType="Boolean" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Is Literature Name", FieldName="bitLiteratureName", TableName="tblBiota", Category="Nomenclature", Description="Taxon is a literature name", Format="", UseInRDE=true, DataType="Boolean" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Is Unverified", FieldName="bitUnverified", TableName="tblBiota", Category="Nomenclature", Description="Taxon is an unverified entry", Format="", UseInRDE=true, DataType="Boolean" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Name Status", FieldName="vchrAvailableNameStatus", TableName="tblBiota", Category="Nomenclature", Description="Status of Available Names", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Storage Location", FieldName="vchrName", TableName="tblBiotaStorage", Category="Nomenclature", Description="Where specimens are stored", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Created", FieldName="vchrWhoCreated", TableName="tblBiota", Category="Nomenclature", Description="The username of the user who created the entry", Format="", UseInRDE=false, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Created", FieldName="dtDateCreated", TableName="tblBiota", Category="Nomenclature", Description="The date the entry was created", Format="", UseInRDE=false, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Last Updated", FieldName="vchrWhoLastUpdated", TableName="tblBiota", Category="Nomenclature", Description="The username of the user who last updated the entry", Format="", UseInRDE=false, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Last Updated", FieldName="dtDateLastUpdated", TableName="tblBiota", Category="Nomenclature", Description="The date the entry was last updated", Format="", UseInRDE=false, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Region", FieldName="vchrName", TableName="tblDistributionRegion", Category="Nomenclature\\Region", Description="Taxon distribution region", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Introduced", FieldName="bitIntroduced", TableName="tblBiotaDistribution", Category="Nomenclature\\Region", Description="True/False if taxon was introduced", Format="", UseInRDE=true, DataType="Boolean" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Associate", FieldName="Associate", TableName="vwAssociateText", Category="Associates", Description="Name of the associate being searched for", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Forward Relationship", FieldName="vchrRelationFromTo", TableName="vwAssociateText", Category="Associates", Description="Eg. Parasite: Aus is a Parasite of Bus", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Reverse Relationship", FieldName="vchrRelationToFrom", TableName="vwAssociateText", Category="Associates", Description="Eg. Host: Bus is a Host of Aus", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Region Identifier", FieldName="intPoliticalRegionID", TableName="tblPoliticalRegion", Category="PoliticalRegion", Description="Internal Database Identifier for this region", Format="", UseInRDE=true, DataType="ObjectID" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Region Name", FieldName="vchrName", TableName="tblPoliticalRegion", Category="PoliticalRegion", Description="Political Region Name", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Region Type", FieldName="vchrRank", TableName="tblPoliticalRegion", Category="PoliticalRegion", Description="Type of Region: Region, Country, Province, etc", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Created", FieldName="vchrWhoCreated", TableName="tblPoliticalRegion", Category="PoliticalRegion", Description="The username of the user who created the entry", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Created", FieldName="dtDateCreated", TableName="tblPoliticalRegion", Category="PoliticalRegion", Description="The date the entry was created", Format="", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Last Updated", FieldName="vchrWhoLastUpdated", TableName="tblPoliticalRegion", Category="PoliticalRegion", Description="The username of the user who last updated the entry", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Last Updated", FieldName="dtDateLastUpdated", TableName="tblPoliticalRegion", Category="PoliticalRegion", Description="The date the entry was last updated", Format="", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Site Identifier", FieldName="intSiteID", TableName="tblSite", Category="Site", Description="Internal Database Identifier for this site", Format="", UseInRDE=true, DataType="ObjectID" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Site Name", FieldName="vchrSiteName", TableName="tblSite", Category="Site", Description="Identifier (name or code) for this site/station assigned by the collector (need not be unique)", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Locality Type", FieldName="tintLocalType", TableName="tblSite", Category="Site", Description="Integer representing the type of locality data (1=Locality only, 2=Locality offset, 3=Informal Locality) ", Format="", UseInRDE=true, DataType="IntCode[1=Locality only,2=Locality offset,3=Informal locality]" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Locality", FieldName="vchrLocal", TableName="tblSite", Category="Site", Description="Place name and optional offset where the material was found", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Distance from Place", FieldName="vchrDistanceFromPlace", TableName="tblSite", Category="Site", Description="Distance from the locality", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Direction from Place", FieldName="vchrDirFromPlace", TableName="tblSite", Category="Site", Description="Direction from the locality", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Informal locality", FieldName="vchrInformalLocal", TableName="tblSite", Category="Site", Description="Informal locality", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position Area Type", FieldName="tintPosAreaType", TableName="tblSite", Category="Site", Description="Integer representing the type of position data (1=point 2=line 3=bounding box)", Format="", UseInRDE=true, DataType="IntCode[1=Point,2=Line,3=Bounding box]" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position Coordinate Type", FieldName="tintPosCoordinates", TableName="tblSite", Category="Site", Description="Integer representing the type of coordinates used (1=Lat/Long 2=Easting/Northings)", Format="", UseInRDE=true, DataType="IntCode[1=Lat/Long,2=Eastings/Northings]" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Longitude", FieldName="fltPosX1", TableName="tblSite", Category="Site", Description="Latitude in decimal degrees", Format="Longitude", UseInRDE=true, DataType="Longitude" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Latitude", FieldName="fltPosY1", TableName="tblSite", Category="Site", Description="Longitude in decimal degrees", Format="Latitude", UseInRDE=true, DataType="Latitude" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Longitude 2", FieldName="fltPosX2", TableName="tblSite", Category="Site", Description="Optional South-East Latitude in decimal degrees", Format="Longitude", UseInRDE=true, DataType="Longitude" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Latitude 2", FieldName="fltPosY2", TableName="tblSite", Category="Site", Description="Optional South-East Longitude in decimal degrees", Format="Latitude", UseInRDE=true, DataType="Latitude" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position source", FieldName="vchrPosSource", TableName="tblSite", Category="Site", Description="Source of position data", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position error", FieldName="vchrPosError", TableName="tblSite", Category="Site", Description="Estimate of the accuracy of position data", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Generated by", FieldName="vchrPosWho", TableName="tblSite", Category="Site", Description="Person who generated position data", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Generated on", FieldName="vchrPosDate", TableName="tblSite", Category="Site", Description="Date the position data were generated", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Original position", FieldName="vchrPosOriginal", TableName="tblSite", Category="Site", Description="Original position", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM source", FieldName="vchrPosUTMSource", TableName="tblSite", Category="Site", Description="UTM source", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map projection", FieldName="vchrPosUTMMapProj", TableName="tblSite", Category="Site", Description="UTM map projection", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map name", FieldName="vchrPosUTMMapName", TableName="tblSite", Category="Site", Description="UTM map name", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map version", FieldName="vchrPosUTMMapVer", TableName="tblSite", Category="Site", Description="UTM map version", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation type", FieldName="tintElevType", TableName="tblSite", Category="Site", Description="Integer representing the type of elevation data(1=Elevation 2=Depth)", Format="", UseInRDE=true, DataType="IntCode[1=Elevation,2=Depth]" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation upper", FieldName="fltElevUpper", TableName="tblSite", Category="Site", Description="Elevation upper", Format="", UseInRDE=true, DataType="Double" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation lower", FieldName="fltElevLower", TableName="tblSite", Category="Site", Description="Elevation lower", Format="", UseInRDE=true, DataType="Double" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation depth", FieldName="fltElevDepth", TableName="tblSite", Category="Site", Description="Elevation depth", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation units", FieldName="vchrElevUnits", TableName="tblSite", Category="Site", Description="Elevation units", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation source", FieldName="vchrElevSource", TableName="tblSite", Category="Site", Description="Elevation source", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation error", FieldName="vchrElevError", TableName="tblSite", Category="Site", Description="Elevation error", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Era", FieldName="vchrGeoEra", TableName="tblSite", Category="Site", Description="Geological Era", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological State", FieldName="vchrGeoState", TableName="tblSite", Category="Site", Description="Geological State", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Plate", FieldName="vchrGeoPlate", TableName="tblSite", Category="Site", Description="Geological Plate", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Formation", FieldName="vchrGeoFormation", TableName="tblSite", Category="Site", Description="Geological Formation", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Member", FieldName="vchrGeoMember", TableName="tblSite", Category="Site", Description="Geological Member", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Bed", FieldName="vchrGeoBed", TableName="tblSite", Category="Site", Description="Geological Bed", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Name", FieldName="vchrGeoName", TableName="tblSite", Category="Site", Description="Geological Name", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Age Bottom", FieldName="vchrGeoAgeBottom", TableName="tblSite", Category="Site", Description="Geological Age Bottom", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Age Top", FieldName="vchrGeoAgeTop", TableName="tblSite", Category="Site", Description="Geological Age Top", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological Notes", FieldName="vchrGeoNotes", TableName="tblSite", Category="Site", Description="Geological Notes", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Created", FieldName="vchrWhoCreated", TableName="tblSite", Category="Site", Description="The username of the user who created the entry", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Created", FieldName="dtDateCreated", TableName="tblSite", Category="Site", Description="The date the entry was created", Format="", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Last Updated", FieldName="vchrWhoLastUpdated", TableName="tblSite", Category="Site", Description="The username of the user who last updated the entry", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Last Updated", FieldName="dtDateLastUpdated", TableName="tblSite", Category="Site", Description="The date the entry was last updated", Format="", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Visit Identifier", FieldName="intSiteVisitID", TableName="tblSiteVisit", Category="SiteVisit", Description="Internal Database Indentifier for the visit", Format="", UseInRDE=true, DataType="ObjectID" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collector(s)", FieldName="vchrCollector", TableName="tblSiteVisit", Category="SiteVisit", Description="Persons collecting the material", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Start Date", FieldName="intDateStart", TableName="tblSiteVisit", Category="SiteVisit", Description="Date the collection took place or began", Format="Date", UseInRDE=true, DataType="BLDate" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Start Time", FieldName="intTimeStart", TableName="tblSiteVisit", Category="SiteVisit", Description="Optional Time the collection took place or began", Format="", UseInRDE=true, DataType="BLTime" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "End Date", FieldName="intDateEnd", TableName="tblSiteVisit", Category="SiteVisit", Description="Optional date the collection concluded", Format="Date", UseInRDE=true, DataType="BLDate" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "End Time", FieldName="intTimeEnd", TableName="tblSiteVisit", Category="SiteVisit", Description="Optional time the collection concluded", Format="", UseInRDE=true, DataType="BLTime" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Casual Time", FieldName="vchrCasualTime", TableName="tblSiteVisit", Category="SiteVisit", Description="Less rigorous date/time structure, eg. Spring 45", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Visit Name", FieldName="vchrSiteVisitName", TableName="tblSiteVisit", Category="SiteVisit", Description="Name given to site visit", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Field number", FieldName="vchrFieldNumber", TableName="tblSiteVisit", Category="SiteVisit", Description="Field number", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Created", FieldName="vchrWhoCreated", TableName="tblSiteVisit", Category="SiteVisit", Description="The username of the user who created the entry", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Created", FieldName="dtDateCreated", TableName="tblSiteVisit", Category="SiteVisit", Description="The date the entry was created", Format="Date", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Last Updated", FieldName="vchrWhoLastUpdated", TableName="tblSiteVisit", Category="SiteVisit", Description="The username of the user who last updated the entry", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Last Updated", FieldName="dtDateLastUpdated", TableName="tblSiteVisit", Category="SiteVisit", Description="The date the entry was last updated", Format="Date", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Material Identifier", FieldName="intMaterialID", TableName="tblMaterial", Category="Material", Description="Internal Database Indentifier for the material", Format="", UseInRDE=true, DataType="ObjectID" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Material name", FieldName="vchrMaterialName", TableName="tblMaterial", Category="Material", Description="Descriptive name given to material", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Accession number", FieldName="vchrAccessionNo", TableName="tblMaterial", Category="Material", Description="Unique collection number", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Registration number", FieldName="vchrRegNo", TableName="tblMaterial", Category="Material", Description="Code assigned by collection owner", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collector number", FieldName="vchrCollectorNo", TableName="tblMaterial", Category="Material", Description="Code assigned by collector", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identified by", FieldName="vchrIDBy", TableName="tblMaterial", Category="Material", Description="Person providing identification", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identified on", FieldName="dtIDDate", TableName="tblMaterial", Category="Material", Description="Date identification was made", Format="", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification Reference Code", FieldName="vchrIDRefID", TableName="tblMaterial", Category="Material", Description="The publication in which this identification appears", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification method", FieldName="vchrIDMethod", TableName="tblMaterial", Category="Material", Description="The method used to make the identification", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification accuracy", FieldName="vchrIDAccuracy", TableName="tblMaterial", Category="Material", Description="Likelihood that ID is correct: (0) unchecked by any authority, (1) compared with other named specimens, (2) determined by authority based on existing classification or named material, (3) determined by authority during revision, (4) part of type series", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification name qualifier", FieldName="vchrIDNameQual", TableName="tblMaterial", Category="Material", Description="An indication that the cited name is uncertain.  Includes cf (compare with), near, incorrect (current name is incorrect but true name is unknown), ? (questionable)", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification notes", FieldName="vchrIDNotes", TableName="tblMaterial", Category="Material", Description="Assorted notes on this identification", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Institute", FieldName="vchrInstitution", TableName="tblMaterial", Category="Material", Description="Institute where material is held", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collection method", FieldName="vchrCollectionMethod", TableName="tblMaterial", Category="Material", Description="Method used to collect material", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Abundance", FieldName="vchrAbundance", TableName="tblMaterial", Category="Material", Description="Abundance or frequency of material", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Macrohabitat", FieldName="vchrMacroHabitat", TableName="tblMaterial", Category="Material", Description="General description of habitat.  Can include vegetation, soil, landform, etc.", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Microhabitat", FieldName="vchrMicroHabitat", TableName="tblMaterial", Category="Material", Description="Specific, small-scale habitat or situation of collection", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Source", FieldName="vchrSource", TableName="tblMaterial", Category="Material", Description="Source of this information (collection (specimen), electronic (specimen no longer available), literature (published record only), observation (unvouchered sighting), photograph (of existing specimen)", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Special label", FieldName="vchrSpecialLabel", TableName="tblMaterial", Category="Material", Description="Special text to appear on printed labels", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Original label", FieldName="vchrOriginalLabel", TableName="tblMaterial", Category="Material", Description="Special text to appear on printed labels", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Created", FieldName="vchrWhoCreated", TableName="tblMaterial", Category="Material", Description="The username of the user who created the entry", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Created", FieldName="dtDateCreated", TableName="tblMaterial", Category="Material", Description="The date the entry was created", Format="", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Who Last Updated", FieldName="vchrWhoLastUpdated", TableName="tblMaterial", Category="Material", Description="The username of the user who last updated the entry", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Date Last Updated", FieldName="dtDateLastUpdated", TableName="tblMaterial", Category="Material", Description="The date the entry was last updated", Format="", UseInRDE=true, DataType="Date" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Name", FieldName="vchrPartName", TableName="tblMaterialPart", Category="MaterialPart", Description="Name of the part", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Sample Type", FieldName="vchrSampleType", TableName="tblMaterialPart", Category="MaterialPart", Description="Sample type", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Number of Specimens", FieldName="intNoSpecimens", TableName="tblMaterialPart", Category="MaterialPart", Description="Number of specimens in the subpart", Format="", UseInRDE=true, DataType="Integer" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Qualification", FieldName="vchrNoSpecimensQual", TableName="tblMaterialPart", Category="MaterialPart", Description="Qualification of the number of specimens", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Lifestage", FieldName="vchrLifestage", TableName="tblMaterialPart", Category="MaterialPart", Description="Lifestage", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Gender", FieldName="vchrGender", TableName="tblMaterialPart", Category="MaterialPart", Description="Gender", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Registration #", FieldName="vchrRegNo", TableName="tblMaterialPart", Category="MaterialPart", Description="registration of the Subpart", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Condition", FieldName="vchrCondition", TableName="tblMaterialPart", Category="MaterialPart", Description="Condition of the material", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Storage Site", FieldName="vchrStorageSite", TableName="tblMaterialPart", Category="MaterialPart", Description="Where is the subpart stored", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Storage Method", FieldName="vchrStorageMethod", TableName="tblMaterialPart", Category="MaterialPart", Description="How is the subpart stored", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Curation Status", FieldName="vchrCurationStatus", TableName="tblMaterialPart", Category="MaterialPart", Description="Curation status of the part", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "Notes", FieldName="txtNotes", TableName="tblMaterialPart", Category="MaterialPart", Description="Notes", Format="", UseInRDE=true, DataType="Text" });
		    FieldDescriptors.Add(new FieldDescriptor { DisplayName = "On Loan", FieldName="tintOnLoan", TableName="tblMaterialPart", Category="MaterialPart", Description="= 1 if on loan", Format="", UseInRDE=true, DataType="Boolean" });
            // END FIELD DESCRIPTORS

            // Import field descriptors
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Region", Category = "Region", Description = "Region the locality falls in" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Country", Category = "Region", Description = "Country the locality falls in" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "State/Province", Category = "Region", Description = "State/Province the locality falls in" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "County", Category = "Region", Description = "County the locality falls in" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Site Name", Category = "Site", Description = "Identifier (name or code) for this site/station assigned by the collector (need not be unique)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Locality", Category = "Site", Description = "Place name and optional offset where the material was found" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Distance from place", Category = "Site", Description = "Distance (with units) from named place (e.g. 10km) (used in conjunction with vchrLocal and vchrDirFromPlace)." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Direction from place", Category = "Site", Description = "Direction from named place (e.g. S) (used in conjunction with vchrLocal and vchrDistanceFromPlace)." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Informal locality", Category = "Site", Description = "Description based on a non-named place or based on features which are only meaningful in a local context (e.g. 'near the small hill just south of the river crossing')" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position area type", Category = "Site", Description = "Shape of the area defined by Coordinates (1 = Point/Circle, 2 = Line, 3 = Rectangle)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Coordinate type", Category = "Site", Description = "Integer representing the coordinate data format, Latitude and longitude = 1, UTM = 2" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Latitude", Category = "Site", Description = "Latitude (in decimal degrees, positive = N, negative = S) of point, N or W end of Line, NW corner of Rectangle" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Longitude", Category = "Site", Description = "Longitude (in decimal degrees, positive = E, negative = W) of point, N or W end of Line, NW corner of Rectangle" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Latitude 2", Category = "Site", Description = "Latitude (in decimal degrees, positive = N, negative = S) of S or E end of Line, SE corner of Rectangle" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Longitude 2", Category = "Site", Description = "Longitude (in decimal degrees, positive = E, negative = W) of S or E end of Line, SE corner of Rectangle" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position source", Category = "Site", Description = "Where or how the Latitude/Longitude coordinates were determined.  Can include sources such as map using label data, gazetteer using label data, GPS, collector, compiler." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Position error", Category = "Site", Description = "Error associated with Lat/Long in meters (1sec = 25m, 5sec = 100m, 10sec = 200m, 30sec = 500m, 1min = 1km, 5min = 5km, 10min = 10km, 1deg = 60km, 5deg = 300km, 10deg = 600km)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Generated by", Category = "Site", Description = "Name of person determining coordinates." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Generated on", Category = "Site", Description = "Date the position data were generated" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Original position", Category = "Site", Description = "..." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM ellipsoid", Category = "Site", Description = "Ellipsoid defining northings and eastings" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM zone number", Category = "Site", Description = "UTM zone number the coordinates are in" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM source", Category = "Site", Description = "The projection used to convert between Latitude/Longitude and UTM/map grids." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map projection", Category = "Site", Description = "Map projection of map or GPS used when determining Coordinates" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map name", Category = "Site", Description = "Name of map used to determine Coordinates" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "UTM map version", Category = "Site", Description = "Version or date of map used to determine Coordinates" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation upper", Category = "Site", Description = "Capture or upper elevation (if elevation) or capture or upper water depth (if depth)." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation lower", Category = "Site", Description = "Lower elevation or lower water depth." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation depth", Category = "Site", Description = "Water Depth if different from Capture Depth" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation units", Category = "Site", Description = "Units used for elevation (i.e. m, ft.)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation source", Category = "Site", Description = "Where or how the elevation or depth was determined.  Can include sources such as map using label data, digital elevation model using label data, GPS, altimeter, collector, compiler, field estimate." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Elevation error", Category = "Site", Description = "Error associated with altitude/depth in meters" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological era", Category = "Site", Description = "Geologic Era" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological state", Category = "Site", Description = "..." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological plate", Category = "Site", Description = "Plate Tectonic Name" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological formation", Category = "Site", Description = "Lithostratigraphic Formation" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological member", Category = "Site", Description = "Lithostratigraphic Member" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological bed", Category = "Site", Description = "Lithostratigraphic Bed" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological name", Category = "Site", Description = "Stratigraphy Name" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological age bottom", Category = "Site", Description = "Stratigraphy Bottom Age" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological age top", Category = "Site", Description = "Stratigraphy Top Age" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Geological notes", Category = "Site", Description = "..." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Other", Category = "Site", Description = "Other user defined field" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collector(s)", Category = "SiteVisit", Description = "Persons collecting the material" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Start Date", Category = "SiteVisit", Description = "Date the collection took place or began" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Start Time", Category = "SiteVisit", Description = "Optional Time the collection took place or began" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "End Date", Category = "SiteVisit", Description = "Optional date the collection concluded" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "End Time", Category = "SiteVisit", Description = "Optional time the collection concluded" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Visit Name", Category = "SiteVisit", Description = "Name given to site visit" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Field number", Category = "SiteVisit", Description = "Number assigned by collector(s) to material" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Casual Date", Category = "SiteVisit", Description = "A casual date for the site visit" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Other", Category = "SiteVisit", Description = "Other user defined field" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Material name", Category = "Material", Description = "Descriptive name given to material" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Accession number", Category = "Material", Description = "An identification number assigned by the collection for this material.  This number will normally uniquely identify the material." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Registration number", Category = "Material", Description = "Code assigned by collection owner" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collector number", Category = "Material", Description = "Code assigned by collector" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identified by", Category = "Material", Description = "Person providing identification" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identified on", Category = "Material", Description = "Date identification was made" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification reference", Category = "Material", Description = "The publication in which this identification appears" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification method", Category = "Material", Description = "The method used to make the identification" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification accuracy", Category = "Material", Description = "Likelihood that ID is correct: (0) unchecked by any authority, (1) compared with other named specimens, (2) determined by authority based on existing classification or named material, (3) determined by authority during revision, (4) part of type series" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification name qualifier", Category = "Material", Description = "An indication that the cited name is uncertain.  Includes cf (compare with), near, incorrect (current name is incorrect but true name is unknown), ? (questionable)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Identification notes", Category = "Material", Description = "Assorted notes on this identification" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Institute", Category = "Material", Description = "Institute where material is held" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Collection method", Category = "Material", Description = "Method used to collect sample or origin of sample (e.g. pitfall, Malaise trap, reared, cultivated, from wild)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Abundance", Category = "Material", Description = "Abundance or frequency of material" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Macrohabitat", Category = "Material", Description = "General description of habitat.  Can include vegetation, soil, landform, etc." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Microhabitat", Category = "Material", Description = "Specific, small-scale habitat or situation of collection" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Source", Category = "Material", Description = "Source of this information (collection (specimen), electronic (specimen no longer available), literature (published record only), observation (unvouchered sighting), photograph (of existing specimen)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Special label", Category = "Material", Description = "Special text to appear on printed labels" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Original label", Category = "Material", Description = "Verbatim text of label(s) that originally appeared with specimen(s)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Part name", Category = "Material", Description = "The name of the subpart.  This should describe the subpart type, for example wings, genitalia, skin, bark." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Sample type", Category = "Material", Description = "Type of material in this sample (e.g. bulk sample, individual specimen, dissection, subpart name or type)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Number of specimens", Category = "Material", Description = "Number of individual specimens represented by the record." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Number of specimens qualifier", Category = "Material", Description = "Qualification of intNoSpecimens.  Possible values include exactly, about, at least, at most" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Life stage", Category = "Material", Description = "The life stage (e.g. larvae, immature, adult) or age (e.g. 20 days, 2 years) of the specimen(s)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Gender", Category = "Material", Description = "The gender or caste of the specimen(s)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Part registration number", Category = "Material", Description = "..." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Condition", Category = "Material", Description = "Condition of specimen (e.g. good, damaged, faded)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Storage site", Category = "Material", Description = "The location within collection where specimen(s) is stored" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Storage Method", Category = "Material", Description = "Location or type of storage used for specimen(s); can also indicate preparation method (e.g. on slide, pin, card, point, etc)" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Curation status", Category = "Material", Description = "Current curation status of this material" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Notes", Category = "Material", Description = "Current curation status of this material" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Other", Category = "Material", Description = "Other user defined field" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "KingdomType", Category = "Taxon", Description = "Defaults are P for Plantae, A for Animalia" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Author", Category = "Taxon", Description = "Author of the name" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Year", Category = "Taxon", Description = "Year of publication of the name" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Changed Combination", Category = "Taxon", Description = "Indicates that the species epithet is in other than the original combination" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Verified", Category = "Taxon", Description = "Name needs checking, suspected of being no longer valid or is known to be unavailable, non zero = Verified, 0 = Unverified" });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Common Name", Category = "Taxon", Description = "Common name given to the taxa." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Name Status", Category = "Taxon", Description = "The status of the available names." });
            ImportFieldDescriptors.Add(new FieldDescriptor { DisplayName = "Other", Category = "Taxon", Description = "Other user defined field" });
            // END Import Field Descriptors

        }

        #endregion

        public SupportService(User user)  : base(user) { }

        #region Traits

        public TraitCategory GetTraitCategory(string category) {
            TraitCategory cat = null;
            int catId = StoredProcReturnVal<int>("spTraitCategoryGetSet", _P("vchrTraitCategory", category), ReturnParam("intTraitCategoryID", SqlDbType.Int));
            cat = new TraitCategory();
            cat.Category = category;
            cat.TraitCategoryID = catId;
            return cat;
        }

        public List<Trait> GetTraits(string category, int intraCategoryID) {
            var mapper = new GenericMapperBuilder<Trait>().Map("Trait", "Name").PostMapAction((t) => {
                t.Category = category;
            }).build();
            return StoredProcToList("spTraitList", mapper, _P("vchrCategory", category), _P("vchrIntraCatID", intraCategoryID + ""));
        }

        public List<String> GetTraitDistinctValues(string traitName, string category) {
            var results = new List<string>();
            StoredProcReaderForEach("spTraitDistinctValues", (reader) => {
                results.Add(reader[0] as string);
            }, _P("vchrTraitType", traitName), _P("vchrCategory", category));

            return results;
        }

        public List<String> GetDistinctValues(string table, string field) {

            var results = new List<string>();

            // First we check to see if there is a Phrase Category that matches the concatenation of the table and the field. This is as 
            // per the legacy BioLink application - perhaps to allow installations to restrict otherwise distinct list queries to a constrained vocabulary
            var phraseCat = string.Format("{0}_{1}", table, field);

            var supportService = new SupportService(User);
            int phraseCatId = supportService.GetPhraseCategoryId(phraseCat, false);
            if (phraseCatId > 0) {
                Logger.Debug("Using phrase category {0} (id={1}) for Distinct List lookup", phraseCat, phraseCatId);
                var phrases = supportService.GetPhrases(phraseCatId);
                results = phrases.ConvertAll((phrase) => {
                    return phrase.PhraseText;
                });
            } else {
                Logger.Debug("Selecting distinct values for field {0} from table {1}", field, table);
                StoredProcReaderForEach("spSelectDistinct", (reader) => {
                    results.Add(reader[0] as string);
                }, _P("vchrTableName", table), _P("vchrFieldName", field));
            }

            return results;
        }


        public void DeleteTrait(int traitId) {
            StoredProcUpdate("spTraitDelete", _P("intTraitID", traitId));
        }

        public List<String> GetTraitNamesForCategory(string traitCategory) {
            var results = new List<string>();
            StoredProcReaderForEach("spTraitTypeListForCategory", (reader) => {
                results.Add(reader["Trait"] as string);
            }, _P("vchrCategory", traitCategory));
            return results;
        }

        public int InsertOrUpdateTrait(Trait trait) {
            if (trait.TraitID < 0) {
                var retval = ReturnParam("NewTraitId", SqlDbType.Int);
                StoredProcUpdate("spTraitInsert",
                    _P("vchrCategory", trait.Category),
                    _P("intIntraCatID", trait.IntraCatID),
                    _P("vchrTrait", trait.Name),
                    _P("vchrValue", trait.Value ?? ""),
                    _P("vchrComment", trait.Comment ?? ""),
                    retval);
                return (int)retval.Value;
            } else {
                StoredProcUpdate("spTraitUpdate",
                    _P("intTraitID", trait.TraitID),
                    _P("vchrCategory", trait.Category),
                    _P("vchrTrait", trait.Name),
                    _P("vchrValue", trait.Value),
                    _P("vchrComment", trait.Comment));

                return trait.TraitID;
            }
        }

        #endregion

        #region Multimedia

        public List<MultimediaLink> GetMultimediaItems(string category, int intraCatID) {
            var mapper = new GenericMapperBuilder<MultimediaLink>().Map("FileExtension", "Extension").build();
            List<MultimediaLink> ret = StoredProcToList("spMultimediaList", mapper, _P("vchrCategory", category), _P("intIntraCatID", intraCatID));
            return ret;
        }

        public byte[] GetMultimediaBytes(int mediaId) {
            byte[] ret = null;
            StoredProcReaderFirst("spMultimediaGetOne", (reader) => {
                var x = reader.GetSqlBinary(0);
                ret = x.Value;
            }, _P("intMultimediaID", mediaId));
            return ret;
        }

        public List<MultimediaType> GetMultimediaTypes() {
            var mapper = new GenericMapperBuilder<MultimediaType>().Map("MultimediaType", "Name").build();
            return StoredProcToList("spMultimediaTypeList", mapper);
        }

        public Multimedia GetMultimedia(int mediaID) {
            var mapper = new GenericMapperBuilder<Multimedia>().Map("vchrname", "Name").build();
            Multimedia ret = null;
            StoredProcReaderFirst("spMultimediaGet", (reader) => {
                ret = mapper.Map(reader);
                ret.MultimediaID = mediaID;
            }, _P("intMMID", mediaID));
            return ret;
        }

        public void DeleteMultimediaLink(int? multimediaLinkId) {
            StoredProcUpdate("spMultimediaLinkDelete", _P("intMultimediaLinkID", multimediaLinkId.Value));
        }

        public int InsertMultimedia(string name, string extension, byte[] bytes) {
            var retval = ReturnParam("NewMultimediaID", SqlDbType.Int);
            StoredProcUpdate("spMultimediaInsert", _P("vchrName", name), _P("vchrFileExtension", extension), _P("intSizeInBytes", bytes.Length), retval);
            // Now insert the actual bytes...
            UpdateMultimediaBytes((int)retval.Value, bytes);

            return (int)retval.Value;
        }

        public int InsertMultimediaLink(string category, int intraCatID, string multimediaType, int multimediaID, string caption) {
            var retval = ReturnParam("[NewMultimediaLinkID]", SqlDbType.Int);

            if (multimediaType == null) {
                multimediaType = "";
            }

            StoredProcUpdate("spMultimediaLinkInsert",
                _P("vchrCategory", category.ToString()),
                _P("intIntraCatID", intraCatID),
                _P("vchrMultimediaType", multimediaType),
                _P("intMultimediaID", multimediaID),
                _P("vchrCaption", caption),
                retval);

            return (int)retval.Value;
        }

        public void UpdateMultimedia(int multimediaId, string name, string number, string artist, string dateRecorded, string owner, string copyright) {

            StoredProcUpdate("spMultimediaUpdateLong",
                _P("intMultimediaID", multimediaId),
                _P("vchrName", name),
                _P("vchrNumber", number),
                _P("vchrArtist", artist),
                _P("vchrDateRecorded", dateRecorded),
                _P("vchrOwner", owner),
                _P("txtCopyright", copyright)
            );
        }

        public void UpdateMultimediaLink(int multimediaLinkID, string category, string multimediaType, string caption) {
            StoredProcUpdate("spMultimediaLinkUpdate",
                _P("intMultimediaLinkID", multimediaLinkID),
                _P("vchrCategory", category),
                _P("vchrMultimediaType", multimediaType),
                _P("vchrCaption", caption)
            );
        }

        public void UpdateMultimediaBytes(int? multimediaId, byte[] bytes) {
            // Multimedia is the only place where we don't have a stored procedure for the insert/update. This is probably due to a 
            // limitation of ADO.NET or SQL Server back in the 90's or something like that. Either way, we need to insert the actual blob
            // "manually"...
            Command((conn, cmd) => {

                if (User.InTransaction && User.CurrentTransaction != null) {
                    cmd.Transaction = User.CurrentTransaction;
                }

                cmd.CommandText = "UPDATE [tblMultimedia] SET imgMultimedia = @blob, intSizeInBytes=@size WHERE intMultimediaID = @multimediaId";
                cmd.Parameters.Add(_P("blob", bytes));
                cmd.Parameters.Add(_P("size", bytes.Length));
                cmd.Parameters.Add(_P("multimediaId", (int)multimediaId));
                cmd.ExecuteNonQuery();
            });

        }

        public Multimedia FindDuplicateMultimedia(System.IO.FileInfo file, out int sizeInBytes) {
            var name = file.Name;
            if (file.Name.Contains(".")) {
                name = file.Name.Substring(0, file.Name.LastIndexOf("."));
            }
            sizeInBytes = 0;
            var extension = file.Extension.Substring(1);
            // Not that the following service all returns partial results (incomplete stored proc), so a two stage approach is required.
            // First we find candidates based on name.
            var candidates = FindMultimediaByName(name);
            if (candidates.Count > 0) {
                // Look for matching names and extensions...
                foreach (Multimedia candidate in candidates) {
                    if (candidate.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) && candidate.FileExtension.Equals(extension, StringComparison.CurrentCultureIgnoreCase)) {
                        // Now we do a deeper analysis of each matching candidate, checking the filelength. Theoretically, if we kept a hash on the content in the database
                        // we should compare that, but for now we'll use name and size.
                        sizeInBytes = GetMultimediaSizeInBytes(candidate.MultimediaID);
                        if (sizeInBytes > -1) {
                            if (sizeInBytes == file.Length) {
                                return GetMultimedia(candidate.MultimediaID);
                            }
                        } else {
                            throw new Exception("Failed to get size of multimedia " + candidate.MultimediaID);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// TODO: should probably be made into a stored proc!
        /// </summary>
        /// <param name="multimediaId"></param>
        /// <returns></returns>
        public int GetMultimediaSizeInBytes(int multimediaId) {
            int result = -1;
            SQLReaderForEach("SELECT DATALENGTH(imgMultimedia) FROM [tblMultimedia] where intMultimediaID = @mmid", (reader) => {
                result = (int)reader[0];
            }, _P("mmid", multimediaId));

            return result;
        }

        public List<Multimedia> FindMultimediaByName(string name) {
            string searchTerm = EscapeSearchTerm(name) + "%";
            var mapper = new GenericMapperBuilder<Multimedia>().Map("Extension", "FileExtension").build();
            List<Multimedia> results = StoredProcToList("spMultimediaFindByName", mapper, _P("txtSearchTerm", searchTerm));
            return results;
        }

        #endregion

        #region Notes

        public List<Note> GetNotes(string category, int intraCatID) {
            var mapper = new GenericMapperBuilder<Note>().PostMapAction((n) => {
                n.NoteCategory = category;
                n.IntraCatID = intraCatID;
            }).build();

            return StoredProcToList("spNoteList", mapper, _P("vchrCategory", category), _P("intIntraCatID", intraCatID));
        }

        public List<string> GetNoteTypesForCategory(string categoryName) {
            var results = new List<string>();
            StoredProcReaderForEach("spNoteTypeListForCategory", (reader) => {
                results.Add(reader["Note"] as string);
            }, _P("vchrCategory", categoryName));
            return results;
        }

        public void DeleteNote(int noteID) {
            StoredProcUpdate("spNoteDelete", _P("intNoteID", noteID));
        }

        public int InsertNote(string category, int intraCatID, string noteType, string note, string author, string comments, bool useInReports, int refID, string refPages) {
            var retval = ReturnParam("NewNoteID", SqlDbType.Int);
            StoredProcUpdate("spNoteInsert",
                _P("vchrCategory", category),
                _P("intIntraCatID", intraCatID),
                _P("vchrNoteType", noteType),
                _P("txtNote", note),
                _P("vchrAuthor", author),
                _P("txtComments", comments),
                _P("bitUseInReports", useInReports),
                _P("intRefID", refID),
                _P("vchrRefPages", refPages),
                retval);

            return (int)retval.Value;
        }

        public void UpdateNote(int noteID, string category, string noteType, string note, string author, string comments, bool useInReports, int refID, string refPages) {
            StoredProcUpdate("spNoteUpdate",
                _P("intNoteID", noteID),
                _P("vchrCategory", category),
                _P("vchrNoteType", noteType),
                _P("txtNote", note),
                _P("vchrAuthor", author),
                _P("txtComments", comments),
                _P("bitUseInReports", useInReports),
                _P("intRefID", refID),
                _P("vchrRefPages", refPages)
            );
        }

        #endregion        

        #region Phrases

        public List<PhraseCategory> GetPhraseCategories() {
            var mapper = new GenericMapperBuilder<PhraseCategory>().build();
            return StoredProcToList<PhraseCategory>("spPhraseCategoryList", mapper);
        }

        public int GetPhraseCategoryId(string categoryName, bool @fixed) {
            return StoredProcReturnVal<int>("spPhraseCategoryGetID", new SqlParameter("vchrCategory", categoryName), new SqlParameter("bitFixed", @fixed));
        }

        public List<Phrase> GetPhrases(int categoryId) {
            var mapper = new GenericMapperBuilder<Phrase>().Map("Phrase", "PhraseText").build();
            return StoredProcToList<Phrase>("spPhraseList", mapper, new SqlParameter("intCategory", categoryId));
        }

        public void AddPhrase(Phrase phrase) {
            // Obviously a copy-pasta error in the Stored Proc, as the return value is called NewRegionID...oh well...
            SqlParameter retval = ReturnParam("NewRegionID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spPhraseInsert", _P("intPhraseCatID", phrase.PhraseCatID), _P("vchrPhrase", phrase.PhraseText), retval);
            if (retval != null && retval.Value != null) {
                phrase.PhraseID = (Int32)retval.Value;
            }
        }

        public void RenamePhrase(int phraseId, string phrase) {
            StoredProcUpdate("spPhraseRename", _P("intPhraseID", phraseId), _P("vchrPhrase", phrase)); 
        }

        public void DeletePhrase(int phraseId) {
            StoredProcUpdate("spPhraseDelete", _P("intPhraseID", phraseId));
        }

        public void DeletePhraseCategory(int categoryId) {
            StoredProcUpdate("spPhraseCategoryDelete", _P("intCatID", categoryId));
        }

        #endregion

        #region References

        public Reference GetReference(int refID) {
            var mapper = new GenericMapperBuilder<Reference>().build();
            return StoredProcGetOne("spReferenceList", mapper, _P("vchrRefIDList", refID + ""));
        }

        public void DeleteReference(int refID) {
            StoredProcUpdate("spReferenceDelete", _P("intRefID", refID));
        }

        public int InsertReference(Reference r) {
            var retval = ReturnParam("NewRefID", SqlDbType.Int);
            StoredProcUpdate("spReferenceInsert",
                _P("vchrRefCode", r.RefCode),
			    _P("vchrAuthor", r.Author),
			    _P("vchrTitle", r.Title),
			    _P("vchrBookTitle", r.BookTitle),
			    _P("vchrEditor", r.Editor),
			    _P("vchrRefType", r.RefType),
			    _P("vchrYearOfPub", r.YearOfPub),
			    _P("vchrActualDate", r.ActualDate),
			    _P("intJournalID", r.JournalID),
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
			    _P("txtFullText", r.FullText),
			    _P("txtFullRTF", r.FullRTF),
			    _P("intStartPage", r.StartPage),
			    _P("intEndPage", r.EndPage),
                retval
            );
            
            return (int) retval.Value;
        }

        public void UpdateReference(Reference r) {
            StoredProcUpdate("spReferenceUpdate",
                _P("intRefID", r.RefID),
                _P("vchrRefCode", r.RefCode),
                _P("vchrAuthor", r.Author),
                _P("vchrTitle", r.Title),
                _P("vchrBookTitle", r.BookTitle),
                _P("vchrEditor", r.Editor),
                _P("vchrRefType", r.RefType),
                _P("vchrYearOfPub", r.YearOfPub),
                _P("vchrActualDate", r.ActualDate),
                _P("intJournalID", r.JournalID),
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
                _P("txtFullText", r.FullText),
                _P("txtFullRTF", r.FullRTF),
                _P("intStartPage", r.StartPage),
                _P("intEndPage", r.EndPage));

        }

        public List<ReferenceSearchResult> FindReferences(string refCode, string author, string year, string other) {
            var mapper = new GenericMapperBuilder<ReferenceSearchResult>().Map("FullRTF","RefRTF").build();
            return StoredProcToList("spReferenceFind", mapper,
                _P("vchrRefCode", refCode, DBNull.Value),
                _P("vchrAuthor", author, DBNull.Value),
                _P("vchrYear", year, DBNull.Value),
                _P("vchrOther", other, DBNull.Value));
        }

        public List<RefLink> GetReferenceLinks(string categoryName, int intraCatID) {
            var mapper = new GenericMapperBuilder<RefLink>().Map("intCatID", "CategoryID").Map("RefLink", "RefLinkType").PostMapAction((link) => {
                link.IntraCatID = intraCatID;
            }).build();

            return StoredProcToList("spRefLinkList", mapper,
                _P("vchrCategory", categoryName),
                _P("intIntraCatID", intraCatID));
        }

        public void UpdateRefLink(RefLink link, string categoryName) {
            StoredProcUpdate("spRefLinkUpdate",
                _P("intRefLinkID", link.RefLinkID),
                _P("vchrRefLink", link.RefLinkType),
                _P("vchrCategory", categoryName),
                _P("intRefID", link.RefID),
                _P("vchrRefPage", link.RefPage),
                _P("txtRefQual", link.RefQual),
                _P("intOrder", link.Order),
                _P("bitUseInReport", link.UseInReport));
        }

        public int InsertRefLink(RefLink link, string categoryName) {
            var retval = ReturnParam("RetVal", System.Data.SqlDbType.Int);
            StoredProcUpdate("spRefLinkInsert",
                _P("vchrCategory", categoryName),
                _P("intIntraCatID", link.IntraCatID),                
                _P("vchrRefLink", link.RefLinkType),                
                _P("intRefID", link.RefID),
                _P("vchrRefPage", link.RefPage),
                _P("txtRefQual", link.RefQual),
                _P("intOrder", link.Order),
                _P("bitUseInReport", link.UseInReport),
                retval);
            link.RefLinkID = (int)retval.Value;
            return (int)retval.Value;
        }

        public void DeleteRefLink(int refLinkID) {
            StoredProcUpdate("spRefLinkDelete", _P("intRefLinkID", refLinkID));
        }

        public IEnumerable<string> GetRefLinkTypes() {
            var list = new List<String>();
            StoredProcReaderForEach("spRefLinkTypeList", (reader) => {
                list.Add(reader["RefLink"] as string);
            });
            return list;
        }

        public int InsertRefLinkType(string linkType, string categoryName) {
            var retval = ReturnParam("intRefLinkTypeID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spRefLinkTypeGetSet",
                _P("vchrRefLinkType", linkType),
                _P("vchrCategory", categoryName),
                _P("intRefLinkTypeID", -1),
                retval);
            return (int)retval.Value;
        }

        public List<TaxonRefLink> GetTaxonRefLinks(int referenceID) {
            var mapper = new GenericMapperBuilder<TaxonRefLink>().PostMapAction((model) => {
                model.RefID = referenceID;
            }).build();
            return StoredProcToList("spRefLinkTaxonList", mapper, _P("intReferenceID", referenceID));
        }

        public void UpdateTaxonRefLink(TaxonRefLink link) {
            StoredProcUpdate("spRefLinkTaxonUpdate",
                _P("intRefLinkID", link.RefLinkID),
                _P("vchrRefLink", link.RefLink),
                _P("intBiotaID", link.BiotaID),
                _P("vchrRefPage", link.RefPage),
                _P("txtRefQual", link.RefQual),
                _P("bitUseInReport", link.UseInReports)
            );
        }

        #endregion

        #region AutoNumbers

        public List<AutoNumber> GetAutoNumbersForCategory(string category) {
            var mapper = new GenericMapperBuilder<AutoNumber>().build();
            return StoredProcToList("spAutoNumberCatList", mapper, _P("vchrCategory", category));
        }

        public NewAutoNumber GetNextAutoNumber(int autoNumberCatID, int seed, bool ensureUnique, string table, string field) {
            NewAutoNumber result = null;
            var mapper = new GenericMapperBuilder<NewAutoNumber>().build();
            bool finished = false;
            do {
                StoredProcReaderFirst("spAutoNumberGetNext", (reader) => {
                    result = mapper.Map(reader);
                    result.AutoNumberCatID = autoNumberCatID;
                },
                _P("intAutoNumberCatID", autoNumberCatID),
                _P("intSeed", seed));

                if (ensureUnique) {
                    finished = CheckAutoNumberUnique(result.FormattedNumber, table, field);
                    if (!finished) {
                        seed = -1;
                    }
                } 
            } while (!finished && ensureUnique);

            return result;
        }

        public int InsertAutoNumber(string category, string name, string prefix, string postfix, int numLeadingZeros, bool ensureUnique) {
            var retval = ReturnParam("identity", System.Data.SqlDbType.Int);
            StoredProcUpdate("spAutoNumberInsert",
                _P("vchrCategory", category),
                _P("vchrName", name),
                _P("vchrPrefix", prefix),
                _P("vchrPostfix", postfix),
                _P("intNumLeadingZeros", numLeadingZeros),
                _P("bitEnsureUnique", ensureUnique),
                retval);

            return (int) retval.Value;
        }

        public bool CheckAutoNumberUnique(string number, string table, string field) {
            int retval = StoredProcReturnVal<int>("spAutoNumberEnsureUnique", 
                _P("vchrNumber", number),
                _P("vchrFromTable", table),
                _P("vchrFieldName", field)
            );
            return retval == 1;
        }

        public void UpdateAutoNumber(int autoNumberCatID, string name, string prefix, string postfix, int numLeadingZeros, bool ensureUnique) {
            StoredProcUpdate("spAutoNumberUpdate",
                _P("intAutoNumberCatID", autoNumberCatID),
                _P("vchrName", name),
                _P("vchrPrefix", prefix),
                _P("vchrPostfix", postfix),
                _P("intNumLeadingZeros", numLeadingZeros),
                _P("bitEnsureUnique", ensureUnique)
            );
        }

        public void DeleteAutoNumberCategory(int autoNumberCatID) {
            StoredProcUpdate("spAutoNumberDelete", _P("intAutoNumberCatID", autoNumberCatID));
        }

        #endregion

        #region Favorites

        public List<T> GetTopFavorites<T>(FavoriteType type, bool global, GenericMapper<T> mapper) where T : Favorite, new() {
            var results = new List<T>();            
            StoredProcReaderForEach("spFavoriteListTop", (reader) => {
                var model = mapper.Map(reader);
                model.FavoriteType = type;
                results.Add(model);
            },
            _P("vchrType", type.ToString()),
            _P("bitListGlobal", global));
            return results;
        }

        public List<T> GetFavorites<T>(FavoriteType type, bool global, int parentFavoriteId, GenericMapper<T> mapper) where T : Favorite, new() {
            var results = new List<T>();
            StoredProcReaderForEach("spFavoriteList", (reader) => {
                var model = mapper.Map(reader);
                model.FavoriteType = type;
                results.Add(model);
            },
            _P("vchrType", type.ToString()),
            _P("intParentID", parentFavoriteId),
            _P("bitListGlobal", global));
            return results;
        }

        /// <summary>
        /// <para>Crikey!</para>
        /// <para>
        /// Ok, this function configures a mapper appropriate for the type (T) of favorite being retrieved. The mapping for favorites is complicated because:
        /// </para>
        /// <list type="bullet">
        /// <item><description>the columns in the favorites table do not line up with the columns retrieved by the stored procedure</description></item>
        /// <item><description>Each different type of favorite will yield a different set of columns</description></item>
        /// <item><description>Only a small set of columns are common across favorite, and not all desired common columns are returned (ID1 and ID2, for example).</description></item>
        /// </list>
        /// 
        /// <para>
        /// Of particular note the ID1 and ID2 columns. These columns hold the ID of the noun being held as a favorite. In the case of Taxa items ID1 is the taxon ID, and ID2 is unused
        /// For Site favorites (which can include material, visits, regions etc), ID1 holds the id of the database object, and ID2 holds a string describing what kind of object it is ("Site", for example).
        /// </para>
        /// <para>
        /// ID1 and ID2, therefore, are pretty important, but the stored proc doesn't return them explicitly, but rather returns their values in a "strongly" named column specific to each ID type (intTaxaID, for example).
        /// This is where the funky "Expression&lt;Func&lt;typeparamref name=",int&gt;&gt; params come in. These expressions describe which property of the strong type Favorite type (T) should be bound to ID1 and ID2.
        /// </para>
        /// <para>
        /// For example, in the case of Taxon favorites ID1 should be mapped to TaxonFavorite.TaxaID and ID2 is ignored.
        /// </para>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="isGlobal"></param>
        /// <param name="ID1Expr"></param>
        /// <param name="ID2Expr"></param>
        /// <returns></returns>
        internal GenericMapper<T> ConfigureFavoriteMapper<T>(GenericMapperBuilder<T> builder, bool isGlobal, Expression<Func<T, int>> ID1Expr, Expression<Func<T, string>> ID2Expr) where T : Favorite, new() {

            builder.Map("ElementIsGroup", "IsGroup");

            // ElementIsGroup comes back as a byte value, and needs to be coerced to a boolean...
            var cm = new ConvertingMapper("ElementIsGroup", new Converter<object, object>((o) => {
                if (o is byte) {
                    var b = (byte)o;
                    return b != 0;
                } else if (o is short) {
                    var s = (short)o;
                    return s != 0;
                }
                return null;
            }));

            builder.PostMapAction((favorite) => {
                favorite.IsGlobal = isGlobal;

                // Copy over the nominated value for ID1
                if (ID1Expr != null) {
                    var srcProp = (PropertyInfo)((MemberExpression)ID1Expr.Body).Member;
                    favorite.ID1 = (int) srcProp.GetValue(favorite, null);
                }

                // Copy over the nominated value for ID1
                if (ID2Expr != null) {
                    var srcProp = (PropertyInfo)((MemberExpression)ID2Expr.Body).Member;
                    favorite.ID2 = (string) srcProp.GetValue(favorite, null);
                }

            });

            builder.Override(cm);
            return builder.build();
        }

        public List<TaxonFavorite> GetTopTaxaFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<TaxonFavorite>(), global, fav => fav.TaxaID, null);
            return GetTopFavorites<TaxonFavorite>(FavoriteType.Taxa, global, mapper);
        }

        public List<TaxonFavorite> GetTaxaFavorites(int parentFavoriteId, bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<TaxonFavorite>(), global, fav => fav.TaxaID, null);
            return GetFavorites<TaxonFavorite>(FavoriteType.Taxa, global, parentFavoriteId, mapper);
        }

        public List<SiteFavorite> GetTopSiteFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<SiteFavorite>(), global, fav => fav.ElemID, fav => fav.ElemType);
            return GetTopFavorites<SiteFavorite>(FavoriteType.Site, global, mapper);
        }

        public List<SiteFavorite> GetSiteFavorites(int parentFavoriteId, bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<SiteFavorite>(), global, fav => fav.ElemID, fav => fav.ElemType);
            return GetFavorites<SiteFavorite>(FavoriteType.Site, global, parentFavoriteId, mapper);
        }

        public List<ReferenceFavorite> GetTopReferenceFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<ReferenceFavorite>(), global, fav => fav.RefID, null);
            return GetTopFavorites<ReferenceFavorite>(FavoriteType.Reference, global, mapper);
        }

        public List<ReferenceFavorite> GetReferenceFavorites(int parentFavoriteId, bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<ReferenceFavorite>(), global, fav => fav.RefID, null);
            return GetFavorites<ReferenceFavorite>(FavoriteType.Reference, global, parentFavoriteId, mapper);
        }

        public List<DistRegionFavorite> GetTopDistRegionFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<DistRegionFavorite>(), global, fav => fav.DistRegionID, null);
            return GetTopFavorites<DistRegionFavorite>(FavoriteType.DistRegion, global, mapper);
        }

        public List<BiotaStorageFavorite> GetTopBiotaStorageFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<BiotaStorageFavorite>(), global, fav=>fav.BiotaStorageID, null);
            return GetTopFavorites<BiotaStorageFavorite>(FavoriteType.BiotaStorage, global, mapper);
        }

        public void DeleteFavorite(int favoriteID) {
            StoredProcUpdate("spFavoriteDelete", _P("intFavoriteID", favoriteID));
        }

        public int InsertFavoriteGroup(FavoriteType favType, int parentID, string name, bool global) {
            var retval = ReturnParam("intNewFavoriteID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spFavoriteInsert",
                _P("vchrType", favType.ToString()),
                _P("intParentID", parentID),
                _P("tintGroup", 1),
                _P("vchrGroupName", name),
                _P("intID1", 0),
                _P("vchrID2", ""),
                _P("bitGlobal", global),
                retval
            );

            return (int) retval.Value;
        }

        public void RenameFavoriteGroup(int favoriteID, string newName) {
            StoredProcUpdate("spFavoriteRename",
                _P("intFavoriteID", favoriteID),
                _P("vchrName", newName)
            );
        }

        public void MoveFavorite(int favoriteID, int newParentID) {
            StoredProcUpdate("spFavoriteMove",
                _P("intFavoriteID", favoriteID),
                _P("intNewParentID", newParentID)
            );
        }

        public int InsertFavorite(FavoriteType favType, int parentID, int id1, string id2, bool global) {
            var retval = ReturnParam("intNewFavoriteID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spFavoriteInsert",
                _P("vchrType", favType.ToString()),
                _P("intParentID", parentID),
                _P("tintGroup", 0),
                _P("vchrGroupName", null),
                _P("intID1", id1),
                _P("vchrID2", id2),
                _P("bitGlobal", global),
                retval
            );

            return (int)retval.Value;
        }

        #endregion

        #region Associates

        public List<Associate> GetAssociates(string category, params int[] intraCatIDs) {
            var mapper = new GenericMapperBuilder<Associate>().build();
            var ids = intraCatIDs.Join(",");
            return StoredProcToList("spAssociateLoadFromList", mapper, _P("vchrCategory", category), _P("txtIntraCatIDList", ids));
        }

        public int InsertAssociate(Associate a) {
            var retval = ReturnParam("NewAssociateID", SqlDbType.Int);
            StoredProcUpdate("spAssociateInsert",
                _P("intFromIntraCatID", a.FromIntraCatID),
                _P("FromCategory", a.FromCategory),
                _P("intToIntraCatID", a.ToIntraCatID),
                _P("ToCategory", a.ToCategory),
                _P("txtAssocDescription", a.AssocDescription),
                _P("vchrRelationFromTo", a.RelationFromTo),
                _P("vchrRelationToFrom", a.RelationToFrom),
                _P("intPoliticalRegionID", a.PoliticalRegionID),
                _P("vchrSource", a.Source),
                _P("intRefID", a.RefID),
                _P("vchrRefPage", a.RefPage),
                _P("bitUncertain", a.Uncertain),
                _P("txtNotes", a.Notes),
                retval
            );

            return (int)retval.Value;
        }

        public void UpdateAssociate(Associate a) {

            StoredProcUpdate("spAssociateUpdate",
                _P("intAssociateID", a.AssociateID),
                _P("intFromIntraCatID", a.FromIntraCatID),
                _P("FromCategory", a.FromCategory),
                _P("intToIntraCatID", a.ToIntraCatID),
                _P("ToCategory", a.ToCategory),
                _P("txtAssocDescription", a.AssocDescription),
                _P("vchrRelationFromTo", a.RelationFromTo),
                _P("vchrRelationToFrom", a.RelationToFrom),
                _P("intPoliticalRegionID", a.PoliticalRegionID),
                _P("vchrSource", a.Source),
                _P("intRefID", a.RefID),
                _P("vchrRefPage", a.RefPage),
                _P("bitUncertain", a.Uncertain),
                _P("txtNotes", a.Notes)
            );
        }

        public void DeleteAssociate(int associateId) {
            StoredProcUpdate("spAssociateDelete", _P("intAssociateID", associateId));
        }


        #endregion

        #region Journals
        
        public Journal GetJournal(int journalID) {
            var mapper = new GenericMapperBuilder<Journal>().build();
            return StoredProcGetOne("spJournalGet", mapper, _P("intJournalID", journalID));
        }

        public int InsertJournal(Journal journal) {
            var retval = ReturnParam("NewJournalID", SqlDbType.Int);
            StoredProcUpdate("spJournalInsert",
                _P("vchrAbbrevName", journal.AbbrevName),
                _P("vchrAbbrevName2", journal.AbbrevName2),
                _P("vchrAlias", journal.Alias),
                _P("vchrFullName", journal.FullName),
                _P("txtNotes", journal.Notes),
                retval
            );
            return (int)retval.Value;
        }

        public void UpdateJournal(Journal journal) {
            StoredProcUpdate("spJournalUpdate",
                _P("intJournalID", journal.JournalID),
                _P("vchrAbbrevName", journal.AbbrevName),
                _P("vchrAbbrevName2", journal.AbbrevName2),
                _P("vchrAlias", journal.Alias),
                _P("vchrFullName", journal.FullName),
                _P("txtNotes", journal.Notes)
            );
        }

        public void DeleteJournal(int journalID) {
            StoredProcUpdate("spJournalDelete", _P("intJournalID", journalID));
        }

        public List<Journal> FindJournals(string criteria) {
            var mapper = new GenericMapperBuilder<Journal>().build();
            return StoredProcToList("spJournalFind", mapper, _P("vchrCriteria", criteria + "%"));
        }

        public List<Journal> ListJournalRange(string where) {
            var mapper = new GenericMapperBuilder<Journal>().build();
            return StoredProcToList("spJournalListRange", mapper, _P("vchrWhere", where));
        }

        public List<Journal> LookupJournal(string filter) {
            var mapper = new GenericMapperBuilder<Journal>().build();
            return StoredProcToList("spJournalLookup", mapper, _P("vchrFilter", filter));
        }

        public Boolean OkToDeleteJournal(int journalID) {            
            int refcount = StoredProcReturnVal<int>("spJournalOkToDelete", _P("intJournalID", journalID));
            return refcount == 0;
        }

        #endregion

        #region Query Tool

        public List<FieldDescriptor> GetFieldMappings() {

            // Start off with the base list of static fields...
            var list = new List<FieldDescriptor>(FieldDescriptors);

            // Now need to add trait fields for key nouns
            list.AddRange(ExtractTraits("Taxon", "Nomenclature", "tblBiota"));
            list.AddRange(ExtractTraits("Region", "PoliticalRegion", "tblPoliticalRegion"));
            list.AddRange(ExtractTraits("Site", "Site", "tblSite"));
            list.AddRange(ExtractTraits("SiteVisit", "SiteVisit", "tblSiteVisit"));
            list.AddRange(ExtractTraits("Material", "Material", "tblMaterial"));

            return list;
        }

        private List<FieldDescriptor> ExtractTraits(string traitCategory, string fieldCategory, string table) {

            var list = new List<FieldDescriptor>();

            StoredProcReaderForEach("spTraitTypeListForCategory", (reader) => {
                var desc = new FieldDescriptor { TableName = String.Format("{0}.{1}.{2}", table, reader["ID"], reader["CategoryID"]) };
                desc.FieldName = reader["Trait"] as string;
                desc.Category = fieldCategory;
                desc.DisplayName = desc.FieldName;
                desc.Description = "Trait category";

                list.Add(desc);
                
            }, _P("vchrCategory", traitCategory));

            return list;
        }

        public string GenerateQuerySQL(IEnumerable<QueryCriteria> criteria, bool distinct) {
            var query = QuerySQLGenerator.GenerateSQL(User, criteria, distinct);
            var sql = String.Format("SELECT {0}\nFROM {1}\nWHERE {2}\n", query.Select, query.From, query.Where);
            return sql;
        }

        public DataMatrix ExecuteQuery(IEnumerable<QueryCriteria> criteria, bool distinct) {
            var query = QuerySQLGenerator.GenerateSQL(User, criteria, distinct);
            return StoredProcDataMatrix("spQuerySelect", _P("txtSELECT", query.SelectHidden), _P("txtFROM", query.From), _P("txtWHERE", query.Where));
        }

        #endregion

        #region User Manager

        public List<UserSearchResult> GetUsers() {
            var mapper = new GenericMapperBuilder<UserSearchResult>().build();
            return StoredProcToList<UserSearchResult>("spUserListWithIds", mapper);
        }

        public List<Group> GetGroups() {
            var mapper = new GenericMapperBuilder<Group>().build();
            return StoredProcToList<Group>("spGroupList", mapper);
        }

        public BiolinkUser GetUser(string username) {
            var mapper = new GenericMapperBuilder<BiolinkUser>().build();
            return StoredProcGetOne<BiolinkUser>("spUserGet", mapper, _P("vchrUserName", username));
        }

        public List<Permission> GetPermissions(int groupID) {
            var mapper = new GenericMapperBuilder<Permission>().build();
            var list = StoredProcToList("spPermissionList", mapper, _P("intGroupID", groupID));
            return list;
        }

        public bool HasBiotaPermission(int taxonID, PERMISSION_MASK mask) {
            var ret = StoredProcReturnVal<int>("spUserHasBiotaPermission", _P("vchrUsername", User.Username), _P("intBiotaID", taxonID), _P("intRequiredPermissionMask", (int)mask));
            // SP's return 0 when they succeed !
            return ret == 0;
        }

        public void InsertUser(BiolinkUser user) {
            
            StoredProcUpdate("spUserInsert",
                _P("vchrUsername", user.UserName),
                _P("vchrPassword", User.ManglePassword(user.UserName, user.Password)),
                _P("vchrFullname", user.FullName),
                _P("vchrDescription", user.Description),
                _P("vchrNotes", user.Notes),
                _P("intGroupID", user.GroupID),
                _P("bitCanCreateUsers", user.CanCreateUsers)
            );
        }

        public void UpdateUser(BiolinkUser user) {
            StoredProcUpdate("spUserUpdate",
                _P("vchrUsername", user.UserName),                
                _P("vchrFullname", user.FullName),
                _P("vchrDescription", user.Description),
                _P("vchrNotes", user.Notes),
                _P("intGroupID", user.GroupID),
                _P("bitCanCreateUsers", user.CanCreateUsers)
            );
        }

        public void UpdateUserPassword(string username, string password) {
            StoredProcUpdate("spUserPasswordUpdate",
                _P("vchrUsername", username),
                _P("vchrPassword", User.ManglePassword(username, password))
            );
        }

        public void DeleteUser(string username) {
            StoredProcUpdate("spUserDelete", _P("vchrUsername", username));
        }

        public int InsertGroup(string groupName) {
            var retval = ReturnParam("intNewGroupID");
            StoredProcUpdate("spGroupInsert", _P("vchrGroupName", groupName), retval);
            return (int)retval.Value;
        }

        public void RenameGroup(int groupID, string newName) {
            StoredProcUpdate("spGroupUpdate",
                _P("intGroupID", groupID),
                _P("vchrGroupName", newName)
            );
        }

        public void DeleteGroup(int groupID) {
            StoredProcUpdate("spGroupDelete", _P("intGroupID", groupID));
        }

        public void CopyGroupPermissions(int sourceGroupID, int targetGroupID) {
            StoredProcUpdate("spPermissionCopyGroup", _P("intFromGroupID", sourceGroupID), _P("intToGroupID", targetGroupID));
        }

        public void SetPermission(int groupID, int permissionID, int mask1, int mask2 = 0) {
            StoredProcUpdate("spPermissionSet", _P("intGroupID", groupID), _P("intPermID", permissionID), _P("intPermMask1", mask1), _P("intPermMask2", mask2));
        }

        #endregion

        #region Import

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


        #endregion

        #region LookUp

        public List<LookupResult> LookupSearch(string filter, LookupType lookupType) {

            filter = EscapeSearchTerm(filter);

            string storedProc = null;
            string paramName = "vchrFilter";
            switch (lookupType) {
                case LookupType.Taxon:
                    storedProc = "spBiotaLookup";
                    break;
                case LookupType.Journal:
                    storedProc = "spJournalLookup";
                    break;
                case LookupType.Material:
                    storedProc = "spMaterialLookup";
                    break;
                case LookupType.Reference:
                    storedProc = "spReferenceLookup";
                    break;
                case LookupType.Region:
                    storedProc = "spRegionLookup";
                    break;
                case LookupType.Site:
                    storedProc = "spSiteLookup";
                    break;
                case LookupType.SiteVisit:
                    storedProc = "spSiteVisitLookup";
                    break;
                case LookupType.Unknown:
                case LookupType.Trap:
                case LookupType.PlaceName:
                    break;
            }

            if (storedProc != null) {
                var results = new List<LookupResult>();
                StoredProcReaderForEach(storedProc, (reader) => {
                    var model = new LookupResult();
                    model.LookupType = lookupType;
                    model.LookupObjectID = (int) reader[0];
                    model.Label = (string) reader[1];
                    results.Add(model);
                }, _P(paramName, filter));
                return results;

            }
            return null;
        }

        #endregion
    }

    public class RefTypeMapping {

        public RefTypeMapping(string code, string name) {
            this.RefTypeCode = code;
            this.RefTypeName = name;
        }

        public string RefTypeCode { get; set; }
        public string RefTypeName { get; set; }
    }

    public enum LookupType {
        Unknown,
        Taxon,
        Region,
        Material,
        Site,
        SiteVisit,
        Trap,
        Reference,
        Journal,
        PlaceName
    }


}
