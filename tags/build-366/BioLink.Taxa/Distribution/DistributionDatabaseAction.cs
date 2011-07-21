using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {

    public class UpdateDistQualDatabaseCommand : GenericDatabaseCommand<Taxon> {

        public UpdateDistQualDatabaseCommand(Taxon model) : base(model) { }

        protected override void ProcessImpl(User user) {
            new TaxaService(user).UpdateDistributionQualification(Model.TaxaID, Model.DistQual);            
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

    }

    public class SaveDistributionRegionsCommand : GenericDatabaseCommand<Taxon> {

        private ObservableCollection<HierarchicalViewModelBase> _regionTree;

        public SaveDistributionRegionsCommand(Taxon taxon, ObservableCollection<HierarchicalViewModelBase> regionTree) : base(taxon) {
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

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

    }
}
