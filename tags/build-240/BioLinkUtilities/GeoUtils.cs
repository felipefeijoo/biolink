using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BioLink.Client.Utilities {

    public static class GeoUtils {

        private static DirectionRange[] FourPoints = new DirectionRange[] {
            new DirectionRange(0,45, "N"),
            new DirectionRange(45, 135, "E"),
            new DirectionRange(135, 225, "S"),
            new DirectionRange(225, 315, "W"),
            new DirectionRange(315, 361, "N")
        };

        private static DirectionRange[] EightPoints = new DirectionRange[] {
            new DirectionRange(0,22.5, "N"),
            new DirectionRange(22.5, 67.5, "NE"),
            new DirectionRange(67.5,112.5, "E"),
            new DirectionRange(112.5,157.5, "SE"),
            new DirectionRange(157.5,202.5, "S"),
            new DirectionRange(202.5,247.5, "SW"),
            new DirectionRange(247.5,292.5, "W"),
            new DirectionRange(292.5,337.5, "NW"),
            new DirectionRange(337.5, 361, "N")
        };

        private static DirectionRange[] SixteenPoints = new DirectionRange[] {
            new DirectionRange(0,11.25, "N"),
            new DirectionRange(11.25,33.75, "NNE"),
            new DirectionRange(33.75,56.25, "NE"),
            new DirectionRange(56.25,78.75, "ENE"),
            new DirectionRange(78.75,101.25, "E"),
            new DirectionRange(101.25,123.75, "ESE"),
            new DirectionRange(123.75,146.25, "SE"),
            new DirectionRange(146.25,168.75, "SSE"),
            new DirectionRange(168.75,191.25, "S"),
            new DirectionRange(191.25,213.75, "SSW"),
            new DirectionRange(213.75,236.25, "SW"),
            new DirectionRange(236.25,258.75, "WSW"),
            new DirectionRange(258.75,281.75, "W"),
            new DirectionRange(281.75,303.75, "WNW"),
            new DirectionRange(303.75,326.25, "NW"),
            new DirectionRange(326.25,348.75, "NNW"),
            new DirectionRange(348.75,361, "N"),
        };

        private static DirectionRange[] ThirtyTwoPoints = new DirectionRange[] {            
                new DirectionRange(0 , 5.625, "N"),
                new DirectionRange(5.625 ,16.875, "NbyE"),
                new DirectionRange(16.875, 28.125, "NNE"),
                new DirectionRange(28.125 , 39.375, "NEbyN"),
                new DirectionRange(39.375 , 50.625,"NE"),
                new DirectionRange(50.625 , 61.875, "NEbyE"),
                new DirectionRange(61.875 , 73.125,  "ENE"),
                new DirectionRange(73.125 , 84.375, "EbyN"),
                new DirectionRange(84.375 , 95.625,"E"),
                new DirectionRange(95.625 , 106.875, "EbyS"),
                new DirectionRange(106.875 , 118.125, "ESE"),
                new DirectionRange(118.125 , 129.375, "SEbyE"),
                new DirectionRange(129.375 , 140.625, "SE"),
                new DirectionRange(140.625 , 151.875, "SEbyS"),
                new DirectionRange(151.875 , 163.125, "SSE"),
                new DirectionRange(163.125 , 174.375, "SbyE"),
                new DirectionRange(174.375 , 185.625, "S"),
                new DirectionRange(185.625 , 196.875, "SbyW"),
                new DirectionRange(196.875 , 208.125, "SSW"),
                new DirectionRange(208.125 , 219.375, "SWbyS"),
                new DirectionRange(219.375 , 230.625,  "SW"),
                new DirectionRange(230.625 , 241.875, "SWbyW"),
                new DirectionRange(241.875 , 253.125, "WSW"),
                new DirectionRange(253.125 , 264.375, "WbyS"),
                new DirectionRange(264.375 , 275.625, "W"),
                new DirectionRange(275.625,286.875,"WbyN"),
                new DirectionRange(286.875,298.125,"WNW"),
                new DirectionRange(298.125,309.375,"NWbyW"),
                new DirectionRange(309.375,320.625,"NW"),
                new DirectionRange(320.625,331.875,"NWbyN"),
                new DirectionRange(331.875,343.125,"NNW"),
                new DirectionRange(343.125,354.375,"NbyW"),
                new DirectionRange(354.375,361,"N")
        };


        public static string GreatCircleArcDirection(double nsLat1, double nsLong1, double nsLat2, double nsLong2, int niNumberOfPoints) {
            // Calculate the Great Circle Arc direction between two points
            double ndPi = Math.PI;
            double ndRadLat1;
            double ndRadLong1;
            double ndRadLat2;
            double ndRadLong2;
            double nsDistance;
            double ndPartial;
            double nsDirection;


            // Convert degrees to radians
            // Radians = Degrees * (Pi / 180)
            ndRadLat1 = nsLat1 * (ndPi / 180);
            ndRadLong1 = nsLong1 * (ndPi / 180) * -1;  // The * -1 is a bug fix
            ndRadLat2 = nsLat2 * (ndPi / 180);
            ndRadLong2 = nsLong2 * (ndPi / 180) * -1;  // The * -1 is a bug fix

            // Get distance between the points in radians
            nsDistance = GreatCircleArcLength(nsLat1, nsLong1, nsLat2, nsLong2, "R");

            if (nsDistance == 0) { // the points are very close together
                return "-";  // Return nothing
            }

            // Calculate direction in radians
            ndPartial = (Math.Sin(ndRadLat2) - Math.Sin(ndRadLat1) * Math.Cos(nsDistance)) / (Math.Sin(nsDistance) * Math.Cos(ndRadLat1));
            // Arccos(X) = Atn(-X / Sqr(-X * X + 1)) + 1.5708
            // ndPartial = Math.Atan((ndPartial * -1) / Math.Sqr((ndPartial * -1) * ndPartial + 1)) + 1.5708;
            ndPartial = Math.Acos(ndPartial);

            if (Math.Sin(ndRadLong2 - ndRadLong1) <= 0) {
                nsDirection = ndPartial;
            } else {
                nsDirection = 2 * ndPi - ndPartial;
            }

            // Convert to degrees
            nsDirection = (180 / ndPi) * nsDirection;

            DirectionRange[] arr = null;

            // Convert to compass direction
            switch (niNumberOfPoints) {
                case 0: // ' Return the direction in degrees
                    return nsDirection + "";
                case 4:
                    arr = FourPoints;
                    break;
                case 8:
                    arr = EightPoints;
                    break;
                case 16:
                    arr = SixteenPoints;
                    break;
                case 32:
                    arr = ThirtyTwoPoints;
                    break;
            }
            if (arr != null) {
                foreach (DirectionRange r in arr) {
                    if (nsDirection >= r.StartAngle && nsDirection <= r.EndAngle) {
                        return r.Direction;
                    }
                }
                return "Direction not found for angle " + nsDirection;
            } else {
                return "Invalid number of points: " + niNumberOfPoints;
            }
        }

        //BIOLINKCUTILS_API DWORD WINAPI BLGreatCircleArcLength( float fLat1, float fLong1, float fLat2, float fLong2, BSTR bszUnits, float *fRet ) {
        public static double GreatCircleArcLength(double fLat1, double fLong1, double fLat2, double fLong2, string bszUnits) {
            double dfRadLat1, dfRadLat2;
            double dfRadDeltaLong;
            double dfCosZ, dfACosZ;
            double dfPi = Math.PI;
            double dfDiv;

            dfRadLat1 = fLat1 * (dfPi / (double)180);
            dfRadLat2 = fLat2 * (dfPi / (double)180);
            dfRadDeltaLong = (fLong2 - fLong1) * (dfPi / (double)180);
            dfCosZ = Math.Sin(dfRadLat1) * Math.Sin(dfRadLat2) + Math.Cos(dfRadLat1) * Math.Cos(dfRadLat2) * Math.Cos(dfRadDeltaLong);

            if (Math.Abs(dfCosZ - 1.0) < 0.0000000001) {
                // The points were very close together
                return 0;
            } else {
                dfDiv = Math.Sqrt((dfCosZ * -1) * dfCosZ + 1);
                if (dfDiv == 0) {
                    return 0;
                } else {
                    dfACosZ = Math.Atan((dfCosZ * -1) / dfDiv) + 1.5708; // Magic Number ?			
                    switch (Char.ToLower(bszUnits[0])) {
                        case 'k':
                            return (double)dfACosZ * (float)6371.1;	// Mean radius of Earth is 6371.1 KM
                        case 'm':
                            return (double)dfACosZ * (float)3959;	// Mean radius of Earth is 3959 Miles
                        default:
                            return (double)dfACosZ;					// Users can apply their own Mean Radius to get different units
                    }
                }
            }
        }
        public static bool DMSStrToDecDeg(string szIn, CoordinateType iCoordType, out double dfRet, out string bszReason) {

            int iDegrees = 0, iMinutes = 0;
            double dfSeconds = 0.0;
            char cDirection = '_';
            int i = 0;
            bool isDMS = false;
            int iFirstLength = 2;

            dfRet = 0;
            bszReason = "";

            int lStrLen = szIn.Length;

            for (i = lStrLen - 1; i >= 0; i--) {

                if ("NSEW:;'\"°".Contains(Char.ToUpper(szIn[i]))) {
                    isDMS = true;

                    if ("NSEW".Contains(Char.ToUpper(szIn[i]))) {
                        cDirection = Char.ToUpper(szIn[i]);
                        if ((Char.ToUpper(szIn[i]) == 'N') || (Char.ToUpper(szIn[i]) == 'S')) {
                            iFirstLength = 2;		// Latitudes can only have two digits (00-90)
                        } else {
                            iFirstLength = 3;		// Longitudes can have 3 (000-180)
                        }
                        //break;
                    }
                }
            }


            // -db- #19/01/00# Validate direction...
            if (!isDMS) {
                bszReason = string.Format("Coordinate to be converted incorrectly formatted [{0}] ! - try 'dd:mm:ss D'", szIn);
                return false;
            }

            if (cDirection == '_') {
                bszReason = String.Format("No valid direction in '{0}'. Try 'dd:mm:ss D' where D is either N,S,E or W.", szIn);
                return false;
            }

            switch (iCoordType) {
                case CoordinateType.Latitude:			// 0 = Latitude
                    if ((cDirection != 'N') && (cDirection != 'S')) {
                        bszReason = string.Format("Invalid direction for a Latitude. Expected 'N' or 'S' but got '{0}'.", cDirection);
                        return false;
                    }
                    break;
                case CoordinateType.Longitude:			// 1 = Longitude
                    if ((cDirection != 'E') && (cDirection != 'W')) {
                        bszReason = string.Format("Invalid direction for a Longitude. Expected 'E' or 'W' but got '{0}'.", cDirection);
                        return false;
                    }
                    break;
            }

            if (isDMS) {
                int idx = 0;

                // Get Degrees		
                while ((idx < szIn.Length) && !Char.IsDigit(szIn[idx])) idx++;		// find first numeric
                if (idx < szIn.Length) {										// Not at end of string
                    i = 0;
                    while (Char.IsDigit(szIn[idx + i]) && (i < iFirstLength)) i++;						// find last digit for this number
                    string strBuf = szIn.Substring(idx, i);
                    iDegrees = Int32.Parse(strBuf);
                    idx += i;
                }

                // Get Minutes
                while ((idx < szIn.Length) && !Char.IsDigit(szIn[idx])) idx++;		// find first numeric
                if (idx < szIn.Length) {										// Not at end of string
                    i = 0;
                    while (Char.IsDigit(szIn[idx + i]) && (i < 2)) i++;		// find last digit for this number
                    string szBuf = szIn.Substring(idx, i);
                    iMinutes = Int32.Parse(szBuf);
                    idx += i;
                }

                // Get Seconds
                while ((idx < szIn.Length) && !Char.IsDigit(szIn[idx])) idx++;		// find first numeric
                if (idx < szIn.Length) {										// Not at end of string
                    i = 0;
                    while (char.IsDigit(szIn[idx + i]) || (szIn[idx + i] == '.') && (i < 2)) i++;// find last digit for this number
                    string szBuf = szIn.Substring(idx, i);

                    dfSeconds = double.Parse(szBuf);
                }

                dfRet = (double)iDegrees + ((double)iMinutes / (double)60) + (dfSeconds / (double)3600);

                switch (cDirection) {
                    case 'S':
                    case 'W':
                        dfRet *= -1;								// Change sign for these directions
                        break;
                }

            }

            return isDMS;
        }


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
            int iMinutes = (int)Math.Floor(dfMinutes);
            var dfSeconds = (dfMinutes - iMinutes) * 60;
            int iSeconds = (int)Math.Round(dfSeconds);

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

        public static double DDecMToDecDeg(int degrees, double minutes) {
            return degrees + (minutes / 60.0) * (degrees < 0 ? -1 : 1);
        }

        /*
        ' Converts Degrees, Decimal Minutes and a direction to Decimal Degrees
        Public Function DDecMDirToDecDeg(Degrees As Integer, Minutes As Single, Direction As String) As Single
            Dim sign As Integer: sign = 1
            If LCase(Direction) = "s" Or LCase(Direction) = "w" Then sign = -1
            DDecMDirToDecDeg = (Abs(Degrees) + (Minutes / 60)) * sign
        End Function
        */

        public static double DDecMDirToDecDeg(int degrees, double minutes, string direction) {
            int sign = ("sw".Contains(direction.ToLower()) ? -1 : 1);
            return (Math.Abs(degrees) + (minutes / 60)) * (double)sign;
        }

        public static void DecDegToDMS(double decdeg, CoordinateType coordType, out int degrees, out int minutes, out int seconds, out string direction) {
            degrees = (int)Math.Abs(decdeg);
            double leftover = (Math.Abs(decdeg) - degrees);
            minutes = (int)(leftover * 60);
            leftover = leftover - ((double)minutes / 60.0);
            seconds = (int)Math.Round(leftover * 3600, MidpointRounding.AwayFromZero);

            if (seconds >= 60) {
                minutes++;
                seconds -= 60;
            }

            switch (coordType) {
                case CoordinateType.Latitude:
                    direction = decdeg < 0 ? "S" : "N";
                    break;
                case CoordinateType.Longitude:
                    direction = decdeg < 0 ? "W" : "E";
                    break;
                default:
                    throw new Exception();
            }
        }

        public static void DecDegToDDecM(double decdeg, out int degrees, out double minutes) {
            degrees = (int)Math.Abs(decdeg);
            double leftover = (Math.Abs(decdeg) - degrees);
            minutes = Math.Round(leftover * 60, 4);
            if (decdeg < 0) {
                degrees *= -1;
            }
        }

        public static void DecDegToDDecMDir(double decdeg, out int degrees, out double minutes, out string direction, CoordinateType coordType) {
            DecDegToDDecM(decdeg, out degrees, out minutes);
            if (coordType == CoordinateType.Latitude) {
                direction = (decdeg < 0) ? "S" : "N";
            } else {
                direction = (decdeg < 0) ? "W" : "E";
            }
        }

        public static double DMSToDecDeg(int degrees, int minutes, double seconds, string direction) {
            if (direction == null) {
                direction = "N";
            }
            var decdeg = (double)degrees + ((double)minutes / 60.0) + (seconds / 3600.0);
            var sign = (double)("sw".Contains(direction.ToLower()) ? -1 : 1);
            return decdeg * sign;
        }

        public const double PI = 3.14159265;
        public const double FOURTHPI = PI / 4;
        public const double DEGREES_TO_RADS = PI / 180;
        public const double RADS_TO_DEGREES = 180.0 / PI;

        private static char UTMLetterDesignator(double Lat) {
            //This routine determines the correct UTM letter designator for the given latitude
            //returns 'Z' if latitude is outside the UTM limits of 80N to 80S
            //Written by Chuck Gantz- chuck.gantz@globalstar.com.
            char LetterDesignator;

            if ((80 >= Lat) && (Lat > 72)) LetterDesignator = 'X';
            else if ((72 >= Lat) && (Lat > 64)) LetterDesignator = 'W';
            else if ((64 >= Lat) && (Lat > 56)) LetterDesignator = 'V';
            else if ((56 >= Lat) && (Lat > 48)) LetterDesignator = 'U';
            else if ((48 >= Lat) && (Lat > 40)) LetterDesignator = 'T';
            else if ((40 >= Lat) && (Lat > 32)) LetterDesignator = 'S';
            else if ((32 >= Lat) && (Lat > 24)) LetterDesignator = 'R';
            else if ((24 >= Lat) && (Lat > 16)) LetterDesignator = 'Q';
            else if ((16 >= Lat) && (Lat > 8)) LetterDesignator = 'P';
            else if ((8 >= Lat) && (Lat > 0)) LetterDesignator = 'N';
            else if ((0 >= Lat) && (Lat > -8)) LetterDesignator = 'M';
            else if ((-8 >= Lat) && (Lat > -16)) LetterDesignator = 'L';
            else if ((-16 >= Lat) && (Lat > -24)) LetterDesignator = 'K';
            else if ((-24 >= Lat) && (Lat > -32)) LetterDesignator = 'J';
            else if ((-32 >= Lat) && (Lat > -40)) LetterDesignator = 'H';
            else if ((-40 >= Lat) && (Lat > -48)) LetterDesignator = 'G';
            else if ((-48 >= Lat) && (Lat > -56)) LetterDesignator = 'F';
            else if ((-56 >= Lat) && (Lat > -64)) LetterDesignator = 'E';
            else if ((-64 >= Lat) && (Lat > -72)) LetterDesignator = 'D';
            else if ((-72 >= Lat) && (Lat > -80)) LetterDesignator = 'C';
            else LetterDesignator = 'Z'; //This is here as an error flag to show that the Latitude is outside the UTM limits

            return LetterDesignator;
        }

        // converts lat/long to UTM coords.  Equations from USGS Bulletin 1532 
        // East Longitudes are positive, West longitudes are negative. 
        // North latitudes are positive, South latitudes are negative
        // Lat and Long are in decimal degrees
        // Does not take into account thespecial UTM zones between 0 degrees and 
        // 36 degrees longitude above 72 degrees latitude and a special zone 32 
        // between 56 degrees and 64 degrees north latitude 
        // Originally written by Chuck Gantz- chuck.gantz@globalstar.com. Ported to C by David Baird, 1998, Re-ported to c# David Baird 2010
        public static void LatLongToUTM(Ellipsoid referenceEllipsoid, double latitude, double longitude, out double northing, out double easting, out string zone) {
            northing = 0;
            easting = 0;
            zone = "";

            double a = referenceEllipsoid.EquatorialRadius;
            double eccSquared = referenceEllipsoid.EccentricitySquared;
            double k0 = 0.9996;  // Magic constant :-(

            double LongOrigin;
            double eccPrimeSquared;
            double N, T, C, A, M;

            double LatRad = latitude * DEGREES_TO_RADS;
            double LongRad = longitude * DEGREES_TO_RADS;
            double LongOriginRad;

            // Outside bounds checking -db- 01/09/1998
            if (latitude < -90.0) {
                latitude = -90.0;
            }

            if (latitude > 90.0) {
                latitude = 90.0;
            }

            if (longitude < -180.0) {
                longitude = -180.0;
            }

            if (longitude > 180.0) {
                longitude = 180.0;
            }

            if (longitude > -6 && longitude <= 0) {
                LongOrigin = -3;	//arbitrarily set origin at 0 longitude to 3W
            } else if (longitude < 6 && longitude > 0) {
                LongOrigin = 3;
            } else {
                LongOrigin = ((int)longitude / 6) * 6 + 3 * ((int)longitude / 6) / Math.Abs((int)longitude / 6);
            }

            LongOriginRad = LongOrigin * DEGREES_TO_RADS;

            //compute the UTM Zone from the latitude and longitude
            zone = string.Format("{0}{1}", (int)((longitude + 180) / 6) + 1, UTMLetterDesignator(latitude));

            eccPrimeSquared = (eccSquared) / (1 - eccSquared);

            N = a / Math.Sqrt(1 - eccSquared * Math.Sin(LatRad) * Math.Sin(LatRad));
            T = Math.Tan(LatRad) * Math.Tan(LatRad);
            C = eccPrimeSquared * Math.Cos(LatRad) * Math.Cos(LatRad);
            A = Math.Cos(LatRad) * (LongRad - LongOriginRad);

            M = a * ((1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256) * LatRad
                - (3 * eccSquared / 8 + 3 * eccSquared * eccSquared / 32 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(2 * LatRad)
                                    + (15 * eccSquared * eccSquared / 256 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(4 * LatRad)
                                    - (35 * eccSquared * eccSquared * eccSquared / 3072) * Math.Sin(6 * LatRad));

            easting = (double)(k0 * N * (A + (1 - T + C) * A * A * A / 6 + (5 - 18 * T + T * T + 72 * C - 58 * eccPrimeSquared) * A * A * A * A * A / 120) + 500000.0);

            northing = (double)(k0 * (M + N * Math.Tan(LatRad) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * A * A * A * A / 24 + (61 - 58 * T + T * T + 600 * C - 330 * eccPrimeSquared) * A * A * A * A * A * A / 720)));
            if (latitude <= 0) {
                northing += 10000000.0; //10000000 meter offset for southern hemisphere
            }

        }

        private static Regex ZONE_REGEX = new Regex(@"^(\d+)([A-Za-z]{1})$");


        //converts UTM coords to lat/long.  Equations from USGS Bulletin 1532 
        //East Longitudes are positive, West longitudes are negative. 
        //North latitudes are positive, South latitudes are negative
        //Lat and Long are in decimal degrees. 
        //Does not take into account the special UTM zones between 0 degrees 
        //and 36 degrees longitude above 72 degrees latitude and a special 
        //zone 32 between 56 degrees and 64 degrees north latitude
        //Written by Chuck Gantz- chuck.gantz@globalstar.com
        // Ported to C by David Baird, 1998, Re-ported to C# by David Baird 2010
        public static void UTMToLatLong(Ellipsoid referenceEllipsoid, double northing, double easting, string zone, out double latitude, out double longitude) {

            latitude = 0;
            longitude = 0;

            double k0 = 0.9996;
            double a = referenceEllipsoid.EquatorialRadius;
            double eccSquared = referenceEllipsoid.EccentricitySquared;
            double eccPrimeSquared;
            double e1 = (1 - Math.Sqrt(1 - eccSquared)) / (1 + Math.Sqrt(1 - eccSquared));
            double N1, T1, C1, R1, D, M;
            double LongOrigin;
            double mu, phi1, phi1Rad;
            double x, y;
            int ZoneNumber;
            char ZoneLetter;
            // int NorthernHemisphere; //1 for northern hemispher, 0 for southern

            x = easting - 500000.0; //remove 500,000 meter offset for longitude
            y = northing;
            //if ( x <= 2875) x = 2875;		    // -db- 01/09/1998 To stop errors when Easting = 500000
            if (y == 0) y = 0.000000001;		// -db- 01/09/1998 To stop errors when Northing = 0

            var m = ZONE_REGEX.Match(zone);
            if (!m.Success) {
                return;
            } else {
                ZoneLetter = m.Groups[2].Value[0];
                ZoneNumber = Int32.Parse(m.Groups[1].Value);
            }

            if ((ZoneLetter - 'N') > 0) {
                // NorthernHemisphere = 1;//point is in northern hemisphere
            } else {
                // NorthernHemisphere = 0;//point is in southern hemisphere
                y -= 10000000.0;//remove 10,000,000 meter offset used for southern hemisphere
            }

            LongOrigin = (ZoneNumber - 1) * 6 - 180 + 3;  //+3 puts origin in middle of zone

            eccPrimeSquared = (eccSquared) / (1 - eccSquared);

            M = y / k0;
            mu = M / (a * (1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256));

            phi1Rad = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
                        + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
                        + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);
            phi1 = phi1Rad * RADS_TO_DEGREES;

            N1 = a / Math.Sqrt(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
            T1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
            C1 = eccPrimeSquared * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
            R1 = a * (1 - eccSquared) / Math.Pow(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
            D = x / (N1 * k0);

            latitude = phi1Rad - (N1 * Math.Tan(phi1Rad) / R1) * (D * D / 2 - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * eccPrimeSquared) * D * D * D * D / 24
                            + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * eccPrimeSquared - 3 * C1 * C1) * D * D * D * D * D * D / 720);
            latitude = latitude * RADS_TO_DEGREES;

            longitude = (D - (1 + 2 * T1 + C1) * D * D * D / 6 + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * eccPrimeSquared + 24 * T1 * T1)
                            * D * D * D * D * D / 120) / Math.Cos(phi1Rad);
            longitude = LongOrigin + longitude * RADS_TO_DEGREES;
        }

        public static int GetEllipsoidIndex(string nameOrIndex) {

            if (nameOrIndex.IsNumeric()) {
                var idx = Int32.Parse(nameOrIndex);
                if (idx <= 0 || idx >= ELLIPSOIDS.Length) {
                    throw new Exception("Index is outside of the range of allowed ellipsoid indexes: " + nameOrIndex + " (valid range is 1-" + (ELLIPSOIDS.Length - 1));
                } else {
                    return idx;
                }
            }
            foreach (Ellipsoid e in ELLIPSOIDS) {
                if (e.Name.Equals(nameOrIndex, StringComparison.CurrentCultureIgnoreCase)) {
                    return e.ID;
                }
            }
            return -1;
        }

        public static Ellipsoid[] ELLIPSOIDS = new Ellipsoid[] {
	        new Ellipsoid( -1, "Placeholder", 0, 0),			
	        new Ellipsoid( 1, "Airy", 6377563, 0.00667054),
	        new Ellipsoid( 2, "Australian National", 6378160, 0.006694542),
	        new Ellipsoid( 3, "Bessel 1841", 6377397, 0.006674372),
	        new Ellipsoid( 4, "Bessel 1841 (Nambia) ", 6377484, 0.006674372),
	        new Ellipsoid( 5, "Clarke 1866", 6378206, 0.006768658),
	        new Ellipsoid( 6, "Clarke 1880", 6378249, 0.006803511),
	        new Ellipsoid( 7, "Everest", 6377276, 0.006637847),
	        new Ellipsoid( 8, "Fischer 1960 (Mercury) ", 6378166, 0.006693422),
	        new Ellipsoid( 9, "Fischer 1968", 6378150, 0.006693422),
	        new Ellipsoid( 10, "GRS 1967", 6378160, 0.006694605),
	        new Ellipsoid( 11, "GRS 1980", 6378137, 0.00669438),
	        new Ellipsoid( 12, "Helmert 1906", 6378200, 0.006693422),
	        new Ellipsoid( 13, "Hough", 6378270, 0.00672267),
	        new Ellipsoid( 14, "International", 6378388, 0.00672267),
	        new Ellipsoid( 15, "Krassovsky", 6378245, 0.006693422),
	        new Ellipsoid( 16, "Modified Airy", 6377340, 0.00667054),
	        new Ellipsoid( 17, "Modified Everest", 6377304, 0.006637847),
	        new Ellipsoid( 18, "Modified Fischer 1960", 6378155, 0.006693422),
	        new Ellipsoid( 19, "South American 1969", 6378160, 0.006694542),
	        new Ellipsoid( 20, "WGS 60", 6378165, 0.006693422),
	        new Ellipsoid( 21, "WGS 66", 6378145, 0.006694542),
	        new Ellipsoid( 22, "WGS-72", 6378135, 0.006694318),
	        new Ellipsoid( 23, "WGS-84", 6378137, 0.00669438),
	        new Ellipsoid( 24, "GDA", 6378137, 0.0066943800229)
        };


        public static Ellipsoid FindEllipsoidByName(string Datum) {
            foreach (Ellipsoid e in ELLIPSOIDS) {
                if (e.Name.Equals(Datum, StringComparison.InvariantCultureIgnoreCase)) {
                    return e;
                }
            }
            return null;
        }
    }

    public class Ellipsoid {

        public Ellipsoid(int id, string name, double radius, double eccentricity) {
            this.ID = id;
            this.Name = name;
            this.EquatorialRadius = radius;
            this.EccentricitySquared = eccentricity;
        }

        public int ID { get; private set; }
        public string Name { get; private set; }
        public double EquatorialRadius { get; private set; }
        public double EccentricitySquared { get; private set; }


    }

    public enum CoordinateType {
        Latitude, Longitude
    }

    public enum AreaType {
        Point = 1,
        Line = 2,
        Box = 3
    }

    public class DirectionRange {

        public DirectionRange(double start, double end, string dir) {
            this.StartAngle = start;
            this.EndAngle = end;
            this.Direction = dir;
        }

        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
        public string Direction { get; set; }
    }
}
