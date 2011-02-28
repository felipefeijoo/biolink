using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using System.Windows;

namespace BioLink.Client.Material {

    public class SiteViewModel : GenericOwnedViewModel<Site> {

        public SiteViewModel(Site model) : base(model, ()=>model.SiteID) { }

        public int SiteID {
            get { return Model.SiteID; }
            set { SetProperty(() => Model.SiteID, value); }
        }

        public string SiteName {
            get { return Model.SiteName; }
            set { SetProperty(() => Model.SiteName, value); }
        }

        public int PoliticalRegionID {
            get { return Model.PoliticalRegionID; }
            set { SetProperty(() => Model.PoliticalRegionID, value); }
        }

        public int SiteGroupID {
            get { return Model.SiteGroupID; }
            set { SetProperty(() => Model.SiteGroupID, value); }
        }        

        public int LocalityType {
            get { return Model.LocalityType; }
            set { SetProperty(() => Model.LocalityType, value); }
        }        

        public string Locality {
            get { return Model.Locality; }
            set { SetProperty(() => Model.Locality, value); }
        }

        public string DistanceFromPlace {
            get { return Model.DistanceFromPlace; }
            set { SetProperty(() => Model.DistanceFromPlace, value); }
        }

        public string DirFromPlace {
            get { return Model.DirFromPlace; }
            set { SetProperty(() => Model.DirFromPlace, value); }
        }

        public string InformalLocal {
            get { return Model.InformalLocal; }
            set { SetProperty(() => Model.InformalLocal, value); }
        }

        public int PosCoordinates {
            get { return Model.PosCoordinates; }
            set { SetProperty(() => Model.PosCoordinates, value); }
        }

        public int PosAreaType {
            get { return Model.PosAreaType; }
            set { SetProperty(() => Model.PosAreaType, value); }
        }

        public double? PosY1 {
            get { return Model.PosY1; }
            set { SetProperty(() => Model.PosY1, value); }
        }

        public double? PosX1 {
            get { return Model.PosX1; }
            set { SetProperty(() => Model.PosX1, value); }
        }

        public double? PosY2 {
            get { return Model.PosY2; }
            set { SetProperty(() => Model.PosY2, value); }
        }

        public double? PosX2 {
            get { return Model.PosX2; }
            set { SetProperty(() => Model.PosX2, value); }
        }

        public int PosXYDisplayFormat {
            get { return Model.PosXYDisplayFormat; }
            set { SetProperty(() => Model.PosXYDisplayFormat, value); }
        }

        public string PosSource {
            get { return Model.PosSource; }
            set { SetProperty(() => Model.PosSource, value); }
        }

        public string PosError {
            get { return Model.PosError; }
            set { SetProperty(() => Model.PosError, value); }
        }

        public string PosWho {
            get { return Model.PosWho; }
            set { SetProperty(() => Model.PosWho, value); }
        }

        public string PosDate {
            get { return Model.PosDate; }
            set { SetProperty(() => Model.PosDate, value); }
        }

        public string PosOriginal {
            get { return Model.PosOriginal; }
            set { SetProperty(() => Model.PosOriginal, value); }
        }

        public string PosUTMSource {
            get { return Model.PosUTMSource; }
            set { SetProperty(() => Model.PosUTMSource, value); }
        }

        public string PosUTMMapProj {
            get { return Model.PosUTMMapProj; }
            set { SetProperty(() => Model.PosUTMMapProj, value); }
        }

        public string PosUTMMapName {
            get { return Model.PosUTMMapName; }
            set { SetProperty(() => Model.PosUTMMapName, value); }
        }

        public string PosUTMMapVer {
            get { return Model.PosUTMMapVer; }
            set { SetProperty(() => Model.PosUTMMapVer, value); }
        }

        public int ElevType {
            get { return Model.ElevType; }
            set { SetProperty(() => Model.ElevType, value); }
        }

        public double? ElevUpper {
            get { return Model.ElevUpper; }
            set { SetProperty(() => Model.ElevUpper, value); }
        }

        public double? ElevLower {
            get { return Model.ElevLower; }
            set { SetProperty(() => Model.ElevLower, value); }
        }

        public double? ElevDepth {
            get { return Model.ElevDepth; }
            set { SetProperty(()=>Model.ElevDepth, value); }
        }

        public string ElevUnits {
            get { return Model.ElevUnits; }
            set { SetProperty(() => Model.ElevUnits, value); }
        }

        public string ElevSource {
            get { return Model.ElevSource; }
            set { SetProperty(() => Model.ElevSource, value); }
        }

        public string ElevError {
            get { return Model.ElevError; }
            set { SetProperty(() => Model.ElevError, value); }
        }

        public string GeoEra {
            get { return Model.GeoEra; }
            set { SetProperty(() => Model.GeoEra, value); }
        }

        public string GeoState {
            get { return Model.GeoState; }
            set { SetProperty(() => Model.GeoState, value); }
        }

        public string GeoPlate {
            get { return Model.GeoPlate; }
            set { SetProperty(() => Model.GeoPlate, value); }
        }

        public string GeoFormation {
            get { return Model.GeoFormation; }
            set { SetProperty(() => Model.GeoFormation, value); }
        }

        public string GeoMember {
            get { return Model.GeoMember; }
            set { SetProperty(() => Model.GeoMember, value); }
        }

        public string GeoBed {
            get { return Model.GeoBed; }
            set { SetProperty(() => Model.GeoBed, value); }
        }

        public string GeoName {
            get { return Model.GeoName; }
            set { SetProperty(() => Model.GeoName, value); }
        }

        public string GeoAgeBottom {
            get { return Model.GeoAgeBottom; }
            set { SetProperty(() => Model.GeoAgeBottom, value); }
        }

        public string GeoAgeTop {
            get { return Model.GeoAgeTop; }
            set { SetProperty(() => Model.GeoAgeTop, value); }
        }

        public string GeoNotes {
            get { return Model.GeoNotes; }
            set { SetProperty(() => Model.GeoAgeBottom, value); }
        }

        public int Order {
            get { return Model.Order; }
            set { SetProperty(() => Model.Order, value); }
        }

        public bool IsTemplate {
            get { return Model.IsTemplate; }
            set { SetProperty(() => Model.IsTemplate, value); }
        }

        public string PoliticalRegion {
            get { return Model.PoliticalRegion; }
            set { SetProperty(() => Model.PoliticalRegion, value); }
        }

    }
}
