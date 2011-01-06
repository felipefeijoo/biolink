using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace BioLink.Client.Utilities {
    public static class RTFUtils {

        public static string StripMarkup(string rtf) {
            if (string.IsNullOrEmpty(rtf)) {
                return rtf;
            }

            var rtb = new RichTextBox();
            rtb.Rtf = rtf;
            return rtb.Text;
        }
    }

}
