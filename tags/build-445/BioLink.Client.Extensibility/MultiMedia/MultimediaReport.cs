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
        private bool _includeMaterial;

        private TaxaService _taxaService;
        private SupportService _supportService;
        private XMLIOService _xmlService;
        private MaterialService _materialService;
        private Dictionary<string, bool> _idCache = new Dictionary<string, bool>();

        public MultimediaReport(User user, ViewModelBase target, TraitCategoryType lookupType) : base(user) {
            Target = target;
            ObjectType = lookupType;
            RegisterViewer(new TabularDataViewerSource());
            RegisterViewer(new MultimediaThumbnailViewerSource());
        }

        public override string Name {
            get { return string.Format("Multimedia Report for '{0}'", Target.DisplayLabel); }
        }

        public override Data.DataMatrix ExtractReportData(IProgressObserver progress) {

            DataMatrix results = null;
            
            switch (ObjectType) {
                case TraitCategoryType.Taxon:
                    results = AddMediaForTaxon(Target.ObjectID.Value, progress);
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
                _includeMaterial = options.IncludeMaterial;
                return true;
            }
            return false;
        }

        private void AddTaxonRow(DataMatrix results, Taxon taxa, MultimediaLink link, string multimediaSource = "Taxon", int? materialId= null) {

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
                row[9] = multimediaSource;
                row[10] = materialId;
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
            results.Columns.Add(new MatrixColumn { Name = "Attached To" });
            results.Columns.Add(new MatrixColumn { Name = "MaterialID", IsHidden= true });

            if (progress != null) {
                progress.ProgressMessage("Extracting multimedia details for item...");
            }
                        
            // First add the multimedia for this item
            var links = SupportService.GetMultimediaItems(TraitCategoryType.Taxon.ToString(), taxonId);
            var taxon = TaxaService.GetTaxon(taxonId);
            
            foreach (MultimediaLink link in links) {
                AddTaxonRow(results, taxon, link);            
            }

            if (_includeMaterial) {
                AddMaterialRowsForTaxon(results, taxon);
            }

            if (_recurse) {
                // Now find all the children of this item
                if (progress != null) {
                    progress.ProgressMessage("Retrieving child items...");
                }

                var children = TaxaService.GetExpandFullTree(taxonId);

                var elementCount = 0;
                int total = children.Count;

                if (progress != null) {
                    progress.ProgressStart("Extracting multimedia for children...");
                }
                
                foreach (Taxon child in children) {

                    if (progress != null) {
                        double percent = (((double)elementCount) / ((double)total)) * 100.0;
                        progress.ProgressMessage(string.Format("Processing {0}", child.TaxaFullName), percent);
                    }
                    elementCount++;

                    links = SupportService.GetMultimediaItems(TraitCategoryType.Taxon.ToString(), child.TaxaID.Value);
                    foreach (MultimediaLink link in links) {
                        AddTaxonRow(results, child, link);
                    }

                    if (_includeMaterial) {
                        AddMaterialRowsForTaxon(results, child);
                    }

                }
            }

            if (progress != null) {
                progress.ProgressEnd(string.Format("{0} multimedia items found.", results.Rows.Count ));
            }


            return results;
        }

        private void AddMaterialRowsForTaxon(DataMatrix results, Taxon taxon) {
            var ids = XMLIOService.GetMaterialForTaxon(taxon.TaxaID.Value);
            foreach (XMLIOMaterialID id in ids) {
                var links = SupportService.GetMultimediaItems(TraitCategoryType.Material.ToString(), id.MaterialID);
                foreach (MultimediaLink link in links) {
                    AddTaxonRow(results, taxon, link, "Material", id.MaterialID);
                }
            }
        }

        protected TaxaService TaxaService {
            get {
                if (_taxaService == null) {
                    _taxaService = new TaxaService(User);
                }
                return _taxaService;
            }
        }

        protected SupportService SupportService {
            get {
                if (_supportService == null) {
                    _supportService = new SupportService(User);
                }
                return _supportService;
            }
        }

        protected XMLIOService XMLIOService {
            get {
                if (_xmlService == null) {
                    _xmlService = new XMLIOService(User);
                }
                return _xmlService;
            }
        }

        protected MaterialService MaterialService {
            get {
                if (_materialService == null) {
                    _materialService = new MaterialService(User);
                }
                return _materialService;
            }
        }

        protected ViewModelBase Target { get; private set; }        

        protected TraitCategoryType ObjectType { get; private set; }
    }
}
