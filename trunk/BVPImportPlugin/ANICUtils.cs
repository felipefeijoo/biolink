using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BioLink.Client.BVPImport {

    public static class ANICUtils {

        private static Regex _pattern = new Regex(@"^((?:[A-Z][.\s,]+)+)\s*(.*)$");

        public static string ConvertNameFormat(String name) {
            var m = _pattern.Match(name);
            if (m.Success) {
                name = String.Format("{0},{1}", m.Groups[2].Value.Trim(), m.Groups[1].Value.Replace(" ", ""));
            }
            return name;
        }
    }
}
