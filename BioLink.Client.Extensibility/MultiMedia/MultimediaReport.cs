using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class MultimediaReport : ReportBase {

        private bool _recurse;
        private string _extensionFilter;
        private string _typeFilter;

        public MultimediaReport(User user, int objectId, TraitCategoryType lookupType) : base(user) {
            ObjectID = objectId;
            ObjectType = lookupType;
            RegisterViewer(new TabularDataViewerSource());
            RegisterViewer(new MultimediaThumbnailViewerSource());
        }

        public override string Name {
            get { return "Multimedia Report"; }
        }

        public override Data.DataMatrix ExtractReportData(IProgressObserver progress) {

            DataMatrix results = null;
            
            switch (ObjectType) {
                case TraitCategoryType.Taxon:
                    results = AddMediaForTaxon(ObjectID, progress);
                    break;
            }

            return results;
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            var options = new MultimediaReportOptions();
            options.Owner = parentWindow;
            if (options.ShowDialog().ValueOrFalse()) {
                _recurse = options.Recurse;
                _extensionFilter = options.ExtensionFilter;
                _typeFilter = options.TypeFilter;
                return true;
            }
            return false;
        }

        private void AddTaxonRow(DataMatrix results, Taxon taxa, MultimediaLink link) {

            // Filter the rows...
            bool addRow = true;
            if (!string.IsNullOrWhiteSpace(_extensionFilter)) {
                addRow = _extensionFilter.Equals(link.Extension, StringComparison.CurrentCultureIgnoreCase);
            }

            if (addRow && !string.IsNullOrWhiteSpace(_typeFilter)) {
                addRow = _typeFilter.Equals(link.MultimediaType, StringComparison.CurrentCultureIgnoreCase);
            }

            if (addRow) {
                var row = results.AddRow();
                row[0] = link.MultimediaID;
                row[1] = taxa.TaxaID.Value;
                row[2] = link;
                row[3] = taxa.TaxaFullName;
                row[4] = taxa.Rank;
                row[5] = link.Name;
                row[6] = link.Extension;
                row[7] = link.MultimediaType;
                row[8] = link.SizeInBytes;
            }
        }

        private DataMatrix AddMediaForTaxon(int taxonId, IProgressObserver progress) {
            var results = new DataMatrix();

            results.Columns.Add(new MatrixColumn { Name = "MultimediaID", IsHidden = true });
            results.Columns.Add(new MatrixColumn { Name = "TaxonID", IsHidden = true });
            results.Columns.Add(new MatrixColumn { Name = "MultimediaLink", IsHidden = true });
            results.Columns.Add(new MatrixColumn { Name = "Taxon name" });            
            results.Columns.Add(new MatrixColumn { Name = "Rank" });
            results.Columns.Add(new MatrixColumn { Name = "Multimedia Name" });
            results.Columns.Add(new MatrixColumn { Name = "Extension" });
            results.Columns.Add(new MatrixColumn { Name = "Multimedia Type" });
            results.Columns.Add(new MatrixColumn { Name = "Size" });

            if (progress != null) {
                progress.ProgressMessage("Extracting multimedia details for item...");
            }
            
            var service = new SupportService(User);
            // First add the multimedia for this item
            var links = service.GetMultimediaItems(TraitCategoryType.Taxon.ToString(), taxonId);
            var taxaService = new TaxaService(User);
            var taxon = taxaService.GetTaxon(taxonId);
            
            foreach (MultimediaLink link in links) {
                AddTaxonRow(results, taxon, link);            
            }

            if (_recurse) {
                // Now find all the children of this item
                if (progress != null) {
                    progress.ProgressMessage("Retrieving child items...");
                }

                var children = taxaService.GetExpandFullTree(taxonId);

                if (progress != null) {
                    progress.ProgressStart("Extracting multimedia for children...");
                }

                var childCount = 0;
                foreach (Taxon child in children) {

                    if (progress != null) {
                        double percent = (((double)childCount) / ((double)children.Count)) * 100.0;
                        progress.ProgressMessage(string.Format("Processing {0}", child.TaxaFullName), percent);
                    }
                    childCount++;

                    links = service.GetMultimediaItems(TraitCategoryType.Taxon.ToString(), child.TaxaID.Value);
                    foreach (MultimediaLink link in links) {
                        AddTaxonRow(results, child, link);
                    }

                }

            }

            if (progress != null) {
                progress.ProgressEnd(string.Format("{0} multimedia items found.", results.Rows.Count ));
            }


            return results;
        }

        protected int ObjectID { get; private set; }

        protected TraitCategoryType ObjectType { get; private set; }
    }
}
