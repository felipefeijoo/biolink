using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class RenameRegionCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public RenameRegionCommand(SiteExplorerNode model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameRegion(Model.ElemID, Model.Name);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_REGION, PERMISSION_MASK.UPDATE);
        }

    }

    public class InsertRegionCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public InsertRegionCommand(SiteExplorerNode model, SiteExplorerNodeViewModel viewModel) : base(model) {
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

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_REGION, PERMISSION_MASK.INSERT);
        }

        protected SiteExplorerNodeViewModel ViewModel { get; private set; }

    }

    public class DeleteRegionCommand : DatabaseCommand {

        public DeleteRegionCommand(int regionID) {
            this.RegionID = regionID;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteRegion(RegionID);
        }

        public int RegionID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_REGION, PERMISSION_MASK.DELETE);
        }

    }

    public class MoveRegionCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public MoveRegionCommand(SiteExplorerNode source, SiteExplorerNode dest)
            : base(source) {
            this.Destination = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MoveRegion(Model.RegionID, Destination.RegionID);
        }

        public SiteExplorerNode Destination { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_EXPLORER, PERMISSION_MASK.ALLOW);
        }

    }

}
