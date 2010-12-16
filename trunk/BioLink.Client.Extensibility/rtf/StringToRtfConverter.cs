using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace BioLink.Client.Extensibility {

    public class StringToRtfConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null) {
                var flowDocument = new FlowDocument();
                var rtf = value.ToString();

                using (var stream = new MemoryStream((new UTF8Encoding()).GetBytes(rtf))) {
                    var text = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    text.Load(stream, DataFormats.Rtf);
                }

                return flowDocument;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null) {
                var flowDocument = (FlowDocument)value;
                var range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                string rtf;

                using (var stream = new MemoryStream()) {
                    range.Save(stream, DataFormats.Rtf);
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream)) {
                        rtf = reader.ReadToEnd();
                    }
                }

                return rtf;
            }
            return value;
        }
    }

}
