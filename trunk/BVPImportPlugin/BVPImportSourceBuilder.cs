using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using Ionic.Zip;
using GenericParsing;
using System.IO;

namespace BioLink.Client.BVPImport {

    public class BVPImportSourceBuilder {

        public String Filename { get; private set; }
        protected ImportStagingService Service { get; private set; }

        public BVPImportSourceBuilder(String filename) {
            this.Filename = filename;
            this.Service = new ImportStagingService();
        }

        public ImportRowSource BuildRowSource() {
            var rowCount = 0;

            var extra = new Dictionary<int, Dictionary<String, List<String>>>();

            // first define the columns...
            var columnBuilder = new ColumnDefinitionBuilder();
            using (ZipFile zipfile = ZipFile.Read(Filename)) {
                foreach (ZipEntry entry in zipfile) {
                    if (entry.FileName.Equals("tasks.csv", StringComparison.OrdinalIgnoreCase)) {
                        GetTaskColumns(entry, columnBuilder);
                    } else if (entry.FileName.Equals("recordedBy.csv", StringComparison.CurrentCultureIgnoreCase)) {
                        AddToExtraData(entry, "recordedBy", extra);
                        var valueExtractor = new ANICCollectorNameFormattingValueExtractor();
                        columnBuilder.ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = "Collector(s)", SourceColumnName = "recordedBy", SourceFilename = entry.FileName, ValueExtractor = valueExtractor });
                    } else if (entry.FileName.Equals("associatedMedia.csv", StringComparison.CurrentCultureIgnoreCase)) {
                        AddToExtraData(entry, "associatedMedia", extra);
                        columnBuilder.ColumnDefinitions.Add(new BVPImportColumnDefinition { OutputColumnName = "associatedMedia", SourceColumnName = "associatedMedia", SourceFilename = entry.FileName, ValueExtractor = new AssociatedMediaValueExtractor() });
                    }
                }
            }
            // Now we have all the columns, we can create the staging table
            Service.BeginTransaction();
            Service.CreateImportTable(columnBuilder.ColumnNames);

            // And get the values...            
            using (ZipFile zipfile = ZipFile.Read(Filename)) {
                foreach (ZipEntry entry in zipfile) {
                    if (entry.FileName.Equals("tasks.csv")) {
                        IterateOverCSVZipEntry(entry, (parser) => {
                            var taskId = Int32.Parse(parser["taskID"]);
                            Dictionary<String, List<String>> fieldMap = null;
                            if (extra.ContainsKey(taskId)) {
                                fieldMap = extra[taskId];
                            }
                            var values = new List<String>();
                            columnBuilder.ColumnDefinitions.ForEach((coldef) => {
                                var value = coldef.ValueExtractor.ExtractValue(coldef, parser, fieldMap);
                                values.Add(value);
                            });
                            Service.InsertImportRow(values);
                            rowCount++;
                            return true;
                        });
                    }
                }
            }
            Service.CommitTransaction();
            return new ImportRowSource(Service, rowCount);
        }

        private void AddToExtraData(ZipEntry entry, String columnName, Dictionary<int, Dictionary<String, List<String>>> data) {
            var rowCount = 0;
            IterateOverCSVZipEntry(entry, (parser) => {
                var taskId = Int32.Parse(parser["taskID"]);
                if (!data.ContainsKey(taskId)) {
                    data[taskId] = new Dictionary<string, List<string>>();
                }
                var fieldMap = data[taskId];
                if (!fieldMap.ContainsKey(columnName)) {
                    fieldMap[columnName] = new List<string>();
                }
                var fieldValues = fieldMap[columnName];
                var fieldValue = parser[columnName];
                fieldValues.Add(fieldValue);
                rowCount++;
                return true;
            });
        }

        private void GetTaskColumns(ZipEntry entry, ColumnDefinitionBuilder colummBuilder) {
            IterateOverCSVZipEntry(entry, (parser) => {
                for (int i = 0; i < parser.ColumnCount; ++i) {
                    var dwcName = parser.GetColumnName(i);
                    colummBuilder.ProcessDwCField(dwcName, entry.FileName);
                }
                return false;
            });
        }

        protected String RegexCapture(BVPImportColumnDefinition colDef, GenericParserAdapter row, Dictionary<String, List<String>> extra) {
            return row[colDef.SourceColumnName];
        }

        protected void IterateOverCSVZipEntry(ZipEntry entry, Func<GenericParserAdapter, bool> func) {
            using (var reader = new StreamReader(entry.OpenReader())) {
                using (var parser = new GenericParserAdapter(reader)) {
                    parser.ColumnDelimiter = ',';
                    parser.FirstRowHasHeader = true;
                    parser.TextQualifier = '\"';
                    parser.FirstRowSetsExpectedColumnCount = true;
                    while (parser.Read()) {
                        if (func != null) {
                            if (!func(parser)) {
                                return;
                            }
                        }
                    }
                }
            }
        }

    }
}
