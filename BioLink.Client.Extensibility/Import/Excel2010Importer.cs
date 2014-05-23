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
using BioLink.Data;
using System.Data;
using System.IO;
using Excel;

namespace BioLink.Client.Extensibility {

    public class Excel2010Importer : TabularDataImporter {

        private Excel2010ImporterOptions _options;

        public override bool GetOptions(System.Windows.Window parentWindow, ImportWizardContext context) {

            var frm = new Excel2010ImporterOptionsWindow(_options);
            if (frm.ShowDialog().ValueOrFalse()) {
                _options = new Excel2010ImporterOptions { Filename = frm.Filename, Worksheet = frm.Worksheet };
                return true;
            }

            return false;
        }

        public override ImportRowSource CreateRowSource() {

            if (_options == null) {
                throw new Exception("Null or incorrect options type received!");
            }

            ImportRowSource rowsource = null;

            var columnNames = GetColumnNames();
            var service = new ImportStagingService();
            service.CreateImportTable(columnNames);

            ExcelDataTable(_options.Filename, _options.Worksheet, 0, dt => {

                service.BeginTransaction();
                var values = new List<string>();
                int rowcount = 0;
                foreach (DataRow row in dt.Rows) {
                    values.Clear();
                    foreach (DataColumn col in dt.Columns) {
                        var value = row[col];
                        values.Add((value == null ? "" : value.ToString()));
                    }
                    service.InsertImportRow(values);
                    rowcount++;
                }
                service.CommitTransaction();

                rowsource = new ImportRowSource(service, rowcount);
            });

            return rowsource;
        }

        public override string Name {
            get { return "Excel 2007/2010 (*.xlsx)"; }
        }

        public override string Description {
            get { return "Imports data from a Microsoft Excel Open XML worksheet (*.xlsx)"; }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get { return ImageCache.GetPackedImage("images/excel2007_exporter.png", GetType().Assembly.GetName().Name); }
        }

        public override List<string> GetColumnNames() {
            var columns = new List<string>();

            ExcelDataTable(_options.Filename, _options.Worksheet, 1, dt => {
                foreach (DataColumn column in dt.Columns) {
                    columns.Add(column.ColumnName);
                }

            });

            return columns;
        }

        protected override void WriteEntryPoint(Utilities.EntryPoint ep) {
            if (_options != null) {
                ep.AddParameter("Filename", _options.Filename);
                ep.AddParameter("Worksheet", _options.Worksheet);
            }
        }

        protected override void ReadEntryPoint(Utilities.EntryPoint ep) {
            _options = new Excel2010ImporterOptions { Filename = ep["Filename"], Worksheet = ep["Worksheet"] };
        }

        public static List<String> GetWorksheetNames(String filename, Boolean suppressException = false) {

            var sheetNames = new List<string>();

            try {
                var stream = new FileStream(filename, FileMode.Open);
                using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream)) {
                    using (DataSet result = excelReader.AsDataSet()) {
                        if (result != null) {
                            foreach (DataTable tbl in result.Tables) {
                                sheetNames.Add(tbl.TableName);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                ErrorMessage.Show("An error occurred trying to open the selected file: {0}", ex.Message);
            }

            return sheetNames;            
        }

        public static void ExcelDataTable(String filename, String worksheet, int maxRows, Action<DataTable> action) {

            if (String.IsNullOrEmpty(filename) || String.IsNullOrEmpty(worksheet)) {
                return;
            }

            var stream = new FileStream(filename, FileMode.Open);
            using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream)) {
                excelReader.IsFirstRowAsColumnNames = true;

                do {
                    if (excelReader.Name.Equals(worksheet, StringComparison.CurrentCultureIgnoreCase)) {
                        int row = 0;
                        DataTable dt = new DataTable(excelReader.Name);
                        while (excelReader.Read()) {

                            if (row == 0) {
                                for (int i = 0; i < excelReader.FieldCount; ++i) {
                                    var column = dt.Columns.Add(excelReader.GetString(i), typeof(string));
                                }
                            } else {
                                var rowData = new String[dt.Columns.Count];
                                for (int i = 0; i < excelReader.FieldCount; ++i) {
                                    rowData[i] = excelReader.GetString(i);
                                }
                                dt.Rows.Add(rowData);
                            }
                            if (row++ > maxRows) {
                                break;
                            }
                        }
                        if (action != null) {
                            action(dt);
                        }

                        break;
                    }
                } while (excelReader.NextResult());

            }

        }
    }

    public class Excel2010ImporterOptions {
        public string Filename { get; set; }
        public string Worksheet { get; set; }
    }
}
