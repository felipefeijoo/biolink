using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using System.IO;
using BioLink.Data;


namespace BioLink.Client.Extensibility {

    public class ExcelExporter : TabularDataExporter {

        protected override object GetOptions(Window parentWindow) {

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Export"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.OverwritePrompt = false;
            dlg.Filter = "Excel Workbook (.xlsx)|*.xlsx|All files (*.*)|*.*"; // Filter files by extension
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

            using (StreamWriter writer = new StreamWriter(options.Filename)) {
                writer.WriteLine("<?xml version=\"1.0\"?><ss:Workbook xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
                writer.WriteLine("<ss:Worksheet ss:Name=\"Exported Data\">");
                writer.WriteLine("<ss:Table>");

                foreach (MatrixRow row in matrix.Rows) {
                    writer.WriteLine("<ss:Row>");
                    for (int i = 0; i < matrix.Columns.Count; ++i) {
                        object val = row[i];
                        writer.Write("<ss:Cell><ss:Data ss:Type=\"String\">");
                        writer.Write(val);
                        writer.Write("</ss:Data></ss:Cell>");
                    }
                    writer.WriteLine("</ss:Row>");
                }

                writer.WriteLine("</ss:Table>");
                writer.WriteLine("</ss:Worksheet>");
                writer.WriteLine("</Workbook>");
            }
        }

        public override void Dispose() {
        }

        #region Properties

        public override string Description {
            get { return "Export data as a Microsoft Excel Worksheet"; }
        }

        public override string Name {
            get { return "Excel worksheet"; }
        }

        public override BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/excel_exporter.png", GetType().Assembly.GetName().Name);
            }
        }

        #endregion

    }

    public class ExcelExporterOptions {

        public string Filename { get; set; }

    }
}
