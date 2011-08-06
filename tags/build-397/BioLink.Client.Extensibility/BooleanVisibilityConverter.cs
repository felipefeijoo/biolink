using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace BioLink.Client.Extensibility {

    public class BooleanVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var boolVal = value as bool?;
            if (boolVal != null && boolVal.HasValue && boolVal.Value) {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var v = (Visibility) value;
            if (v == Visibility.Collapsed || v == Visibility.Hidden) {
                return false;
            }

            return true;
        }
    }
}
