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
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Text;

namespace BioLink.Client.Extensibility.Export {

    public class CSVExporter : TabularDataExporter {

        private const string _quote = "\"";

        protected override object GetOptions(Window parentWindow, DataMatrix matrix) {
            var optionsWindow = new CSVExporterOptionsWindow { Owner = parentWindow };
            if (optionsWindow.ShowDialog().GetValueOrDefault(false)) {
                CSVExporterOptions options = optionsWindow.Options;
                if (FileExistsAndNotOverwrite(options.Filename)) {
                    return null;
                }
                
                return options;
            }
            return null;
        }

        public override void ExportImpl(Window parentWindow, DataMatrix matrix, object optionsObj) {

            var options = optionsObj as CSVExporterOptions;

            if (options == null) {
                return;
            }

            ProgressStart(String.Format("Exporting to {0}", options.Filename));

            var encoding = options.Encoding ?? Encoding.GetEncoding(1252);

            using (var writer = new StreamWriter(options.Filename, false, encoding)) {
                int numCols = matrix.Columns.Count;
                if (options.ColumnHeadersAsFirstRow) {
                    // write out the column headers as the first row...

                    for (int colIndex = 0; colIndex < numCols; ++colIndex) {
                        MatrixColumn col = matrix.Columns[colIndex];
                        if (!col.IsHidden) {
                            if (options.QuoteValues) {
                                writer.Write(_quote);
                            }
                            writer.Write(col.Name);
                            if (options.QuoteValues) {
                                writer.Write(_quote);
                            }
                            if (colIndex < numCols - 1) {
                                writer.Write(options.Delimiter);
                            }
                        }
                    }
                    writer.WriteLine();
                }

                // Now emit each row...
                var numRows = matrix.Rows.Count;
                var currentRow = 0;
                var formatter = new MatrixValueFormatter(matrix);

                for (int rowIndex = 0; rowIndex < matrix.Rows.Count; ++rowIndex) {                    
                    for (int colIndex = 0; colIndex < numCols; ++colIndex) {
                        if (!matrix.Columns[colIndex].IsHidden) {

                            var value = formatter.FormatValue(rowIndex, colIndex);

                            var quoteValue = options.QuoteValues || value.Contains(options.Delimiter);

                            if (quoteValue) {
                                writer.Write(_quote);
                            }
                            writer.Write(value);
                            if (quoteValue) {
                                writer.Write(_quote);
                            }
                            if (colIndex < numCols - 1) {
                                writer.Write(options.Delimiter);
                            }
                        }
                    }
                    writer.WriteLine();
                    currentRow++;
                    if ((currentRow % 1000) == 0) {
                        double percent = (currentRow / ((double)numRows)) * 100.0;
                        ProgressMessage(String.Format("{0} rows exported to {1}", currentRow, options.Filename), percent);
                    }
                }
                ProgressEnd(String.Format("{0} rows exported to {1}", matrix.Rows.Count, options.Filename));
            }
        }

        public override void Dispose() {
        }

        #region Properties

        public override string Description {
            get { return "Export data as a delimited text file"; }
        }

        public override string Name {
            get { return "Delimited text file"; }
        }

        public override BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/csv_exporter.png", GetType().Assembly.GetName().Name);
            }
        }

        #endregion


        public override bool CanExport(DataMatrix matrix) {
            return true;
        }
    }

}
