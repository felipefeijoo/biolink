using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {

    class DarwinCoreReportForTaxon : TaxonReportBase {

        public DarwinCoreReportForTaxon(User user, TaxonViewModel taxon)
            : base(user, taxon) {
            RegisterViewer(new TabularDataViewerSource());
        }

        public override string Name {
            get { return String.Format("Darwin Core for '{0}'", Taxon.DisplayLabel); }
        }

        public override Data.DataMatrix ExtractReportData(Utilities.IProgressObserver progress) {
            var service = new MaterialService(PluginManager.Instance.User);
            return service.GetDarwinCoreForBiotaID(Taxon.TaxaID.Value);
        }
    }
}
