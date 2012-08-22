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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class ReferencePreviewConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            string rtf = RTFUtils.filter(value as string, true, false, "b", "i", "sub", "super", "strike", "ul", "ulnone", "nosupersub");

            if (rtf != null && (!rtf.StartsWith(@"{{\rtf") && (!rtf.StartsWith(@"{\rtf")))) {
                rtf = string.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deftab720 {{\fonttbl{{\f0\fswiss Arial;}}}} \plain\f0\fs18 {0} }}", rtf);
            }

            return rtf;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
