using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.BVPImport {

    public class ColumnDefinitionBuilder {

        private static Dictionary<DarwinCoreField, String> DwCColumnMap = new Dictionary<DarwinCoreField, string> {
            { DarwinCoreField.eventID, "ExistingSiteVisitID" },
            { DarwinCoreField.dateIdentified, "Identified On" },
            { DarwinCoreField.catalogNumber, "Accession Number" },
            { DarwinCoreField.coordinateUncertaintyInMeters, "Position Error" },
            { DarwinCoreField.decimalLatitude, "Latitude" },
            { DarwinCoreField.decimalLongitude, "Longitude" },
            { DarwinCoreField.eventDate, "Start Date" },
            { DarwinCoreField.fieldNumber, "Field number" },
            { DarwinCoreField.habitat, "Macrohabitat" },
            { DarwinCoreField.identifiedBy, "Identified By"},
            { DarwinCoreField.locationID, "ExistingSiteID" },
            { DarwinCoreField.maximumElevationInMeters, "Elevation Upper" },
            { DarwinCoreField.minimumElevationInMeters, "Elevation lower" },
            { DarwinCoreField.maximumDepthInMeters, "Elevation Upper" },
            { DarwinCoreField.minimumDepthInMeters, "Elevation Lower" },
            { DarwinCoreField.occurrenceRemarks, "Original Label" },
            { DarwinCoreField.samplingProtocol, "Collection Method" },
            { DarwinCoreField.scientificNameAuthorship, "Author" },
            { DarwinCoreField.sex, "Gender" },
            { DarwinCoreField.stateProvince, "State/Province"},
            { DarwinCoreField.typeStatus, "Name status"},
        };


        public List<BVPImportColumnDefinition> ColumnDefinitions { get; private set; }

        public ColumnDefinitionBuilder() {
            this.ColumnDefinitions = new List<BVPImportColumnDefinition>();
        }

        public void ProcessDwCField(String dwcField, String filename) {
            DarwinCoreField dwc;
            if (Enum.TryParse<DarwinCoreField>(dwcField, out dwc)) {
                switch (dwc) {
                    case DarwinCoreField.scientificName:
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = "Genus", SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new RegexCaptureValueExtractor(@"^(.+?)\s+(?:.*)$") });
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = "Species", SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new RegexCaptureValueExtractor(@"^(?:.+?)\s+(.*)$") });
                        break;
                    case DarwinCoreField.catalogNumber:
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = MapDwCColumnName(dwcField), SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new RegexCaptureValueExtractor(@"^ANIC[_-](.*)$") });
                        break;
                    case DarwinCoreField.decimalLatitude:
                    case DarwinCoreField.decimalLongitude:
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = MapDwCColumnName(dwcField), SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new ANICLatLongValueExtractor() });
                        break;
                    case DarwinCoreField.verbatimLocality:
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = MapDwCColumnName(dwcField), SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new ANICStateStripValueExtractor() });
                        break;
                    case DarwinCoreField.stateProvince:
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = MapDwCColumnName(dwcField), SourceColumnName = dwcField, SourceFilename = filename, 
                            ValueExtractor = new ValueMappingValueExtractor(new Dictionary<String, String> {
                                {"Queensland", "QLD" },
                                {"Victoria", "VIC" },
                                {"Tasmania", "TAS" },
                                {"Western australia", "WA" },
                                {"Northern Territory", "NT" },
                                {"South Australia", "SA" },
                                {"New South Wales", "NSW" },
                                {"Australian Capital Territory", "ACT"}

                            }) });
                        break;
                    case DarwinCoreField.eventDate:
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = "Start Date", SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new StartDateValueExtractor() });
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = "End Date", SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new EndDateValueExtractor() });
                        break;
                    case DarwinCoreField.dateIdentified:
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = MapDwCColumnName(dwcField), SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new StartDateValueExtractor() });
                        break;
                    default:
                        ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = MapDwCColumnName(dwcField), SourceColumnName = dwcField, SourceFilename = filename });
                        break;
                }
            } else {
                // Not a Darwin Core Field? Just pass it straight through as is
                ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = dwcField, SourceColumnName = dwcField, SourceFilename = filename, ValueExtractor = new PassThroughValueExtractor() });
            }

        }

        public List<String> ColumnNames {
            get {
                return new List<String>(ColumnDefinitions.Select((col) => {
                    return col.OutputColumnName;
                }));
            }
        }

        public static String MapDwCColumnName(String name) {
            DarwinCoreField dwc;
            if (Enum.TryParse<DarwinCoreField>(name, out dwc)) {
                if (DwCColumnMap.ContainsKey(dwc)) {
                    return DwCColumnMap[dwc];
                }
            }
            return name;
        }


    }


    public class BVPImportColumnDefinition {

        public BVPImportColumnDefinition() {
            this.ValueExtractor = new PassThroughValueExtractor();
        }

        public String OutputColumnName { get; set; }
        public String SourceFilename { get; set; }
        public String SourceColumnName { get; set; }

        public ValueExtractor ValueExtractor { get; set; }
    }

}
