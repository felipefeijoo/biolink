using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using System.Windows;
using System.Windows.Media;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {

    public abstract class RDEViewModel<T> : GenericViewModelBase<T> where T: RDEObject {

        public RDEViewModel(T model, Expression<Func<int>> objectIDExpr) : base(model, objectIDExpr) {
            Traits = new List<Trait>();
        }

        public int? TemplateID {
            get { return Model.TemplateID; }
            set { SetProperty(() => Model.TemplateID, value); }
        }

        // Locked and Changes are not a really database properties, and we don't really care if they have been changed or not - they never get saved anyway, so 
        // we treat them like regular properties (no changed detection).
        public bool Locked {
            get { return Model.Locked; }
            set { 
                Model.Locked = value;
                RaisePropertyChanged("HeaderForeground");
                RaisePropertyChanged("Locked");
            }
        }

        public int? Changes {
            get { return Model.Changes; }
            set { Model.Changes = value; }
        }

        public Brush HeaderForeground {
            get {
                if (!Locked) {
                    if (ObjectID < 0) {
                        return Brushes.Blue;
                    } else {
                        return Brushes.Red;
                    }
                }
                return SystemColors.ControlTextBrush;
            }
        }

        public List<Trait> Traits { get; set; }

    }

    public class RDESiteViewModel : RDEViewModel<RDESite> {

        public RDESiteViewModel(RDESite model) : base(model, ()=>model.SiteID) {
            SiteVisits = new ObservableCollection<ViewModelBase>();
        }

        public int SiteID {
            get { return Model.SiteID; }
            set { SetProperty(() => Model.SiteID, value); }
        }

        public int ParentID {
            get { return Model.ParentID; }
            set { SetProperty(() => Model.ParentID, value); }
        }

        public string SiteName {
            get { return Model.SiteName; }
            set { SetProperty(() => Model.SiteName, value); }
        }

        public byte? LocalType {
            get { return Model.LocalType; }
            set { SetProperty(() => Model.LocalType, value); }
        }

        public string Locality {
            get { return Model.Locality; }
            set { SetProperty(() => Model.Locality, value); }
        }

        public int? PoliticalRegionID {
            get { return Model.PoliticalRegionID; }
            set { SetProperty(() => Model.PoliticalRegionID, value); }
        }

        public string PoliticalRegion {
            get { return Model.PoliticalRegion; }
            set { SetProperty(() => Model.PoliticalRegion, value); }
        }

        public byte? PosAreaType {
            get { return Model.PosAreaType; }
            set { SetProperty(() => Model.PosAreaType, value); }
        }

        public byte? PosCoordinates {
            get { return Model.PosCoordinates; }
            set { SetProperty(() => Model.PosCoordinates, value); }
        }

        public double? Longitude {
            get { return Model.Longitude; }
            set { SetProperty(()=>Model.Longitude, value); }
        }

        public double? Latitude { 
            get { return Model.Latitude; }
            set { SetProperty(()=>Model.Latitude, value); }
        }

        public double? Longitude2 { 
            get { return Model.Longitude2; }
            set { SetProperty(()=>Model.Longitude2, value); }
        }

        public double? Latitude2 { 
            get { return Model.Latitude2; }
            set { SetProperty(() => Model.Latitude2, value); }
        }

        public byte? ElevType {
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

        public string LLSource {
            get { return Model.LLSource; }
            set { SetProperty(() => Model.LLSource, value); }
        }

        public string LLError {
            get { return Model.LLError; }
            set { SetProperty(() => Model.LLError, value); }
        }

        public ObservableCollection<ViewModelBase> SiteVisits { get; set; }

    }
}
