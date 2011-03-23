using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {

    public class MaterialForTaxonReport : TaxonReportBase {

        public MaterialForTaxonReport(User user, TaxonViewModel taxon)
            : base(user, taxon) {
            RegisterViewer(new TabularDataViewerSource());
            DefineColumn("BiotaFullName", "Taxa");
            DefineColumn("FullRegion", "Name");
            DefineColumn("Local", "Locality");
            DefineColumn("FormattedLatLong", "Lat/Long");
            DefineColumn("Collectors");
            DefineColumn("Dates");
            DefineColumn("AccessionNo");
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            if (progress != null) {
                progress.ProgressStart(string.Format("Retrieving Material records for {0}", Taxon.DisplayLabel), true);
            }

            var serviceMessage = new BioLinkService.ServiceMessageDelegate((message) => {
                progress.ProgressMessage(message, 0);
            });

            Service.ServiceMessage += serviceMessage;
            DataMatrix matrix = Service.GetMaterialForTaxon(Taxon.TaxaID.Value);
            Service.ServiceMessage -= serviceMessage;

            if (progress != null) {
                progress.ProgressEnd(string.Format("{0} rows retreived", matrix.Rows.Count));
            }

            matrix.Columns.Add(new FormattedLatLongVirtualColumn(matrix));
            matrix.Columns.Add(new FormattedDateVirtualColumn(matrix));

            return matrix;
        }

        public override string Name {
            get { return string.Format("Material for taxon '{0}'", Taxon.DisplayLabel); }
        }

    }

}

