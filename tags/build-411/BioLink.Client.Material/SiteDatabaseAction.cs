using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameSiteCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public RenameSiteCommand(SiteExplorerNode model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameSite(Model.ElemID, Model.Name);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_SITE, PERMISSION_MASK.UPDATE);
        }

    }

    public class UpdateSiteCommand : GenericDatabaseCommand<Site> {

        public UpdateSiteCommand(Site model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateSite(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_SITE, PERMISSION_MASK.UPDATE);
        }


    }

    public class InsertSiteCommand : AbstractSiteExplorerCommand {

        public InsertSiteCommand(SiteExplorerNode model, SiteExplorerNodeViewModel viewModel, int templateId = 0) : base(model, viewModel) {
            this.TemplateID = templateId;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSite(base.FindRegionID(ViewModel), base.FindIDOfParentType(ViewModel, SiteExplorerNodeType.SiteGroup), TemplateID);
            base.UpdateChildrenParentID();
        }

        public int TemplateID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_SITE, PERMISSION_MASK.INSERT);
        }

    }

    public class InsertSiteTemplateCommand : GenericDatabaseCommand<SiteExplorerNode> {
        public InsertSiteTemplateCommand(SiteExplorerNode model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSiteTemplate();
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_SITE, PERMISSION_MASK.INSERT);
        }

    }

    public class DeleteSiteCommand : DatabaseCommand {
        public DeleteSiteCommand(int siteID) {
            this.SiteID = siteID;            
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteSite(SiteID);
        }

        public int SiteID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_SITE, PERMISSION_MASK.DELETE);
        }
        
    }

    public class MergeSiteCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public MergeSiteCommand(SiteExplorerNode model, SiteExplorerNode dest)
            : base(model) {
            this.Destination = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);

            service.MergeSite(Model.ElemID, Destination.ElemID);
        }

        public SiteExplorerNode Destination { get; set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_EXPLORER, PERMISSION_MASK.ALLOW);
        }

    }


    public class MoveSiteCommand : GenericDatabaseCommand<SiteExplorerNode> {

        public MoveSiteCommand(SiteExplorerNode model, SiteExplorerNode dest)
            : base(model) {
            this.Destination = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);

            int siteGroupID = 0;

            int regionID = Destination.ElemID;
            if (Destination.ElemType == "SiteGroup") {
                siteGroupID = Destination.ElemID;
                regionID = Destination.RegionID;
            }

            service.MoveSite(Model.ElemID, Destination.RegionID, siteGroupID);
        }

        public SiteExplorerNode Destination { get; set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_EXPLORER, PERMISSION_MASK.ALLOW);
        }
       
    }

    public class InsertRDESiteCommand : GenericDatabaseCommand<RDESite> {

        public InsertRDESiteCommand(RDESite model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.SiteID = service.InsertSite(Model.PoliticalRegionID.GetValueOrDefault(-1), -1);
            // Now do an update to insert all the other goodies!
            var update = new UpdateRDESiteCommand(Model);
            update.Process(user);            
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_SITE, PERMISSION_MASK.INSERT);
        }

    }

    public class UpdateRDESiteCommand : GenericDatabaseCommand<RDESite> {

        public UpdateRDESiteCommand(RDESite model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);

            if (string.IsNullOrWhiteSpace(Model.SiteName)) {
                if (string.IsNullOrEmpty(Model.SiteName)) {
                    Model.SiteName = Model.Locality;
                } else {
                    Model.SiteName = Model.SiteName;
                }
            }

            service.UpdateSiteRDE(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_SITE, PERMISSION_MASK.UPDATE);
        }

    }

}
