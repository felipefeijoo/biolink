using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class UpdateReferenceAction : GenericDatabaseAction<Reference> {

        public UpdateReferenceAction(Reference model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateReference(Model);
        }
    }

    public class InsertReferenceAction : GenericDatabaseAction<Reference> {

        public InsertReferenceAction(Reference model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.RefID = service.InsertReference(Model);
        }
    }

    public class DeleteReferenceAction : DatabaseAction {

        public DeleteReferenceAction(int refID) {
            this.RefID = refID;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteReference(RefID);
        }

        public int RefID { get; private set; }
    }
}
