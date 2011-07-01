using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace BioLink.Client.Extensibility {

    public class RadioButtonIntValueConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string param = parameter as string;
            if (!string.IsNullOrEmpty(param)) {
                int val;
                if (int.TryParse(param, out val)) {
                    return val.Equals(value);
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            int val;
            if (int.TryParse(parameter as string, out val)) {
                return val;
            }
            throw new Exception("Unexepected paramter value: " + parameter);
        }

    } 
}
