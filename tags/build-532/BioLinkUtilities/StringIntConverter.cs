using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace BioLink.Client.Utilities {



    public class StringIntConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            if (value != null) {
                Int32 integer;
                if (Int32.TryParse(value.ToString(), out integer)) {
                    return integer;
                }

                Double dbl;
                if (Double.TryParse(value.ToString(), out dbl)) {
                    return (Int32)dbl;
                }
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value == null ? "" : value.ToString();
        }
    }

    public class IntDoubleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (typeof(double).IsAssignableFrom(value.GetType())) {
                return (int) (double) value;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (typeof(int).IsAssignableFrom(value.GetType())) {
                return (double) (int) value;
            }
            return 0;
        }
    }
}
