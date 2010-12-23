using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameSiteAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public RenameSiteAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameSite(Model.ElemID, Model.Name);
        }
    }

    public class UpdateSiteAction : GenericDatabaseAction<SiteViewModel> {

        public UpdateSiteAction(SiteViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateSite(Model.Model);
        }

    }

    public class InsertSiteAction : AbstractSiteExplorerAction {

        public InsertSiteAction(SiteExplorerNodeViewModel model, int templateId = 0)
            : base(model) {
            this.TemplateID = templateId;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSite(base.FindRegionID(Model), base.FindIDOfParentType(Model,SiteExplorerNodeType.SiteGroup, TemplateID));
            base.UpdateChildrenParentID();
        }

        private int TemplateID { get; private set; }
    }

    public class InsertSiteTemplateAction : GenericDatabaseAction<SiteExplorerNode> {
        public InsertSiteTemplateAction(SiteExplorerNode model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSiteTemplate();
        }
    }

    public class DeleteSiteAction : DatabaseAction {
        public DeleteSiteAction(int siteID) {
            this.SiteID = siteID;            
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteSite(SiteID);
        }


        public int SiteID { get; private set; }
        
    }

    public class MergeSiteAction : GenericDatabaseAction<SiteExplorerNode> {

        public MergeSiteAction(SiteExplorerNode model, SiteExplorerNode dest)
            : base(model) {
            this.Destination = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);

            service.MergeSite(Model.ElemID, Destination.ElemID);
        }

        public SiteExplorerNode Destination { get; set; }

    }


    public class MoveSiteAction : GenericDatabaseAction<SiteExplorerNode> {

        public MoveSiteAction(SiteExplorerNode model, SiteExplorerNode dest)
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

       
    }

}
