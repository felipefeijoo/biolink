using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class UpdateReferenceAction : GenericDatabaseCommand<Reference> {

        public UpdateReferenceAction(Reference model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateReference(Model);
        }
    }

    public class InsertReferenceAction : GenericDatabaseCommand<Reference> {

        public InsertReferenceAction(Reference model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.RefID = service.InsertReference(Model);
        }
    }

    public class DeleteReferenceAction : DatabaseCommand {

        public DeleteReferenceAction(int refID) {
            this.RefID = refID;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteReference(RefID);
        }

        public override bool Equals(object obj) {
            if (obj is DeleteReferenceAction) {
                var other = obj as DeleteReferenceAction;
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
    }
}
