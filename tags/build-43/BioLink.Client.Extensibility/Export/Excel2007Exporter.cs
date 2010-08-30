using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using BioLink.Data;
using OfficeOpenXml;

namespace BioLink.Client.Extensibility.Export {

    /// <summary>
    /// This is marked as internal because its really, really slow! If we want to support this we'll
    /// need to grok the xlsx format and write it directly.
    /// </summary>
    internal class Excel2007Exporter : TabularDataExporter {

        protected override object GetOptions(Window parentWindow) {

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Export"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.OverwritePrompt = false;
            dlg.Filter = "Excel 2007 Workbook (.xslx)|*.xslx|All files (*.*)|*.*"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {
                ExcelExporterOptions options = new ExcelExporterOptions();
                options.Filename = dlg.FileName;
                return options;
            }

            return null;
        }

        public override void ExportImpl(Window parentWindow, Data.DataMatrix matrix, object optionsObj) {
            ExcelExporterOptions options = optionsObj as ExcelExporterOptions;

            if (options == null) {
                return;
            }

            ProgressStart("Exporting...");

            int totalRows = matrix.Rows.Count;

            if (FileExistsAndNotOverwrite(options.Filename)) {
                return;
            }

            FileInfo file = new FileInfo(options.Filename);
            using (ExcelPackage xlPackage = new ExcelPackage(file)) {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets.Add("Exported data");
                int currentRow = 1;                
                int columnCount = matrix.Columns.Count;
                foreach (MatrixRow row in matrix.Rows) {
                    for (int col = 0; col < columnCount; ++col) {
                        object val = row[col];
                        worksheet.Cell(currentRow, col + 1).Value = (val == null ? "" : Escape(val.ToString()));
                    }
                    if (++currentRow % 10 == 0) {
                        double percent = ((double)currentRow / (double)totalRows) * 100.0;
                        ProgressMessage(String.Format("Exported {0} of {1} rows...", currentRow, totalRows), percent);
                    }
                }
                ProgressMessage("Saving file...");
            }
            ProgressEnd(String.Format("{0} rows exported.", totalRows));
        }

        public override void Dispose() {
        }

        #region Properties

        public override string Description {
            get { return "Export data as a Microsoft Excel 2007 Worksheet"; }
        }

        public override string Name {
            get { return "Excel 2007"; }
        }

        public override BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/excel2007_exporter.png", GetType().Assembly.GetName().Name);
            }
        }

        #endregion

    }

}

