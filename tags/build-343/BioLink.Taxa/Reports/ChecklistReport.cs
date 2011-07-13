using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {
    public class ChecklistReport : TaxonReportBase {

        public ChecklistReport(User user, TaxonViewModel taxon) : base(user, taxon) {
            RegisterViewer(new RTFReportViewerSource());
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {

            var frm = new ChecklistReportOptions(user, Taxon);
            frm.Owner = parentWindow;
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog() == true) {
                this.Extent = frm.optFullHierarchy.IsChecked.ValueOrFalse() ? ChecklistReportExtent.FullHierarchy : ChecklistReportExtent.NextLevelOnly;
                this.IncludeAvailableNames = frm.chkIncludeAvailable.IsChecked.ValueOrFalse();
                this.IncludeLiteratureNames = frm.chkIncludeLiterature.IsChecked.ValueOrFalse();
                if (frm.chkIncludeRankDescriptions.IsChecked.ValueOrFalse()) {
                    Depth = frm.optToFamily.IsChecked.ValueOrFalse() ? ChecklistReportRankDepth.Family : ChecklistReportRankDepth.Subgenus;
                } else {
                    Depth = null;
                }
                this.UserDefinedOrder = frm.chkUserDefinedOrder.IsChecked.ValueOrFalse();
                this.VerifiedOnly = frm.chkOnlyVerified.IsChecked.ValueOrFalse();
                this.SelectedRanks = frm.SelectedRanks;
                return true;
            }

            return false;
        }

        public override string Name {
            get { return string.Format("Checklist report: {0}", Taxon.DisplayLabel); }
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            string caption = String.Format("Taxon: {0}", Taxon.TaxaFullName);
            return Service.ChecklistReport(Taxon.TaxaID.Value, caption, Extent, IncludeAvailableNames, IncludeLiteratureNames, Depth, UserDefinedOrder, VerifiedOnly, SelectedRanks);
        }

        protected ChecklistReportExtent Extent { get; set; }
        protected bool IncludeAvailableNames { get; set; }
        protected bool IncludeLiteratureNames { get; set; }
        protected ChecklistReportRankDepth? Depth { get; set; }
        protected bool UserDefinedOrder { get; set; }
        protected bool VerifiedOnly { get; set; }
        protected List<TaxonRankName> SelectedRanks { get; set; }

    }
}
