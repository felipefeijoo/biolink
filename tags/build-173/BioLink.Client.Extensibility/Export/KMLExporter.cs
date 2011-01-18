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
            var filename = PromptForFilename(".kml", "KML Files (*.kml)|*.kml");
            if (!string.IsNullOrEmpty(filename)) {
                KMLExporterOptions options = new KMLExporterOptions();
                options.Filename = filename;
                return options;
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
                int nameIndex = matrix.IndexOf("AccessionNo");
                if (nameIndex < 0) {
                    nameIndex = matrix.IndexOf("Taxa");
                }

                using (var writer = new StreamWriter(filename)) {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    writer.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
                    writer.WriteLine("<Document>");

                    foreach (MatrixRow row in matrix.Rows) {
                        var lat = row[latIndex];
                        var lon = row[lonIndex];
                        if (lat != null && lon != null) {
                            writer.WriteLine("<Placemark>");
                            string name = "";
                            if (nameIndex >= 0) {
                                name = row[nameIndex] as string;
                            }
                            if (!string.IsNullOrEmpty(name)) {
                                writer.WriteLine(string.Format("<name>{0}</name>", name));
                            }

                            writer.WriteLine(string.Format("<description>{0}</description>", BuildDescription(row)));
                            writer.WriteLine(string.Format("<Point><coordinates>{0},{1}</coordinates></Point>", lon, lat));
                            writer.WriteLine("</Placemark>");
                        }
                    }
                    
                    writer.WriteLine("</Document>");
                    writer.WriteLine("</kml>");
                }
            }
        }

        private string BuildDescription(MatrixRow row) {
            StringBuilder b = new StringBuilder("<![CDATA[\n<ul>");
            foreach (MatrixColumn col in row.Matrix.Columns) {
                b.AppendFormat("<li>{0} : {1}</li>", col.Name, row[row.Matrix.IndexOf(col.Name)]);
            }

            b.Append("</ul>\n]]>");

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

    }
}
