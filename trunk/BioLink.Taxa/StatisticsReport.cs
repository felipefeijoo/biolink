using System.Collections.Generic;
using System.Data;
using System.Windows;
using BioLink.Client.Extensibility;
using BioLink.Data;

namespace BioLink.Client.Taxa {

    public abstract class TaxonReportBase : IBioLinkReport {

        private List<IReportViewerSource> _viewers = new List<IReportViewerSource>();

        public TaxonReportBase(TaxaService service, TaxonViewModel taxon) {
            this.Service = service;
            this.Taxon = taxon;
        }

        protected void RegisterViewer(IReportViewerSource viewer) {
            _viewers.Add(viewer);
        }

        public List<IReportViewerSource> Viewers {
            get { return _viewers; }
        }

        #region Properties

        public TaxaService Service { get; private set; }

        public TaxonViewModel Taxon { get; private set; }

        #endregion

        public abstract DataTable ExtractReportData();

        public abstract string Name { get ; }

    }

    public class TaxonStatisticsReport : TaxonReportBase {

        public TaxonStatisticsReport(TaxaService service, TaxonViewModel taxon) : base(service, taxon) {
            RegisterViewer(new TabularDataViewerSource());
        }

        public override DataTable ExtractReportData() {
            DataTable table = Service.GetStatistics(Taxon.TaxaID.Value);
            return table;
        }

        public override string Name {
            get { return "Statistics..."; }
        }
        
    }

    public class MaterialForTaxonReport : TaxonReportBase {

        public MaterialForTaxonReport(TaxaService service, TaxonViewModel taxon)
            : base(service, taxon) {
            RegisterViewer(new TabularDataViewerSource());
        }

        public override DataTable ExtractReportData() {
            DataTable table = Service.GetMaterialForTaxon(Taxon.TaxaID.Value);
            return table;
        }

        public override string Name {
            get { return "Material for taxon list..."; }
        }

    }

}
