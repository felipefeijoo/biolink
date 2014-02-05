using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Taxa {
    class ExportDistributionRegionsReport : ReportBase {

        public ExportDistributionRegionsReport(User user ) : base(user) {
            RegisterViewer(new TabularDataViewerSource());
            Service = new SupportService(user);
        }

        public override string Name {
            get { return "Distribution Regions"; }
        }

        public override Data.DataMatrix ExtractReportData(Utilities.IProgressObserver progress) {
            var matrix = new DataMatrix();

            matrix.Columns.Add(new MatrixColumn { Name = "RegionPath" });

            var rootList = Service.GetDistributionRegions(0);
            rootList.ForEach((region) => {
                ExportChildRegions(region, matrix);
            });
            return matrix;
        }

        private void ExportChildRegions(DistributionRegion parent, DataMatrix matrix) {            
            // First add a row for this parent...
            var row = matrix.AddRow();
            row[0] = Service.GetDistributionFullPath(parent.DistRegionID);
            // Than add any direct children recursively
            if (parent.NumChildren > 0) {
                var list = Service.GetDistributionRegions(parent.DistRegionID);
                if (list != null && list.Count > 0) {
                    list.ForEach((region) => {
                        ExportChildRegions(region, matrix);
                    });
                }
            }
        }

        public SupportService Service { get; private set; }

    }
}
