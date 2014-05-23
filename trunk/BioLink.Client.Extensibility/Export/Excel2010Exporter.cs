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
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using System.IO;
using BioLink.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BioLink.Client.Extensibility.Export {

    public class Excel2010Exporter : TabularDataExporter {

        protected override object GetOptions(Window parentWindow, DataMatrix matrix, String datasetName) {

            var filename = PromptForFilename(".txt", "Excel 2007/2010 Workbook (.xlsx)|*.xlsx", datasetName);
            if (!String.IsNullOrEmpty(filename)) {
                ExcelExporterOptions options = new ExcelExporterOptions();
                options.Filename = filename;
                return options;
            }

            return null;
        }

        public override void ExportImpl(Window parentWindow, Data.DataMatrix matrix, string datasetName, object optionsObj) {
            ExcelExporterOptions options = optionsObj as ExcelExporterOptions;

            if (options == null) {
                return;
            }
            
            if (FileExistsAndNotOverwrite(options.Filename)) {
                return;
            }

            ProgressStart("Preparing to export...");

            int totalRows = matrix.Rows.Count;

            using (ExcelPackage p = new ExcelPackage(new FileInfo(options.Filename))) {
                //Here setting some document properties
                var v = this.GetType().Assembly.GetName().Version;
                var version = String.Format("Version {0}.{1} (build {2})", v.Major, v.Minor, v.Revision);
                p.Workbook.Properties.Author = "BioLink " + version;
                var name = StringUtils.RemoveAll(datasetName, '&', '\'','"','<','>');
                p.Workbook.Properties.Title = name;
                var ws = p.Workbook.Worksheets.Add("DwC");

                // Column headings...
                int colIndex = 1;
                matrix.Columns.ForEach(column => {
                    var cell = ws.Cells[1, colIndex++].Value = column.Name;
                });

                int rowIndex = 0;
                foreach (MatrixRow row in matrix.Rows) {
                    for (int i = 0; i < matrix.Columns.Count; ++i) {
                        if (!matrix.Columns[i].IsHidden) {
                            object val = row[i];
                            String strValue = (val == null ? "" : val.ToString());
                            ws.SetValue(rowIndex + 2, i + 1, strValue);
                        }
                    }
                    if (rowIndex++ % 1000 == 0) {
                        double percent = ((double)rowIndex / (double)totalRows) * 100.0;
                        ProgressMessage(String.Format("Exported {0} of {1} rows...", rowIndex, totalRows), percent);
                    }
                }

                ProgressMessage(String.Format("Exported {0} of {1} rows. Saving file '{2}'", rowIndex, totalRows, options.Filename), 100);

                p.Save();
            }

            ProgressEnd(String.Format("{0} rows exported.", totalRows));
        }

        public override void Dispose() {
        }

        #region Properties

        public override string Description {
            get { return "Export data as a Microsoft Excel 2007/2010 Open XML Worksheet (*.xlsx)"; }
        }

        public override string Name {
            get { return "Excel 2007/2010 XLSX"; }
        }

        public override BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/excel2007_exporter.png", GetType().Assembly.GetName().Name);
            }
        }

        #endregion


        public override bool CanExport(DataMatrix matrix, String datasetName) {
            return true;
        }
    }


}
