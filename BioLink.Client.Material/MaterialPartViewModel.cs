using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using BioLink.Data;
using System.Windows;

namespace BioLink.Client.Material {
    public class MaterialPartViewModel : GenericViewModelBase<MaterialPart> {

        public MaterialPartViewModel(MaterialPart model) : base(model, ()=>model.MaterialPartID) { }

        public override string DisplayLabel {
            get {
                return String.Format("{0}", PartName, NoSpecimens);
            }
        }

        public int MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public int MaterialPartID {
            get { return Model.MaterialPartID; }
            set { SetProperty(() => Model.MaterialPartID, value); }
        }

        public string PartName {
            get { return Model.PartName; }
            set { SetProperty(() => Model.PartName, value); }
        }

        public string SampleType {
            get { return Model.SampleType; }
            set { SetProperty(() => Model.SampleType, value); }
        }

        public int? NoSpecimens {
            get { return Model.NoSpecimens; }
            set { SetProperty(() => Model.NoSpecimens, value); }
        }

        public string NoSpecimensQual {
            get { return Model.NoSpecimensQual; }
            set { SetProperty(() => Model.NoSpecimensQual, value); }
        }

        public string Lifestage {
            get { return Model.Lifestage; }
            set { SetProperty(() => Model.Lifestage, value); }
        }

        public string Gender {
            get { return Model.Gender; }
            set { SetProperty(() => Model.Gender, value); }
        }

        public string RegNo {
            get { return Model.RegNo; }
            set { SetProperty(() => Model.RegNo, value); }
        }

        public string Condition {
            get { return Model.Condition; }
            set { SetProperty(() => Model.Condition, value); }
        }

        public string StorageSite {
            get { return Model.StorageSite; }
            set { SetProperty(() => Model.StorageSite, value); }
        }

        public string StorageMethod {
            get { return Model.StorageMethod; }
            set { SetProperty(() => Model.StorageMethod, value); }

        }

        public string CurationStatus {
            get { return Model.CurationStatus; }
            set { SetProperty(() => Model.CurationStatus, value); }
        }

        public string NoOfUnits {
            get { return Model.NoOfUnits; } 
            set { SetProperty(()=> Model.NoOfUnits, value); }
        }
        
        public string Notes {
            get { return Model.Notes; }
            set { SetProperty(() => Model.Notes, value); }
        }

        public bool OnLoan {
            get { return Model.OnLoan; }
            set { SetProperty(() => Model.OnLoan, value); }
        }

        public int? BasedOnID {
            get { return Model.BasedOnID; }
            set { SetProperty(() => Model.BasedOnID, value); }
        }

        public static readonly DependencyProperty LockedProperty = DependencyProperty.Register("Locked", typeof(bool), typeof(MaterialPartViewModel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool Locked {
            get { return (bool)GetValue(LockedProperty); }
            set { SetValue(LockedProperty, value); }
        }

    }
}
