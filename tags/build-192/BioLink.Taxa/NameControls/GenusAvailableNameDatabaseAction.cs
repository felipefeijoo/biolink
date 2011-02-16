using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class UpdateGenusAvailableNameAction : GenericDatabaseAction<GenusAvailableName> {

        public UpdateGenusAvailableNameAction(GenusAvailableName model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertOrUpdateGenusAvailableName(Model);
        }
    }

    public class DeleteGANIncludedSpeciesAction : GenericDatabaseAction<GANIncludedSpecies> {

        public DeleteGANIncludedSpeciesAction(GANIncludedSpecies model) 
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.DeleteGANIncludedSpecies(Model.GANISID);
        }
    }

    public class InsertGANIncludedSpeciesAction : GenericDatabaseAction<GANIncludedSpecies> {

        public InsertGANIncludedSpeciesAction(GANIncludedSpecies model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertGANIncludedSpecies(Model);
        }

    }

    public class UpdateGANIncludedSpeciesAction : GenericDatabaseAction<GANIncludedSpecies> {

        public UpdateGANIncludedSpeciesAction(GANIncludedSpecies model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.UpdateGANIncludedSpecies(Model);
        }
    }
}
