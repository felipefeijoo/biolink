using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    public class XMLIOService : BioLinkService {

        public XMLIOService(User user) : base(user) { }

        public void ExportXML(List<int> taxonIds, XMLIOExportOptions options, IProgressObserver progress) {

            try {
                if (progress != null) {
                    progress.ProgressStart("Counting total taxa to export...");
                }


            } finally {
                if (progress != null) {
                    progress.ProgressEnd("Export complete.");
                }
            }



        }

    }
}
