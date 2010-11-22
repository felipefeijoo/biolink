using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class RenameRegionAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public RenameRegionAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameRegion(Model.ElemID, Model.Name);
        }
    }

    public class InsertRegionAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public InsertRegionAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertRegion(Model.Name, Model.ParentID);
        }

    }

    public class DeleteRegionAction : DatabaseAction {

        public DeleteRegionAction(int regionID) {
            this.RegionID = regionID;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteRegion(RegionID);
        }

        public int RegionID { get; private set; }
    }

}
