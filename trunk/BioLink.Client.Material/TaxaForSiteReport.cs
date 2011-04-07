using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class TaxaForSiteReport : ReportBase {

        SiteExplorerNode _node;

        public TaxaForSiteReport(User user, SiteExplorerNode node) : base(user) {
            RegisterViewer(new RTFReportViewerSource());
            _node = node;
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            var frm = new TaxaForSiteReportOptions(user, _node);
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            frm.Owner = parentWindow;
            if (frm.ShowDialog().ValueOrFalse()) {
                this.TaxonID = frm.TaxonID;
                this.TaxonName = frm.TaxonName;
                this.SiteRegionID = frm.SiteRegionID;
                this.SiteRegionName = frm.SiteRegionName;
                this.ElemType = frm.SiteRegionElemType;
                this.includeLocations = !frm.chkHideLocalities.IsChecked.ValueOrFalse();
                return true;
            }

            return false;
        }


        public override string Name {
            get { return "Taxa for Site"; }
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            var service = new MaterialService(User);
            var filterText = string.Format("Taxon: {0},  {1}: {2}", TaxonID.HasValue && TaxonID.Value > 0 ? TaxonName : "All taxon", ElemType, SiteRegionName);
            return service.GetTaxaForSites(true, ElemType, SiteRegionID.Value, TaxonID.GetValueOrDefault(-1), filterText);
        }

        protected string TaxonName { get; private set; }
        protected int? TaxonID { get; private set; }
        protected int? SiteRegionID { get; private set; }
        protected String SiteRegionName { get; private set; }
        protected String ElemType { get; private set; }
        protected bool includeLocations { get; private set; }
        
    }
}
