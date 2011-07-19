using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameSiteVisitAction : GenericDatabaseCommand<SiteExplorerNode> {

        public RenameSiteVisitAction(SiteExplorerNode model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameSiteVisit(Model.ElemID, Model.Name);
        }

    }

    public class DeleteSiteVisitAction : DatabaseCommand {

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

        public InsertSiteVisitAction(SiteExplorerNode model, SiteExplorerNodeViewModel viewModel, int templateId = -1) : base(model, viewModel) {
            TemplateID = templateId;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSiteVisit(Model.ParentID, TemplateID);
            UpdateChildrenParentID();
        }

        public int TemplateID { get; private set; }
    }

    public class UpdateSiteVisitAction : GenericDatabaseCommand<SiteVisit> {
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

    public class MergeSiteVisitAction : GenericDatabaseCommand<SiteExplorerNode> {

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

    public class MoveSiteVisitAction : GenericDatabaseCommand<SiteExplorerNode> {

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

    public class InsertSiteVisitTemplateAction : GenericDatabaseCommand<SiteExplorerNode> {
        public InsertSiteVisitTemplateAction(SiteExplorerNode model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSiteVisitTemplate();
        }
    }

    public class InsertRDESiteVisitAction : GenericDatabaseCommand<RDESiteVisit> {

        public InsertRDESiteVisitAction(RDESiteVisit model, RDESite owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.SiteID = Owner.SiteID;
            Model.SiteVisitID = service.InsertSiteVisit(Model.SiteID);
        }

        protected RDESite Owner { get; private set; }

    }

    public class UpdateRDESiteVisitAction : GenericDatabaseCommand<RDESiteVisit> {

        public UpdateRDESiteVisitAction(RDESiteVisit model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateSiteVisit(MapToSiteVisit(Model));
        }

        private static SiteVisit MapToSiteVisit(RDESiteVisit model) {
            var visit = new SiteVisit();

            if (string.IsNullOrEmpty(model.VisitName)) {
                int? date = model.DateStart;
                if (!date.HasValue || date.Value == 0) {
                    date = model.DateEnd;
                }

                if (date.HasValue && date.Value != 0) {
                    model.VisitName = string.Format("{0} {1}", model.Collector, DateControl.DateToStr(date.ToString()));
                } else {
                    model.VisitName = model.Collector;
                }
            }

            visit.SiteVisitName = model.VisitName;
            visit.SiteVisitID = model.SiteVisitID;
            visit.SiteID = model.SiteID;

            visit.Collector = model.Collector;
            visit.DateStart = model.DateStart;
            visit.DateEnd = model.DateEnd;

            return visit;
        }
    }

}
