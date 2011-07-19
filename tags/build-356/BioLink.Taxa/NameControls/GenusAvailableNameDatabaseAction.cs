using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class UpdateGenusAvailableNameAction : GenericDatabaseCommand<GenusAvailableName> {

        public UpdateGenusAvailableNameAction(GenusAvailableName model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertOrUpdateGenusAvailableName(Model);
        }
    }

    public class DeleteGANIncludedSpeciesAction : GenericDatabaseCommand<GANIncludedSpecies> {

        public DeleteGANIncludedSpeciesAction(GANIncludedSpecies model) 
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.DeleteGANIncludedSpecies(Model.GANISID);
        }
    }

    public class InsertGANIncludedSpeciesAction : GenericDatabaseCommand<GANIncludedSpecies> {

        public InsertGANIncludedSpeciesAction(GANIncludedSpecies model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertGANIncludedSpecies(Model);
        }

    }

    public class UpdateGANIncludedSpeciesAction : GenericDatabaseCommand<GANIncludedSpecies> {

        public UpdateGANIncludedSpeciesAction(GANIncludedSpecies model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.UpdateGANIncludedSpecies(Model);
        }
    }
}
