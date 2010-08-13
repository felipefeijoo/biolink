using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public class CSVExporter : ITabularDataExporter<CSVExporterOptions> {

        public CSVExporterOptions GetOptions(System.Windows.Window parentWindow) {
            return new CSVExporterOptions();
        }

        public void Export(Data.DataMatrix matrix, CSVExporterOptions options, Utilities.IProgressObserver progress) {            
        }

        public void Dispose() {
        }

        #region Properties

        public string Description {
            get { return "Export data as a delimited text file"; }
        }

        public string Name {
            get { return "Delimited text file exporter"; }
        }

        #endregion


    }

    public class CSVExporterOptions {

        public string Delimiter { get; set; }
        public bool ColumnHeadersAsFirstRow { get; set; }
        public string Filename { get; set; }

    }
}
