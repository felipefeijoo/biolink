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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Windows;

namespace BioLink.Client.Extensibility {

    public class ErrorDatabaseImporter : TabularDataImporter {

        private ErrorDatabaseImporterOptions _options;
        private List<string> _columnNames;

        public override bool GetOptions(System.Windows.Window parentWindow, ImportWizardContext context) {
            var filename = PromptForFilename("*.sqlite", "Error databases (*.sqlite)|*.sqlite");
            if (!string.IsNullOrEmpty(filename)) {
                _options = new ErrorDatabaseImporterOptions { Filename = filename };
                var frm = new ErrorDatabaseImportOptionsWindow(_options, context);
                frm.Owner = parentWindow;
                frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                return frm.ShowDialog().ValueOrFalse();
            }

            return false;
        }

        public override ImportRowSource CreateRowSource() {
            var errorSource = new ImportStagingService(_options.Filename);

            var service = new ImportStagingService();
            service.CreateImportTable(_columnNames);
            int rowcount = 0;
            var reader = errorSource.GetErrorReader();

            while (reader.Read()) {
                var values = new List<String>();
                for (int i = 0; i < reader.FieldCount - 1; ++i) {
                    var val = reader[i];
                    values.Add(val == null ? null : val.ToString());
                }
                service.InsertImportRow(values);
                rowcount++;
            }

            return new ImportRowSource(service, rowcount);

        }

        public override string Name {
            get { return "Import Error Database"; }
        }

        public override string Description {
            get { return "Facilitates the correction and reimporting of rows from a previous import attempt"; }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get { return ImageCache.GetPackedImage("images/sqlite_exporter.png", GetType().Assembly.GetName().Name); }
        }

        public override List<string> GetColumnNames() {
            var service = new ImportStagingService(_options.Filename);
            var mappings = service.GetMappings();
            service.Dispose();
            _columnNames = new List<string>(mappings.Select((mapping) => {
                return mapping.SourceColumn;
            }));

            return _columnNames;            
        }

        protected override void WriteEntryPoint(Utilities.EntryPoint ep) {
            ep.AddParameter("Filename", _options.Filename);
        }

        protected override void ReadEntryPoint(Utilities.EntryPoint ep) {
            _options = new ErrorDatabaseImporterOptions { Filename = ep["Filename"] };
        }
    }

    public class ErrorDatabaseImporterOptions {

        public string Filename { get; set; }

    }
}
