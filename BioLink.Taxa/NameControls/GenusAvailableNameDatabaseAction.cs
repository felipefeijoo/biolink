using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class UpdateGenusAvailableNameCommand : GenericDatabaseCommand<GenusAvailableName> {

        public UpdateGenusAvailableNameCommand(GenusAvailableName model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertOrUpdateGenusAvailableName(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteGANIncludedSpeciesCommand : GenericDatabaseCommand<GANIncludedSpecies> {

        public DeleteGANIncludedSpeciesCommand(GANIncludedSpecies model) 
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.DeleteGANIncludedSpecies(Model.GANISID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

    }

    public class InsertGANIncludedSpeciesCommand : GenericDatabaseCommand<GANIncludedSpecies> {

        public InsertGANIncludedSpeciesCommand(GANIncludedSpecies model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertGANIncludedSpecies(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

    }

    public class UpdateGANIncludedSpeciesCommand : GenericDatabaseCommand<GANIncludedSpecies> {

        public UpdateGANIncludedSpeciesCommand(GANIncludedSpecies model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.UpdateGANIncludedSpecies(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

    }
}
