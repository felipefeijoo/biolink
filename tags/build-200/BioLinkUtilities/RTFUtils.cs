using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace BioLink.Client.Utilities {
    public static class RTFUtils {

        private static Regex XX = new Regex(@"(?s)^{\s*{(.*)}\s*}\s*$");

        public static string StripMarkup(string rtf) {
            if (string.IsNullOrEmpty(rtf)) {
                return rtf;
            }

            if (!rtf.StartsWith("{") && !rtf.EndsWith("}")) {
                return rtf;
            }

            var m = XX.Match(rtf);
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
    }

}
