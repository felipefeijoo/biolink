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
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Some useful routines for dealing with RTF markup
    /// </summary>
    public static class RTFUtils {

        public static String filter(String rtf, bool newLinesToSpace, bool strict, params String[] allowedKeywords) {

		    if (string.IsNullOrWhiteSpace(rtf)) {
			    return rtf;
		    }

		    FilteringRTFHandler handler = new FilteringRTFHandler(newLinesToSpace, allowedKeywords);
		    RTFReader reader = new RTFReader(rtf, handler, strict);
		    
			reader.parse();
		    return handler.getFilteredText();
	    }

        public static String StripMarkup(String rtf, bool newlinesToSpace = true) {
            return filter(rtf, newlinesToSpace, false);
        }

        public static string EscapeUnicode(string value) {
            var sb = new StringBuilder();

            foreach (char ch in value) {
                if (ch > 127) {
                    sb.AppendFormat("\\u{0}?", (int)ch);
                } else {
                    if (ch == '\n') {
                        sb.Append("\\par ");
                    } else {
                        sb.Append(ch);
                    }
                }
            }

            return sb.ToString();
        }
    }

}
