using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class RenameRegionAction : GenericDatabaseAction<SiteExplorerNode> {

        public RenameRegionAction(SiteExplorerNode model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameRegion(Model.ElemID, Model.Name);
        }
    }

    public class InsertRegionAction : GenericDatabaseAction<SiteExplorerNode> {

        public InsertRegionAction(SiteExplorerNode model, SiteExplorerNodeViewModel viewModel) : base(model) {
            this.ViewModel = viewModel;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertRegion(Model.Name, Model.ParentID);
            Model.RegionID = Model.ElemID;
            foreach (SiteExplorerNodeViewModel child in ViewModel.Children) {
                child.ParentID = Model.ElemID;
            }
        }

        protected SiteExplorerNodeViewModel ViewModel { get; private set; }

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

    public class MoveRegionAction : GenericDatabaseAction<SiteExplorerNode> {

        public MoveRegionAction(SiteExplorerNode source, SiteExplorerNode dest)
            : base(source) {
            this.Destination = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MoveRegion(Model.RegionID, Destination.RegionID);
        }

        public SiteExplorerNode Destination { get; private set; }
    }

}
