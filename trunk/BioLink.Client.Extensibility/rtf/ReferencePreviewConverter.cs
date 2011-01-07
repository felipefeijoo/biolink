using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace BioLink.Client.Extensibility {

    public class ReferencePreviewConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string rtf = value as string;

            if (rtf != null && (!rtf.StartsWith(@"{{\rtf") && (!rtf.StartsWith(@"{\rtf")))) {
                rtf = string.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deftab720 {{\fonttbl{{\f0\fswiss Arial;}}}} \plain\f0\fs16 {0} }}", rtf);
            }

            return rtf;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
