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

        public InsertSiteAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSite(base.FindRegionID(Model), base.FindIDOfParentType(Model,SiteExplorerNodeType.SiteGroup, 0));
            base.UpdateChildrenParentID();
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

}
