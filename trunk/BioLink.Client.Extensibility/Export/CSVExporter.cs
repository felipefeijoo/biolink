using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility.Export {

    public class CSVExporter : TabularDataExporter {

        private string _quote = "\"";

        protected override object GetOptions(Window parentWindow, DataMatrix matrix) {
            CSVExporterOptionsWindow optionsWindow = new CSVExporterOptionsWindow();
            optionsWindow.Owner = parentWindow;
            if (optionsWindow.ShowDialog().GetValueOrDefault(false)) {
                CSVExporterOptions options = optionsWindow.Options;
                if (FileExistsAndNotOverwrite(options.Filename)) {
                    return null;
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


        public override bool CanExport(DataMatrix matrix) {
            return true;
        }
    }

}
