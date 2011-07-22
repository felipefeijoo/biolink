using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class InsertCurationEventCommand : GenericDatabaseCommand<CurationEvent> {

        public InsertCurationEventCommand(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.CurationEventID = service.InsertCurationEvent(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class UpdateCurationEventCommand : GenericDatabaseCommand<CurationEvent> {

        public UpdateCurationEventCommand(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateCurationEvent(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteCurationEventCommand : GenericDatabaseCommand<CurationEvent> {
        public DeleteCurationEventCommand(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteCurationEvent(Model.CurationEventID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

}
