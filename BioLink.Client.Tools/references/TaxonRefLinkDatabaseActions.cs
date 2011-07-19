using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Tools {

    public class UpdateTaxonRefLinkAction : GenericDatabaseCommand<TaxonRefLink> {

        public UpdateTaxonRefLinkAction(TaxonRefLink model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateTaxonRefLink(Model);
        }

    }

    public class InsertTaxonRefLinkAction : GenericDatabaseCommand<TaxonRefLink> {

        public InsertTaxonRefLinkAction(TaxonRefLink model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);

            var reflink = new RefLink();
            reflink.RefLinkID = Model.RefLinkID;
            reflink.IntraCatID = Model.BiotaID;
            reflink.RefID = Model.RefID;
            reflink.RefPage = Model.RefPage;
            reflink.RefQual = Model.RefQual;
            reflink.RefLinkType = Model.RefLink;
            reflink.UseInReport = Model.UseInReports;

            Model.RefLinkID = service.InsertRefLink(reflink, TraitCategoryType.Taxon.ToString());
        }

    }

    public class DeleteTaxonRefLinkAction : GenericDatabaseCommand<TaxonRefLink> {
        public DeleteTaxonRefLinkAction(TaxonRefLink model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            if (Model.RefLinkID >= 0) {
                var service = new SupportService(user);
                service.DeleteRefLink(Model.RefLinkID);
            }
        }
    }
}
