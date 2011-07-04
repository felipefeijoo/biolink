using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class FormattedLatLongVirtualColumn : VirtualMatrixColumn {

        public FormattedLatLongVirtualColumn(DataMatrix matrix) {

            var areaTypeCol = matrix.IndexOf("AreaType");
            var latCol = matrix.IndexOf("Lat");
            var longCol = matrix.IndexOf("Long");
            var latCol2 = matrix.IndexOf("Lat2");
            var longCol2 = matrix.IndexOf("Long2");

            this.Name = "FormattedLatLong";

            this.ValueGenerator = (row) => {

                object objLat = row[latCol];
                object objlong = row[longCol];
                object objAreaType = row[areaTypeCol];

                if (objAreaType != null && objLat != null && objlong != null) {
                    int areaType = int.Parse(objAreaType.ToString());
                    double lat = (double)objLat;
                    double @long = (double)objlong;
                    AreaType t = (AreaType)areaType;
                    switch (t) {
                        case AreaType.Point:
                            return string.Format("{0}, {1}", GeoUtils.DecDegToDMS(lat, CoordinateType.Latitude), GeoUtils.DecDegToDMS(@long, CoordinateType.Longitude));
                        case AreaType.Line:
                            double lat2 = (double)row[latCol2];
                            double long2 = (double)row[longCol2];
                            return string.Format("Line: {0}, {1} - {2}, {3}",
                                GeoUtils.DecDegToDMS(lat, CoordinateType.Latitude), GeoUtils.DecDegToDMS(@long, CoordinateType.Longitude),
                                GeoUtils.DecDegToDMS(lat2, CoordinateType.Latitude), GeoUtils.DecDegToDMS(long2, CoordinateType.Longitude));
                        case AreaType.Box:
                            lat2 = (double)row[latCol2];
                            long2 = (double)row[longCol2];
                            return string.Format("Box: {0}, {1} - {2}, {3}",
                                GeoUtils.DecDegToDMS(lat, CoordinateType.Latitude), GeoUtils.DecDegToDMS(@long, CoordinateType.Longitude),
                                GeoUtils.DecDegToDMS(lat2, CoordinateType.Latitude), GeoUtils.DecDegToDMS(long2, CoordinateType.Longitude));
                    }
                }

                return "";
            };
        }        
    }

    public class FormattedDateVirtualColumn : VirtualMatrixColumn {

        public FormattedDateVirtualColumn(DataMatrix matrix) {

            int dateTypeCol = matrix.IndexOf("DateType");
            int startDateCol = matrix.IndexOf("StartDate");
            int endDateCol = matrix.IndexOf("EndDate");
            int casualDateCol = matrix.IndexOf("CasualDate");

            Name = "Dates";
            ValueGenerator = (row) => {
                int dateType = 0;
                object objDateType = row[dateTypeCol];
                if (objDateType != null) {
                    int.TryParse(objDateType.ToString(), out dateType);
                    return DateUtils.FormatDates(dateType, row[startDateCol] as int?, row[endDateCol] as int?, row[casualDateCol] as string);

                }
                return "";
            };

        }

        private string FormatDate(int d, bool asRomanMonth = false) {
            if (d == 0) {
                return "";
            }

            if (asRomanMonth) {
                return DateUtils.DateRomanMonth(d);
            } else {
                return DateUtils.BLDateToStr(d);
            }
        }


    }
}
