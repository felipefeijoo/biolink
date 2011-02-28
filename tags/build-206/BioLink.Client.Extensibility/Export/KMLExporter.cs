using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.IO;

namespace BioLink.Client.Extensibility.Export {

    public class KMLExporter : TabularDataExporter {

        public override bool CanExport(Data.DataMatrix matrix) {
            return (matrix.ContainsColumn("Lat") && matrix.ContainsColumn("Long"));
        }

        protected override object GetOptions(System.Windows.Window parentWindow, DataMatrix matrix) {

            var frm = new MatrixColumnChooser(matrix, "Select the label column for each point:");
            frm.Owner = parentWindow;
            if (frm.ShowDialog().ValueOrFalse()) {

                var filename = PromptForFilename(".kml", "KML Files (*.kml)|*.kml");
                if (!string.IsNullOrEmpty(filename)) {
                    KMLExporterOptions options = new KMLExporterOptions();                    
                    options.Filename = filename;
                    options.LabelColumn = frm.SelectedColumn;
                    return options;
                }
            }

            return null;
        }

        public override void ExportImpl(System.Windows.Window parentWindow, Data.DataMatrix matrix, object options) {
            var opt = options as KMLExporterOptions;
            if (opt != null) {
                var filename = opt.Filename;

                ProgressStart("Exporting points to '" + filename + "'");

                int latIndex = matrix.IndexOf("Lat");
                int lonIndex = matrix.IndexOf("Long");
                int nameIndex = matrix.IndexOf(opt.LabelColumn);

                int rowCount = 0;
                using (var writer = new StreamWriter(filename)) {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    writer.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
                    writer.WriteLine("<Document>");

                    
                    foreach (MatrixRow row in matrix.Rows) {
                        var lat = row[latIndex];
                        var lon = row[lonIndex];
                        if (lat != null && lon != null) {
                            writer.WriteLine("<Placemark>");                            
                            string name = "" + rowCount;
                            if (nameIndex >= 0) {
                                var nameObj = row[nameIndex];
                                if (nameObj != null) {
                                    name = row[nameIndex].ToString();
                                } else {
                                    name = "";
                                }
                            } 
                            if (!string.IsNullOrEmpty(name)) {
                                writer.WriteLine(string.Format("<name>{0}</name>", name));
                            }

                            writer.WriteLine(string.Format("<description>{0}</description>", BuildDescription(row)));
                            writer.WriteLine(string.Format("<Point><coordinates>{0},{1}</coordinates></Point>", lon, lat));
                            writer.WriteLine("</Placemark>");
                        }

                        if (++rowCount % 1000 == 0) {
                            double percent = ((double)rowCount / (double)matrix.Rows.Count) * 100;
                            this.ProgressMessage("Exporting points to '" + filename + "'", percent);
                        }
                    }
                    
                    writer.WriteLine("</Document>");
                    writer.WriteLine("</kml>");
                }

                ProgressEnd(string.Format("{0} rows exported.", rowCount));
            }
        }

        private string BuildDescription(MatrixRow row) {
            StringBuilder b = new StringBuilder("<![CDATA[\n<table>");
            foreach (MatrixColumn col in row.Matrix.Columns) {
                b.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", col.Name, row[row.Matrix.IndexOf(col.Name)]);
            }

            b.Append("</table>\n]]>");

            return b.ToString();
        }

        public override void Dispose() {            
        }

        public override string Name {
            get { return "Keyhole Markup Language"; }
        }

        public override string Description {
            get { return "Export point data to Google Earth"; }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get { 
                return ImageCache.GetPackedImage("images/kml_exporter.png", GetType().Assembly.GetName().Name); 
            }
        }
    }

    internal class KMLExporterOptions {

        public string Filename { get; set; }

        public string LabelColumn { get; set; }

    }
}
