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


namespace BioLink.Client.Extensibility.Export {

    public class Excel2003Exporter : TabularDataExporter {

        protected override object GetOptions(Window parentWindow, DataMatrix matrix) {

            var filename = PromptForFilename(".txt", "XML Excel Workbook (.xml)|*.xml");
            if (!String.IsNullOrEmpty(filename)) {
                ExcelExporterOptions options = new ExcelExporterOptions();
                options.Filename = filename;
                return options;
            }

            return null;
        }

        public override void ExportImpl(Window parentWindow, Data.DataMatrix matrix, object optionsObj) {
            ExcelExporterOptions options = optionsObj as ExcelExporterOptions;

            if (options == null) {
                return;
            }
            
            if (FileExistsAndNotOverwrite(options.Filename)) {
                return;
            }

            ProgressStart("Preparing to export...");

            int totalRows = matrix.Rows.Count;

            using (StreamWriter writer = new StreamWriter(options.Filename)) {
                writer.WriteLine("<?xml version=\"1.0\"?><ss:Workbook xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
                writer.WriteLine("<ss:Worksheet ss:Name=\"Exported Data\">");
                writer.WriteLine("<ss:Table>");
                int currentRow = 0;                
                foreach (MatrixRow row in matrix.Rows) {                    
                    writer.WriteLine("<ss:Row>");
                    for (int i = 0; i < matrix.Columns.Count; ++i) {
                        if (!matrix.Columns[i].IsHidden) {
                            object val = row[i];
                            writer.Write("<ss:Cell><ss:Data ss:Type=\"String\">");
                            String str = (val == null ? "" : val.ToString());
                            writer.Write(Escape(str));
                            writer.Write("</ss:Data></ss:Cell>");
                        }
                    }
                    if (++currentRow % 1000 == 0) {
                        double percent = ((double)currentRow / (double)totalRows) * 100.0;
                        ProgressMessage(String.Format("Exported {0} of {1} rows...", currentRow, totalRows), percent);
                    }

                    writer.WriteLine("</ss:Row>");
                }

                writer.WriteLine("</ss:Table>");
                writer.WriteLine("</ss:Worksheet>");
                writer.WriteLine("</ss:Workbook>");
            }

            ProgressEnd(String.Format("{0} rows exported.", totalRows));
        }

        public override void Dispose() {
        }

        #region Properties

        public override string Description {
            get { return "Export data as a Microsoft Excel 2003 XML Worksheet"; }
        }

        public override string Name {
            get { return "Excel 2003 XML"; }
        }

        public override BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/excel2003_exporter.png", GetType().Assembly.GetName().Name);
            }
        }

        #endregion


        public override bool CanExport(DataMatrix matrix) {
            return true;
        }
    }


    public class ExcelExporterOptions {

        public string Filename { get; set; }

    }
}
