using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace BioLink.Client.Utilities {

    public class ByteConverter : IValueConverter {

        private const int SCALE = 1024;
        private static readonly string[] UNITS = new string[] { "TB", "GB", "MB", "KB", "Bytes" };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int bytes = (int)value;
            return FormatBytes(bytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public static string FormatBytes(long bytes) {
            long max = (long)Math.Pow(SCALE, UNITS.Length - 1);
            foreach (string order in UNITS) {
                if (bytes > max) {
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);
                }
                max /= SCALE;
            }
            return "0 Bytes";
        }

    }
}
