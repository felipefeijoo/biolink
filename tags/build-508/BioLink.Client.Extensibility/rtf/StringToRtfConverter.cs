/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
