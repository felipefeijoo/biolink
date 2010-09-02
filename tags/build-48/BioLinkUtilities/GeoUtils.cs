using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Utilities {

    public static class GeoUtils {

        /// <summary>
        /// Ported from BioLink 2.x
        /// Originally Adapted from VB code as follows...
	    /// Convert Decimal Degrees to Degrees, Minutes, Seconds or Hemisphere (N,S,E,W)
	    /// From Robert K. Colwell, Univ. of Connecticut, from TAXACOM list server posting,
	    /// 30 July, 1996
	    /// AbsDD = Abs(Decimal Degrees)
	    /// Degrees = Int(AbsDD)
	    /// Decimal Minutes = (AbsDD - Degrees) * 60
	    /// Minutes = Int(Decimal Minutes)
	    /// Decimal Seconds = (Decimal Minutes - Minutes) * 60
	    /// Seconds = Round(Decimal Seconds)
        /// </summary>
        /// <param name="decdeg"></param>
        /// <param name="coordType"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string DecDegToDMS(double decdeg, CoordinateType coordType, String format = "%D°%m'%s\"%r") {

            var absDecDeg = Math.Abs(decdeg);
            int iDegrees = (int)Math.Floor(absDecDeg);
            double dfMinutes = (absDecDeg - iDegrees) * 60;
            int iMinutes = (int) Math.Floor(dfMinutes);
            var dfSeconds = (dfMinutes - iMinutes) * 60;
            int iSeconds = (int) Math.Round(dfSeconds);

            if (iSeconds > 59) {
                iMinutes++;
                iSeconds = 0;
            }

            StringBuilder b = new StringBuilder();
            for (int i = 0; i < format.Length; ++i) {
                // Next character should be a place holder !
                if (format[i] == '%') {
                    i++;
                    switch (format[i]) {
                        case 'd':
                            b.Append(String.Format("{0:00}", iDegrees));
                            break;
                        case 'D':
                            b.Append(String.Format("{0}", iDegrees));
                            break;
                        case 'm':
                            b.Append(String.Format("{0:00}", iMinutes));
                            break;
                        case 'M':
                            b.Append(String.Format("{0}", iMinutes));
                            break;
                        case 's':
                            b.Append(String.Format("{0:00}", iSeconds));
                            break;
                        case 'S':
                            if (iSeconds != 0) {
                                b.Append(String.Format("{0}\"", iSeconds));
                            }
                            break;
                        case 'r':
                            if (coordType == CoordinateType.Latitude) {
                                b.Append(decdeg > 0 ? "N" : "S");
                            } else {
                                b.Append(decdeg > 0 ? "E" : "W");
                            }
                            break;
                        case 'R':
                            if (coordType == CoordinateType.Latitude) {
                                b.Append(decdeg > 0 ? "North" : "South");
                            } else {
                                b.Append(decdeg > 0 ? "East" : "West");
                            }
                            break;
                        default:
                            b.Append(format[i]);
                            break;
                    }

                } else if ((format[i] == '\\')) { // Next character is a special char (e.g. double quote)
                    b.Append(format[++i]);
                } else {
                    b.Append(format[i]); // Otherwise is part of the output string.
                }
            }

            return b.ToString();
        }

    }

    public enum CoordinateType {
        Latitude, Longitude
    }

    public enum AreaType {
        Point = 1,
        Line = 2,
        Box = 3
    }
}
