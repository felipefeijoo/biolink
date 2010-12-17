using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Tools {

    public class UpdateTaxonRefLinkAction : GenericDatabaseAction<TaxonRefLink> {

        public UpdateTaxonRefLinkAction(TaxonRefLink model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateTaxonRefLink(Model);
        }

    }
}
