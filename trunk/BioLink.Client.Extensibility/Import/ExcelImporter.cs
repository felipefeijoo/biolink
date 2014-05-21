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
using System.Data.OleDb;

namespace BioLink.Client.Extensibility {

    public class ExcelImporter : TabularDataImporter {

        private ExcelImporterOptions _options;

        public override bool GetOptions(System.Windows.Window parentWindow, ImportWizardContext context) {

            var frm = new ExcelImporterOptionsWindow(_options);
            if (frm.ShowDialog().ValueOrFalse()) {
                _options = new ExcelImporterOptions { Filename = frm.Filename, Worksheet = frm.Worksheet };
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

            if (WorksheetDataTable(string.Format("SELECT * FROM [{0}]", _options.Worksheet), (dt) => {

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

            })) {
                return rowsource;
            }

            return null;
        }

        public override string Name {
            get { return "Excel worksheet (*.xls)"; }
        }

        public override string Description {
            get { return "Imports data from an Excel worksheet"; }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get { return ImageCache.GetPackedImage("images/excel2003_exporter.png", GetType().Assembly.GetName().Name); }
        }

        private bool WorksheetDataTable(string sql, Action<DataTable> action) {

            OleDbConnection conn = null;
            DataTable dt = null;

            try {
                String connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=Excel 8.0;", _options.Filename);

                conn = new OleDbConnection(connString);
                conn.Open();
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                OleDbDataAdapter dbAdapter = new OleDbDataAdapter();

                dbAdapter.SelectCommand = cmd;

                DataSet ds = new DataSet();
                dbAdapter.Fill(ds);
                if (ds.Tables.Count == 0) {
                    return false;
                }
                dt = ds.Tables[0];
                conn.Close();

                if (dt == null) {
                    return false;
                }

                if (action != null) {
                    action(dt);
                }

                return true;

            } catch (Exception ex) {
                ErrorMessage.Show("An error occurred whilst attempting to read the worksheet: {0}", ex.Message);
                return false;
            } finally {
                if (conn != null) {
                    conn.Close();
                    conn.Dispose();
                }
                if (dt != null) {
                    dt.Dispose();
                }
            }

        }

        public override List<string> GetColumnNames() {
            var columns = new List<string>();

            if (WorksheetDataTable(string.Format("SELECT TOP 1 * FROM [{0}]", _options.Worksheet), (dt) => {
                foreach (DataColumn col in dt.Columns) {
                    columns.Add(col.ColumnName);
                }
            } )) {
                return columns;
            }

            return null;
        }

        protected override void WriteEntryPoint(Utilities.EntryPoint ep) {
            if (_options != null) {
                ep.AddParameter("Filename", _options.Filename);
                ep.AddParameter("Worksheet", _options.Worksheet);
            }
        }

        protected override void ReadEntryPoint(Utilities.EntryPoint ep) {
            _options = new ExcelImporterOptions { Filename = ep["Filename"], Worksheet = ep["Worksheet"] };
        }
    }

    public class ExcelImporterOptions {
        public string Filename { get; set; }
        public string Worksheet { get; set; }
    }
}
