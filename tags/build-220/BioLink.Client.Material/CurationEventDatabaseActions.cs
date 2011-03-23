using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class InsertCurationEventAction : GenericDatabaseAction<CurationEvent> {

        public InsertCurationEventAction(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.CurationEventID = service.InsertCurationEvent(Model);
        }
    }

    public class UpdateCurationEventAction : GenericDatabaseAction<CurationEvent> {

        public UpdateCurationEventAction(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateCurationEvent(Model);
        }
    }

    public class DeleteCurationEventAction : GenericDatabaseAction<CurationEvent> {
        public DeleteCurationEventAction(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteCurationEvent(Model.CurationEventID);
        }
    }

}
