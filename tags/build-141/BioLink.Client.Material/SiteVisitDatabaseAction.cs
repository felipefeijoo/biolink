using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameSiteVisitAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public RenameSiteVisitAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameSiteVisit(Model.ElemID, Model.Name);
        }

    }

    public class DeleteSiteVisitAction : DatabaseAction {

        public DeleteSiteVisitAction(int siteVisitID) {
            this.SiteVisitID = siteVisitID;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteSiteVisit(SiteVisitID);
        }

        public int SiteVisitID { get; private set; }
    }

    public class InsertSiteVisitAction : AbstractSiteExplorerAction {

        public InsertSiteVisitAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSiteVisit(Model.ParentID);
            UpdateChildrenParentID();
        }
    }

    public class UpdateSiteVisitAction : GenericDatabaseAction<SiteVisit> {
        public UpdateSiteVisitAction(SiteVisit model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            if (Model.DateStart.GetValueOrDefault(-1) < 0 && Model.DateEnd.GetValueOrDefault(-1) < 0) {
                Model.DateType = 2;
            } else {
                Model.DateType = 1;
            }
            service.UpdateSiteVisit(Model);
        }

    }

    public class MergeSiteVisitAction : GenericDatabaseAction<SiteExplorerNode> {

        public MergeSiteVisitAction(SiteExplorerNode source, SiteExplorerNode dest)
            : base(source) {
            Dest = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MergeSiteVisit(Model.ElemID, Dest.ElemID);
        }

        public SiteExplorerNode Dest { get; private set; }
    }

    public class MoveSiteVisitAction : GenericDatabaseAction<SiteExplorerNode> {

        public MoveSiteVisitAction(SiteExplorerNode source, SiteExplorerNode dest)
            : base(source) {
            Dest = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);            
            service.MoveSiteVisit(Model.ElemID, Dest.ElemID);
        }

        public SiteExplorerNode Dest { get; private set; }
    }


}
