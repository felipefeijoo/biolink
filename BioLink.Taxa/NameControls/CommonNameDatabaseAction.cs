using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class UpdateCommonNameCommand : GenericDatabaseCommand<CommonName> {

        public UpdateCommonNameCommand(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.UpdateCommonName(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
            required.AddBiota(Model.BiotaID, PERMISSION_MASK.UPDATE);
        }


    }

    public class InsertCommonNameCommand : GenericDatabaseCommand<CommonName> {

        public InsertCommonNameCommand(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertCommonName(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
            required.AddBiota(Model.BiotaID, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteCommonNameCommand : GenericDatabaseCommand<CommonName> {

        public DeleteCommonNameCommand(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.DeleteCommonName(Model.CommonNameID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
            required.AddBiota(Model.BiotaID, PERMISSION_MASK.UPDATE);
        }
    }




}
