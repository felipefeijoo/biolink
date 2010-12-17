using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameTrapAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public RenameTrapAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameTrap(Model.ElemID, Model.Name);
        }

    }

    public class DeleteTrapAction : DatabaseAction {

        public DeleteTrapAction(int trapId) {
            this.TrapID = trapId;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteTrap(TrapID);
        }

        public int TrapID { get; private set; }
    }

    public class InsertTrapAction : AbstractSiteExplorerAction {

        public InsertTrapAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertTrap(Model.ParentID, Model.Name);
            UpdateChildrenParentID();
        }
    }

    public class UpdateTrapAction : GenericDatabaseAction<Trap> {

        public UpdateTrapAction(Trap trap)
            : base(trap) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateTrap(Model);
        }
    }
    

}
