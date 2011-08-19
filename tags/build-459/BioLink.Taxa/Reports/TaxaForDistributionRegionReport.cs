using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class TaxaForDistributionRegionReport : ReportBase {

        public TaxaForDistributionRegionReport(User user, DistributionRegion distRegion, int taxonId = -1) : base(user) {
            this.TaxonID = taxonId;
            this.DistributionRegion = distRegion;
            RegisterViewer(new RTFReportViewerSource());
            var service = new SupportService(user);
        }

        public override string Name {
            get { return string.Format("Taxa for Distribution Region '{0}'", DistributionRegion.DistRegionName); }
        }

        public override Data.DataMatrix ExtractReportData(IProgressObserver progress) {
            var service = new TaxaService(User);
            return service.TaxaForDistributionRegionReport(DistributionRegion.DistRegionID, TaxonID);
        }

        protected DistributionRegion DistributionRegion { get; private set; } 

        protected int TaxonID { get; private set; }
    }
}
