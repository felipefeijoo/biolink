using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class TaxonSelectorContentProvider : IHierarchicalSelectorContentProvider {

        public TaxonSelectorContentProvider(User user) {
            this.User = user;
        }

        public string Caption {
            get { return "Select a taxon"; }
        }

        public bool CanAddNewItem {
            get { return false; }
        }

        public bool CanDeleteItem {
            get { return false; }
        }

        public bool CanRenameItem {
            get { return false; }
        }

        public List<HierarchicalViewModelBase> LoadModel(HierarchicalViewModelBase parent) {                        
            var service = new TaxaService(User);
            List<Taxon> model = null;
            if (parent == null) {
                model = service.GetTopLevelTaxa();
            } else {
                model = service.GetTaxaForParent((parent as TaxonViewModel).TaxaID.Value);
            }

            if (model != null) {
                var list = new List<HierarchicalViewModelBase>(model.ConvertAll((m) => {
                    return new TaxonViewModel(parent, m, null);
                }));
                return list;
            }

            return null;            
        }

        public List<HierarchicalViewModelBase> Search(string searchTerm) {
            var service = new TaxaService(User);
            var list = service.FindTaxa(searchTerm);
            return new List<HierarchicalViewModelBase>(list.ConvertAll((m) => {
                return new TaxonViewModel(null, m, null);
            }));
        }

        public bool CanSelectItem(HierarchicalViewModelBase candidate) {
            var taxon = candidate as TaxonViewModel;
            return taxon != null;
        }

        public SelectionResult CreateSelectionResult(HierarchicalViewModelBase selectedItem) {
            var taxon = selectedItem as TaxonViewModel;
            if (taxon != null) {
                var result = new SelectionResult();
                result.DataObject = taxon;
                result.ObjectID = taxon.TaxaID;
                result.Description = taxon.TaxaFullName;
                return result;
            }
            return null;
        }

        public Data.DatabaseAction AddNewItem(HierarchicalViewModelBase selectedItem) {
            throw new NotImplementedException();
        }

        public Data.DatabaseAction RenameItem(HierarchicalViewModelBase selectedItem, string newName) {
            throw new NotImplementedException();
        }

        public Data.DatabaseAction DeleteItem(HierarchicalViewModelBase selectedItem) {
            throw new NotImplementedException();
        }

        public int? GetElementIDForViewModel(HierarchicalViewModelBase item) {
            var taxon = item as TaxonViewModel;
            if (taxon != null) {
                return taxon.TaxaID;
            }
            return -1;
        }

        #region Properties

        public User User { get; private set; }

        #endregion
    }
}
