using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class MultimediaReport : ReportBase {

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

        private DataMatrix AddMediaForTaxon(int taxonId, IProgressObserver progress) {
            var results = new DataMatrix();

            results.Columns.Add(new MatrixColumn { Name = "MultimediaID" });
            results.Columns.Add(new MatrixColumn { Name = "TaxonID" });
            results.Columns.Add(new MatrixColumn { Name = "Name" });
            results.Columns.Add(new MatrixColumn { Name = "MultimediaLink", IsHidden = true });

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

            // Now find all the children of this item
            if (progress != null) {
                progress.ProgressMessage("Retrieving child items...");
            }

            var children = taxaService.GetExpandFullTree(taxonId);

            if (progress != null) {
                progress.ProgressStart("Extracting multimedia for children...");
            }

            int count = 0;
            foreach (Taxon child in children) {
                links = service.GetMultimediaItems(TraitCategoryType.Taxon.ToString(), child.TaxaID.Value);
                foreach (MultimediaLink link in links) {
                    AddTaxonRow(results, child, link);
                    count++;                    
                    if (progress != null) {
                        double percent = (((double)count) / ((double)children.Count)) * 100.0;
                        progress.ProgressMessage(string.Format("Processing {0}", child.TaxaFullName), percent);
                    }

                }
            }

            if (progress != null) {
                progress.ProgressEnd(string.Format("{0} multimedia items found.", count));
            }


            return results;
        }

        private void AddTaxonRow(DataMatrix results, Taxon taxa, MultimediaLink link) {
            var row = results.AddRow();
            row[0] = link.MultimediaID;
            row[1] = taxa.TaxaID.Value;
            row[2] = taxa.TaxaFullName;
            row[3] = link;
        }

        protected int ObjectID { get; private set; }

        protected TraitCategoryType ObjectType { get; private set; }
    }
}
