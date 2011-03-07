using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class ImportWizardContext {

        public TabularDataImporter Importer { get; set; }

        public IEnumerable<ImportFieldMapping> FieldMappings { get; set; }

    }
}
