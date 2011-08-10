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

namespace BioLink.Client.Utilities {

    /// <summary>
    /// IValueConverter class used to format byte values (integers) into size descriptions (strings)
    /// </summary>
    public class ByteLengthConverter : IValueConverter {

        /// <summary>
        /// Default Scale is 1024 bytes to a kilobyte, etc.
        /// </summary>
        private const int SCALE = 1024;
        /// <summary>
        /// Suffixes used to denote magnitude in descending order
        /// </summary>
        private static readonly string[] UNITS = new string[] { "TB", "GB", "MB", "KB", "Bytes" };

        /// <summary>
        /// Converts a long value to size description with the most appropriate suffix
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int bytes = (int)value;
            return FormatBytes(bytes);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the supplied number of bytes to human readable size description including a magnitude suffix (KB, MB etc).
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatBytes(long bytes) {
            long min = (long)Math.Pow(SCALE, UNITS.Length - 1);
            // Starting with the biggest order of magnitude, work through until the number bytes is greater the minimum number of bytes that can be display (whole number) with that scale
            foreach (string order in UNITS) {
                if (bytes > min) {
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, min), order);
                }
                min /= SCALE;
            }
            return "0 Bytes";
        }

    }
}
