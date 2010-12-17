using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            int sign = ("sw".Contains(direction.ToLower()) ? -1 : 1 );
            return (Math.Abs(degrees) + (minutes / 60)) * (double) sign;
        }

        public static void DecDegToDMS(double decdeg, CoordinateType coordType, out int degrees, out int minutes, out int seconds, out string direction) {
            degrees = (int) Math.Abs(decdeg);
            double leftover = (Math.Abs(decdeg) - degrees);
            minutes = (int) (leftover * 60);
            leftover = leftover - ((double) minutes / 60.0);
            seconds = (int) Math.Round(leftover * 3600,MidpointRounding.AwayFromZero);

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

        public static double DMSToDecDeg(int degrees, int minutes, int seconds, string direction) {
            if (direction == null) {
                direction = "N";
            }
            var decdeg = (double)degrees + ((double)minutes / 60.0) + ((double)seconds / 3600.0);
            var sign = (double) ("sw".Contains(direction.ToLower()) ? -1 : 1); 
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
	        if ( latitude < -90.0 ) { 
                latitude = -90.0; 
            }

	        if ( latitude > 90.0 ) {
                latitude = 90.0;
            }

	        if ( longitude < -180.0 ) {
                longitude = -180.0;
            }

	        if ( longitude > 180.0 ) {
                longitude = 180.0;
            }

	        if ( longitude > -6 && longitude <= 0 ) {
                LongOrigin = -3;	//arbitrarily set origin at 0 longitude to 3W
            } else if ( longitude < 6 && longitude > 0 ) {
                LongOrigin = 3;
            } else {
                LongOrigin = ((int) longitude / 6 ) * 6 + 3 * ((int) longitude / 6 ) / Math.Abs( (int) longitude / 6 );
            }

	        LongOriginRad = LongOrigin * DEGREES_TO_RADS;

	        //compute the UTM Zone from the latitude and longitude
            zone = string.Format("{0}{1}", (int) ((longitude + 180) / 6) + 1, UTMLetterDesignator(latitude));
	
	        eccPrimeSquared = (eccSquared)/(1-eccSquared);

	        N = a / Math.Sqrt(1-eccSquared * Math.Sin(LatRad) * Math.Sin(LatRad));
	        T = Math.Tan(LatRad) * Math.Tan(LatRad);
	        C = eccPrimeSquared * Math.Cos(LatRad) * Math.Cos(LatRad);
	        A = Math.Cos(LatRad) * (LongRad-LongOriginRad);

	        M = a*((1	- eccSquared/4		- 3*eccSquared*eccSquared/64	- 5*eccSquared*eccSquared*eccSquared/256)*LatRad 
				- (3*eccSquared/8	+ 3*eccSquared*eccSquared/32	+ 45*eccSquared*eccSquared*eccSquared/1024)* Math.Sin(2*LatRad)
									+ (15*eccSquared*eccSquared/256 + 45*eccSquared*eccSquared*eccSquared/1024)*Math.Sin(4*LatRad) 
									- (35*eccSquared*eccSquared*eccSquared/3072) * Math.Sin(6*LatRad));
	
	        easting = (double)(k0*N*(A+(1-T+C)*A*A*A/6 + (5-18*T+T*T+72*C-58*eccPrimeSquared)*A*A*A*A*A/120) + 500000.0);

	        northing = (double)(k0*(M+N* Math.Tan(LatRad)*(A*A/2+(5-T+9*C+4*C*C)*A*A*A*A/24 + (61-58*T+T*T+600*C-330*eccPrimeSquared)*A*A*A*A*A*A/720)));
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
}
