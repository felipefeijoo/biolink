using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;


namespace BioLink.Client.Taxa {

    public class TaxaAssociatesReport : ReportBase {

        public TaxaAssociatesReport(User user, List<TaxonViewModel> taxa) : base(user) {
            this.Taxa = taxa;
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            int[] ids = new int[Taxa.Count];
            int i = 0;
            foreach (TaxonViewModel vm in Taxa) {
                ids[i++] = vm.TaxaID.Value;
            }

            return Service.GetAssociatesForTaxa(RegionID, ids);
        }

        public override string Name {
            get { return string.Format("Associates for Taxa: {0}", Taxa.Count == 1 ? Taxa[0].DisplayLabel : "Multiple taxa"); }
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            var frm = new RegionBasedReportOptions(user, Taxa, "Associates for Taxa options");
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            frm.Owner = parentWindow;
            if (frm.ShowDialog().ValueOrFalse()) {
                this.RegionID = frm.txtRegion.ObjectID.GetValueOrDefault(-1);
                return true;
            }

            return false;            
        }

        protected int RegionID { get; private set; }

        protected List<TaxonViewModel> Taxa { get; private set; }

        protected TaxaService Service { get { return new TaxaService(User); } }
    }
}
