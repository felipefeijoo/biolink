using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace BioLink.Client.Utilities {
    public static class RTFUtils {

        public static String filter(String rtf, bool newLinesToSpace, params String[] allowedKeywords) {

		    if (string.IsNullOrWhiteSpace(rtf)) {
			    return rtf;
		    }

		    FilteringRTFHandler handler = new FilteringRTFHandler(newLinesToSpace, allowedKeywords);
		    RTFReader reader = new RTFReader(rtf, handler);
		    
			reader.parse();
		    return handler.getFilteredText();
	    }

        public static String StripMarkup(String rtf, bool newlinesToSpace = true) {
            return filter(rtf, newlinesToSpace);
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
