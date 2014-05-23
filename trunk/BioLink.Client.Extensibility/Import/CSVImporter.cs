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
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.Windows;
using System.Data;
using System.Data.SQLite;
using GenericParsing;
using BioLink.Data;
using System.IO;
using System.Web;

namespace BioLink.Client.Extensibility.Import {

    public class CSVImporter : TabularDataImporter {

        private CSVImporterOptions _options;

        public override bool GetOptions(System.Windows.Window parentWindow, ImportWizardContext context) {

            var frm = new CSVImportOptionsWindow(_options);
            frm.Owner = parentWindow;
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog().GetValueOrDefault(false)) {
                _options = new CSVImporterOptions { Filename = frm.Filename, Delimiter = frm.Delimiter, FirstRowContainsNames = frm.IsFirstRowContainNames, ColumnNames = frm.ColumnNames };
                return true;
            }

            return false;
        }

        public override ImportRowSource CreateRowSource(IProgressObserver progress) {

            if (_options == null) {
                throw new Exception("Null or incorrect options type received!");
            }

            ImportRowSource rowsource = null;

            using (var parser = new GenericParserAdapter(_options.Filename)) {
                parser.ColumnDelimiter = _options.Delimiter[0];
                parser.FirstRowHasHeader = _options.FirstRowContainsNames;
                parser.TextQualifier = '\"';
                parser.FirstRowSetsExpectedColumnCount = true;

                var service = new ImportStagingService();
                var columnNames = new List<String>();

                int rowCount = 0;
                service.BeginTransaction();
                var values = new List<string>();
                while (parser.Read()) {
                    if (rowCount == 0) {
                        for (int i = 0; i < parser.ColumnCount; ++i) {
                            if (_options.FirstRowContainsNames) {
                                columnNames.Add(parser.GetColumnName(i));
                            } else {
                                columnNames.Add("Column" + i);
                            }
                        }
                        service.CreateImportTable(columnNames);
                    }

                    values.Clear();
                    for (int i = 0; i < parser.ColumnCount; ++i) {
                        values.Add(parser[i]);
                    }

                    service.InsertImportRow(values);

                    rowCount++;
                }

                service.CommitTransaction();

                rowsource = new ImportRowSource(service, rowCount);
            }

            return rowsource;
        }

        public override string Name {
            get { return "Delimited text file"; }
        }

        public override string Description {
            get {
                return "Imports data from a flat text file delimited by specific characters (comma, tab etc.)";
            }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/csv_exporter.png", GetType().Assembly.GetName().Name);
            }
        }

        protected override void WriteEntryPoint(EntryPoint ep) {
            ep.AddParameter("Filename", _options.Filename);
            ep.AddParameter("FirstRowHeaders", _options.FirstRowContainsNames.ToString());
            ep.AddParameter("Delimiter", HttpUtility.HtmlEncode(_options.Delimiter));
        }

        protected override void ReadEntryPoint(EntryPoint ep) {
            _options = new CSVImporterOptions();
            _options.Filename = ep["Filename"];
            _options.FirstRowContainsNames = Boolean.Parse(ep["FirstRowHeaders", "true"]);
            _options.Delimiter = HttpUtility.HtmlDecode(ep["Delimiter"]);
        }

        public override List<string> GetColumnNames() {
            return _options.ColumnNames;
        }
    }

    public class CSVImporterOptions {

        public string Filename { get; set; }
        public string Delimiter { get; set; }
        public bool FirstRowContainsNames { get; set; }
        public List<string> ColumnNames { get; set; }

    }

}
