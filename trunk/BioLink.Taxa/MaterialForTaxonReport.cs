using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {

    public class MaterialForTaxonReport : TaxonReportBase {

        public MaterialForTaxonReport(TaxaService service, TaxonViewModel taxon)
            : base(service, taxon) {
            RegisterViewer(new TabularDataViewerSource());
            DefineColumn("BiotaFullName", "Taxa");
            DefineColumn("FullRegion", "Region");
            DefineColumn("Local", "Locality");
            DefineColumn("FormattedLatLong", "Lat/Long");
            DefineColumn("Collectors");
            DefineColumn("Dates");
            DefineColumn("AccessionNo");
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            progress.ProgressStart(string.Format("Retrieving Material records for {0}", Taxon.DisplayLabel));

            DataMatrix matrix = Service.GetMaterialForTaxon(Taxon.TaxaID.Value);
            progress.ProgressEnd(string.Format("{0} rows retreived", matrix.Rows.Count));

            int areaTypeCol = matrix.IndexOf("AreaType");
            int latCol = matrix.IndexOf("Lat");
            int longCol = matrix.IndexOf("Long");
            int latCol2 = matrix.IndexOf("Lat2");
            int longCol2 = matrix.IndexOf("Long2");

            int dateTypeCol = matrix.IndexOf("DateType");
            int startDateCol = matrix.IndexOf("StartDate");
            int endDateCol = matrix.IndexOf("EndDate");
            int casualDateCol = matrix.IndexOf("CasualDate");

            matrix.Columns.Add(new VirtualMatrixColumn {
                Name = "FormattedLatLong", ValueGenerator = (row) => {

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
                }
            });

            matrix.Columns.Add(new VirtualMatrixColumn {
                Name = "Dates", ValueGenerator = (row) => {
                    int dateType = 0;
                    object objDateType = row[dateTypeCol];
                    if (objDateType != null) {
                        int.TryParse(objDateType.ToString(), out dateType);
                        if (dateType == 2) {
                            return row[casualDateCol];
                        } else {
                            int? startdate = row[startDateCol] as int?;
                            int? enddate = row[endDateCol] as int?;
                            if (startdate.HasValue && enddate.HasValue) {
                                if (startdate == enddate) {
                                    return FormatDate(startdate.Value);
                                } else if (startdate == 0) {
                                    return "Before " + FormatDate(enddate.Value, false);
                                } else if (enddate == 0) {
                                    return FormatDate(startdate.Value, false);
                                } else {
                                    return string.Format("{0} to {1}", FormatDate(startdate.Value), FormatDate(enddate.Value));
                                }
                            }
                        }
                    }
                    return "";
                }
            });

            return matrix;
        }

        private string FormatDate(long d, bool asRomanMonth = false) {
            if (d == 0) {
                return "";
            }

            if (asRomanMonth) {
                return DateUtils.DateRomanMonth(d);
            } else {
                return DateUtils.BLDateToStr(d);
            }
        }

        public override string Name {
            get { return "Material for taxon list..."; }
        }

    }

}

