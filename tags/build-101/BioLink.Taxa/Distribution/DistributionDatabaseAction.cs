using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {

    public class UpdateDistQualDatabaseAction : GenericDatabaseAction<TaxonViewModel> {

        public UpdateDistQualDatabaseAction(TaxonViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            new TaxaService(user).UpdateDistributionQualification(Model.TaxaID, Model.DistQual);            
        }
    }

    public class SaveDistributionRegionsAction : GenericDatabaseAction<TaxonViewModel> {

        private ObservableCollection<HierarchicalViewModelBase> _regionTree;

        public SaveDistributionRegionsAction(TaxonViewModel taxon, ObservableCollection<HierarchicalViewModelBase> regionTree)
            : base(taxon) {
            _regionTree = regionTree;
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            // First we need to delete all of the existing regions...
            service.DeleteAllBiotaDistribution(Model.TaxaID);
            // Then insert the ones from the model...
            var list = new List<DistributionViewModel>();
            foreach (HierarchicalViewModelBase b in _regionTree) {
                b.Traverse((m) => {
                    if (m is DistributionViewModel) {
                        list.Add(m as DistributionViewModel);
                    }
                });
            }

            foreach (DistributionViewModel dvm in list) {
                service.InsertBiotaDist(Model.TaxaID, dvm.Model);
            }

        }
    }
}
