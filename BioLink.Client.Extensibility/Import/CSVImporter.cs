using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.Windows;

namespace BioLink.Client.Extensibility.Import {

    public class CSVImporter : TabularDataImporter {

        public override Object GetOptions(System.Windows.Window parentWindow) {
            MessageBox.Show("CSV Options go here... Derp!");
            return new CSVImporterOptions();
        }

        protected override ImportRowSource CreateRowSource(Object options) {
            throw new NotImplementedException();
        }

        public override string Name {
            get { return "Delimited text file"; }
        }

        public override string Description {
            get {
                return "Imports data from a flat text file delimited by specific characters (comma, tab etc.)"; 
            }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get { 
                return ImageCache.GetPackedImage("images/csv_exporter.png", GetType().Assembly.GetName().Name); 
            }
        }

    }

    public class CSVImporterOptions {
    }

}
