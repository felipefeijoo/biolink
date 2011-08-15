using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class UpdateReferenceCommand : GenericDatabaseCommand<Reference> {

        public UpdateReferenceCommand(Reference model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateReference(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_REFS, PERMISSION_MASK.UPDATE);
        }
    }

    public class InsertReferenceCommand : GenericDatabaseCommand<Reference> {

        public InsertReferenceCommand(Reference model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.RefID = service.InsertReference(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_REFS, PERMISSION_MASK.INSERT);
        }

    }

    public class DeleteReferenceCommand : DatabaseCommand {

        public DeleteReferenceCommand(int refID) {
            this.RefID = refID;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteReference(RefID);
        }

        public override bool Equals(object obj) {
            if (obj is DeleteReferenceCommand) {
                var other = obj as DeleteReferenceCommand;
                if (other.RefID == this.RefID) {
                    return true;
                }
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public int RefID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_REFS, PERMISSION_MASK.DELETE);
        }

    }
}
