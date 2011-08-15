using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {

    public class UpdateSanDatabaseCommand : GenericDatabaseCommand<SpeciesAvailableName> {

        public UpdateSanDatabaseCommand(SpeciesAvailableName name) : base(name) { }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertOrUpdateSpeciesAvailableName(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
            required.AddBiota(Model.BiotaID, PERMISSION_MASK.UPDATE);
        }

    }

    public class UpdateSANTypeDataCommand : GenericDatabaseCommand<SANTypeData> {

        public UpdateSANTypeDataCommand(SANTypeData model) : base(model) { }

        protected override void ProcessImpl(User user) {
            Debug.Assert(Model.SANTypeDataID >= 0);
            var service = new TaxaService(user);
            service.UpdateSANTypeData(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
            required.AddBiota(Model.BiotaID, PERMISSION_MASK.UPDATE);
        }

    }

    public class InsertSANTypeDataCommand : GenericDatabaseCommand<SANTypeData> {

        public InsertSANTypeDataCommand(SANTypeData model) : base(model) { }

        protected override void ProcessImpl(User user) {
            Debug.Assert(Model.SANTypeDataID < 0);
            var service = new TaxaService(user);
            service.InsertSANTypeData(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
            required.AddBiota(Model.BiotaID, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteSANTypeDataCommand : GenericDatabaseCommand<SANTypeData> {

        public DeleteSANTypeDataCommand(SANTypeData model) : base(model) { }

        protected override void ProcessImpl(User user) {
            Debug.Assert(Model.SANTypeDataID >= 0);
            var service = new TaxaService(user);
            service.DeleteSANTypeData(Model.SANTypeDataID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
            required.AddBiota(Model.BiotaID, PERMISSION_MASK.UPDATE);
        }

    }

}
