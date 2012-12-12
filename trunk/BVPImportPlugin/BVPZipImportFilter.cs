using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.BVPImport {

    public class BVPZipImportFilter : TabularDataImporter {

        public override bool GetOptions(System.Windows.Window parentWindow, ImportWizardContext context) {
            return true;
        }

        public override ImportRowSource CreateRowSource() {
            throw new NotImplementedException();
        }

        public override string Name {
            get { return "ALA BVP Zip file"; }
        }

        public override string Description {
            get { return "Imports data exported from the Atlas of Living Australia's Biodiversity Volunteer Portal"; }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/ala_export.png", GetType().Assembly.GetName().Name);
            }
        }

        public override List<string> GetColumnNames() {
            var columns = new List<String>();

            return columns;
        }

        protected override void WriteEntryPoint(EntryPoint ep) {
            // throw new NotImplementedException();
        }

        protected override void ReadEntryPoint(EntryPoint ep) {
            // throw new NotImplementedException();
        }
    }
}
