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

            progress.ProgressStart("Extracting darwin core records...");


            DataMatrix result = null;
            var idSet = new LinkedList<int>(_idSet);
            int chunkSize = 1000;
            var helper = new DarwinCoreReportHelper();
            var chunk = new List<int>();
            var count = 0;
            while (idSet.Count > 0) {
                chunk.Add(idSet.First.Value);
                idSet.RemoveFirst();
                count++;
                if (chunk.Count >= chunkSize || idSet.Count == 0) {
                    progress.ProgressMessage(String.Format("Extracting darwin core records {0}", count));
                    var where = "tblMaterial.intMaterialID in (" + chunk.Join(",") + ")";
                    var dataChunk = helper.RunDwcQuery(service, where);
                    if (result == null) {
                        result = dataChunk;
                    } else {
                        dataChunk.Rows.ForEach(row => {
                            var newrow = result.AddRow();
                            row.ForEach((index, value) => {
                                newrow[index] = value;
                                return true;
                            });
                        });
                    }
                    chunk = new List<int>();
                }
            }

            return result;
        }

    }
}
