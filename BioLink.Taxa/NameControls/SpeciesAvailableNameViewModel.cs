using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using System.Windows;
using System.Windows.Data;

namespace BioLink.Client.Taxa {

    public class SpeciesAvailableNameViewModel : GenericViewModelBase<SpeciesAvailableName> {

        public SpeciesAvailableNameViewModel(SpeciesAvailableName model) : base(model, null) { }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public int? RefID {
            get { return Model.RefID; }
            set { SetProperty(() => Model.RefID, value); }
        }

        public string RefPage {
            get { return Model.RefPage; }
            set { SetProperty(() => Model.RefPage, value); }
        }

        public string RefQual {
            get { return Model.RefQual; }
            set { SetProperty(() => Model.RefQual, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string AvailableNameStatus {
            get { return Model.AvailableNameStatus; }
            set { SetProperty(() => Model.AvailableNameStatus, value); }
        }

        public string PrimaryType {
            get { return Model.PrimaryType; }
            set { SetProperty(() => Model.PrimaryType, value); }
        }

        public string SecondaryType {
            get { return Model.SecondaryType; }
            set { SetProperty(() => Model.SecondaryType, value); }
        }

        public bool PrimaryTypeProbable {
            get { return Model.PrimaryTypeProbable; }
            set { SetProperty(() => Model.PrimaryTypeProbable, value); }
        }

        public bool SecondaryTypeProbable {
            get { return Model.SecondaryTypeProbable; }
            set { SetProperty(() => Model.SecondaryTypeProbable, value); }
        }

    }

    public class SANTypeDataViewModel : GenericViewModelBase<SANTypeData> {

        public SANTypeDataViewModel(SANTypeData model) : base(model, ()=>model.SANTypeDataID) { }

        public int SANTypeDataID {
            get { return Model.SANTypeDataID; }
            set { SetProperty(() => Model.SANTypeDataID, value); }
        }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public string Type {
            get { return Model.Type; }
            set { SetProperty(() => Model.Type, value); }
        }

        public string Museum {
            get { return Model.Museum; }
            set { SetProperty(() => Model.Museum, value); }
        }

        public string AccessionNumber {
            get { return Model.AccessionNumber; }
            set { SetProperty(() => Model.AccessionNumber, value); }
        }

        public string Material {
            get { return Model.Material; }
            set { SetProperty(() => Model.Material, value); }
        }

        public string Locality {
            get { return Model.Locality; }
            set { SetProperty(() => Model.Locality, value); }
        }

        public bool IDConfirmed {
            get { return Model.IDConfirmed; }
            set { SetProperty(() => Model.IDConfirmed, value); }
        }

        public int? MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public string MaterialName {
            get { return Model.MaterialName; }
            set { SetProperty(() => Model.MaterialName, value); }
        }

    }

    public class SANTypeDataTypeConverter : IValueConverter {



        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var list = parameter as List<SANTypeDataType>;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var list = parameter as List<SANTypeDataType>;
            return null;
        }
    }


}
