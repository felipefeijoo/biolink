using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class LabelSetReport : ReportBase {

        public LabelSetReport(User user, LabelSet labelSet, List<LabelSetItem> items, IEnumerable<QueryCriteria> criteria) : base(user) {
            this.Criteria = criteria;
            this.Items = items;
            this.LabelSet = labelSet;
            RegisterViewer(new TabularDataViewerSource());
        }

        public override string Name {
            get { return string.Format("Label Set {0}", LabelSet.Name); }
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            var types = new string[] { "Site", "SiteVisit", "Material" };

            progress.ProgressMessage("Extracting site data...");
            var siteData = ExecuteLabelSetQuery(Items, Criteria, "Site");

            progress.ProgressMessage("Extracting site visit data...");
            var visitData = ExecuteLabelSetQuery(Items, Criteria, "SiteVisit");

            progress.ProgressMessage("Extracting material data...");
            var materialData = ExecuteLabelSetQuery(Items, Criteria, "Material");

            progress.ProgressMessage("Merging results...");
            
            return MergeItemMatrices(siteData, visitData, materialData);
        }

        private DataMatrix ExecuteLabelSetQuery(List<LabelSetItem> items, IEnumerable<QueryCriteria> criteria, string elemType) {
            var idList = new List<Int32>();
            foreach (LabelSetItem item in items) {
                int id = 0;
                switch (elemType) {
                    case "Site":
                    case "PoliticalRegion":
                        id = item.SiteID;
                        break;
                    case "SiteVisit":
                        id = item.VisitID;
                        break;
                    case "Material":
                        id = item.MaterialID;
                        break;
                }

                if (id != 0) {
                    idList.Add(id);
                }
            }

            if (idList.Count == 0) {
                return null;
            }

            List<QueryCriteria> finalCriteria = new List<QueryCriteria>();
            // Now work out the fields
            foreach (QueryCriteria c in criteria) {
                bool include = false;
                switch (elemType) {
                    case "Site":
                    case "PoliticalRegion":
                        include = c.Field.Category.Equals("Site", StringComparison.CurrentCultureIgnoreCase) || c.Field.Category.Equals("PoliticalRegion", StringComparison.CurrentCultureIgnoreCase);
                        break;
                    case "SiteVisit":
                        include = c.Field.Category.Equals("SiteVisit");
                        break;
                    case "Material":
                        include = c.Field.Category.Equals("Material") || c.Field.Category.Equals("Nomenclature");
                        break;
                }

                if (include) {
                    finalCriteria.Add(c);
                }
            }

            // now ensure the id criteria is set...
            FieldDescriptor fd = null;
            switch (elemType) {
                case "Site":
                    fd = SupportService.FieldDescriptors.FirstOrDefault((candidate) => {
                        return candidate.Category.Equals("Site", StringComparison.CurrentCultureIgnoreCase) && candidate.DisplayName.Equals("site identifier", StringComparison.CurrentCultureIgnoreCase);
                    });
                    break;
                case "SiteVisit":
                    fd = SupportService.FieldDescriptors.FirstOrDefault((candidate) => {
                        return candidate.Category.Equals("SiteVisit", StringComparison.CurrentCultureIgnoreCase) && candidate.DisplayName.Equals("Visit identifier", StringComparison.CurrentCultureIgnoreCase);
                    });
                    break;
                case "Material":
                    fd = SupportService.FieldDescriptors.FirstOrDefault((candidate) => {
                        return candidate.Category.Equals("Material", StringComparison.CurrentCultureIgnoreCase) && candidate.DisplayName.Equals("Material identifier", StringComparison.CurrentCultureIgnoreCase);
                    });
                    break;
            }

            if (fd != null) {
                var qc = new QueryCriteria { Field = fd, Output = true, Criteria = string.Format("in ({0})", idList.Join(",")) };
                finalCriteria.Add(qc);
                var service = new SupportService(User);
                return service.ExecuteQuery(finalCriteria, false);
            }

            return null;
        }

        private DataMatrix MergeItemMatrices(DataMatrix siteData, DataMatrix siteVisitData, DataMatrix materialData) {
            var result = new DataMatrix();

            foreach (QueryCriteria c in Criteria) {
                result.Columns.Add(new MatrixColumn { Name = c.Field.DisplayName });
            }


            int currentOrderNum = 1;            
            LabelSetItem item = null;
            while ((item = Items.FirstOrDefault((candidate) => { return candidate.PrintOrder == currentOrderNum; })) != null) {

                if (item.NumCopies > 0) {

                    var row = result.AddRow();

                    if (item.SiteID > 0) {
                        AddFieldData(row, item.SiteID, "Site Identifier", siteData);
                    }

                    if (item.VisitID > 0) {
                        AddFieldData(row, item.VisitID, "Visit Identifier", siteVisitData);
                    }

                    if (item.MaterialID > 0) {
                        AddFieldData(row, item.MaterialID, "Material Identifier", materialData);
                    }

                    if (item.NumCopies > 1) {
                        CopyRow(result, row, item.NumCopies - 1);
                    }
                }

                currentOrderNum++;
            }

            return result;
        }

        private void CopyRow(DataMatrix dest, MatrixRow srcRow, int numCopies) {
            for (int i = 0; i < numCopies; ++i) {
                var newRow = dest.AddRow();
                foreach (MatrixColumn col in dest.Columns) {
                    int index = dest.IndexOf(col.Name);
                    newRow[index] = srcRow[index];
                }
            }
        }

        private void AddFieldData(MatrixRow targetRow, int objectId, string objectFieldName, DataMatrix srcData) {
            var srcRow = srcData.FindRow(objectFieldName, objectId);
            if (srcRow != null) {                
                for (int srcIndex = 0; srcIndex < srcData.Columns.Count; srcIndex++) {
                    var col = srcData.Columns[srcIndex];
                    int targetIndex = targetRow.Matrix.IndexOf(col.Name);
                    if (targetIndex >= 0) {
                        targetRow[targetIndex] = srcRow[srcIndex];
                    }
                }
            }
        }

        public IEnumerable<QueryCriteria> Criteria { get; private set; }
        public List<LabelSetItem> Items { get; private set; }
        public LabelSet LabelSet { get; private set; }
        public bool Distinct { get; private set; }
    }
}
