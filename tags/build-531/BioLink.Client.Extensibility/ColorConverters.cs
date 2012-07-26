using System;
using System.Windows.Data;
using System.Windows.Media;

namespace BioLink.Client.Extensibility {

    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColorToSolidColorBrushConverter : IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ValueConversion(typeof(Color), typeof(System.Drawing.Color))]
    public class ColorToSystemDrawingColorConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var c = (Color)value;
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var c = (System.Drawing.Color)value;
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }

    [ValueConversion(typeof(System.Drawing.Color), typeof(Color))]
    public class SystemDrawingColorToColorConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var c = (System.Drawing.Color)value;
            return Color.FromArgb(c.A, c.R, c.G, c.B);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var c = (Color)value;
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
