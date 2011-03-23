using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Taxa {
    public class TypeListReport : TaxonReportBase {

        public TypeListReport(User user, TaxonViewModel taxon) : base(user, taxon) {
            RegisterViewer(new TabularDataViewerSource());
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            return Service.GetTaxonTypes(Taxon.TaxaID.Value);
        }

        public override string Name {
            get { return string.Format("Taxon Type list: {0}", Taxon.DisplayLabel); }
        }

    }
}
