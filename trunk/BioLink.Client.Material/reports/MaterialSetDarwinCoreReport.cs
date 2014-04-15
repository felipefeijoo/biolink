using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    class MaterialSetDarwinCoreReport : ReportBase {

        private List<int> _idSet;

        public MaterialSetDarwinCoreReport(User user, List<int> idSet) : base(user) {
            _idSet = idSet;
        }

        public override string Name {
            get { return "Darwin Core report for material set"; }
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            var service = new TaxaService(User);

            progress.ProgressStart(String.Format("Preparing Darwin Core records for {0} specimens...", _idSet.Count));

            DataMatrix result = null;
            var idSet = new LinkedList<int>(_idSet);
            int chunkSize = 2000;
            var helper = new DarwinCoreReportHelper();
            var chunk = new List<int>();
            var count = 0;
            while (idSet.Count > 0) {
                chunk.Add(idSet.First.Value);
                idSet.RemoveFirst();
                count++;
                if (chunk.Count >= chunkSize || idSet.Count == 0) {

                    var percentComplete = ((double) count / (double) _idSet.Count) * 100;

                    progress.ProgressMessage(String.Format("Preparing Darwin Core records {0} of {1}", count, _idSet.Count), percentComplete);
                    var where = "tblMaterial.intMaterialID in (" + chunk.Join(",") + ")";
                    var dataChunk = helper.RunDwcQuery(service, where);
                    if (result == null) {
                        result = dataChunk;
                    } else {
                        result.AppendMatrix(dataChunk);
                    }
                    chunk = new List<int>();
                }
            }
            progress.ProgressEnd(String.Format("{0} Darwin Core records retrieved."));

            return result;
        }

    }
}
