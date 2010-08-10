using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {

    public class TaxonStatisticsReport : TaxonReportBase {

        public TaxonStatisticsReport(TaxaService service, TaxonViewModel taxon) : base(service, taxon) {
            RegisterViewer(new TabularDataViewerSource());
            DefineColumn("Category");
            DefineColumn("Count");
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            DataMatrix table = Service.GetStatistics(Taxon.TaxaID.Value);
            return table;
        }

        public override string Name {
            get { return string.Format("Statistics for '{0}'", Taxon.DisplayLabel); }
        }

    }

}

