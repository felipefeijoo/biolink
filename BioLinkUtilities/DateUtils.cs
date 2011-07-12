using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.VisualBasic;

namespace BioLink.Client.Utilities {
    public static class DateUtils {

        private static ReadOnlyCollection<string> ROMAN_MONTHS = new List<string> { "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x", "xi", "xii" }.AsReadOnly();

        private static Regex BLDateRegex = new Regex(@"^(\d\d\d\d)(\d\d)(\d\d)$");

        public static string DateRomanMonth(int bldate) {
            String datestr = bldate.ToString();
            Match m = BLDateRegex.Match(datestr);
            if (m.Success) {
                int Y = Int32.Parse(m.Groups[1].Value);
                int M = Int32.Parse(m.Groups[2].Value);
                int D = Int32.Parse(m.Groups[3].Value);
                if (M == 0) {
                    return Y.ToString();
                } else if (D == 0) {
                    return String.Format("{0}.{1:0000}", ROMAN_MONTHS[M - 1], Y);
                } else {
                    return String.Format("{0}.{1}.{2:0000}", D, ROMAN_MONTHS[M - 1], Y);
                }
            }
            return datestr;
        }

        public static string BLDateToStr(int? bldate, string @default = "") {
            if (!bldate.HasValue) {
                return @default;
            }
            return BLDateToStr(bldate.Value);
        }

        public static string BLDateToStr(int bldate) {
            String datestr = bldate.ToString();
            Match m = BLDateRegex.Match(datestr);
            if (m.Success) {
                int Y = Int32.Parse(m.Groups[1].Value);
                int M = Int32.Parse(m.Groups[2].Value);
                int D = Int32.Parse(m.Groups[3].Value);

                if (M < 1 || M > 12) {
                    return Y.ToString();
                } else if (D == 0) {
                    return String.Format("{0}, {1:0000}", CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[M - 1], Y);
                } else {
                    return String.Format("{0} {1}, {2:0000}", D, CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[M - 1], Y);
                }
            }
            return datestr;

        }

        public static DateTime MakeCompatibleBLDate(int bldate) {
            var str = DateRomanMonth(bldate);
            return DateTime.Parse(str);
        }

        public static string GetMonthName(int month, bool abbrev) {
            DateTime date = new DateTime(1900, month, 1);
            if (abbrev) {
                return date.ToString("MMM");
            } else {
                return date.ToString("MMMM");
            }
        }

        public static bool IsValidBLDate(string date, out string message) {

            message = null;

            if (string.IsNullOrWhiteSpace(date)) {
                message = "Invalid date - Empty string.";
                return false;
            }

            // Pad out with 0's...
            if (date.Length == 4 || date.Length == 6) {
                for (int i = date.Length; i < 8; ++i) {
                    date += '0';
                }
            }

            if (date.Length != 8) {
                message = "Invalid date - BLDate must be 8 characters.";
                return false;
            }

            if (!date.IsNumeric()) {
                message = "Invalid Date - All characters must be digits.";
                return false;
            }

            Match m = BLDateRegex.Match(date);
            if (m.Success) {
                int iYear = Int32.Parse(m.Groups[1].Value);
                int iMonth = Int32.Parse(m.Groups[2].Value);
                int iDay = Int32.Parse(m.Groups[3].Value);

                if (iDay > 31) {
                    message = "Invalid Date - Days must be between 00 and 31";
                    return false;
                }

                if ((iMonth == 0) && (iDay != 0)) {
                    message = "Invalid Date - If months are 00 then days must be 00.";
                    return false;
                }

                if (iMonth > 12) {
                    message = "Invalid Date - Months must be between 00 and 12.";
                    return false;
                }
            } else {
                message = "Invalid Date - Failed regex!";
                return false;
            }

            return true;
        }

        public static string DateStrToBLDate(string date) {
            if (Information.IsDate(date)) {
                var dt = DateAndTime.DateValue(date);
                var regex = new Regex("^\\w+[^\\w]_\\w+$");
                if (regex.IsMatch(date)) {
                    // we have a partial date, and the day has probably been defaulted to 1
                    // temp should be in format YYYYMMDD, so replace the last two digits with 00
                    return date.Substring(0, 6) + "00";
                }

                return DateToBLDate(dt);
            }

            return null;
        }

        public static string DateToBLDate(DateTime dt) {
            return string.Format("{0:0000}{1:00}{2:00}", dt.Year, dt.Month, dt.Day);
        }

        public static int StrToBLTime(string TimeStr) {
            try {
                if (TimeStr == null) {
                    return 0;
                }

                var time = Microsoft.VisualBasic.DateAndTime.TimeValue(TimeStr);
                return (DateAndTime.Hour(time) * 100) + DateAndTime.Minute(time);
            } catch (Exception) {
                return 0;
            }
        }

        public static string ShortDate(DateTime? dt) {
            if (!dt.HasValue) {
                return "";
            }

            return string.Format("{0:d}", dt.Value);
        }

        public static string FormatDate(int d, bool asRomanMonth = false) {
            if (d == 0) {
                return "";
            }

            if (asRomanMonth) {
                return DateRomanMonth(d);
            } else {
                return BLDateToStr(d);
            }
        }

        public static string FormatDates(int dateType, int? startDate, int? endDate, string casualDate) {
            if (dateType == 2) {
                return casualDate;
            } else {
                if (startDate.HasValue && endDate.HasValue) {
                    if (startDate == endDate) {
                        return FormatDate(startDate.Value);
                    } else if (startDate.Value == 0) {
                        return "Before " + FormatDate(endDate.Value, false);
                    } else if (endDate == 0) {
                        return FormatDate(startDate.Value, false);
                    } else {
                        return string.Format("{0} to {1}", FormatDate(startDate.Value), FormatDate(endDate.Value));
                    }
                } else if (startDate.HasValue) {
                    return FormatDate(startDate.Value);
                } else if (endDate.HasValue) {
                    return "Before " + FormatDate(endDate.Value);
                }
            }
            return "";
        }


    }

    

}
