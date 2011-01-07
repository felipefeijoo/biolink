using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace BioLink.Client.Utilities {
    public static class DateUtils {

        private static ReadOnlyCollection<string> ROMAN_MONTHS = new List<string> { "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x", "xi", "xii" }.AsReadOnly();        

        private static Regex BLDateRegex = new Regex(@"^(\d\d\d\d)(\d\d)(\d\d)$");

        public static string DateRomanMonth(long bldate) {            
            String datestr = bldate.ToString();
            Match m = BLDateRegex.Match(datestr);
            if (m.Success) {
                int Y = Int32.Parse(m.Groups[1].Value);
                int M = Int32.Parse(m.Groups[2].Value);
                int D = Int32.Parse(m.Groups[3].Value);
                if (M == 0) {
                    return Y.ToString();
                } else if (D == 0) {
                    return String.Format("{0}.{1:0000}", ROMAN_MONTHS[M-1], Y);
                } else {
                    return String.Format("{0}.{1}.{2:0000}", D, ROMAN_MONTHS[M-1], Y);
                }
            }
            return datestr;
        }

        public static string BLDateToStr(long bldate) {
            String datestr = bldate.ToString();
            Match m = BLDateRegex.Match(datestr);
            if (m.Success) {
                int Y = Int32.Parse(m.Groups[1].Value);
                int M = Int32.Parse(m.Groups[2].Value);
                int D = Int32.Parse(m.Groups[3].Value);
                if (M == 0) {
                    return Y.ToString();
                } else if (D == 0) {
                    return String.Format("{0}, {1:0000}", CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[M-1], Y);
                } else {
                    return String.Format("{0} {1}, {2:0000}", D, CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[M-1], Y);
                }
            }
            return datestr;

        }

        public static string GetMonthName(int month, bool abbrev) {
            DateTime date = new DateTime(1900, month, 1);
            if (abbrev) {
                return date.ToString("MMM");
            } else {
                return date.ToString("MMMM");
            }
        }
    
    }
}
