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
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.VisualBasic;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// <para>
    /// Collection of useful methods use to deal with dates and BLDates (the format used internally by biolink)
    /// </para>
    /// <para>
    /// BLDates are 8 digit numbers, with the first 4 representing the year, numbers 5 and 6 representing the month, and 7 and 8 the Day
    /// BLDates can ommit either the day (00) or the month and day (0000). This allows for the representation of approximate dates. For example:
    /// 20110101 = 1st Jan 2011
    /// 20110100 = somethime in Janurary 2011
    /// 20110000 = sometime in 2011
    /// </para>
    /// </summary>
    public static class DateUtils {

        /* The symbols of the months as Roman Numerals, in order from Jan - Dec */
        private static readonly ReadOnlyCollection<string> RomanMonths = new List<string> { "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x", "xi", "xii" }.AsReadOnly();
        /* Month names */
        private static readonly ReadOnlyCollection<string> JulianMonths = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }.AsReadOnly();

        /* A regular expression used to match a BLDate */
        private static readonly Regex BLDateRegex = new Regex(@"^(\d\d\d\d)(\d\d)(\d\d)$");

        /// <summary>
        /// Returns string representing the supplied BLDate formatted with a Roman Numeral month
        /// </summary>
        /// <param name="bldate">BLDates are 6 </param>
        /// <returns></returns>
        public static string DateRomanMonth(int bldate) {
            String datestr = bldate.ToString();
            Match match = BLDateRegex.Match(datestr);
            if (match.Success) {
                int y = Int32.Parse(match.Groups[1].Value);
                int m = Int32.Parse(match.Groups[2].Value);
                int d = Int32.Parse(match.Groups[3].Value);
                if (m == 0) {
                    return y.ToString();
                }
                if (d == 0) {
                    return String.Format("{0}.{1:0000}", RomanMonths[m - 1], y);
                }
                return String.Format("{0}.{1}.{2:0000}", d, RomanMonths[m - 1], y);
            }
            return datestr;
        }

        /// <summary>
        /// Returns the roman numeral symbol for a given month (1 = jan, 12 = dec). Numbers out of range yield a default value of '???'
        /// </summary>
        /// <param name="month">The month number between 1 and 12</param>
        /// <param name="default">What to return if the month number is outside the range 1-12</param>
        /// <returns>The roman numeral equivalent of the month number 1=i, 12=xii</returns>
        public static string RomanMonth(int month, string @default="???") {
            if (month >= 0 && month < RomanMonths.Count) {
                return RomanMonths[month - 1];
            }
            return @default;
        }

        /// <summary>
        /// Returns a shortened (first 3 characters) of a month number between 1 and 12
        /// </summary>
        /// <param name="month">The month number between 1 and 12</param>
        /// <param name="default">What to return if the month number is outside the range 1 - 12</param>
        /// <returns>The first three letters of the month name</returns>
        public static string MidMonth(int month, string @default="???") {
            if (month >= 0 && month < JulianMonths.Count) {
                return JulianMonths[month - 1].Substring(0,3);
            }
            return @default;

        }

        /// <summary>
        /// Converts a BLDate to human readable date string
        /// </summary>
        /// <param name="bldate">8 digit BLDate, can be null</param>
        /// <param name="default">What to return the bldate is null</param>
        /// <returns>Human readable date in the form &lt;day&gt; &lt;short month&gt;, &lt;year&gt;</returns>
        public static string BLDateToStr(int? bldate, string @default = "") {
            if (!bldate.HasValue) {
                return @default;
            }
            return BLDateToStr(bldate.Value);
        }

        /// <summary>
        /// Converts a BLDate to human readable date string
        /// </summary>
        /// <param name="bldate">8 digit BLDate</param>
        /// <returns>Human readable date in the form &lt;day&gt; &lt;short month&gt;, &lt;year&gt;</returns>
        public static string BLDateToStr(int bldate) {
            String datestr = bldate.ToString();

            if (datestr.Equals("0")) {
                return "";
            }

            Match match = BLDateRegex.Match(datestr);
            if (match.Success) {
                int y = Int32.Parse(match.Groups[1].Value);
                int m = Int32.Parse(match.Groups[2].Value);
                int d = Int32.Parse(match.Groups[3].Value);

                if (m < 1 || m > 12) {
                    return y.ToString();
                }
                if (d == 0) {
                    return String.Format("{0}, {1:0000}", CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[m - 1], y);
                }
                return String.Format("{0} {1}, {2:0000}", d, CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[m - 1], y);
            }
            return datestr;

        }

        /// <summary>
        /// Converts a BLDate to a DateTime value
        /// </summary>
        /// <param name="bldate">The BLDate to convert</param>
        /// <returns>A DateTime value</returns>
        public static DateTime MakeCompatibleBLDate(int bldate) {
            var str = DateRomanMonth(bldate);
            return DateTime.Parse(str);
        }

        /// <summary>
        /// Returns a month name given a month number (1-12), optionally abbreviating it
        /// </summary>
        /// <param name="month">The month</param>
        /// <param name="abbrev">Whether or not to abbreviate the month name</param>
        /// <returns>Either a full or abbreviated month name</returns>
        public static string GetMonthName(int month, bool abbrev) {
            var date = new DateTime(1900, month, 1);
            if (abbrev) {
                return date.ToString("MMM");
            }
            return date.ToString("MMMM");
        }

        /// <summary>
        /// Validates a BLDate string. Will place error messages in the out message param
        /// </summary>
        /// <param name="date">The string to validate</param>
        /// <param name="message">if failed, the message will contain why</param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts a date in string form to BLDate. Uses the VB6 compatibility classes to parse the date. If this fails, attempts to interpret the date
        /// as a BLDate
        /// </summary>
        /// <param name="date">The string to convert</param>
        /// <returns>An 8 digit string</returns>
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

        /// <summary>
        /// Converts a DateTime to BLDate by creating a formatted string
        /// </summary>
        /// <param name="dt">The DateTime to convert</param>
        /// <returns>BLDate in string format</returns>
        public static string DateToBLDate(DateTime dt) {
            return string.Format("{0:0000}{1:00}{2:00}", dt.Year, dt.Month, dt.Day);
        }

        public static int StrToBLTime(string timeStr) {
            try {
                if (timeStr == null) {
                    return 0;
                }

                var time = DateAndTime.TimeValue(timeStr);
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
            }
            return BLDateToStr(d);
        }

        /// <summary>
        /// Helper utility to construct a human readable date range. either dates can be null, resulting in a different description.
        /// </summary>
        /// <param name="dateType"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="casualDate"></param>
        /// <returns></returns>
        public static string FormatDates(int dateType, int? startDate, int? endDate, string casualDate) {
            if (dateType == 2) {
                return casualDate;
            }
            if (startDate.HasValue && endDate.HasValue) {
                if (startDate == endDate) {
                    return FormatDate(startDate.Value);
                }
                if (startDate.Value == 0) {
                    return "Before " + FormatDate(endDate.Value);
                }
                if (endDate == 0) {
                    return FormatDate(startDate.Value);
                }
                return string.Format("{0} to {1}", FormatDate(startDate.Value), FormatDate(endDate.Value));
            }
            if (startDate.HasValue) {
                return FormatDate(startDate.Value);
            }
            if (endDate.HasValue) {
                return "Before " + FormatDate(endDate.Value);
            }
            return "";
        }


        /// <summary>
        /// Extracts the component fields in a BLDate
        /// </summary>
        /// <param name="bldate">The BLDate string</param>
        /// <param name="day">the day component</param>
        /// <param name="month">the month</param>
        /// <param name="year">the year</param>
        /// <returns>True if successful</returns>
        public static bool BLDateComponents(string bldate, out int day, out int month, out int year) {
            day = 0;
            month = 0;
            year = 0;
            string message;

            if (!IsValidBLDate(bldate, out message)) {
                return false;
            }

            Match m = BLDateRegex.Match(bldate);
            if (m.Success) {
                year = Int32.Parse(m.Groups[1].Value);
                month = Int32.Parse(m.Groups[2].Value);
                day = Int32.Parse(m.Groups[3].Value);
                return true;
            }

            return false;
        }

    }

    

}
