using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Windows.Data;

namespace BioLink.Client.Extensibility {

    public class TraitViewModel : GenericViewModelBase<Trait> {

        public TraitViewModel(Trait t)  : base(t, ()=>t.TraitID) { }

        public override string DisplayLabel {
            get { return Name; }
        }

        public int TraitID {
            get { return Model.TraitID; }
            set { SetProperty(() => Model.TraitID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string DataType {
            get { return Model.DataType; }
            set { SetProperty(() => Model.DataType, value); }
        }

        public string Validation {
            get { return Model.Validation; }
            set { SetProperty(() => Model.Validation, value); }
        }

        public int IntraCatID {
            get { return Model.IntraCatID; }
            set { SetProperty(() => Model.IntraCatID, value); }
        }

        public string Value {
            get { return Model.Value; }
            set { SetProperty(() => Model.Value, value); }
        }

        public string Comment {
            get { return Model.Comment; }
            set { SetProperty(() => Model.Comment, value); }
        }

    }

    [ValueConversion(typeof(string), typeof(string))]
    public class PlainTextCommentConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return RTFUtils.StripMarkup(value as string);            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }


}
