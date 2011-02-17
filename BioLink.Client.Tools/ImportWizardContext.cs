using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Tools {

    public class ImportWizardContext {

        public TabularDataImporter Importer { get; set; }

        public ImporterOptions ImporterOptions { get; set; }

        public List<ImportFieldMapping> FieldMappings { get; set; }

    }
}
