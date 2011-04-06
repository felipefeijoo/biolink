using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class SiteForTaxaReport : TaxonReportBase {

        public SiteForTaxaReport(User user, TaxonViewModel taxon) : base(user, taxon) {
            RegisterViewer(new RTFReportViewerSource());
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            var taxa = new List<TaxonViewModel>();
            taxa.Add(Taxon);
            var frm = new RegionBasedReportOptions(user, taxa, "Site for Taxa options");
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            frm.Owner = parentWindow;
            if (frm.ShowDialog().ValueOrFalse()) {
                this.RegionID = frm.txtRegion.ObjectID.GetValueOrDefault(-1);
                this.RegionName = frm.txtRegion.Text;
                return true;
            }

            return false;            
        }

        public override string Name {
            get { return string.Format("Sites for Taxon: {0}", Taxon.DisplayLabel); }
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            String caption = String.Format("Taxon: {0}, {1}", Taxon.TaxaFullName, RegionID < 0 ? "All regions." : RegionName);
            return Service.TaxaForSites(RegionID, Taxon.TaxaID.Value, "Region", caption, true);
        }

        protected int RegionID { get; set; }
        protected String RegionName { get; set; }
    }
}
