﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using System.IO;
using BioLink.Data;

namespace BioLink.Client.Extensibility.Export {

    public class CSVExporter : TabularDataExporter {

        private string _quote = "\"";

        protected override object GetOptions(Window parentWindow) {
            CSVExporterOptionsWindow optionsWindow = new CSVExporterOptionsWindow();
            optionsWindow.Owner = parentWindow;
            if (optionsWindow.ShowDialog().GetValueOrDefault(false)) {

                CSVExporterOptions options = optionsWindow.Options;

                FileInfo f = new FileInfo(options.Filename);
                
                if (f.Exists) {
                    if (!optionsWindow.Question(String.Format("The file {0} already exists. Do you wish to overwrite it?", options.Filename), "Overwrite existing file?")) {
                        // bail!
                        return null;
                    }

                    if (f.IsReadOnly) {
                        ErrorMessage.Show("{0} is not writable. Please ensure that it is not marked as read-only and that you have sufficient priviledges to write to it before trying again");
                    }
                }
                return options;
            }
            return null;
        }

        public override void ExportImpl(Window parentWindow, Data.DataMatrix matrix, object optionsObj) {

            CSVExporterOptions options = optionsObj as CSVExporterOptions;

            if (options == null) {
                return;
            }

            ProgressStart(String.Format("Exporting to {0}", options.Filename));
                
            using (StreamWriter writer = new StreamWriter(options.Filename)) {
                int numCols = matrix.Columns.Count;
                if (options.ColumnHeadersAsFirstRow) {
                    // write out the column headers as the first row...

                    for (int colIndex = 0; colIndex < numCols; ++colIndex) {
                        MatrixColumn col = matrix.Columns[colIndex];
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
                    writer.WriteLine();
                }

                // Now emit each row...
                int numRows = matrix.Rows.Count;
                int currentRow = 0;
                foreach (MatrixRow row in matrix.Rows) {
                    for (int colIndex = 0; colIndex < numCols; ++colIndex) {
                        object value = row[colIndex];
                        if (options.QuoteValues) {
                            writer.Write(_quote);
                        }
                        writer.Write(value);
                        if (options.QuoteValues) {
                            writer.Write(_quote);
                        }
                        if (colIndex < numCols - 1) {
                            writer.Write(options.Delimiter);
                        }
                    }
                    writer.WriteLine();
                    currentRow++;
                    if ((currentRow % 1000) == 0) {
                        double percent = (((double) currentRow) / ((double) numRows)) * 100.0;
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

    }

}
