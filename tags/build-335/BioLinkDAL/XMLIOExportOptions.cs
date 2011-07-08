using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data {

    public class XMLIOExportOptions {

        public string Filename { get; set; }

        public bool ExportChildTaxa { get; set; }

        public bool ExportMaterial { get; set; }

        public bool ExportMultimedia { get; set; }

        public bool ExportTraits { get; set; }

        public bool ExportNotes { get; set; }

        public bool IncludeFullClassification { get; set; }

        public bool KeepLogFile { get; set; }

    }
}
