using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace BioLink.Client.Utilities {
    public static class RTFUtils {

        private static Regex RTF_REGEX = new Regex(@"(?s)^{\s*{(.*)}\s*}\s*$");

        public static string StripMarkup(string rtf) {
            if (string.IsNullOrEmpty(rtf)) {
                return rtf;
            }

            if (!rtf.StartsWith("{") && !rtf.EndsWith("}")) {
                return rtf;
            }

            var m = RTF_REGEX.Match(rtf);
            if (m.Success) {
                rtf = string.Format("{{{0}}}", m.Groups[1]);
            }

            try {
                var rtb = new RichTextBox();
                rtb.Rtf = rtf;
                return rtb.Text;
            } catch (ArgumentException) {
                return rtf;
            }
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
