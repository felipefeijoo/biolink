using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameSiteAction : GenericDatabaseCommand<SiteExplorerNode> {

        public RenameSiteAction(SiteExplorerNode model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameSite(Model.ElemID, Model.Name);
        }
    }

    public class UpdateSiteAction : GenericDatabaseCommand<Site> {

        public UpdateSiteAction(Site model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateSite(Model);
        }

    }

    public class InsertSiteAction : AbstractSiteExplorerAction {

        public InsertSiteAction(SiteExplorerNode model, SiteExplorerNodeViewModel viewModel, int templateId = 0) : base(model, viewModel) {
            this.TemplateID = templateId;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSite(base.FindRegionID(ViewModel), base.FindIDOfParentType(ViewModel, SiteExplorerNodeType.SiteGroup), TemplateID);
            base.UpdateChildrenParentID();
        }

        public int TemplateID { get; private set; }
    }

    public class InsertSiteTemplateAction : GenericDatabaseCommand<SiteExplorerNode> {
        public InsertSiteTemplateAction(SiteExplorerNode model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertSiteTemplate();
        }
    }

    public class DeleteSiteAction : DatabaseCommand {
        public DeleteSiteAction(int siteID) {
            this.SiteID = siteID;            
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteSite(SiteID);
        }


        public int SiteID { get; private set; }
        
    }

    public class MergeSiteAction : GenericDatabaseCommand<SiteExplorerNode> {

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


    public class MoveSiteAction : GenericDatabaseCommand<SiteExplorerNode> {

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

    public class InsertRDESiteAction : GenericDatabaseCommand<RDESite> {

        public InsertRDESiteAction(RDESite model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.SiteID = service.InsertSite(Model.PoliticalRegionID.GetValueOrDefault(-1), -1);
        }

    }

    public class UpdateRDESiteAction : GenericDatabaseCommand<RDESite> {

        public UpdateRDESiteAction(RDESite model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateSite(MapToSite(Model));
        }

        private static Site MapToSite(RDESite model) {
            var site = new Site();

            site.SiteID = model.SiteID;

            site.ElevError = model.ElevError;
            site.ElevLower = model.ElevLower;
            site.ElevSource = model.ElevSource;
            site.ElevType = 1;
            site.ElevUnits = model.ElevUnits;
            site.ElevUpper = model.ElevUpper;

            if (model.Longitude.HasValue) {
                site.PosX1 = model.Longitude.Value;
            }

            if (model.Latitude.HasValue) {
                site.PosY1 = model.Latitude.Value;
            }

            if (string.IsNullOrEmpty(model.SiteName)) {
                site.SiteName = model.Locality;
            } else {
                site.SiteName = model.SiteName;
            }

            site.Locality = model.Locality;
            site.LocalityType = 1;
            site.PoliticalRegionID = model.PoliticalRegionID.GetValueOrDefault(0);
            site.PosAreaType = 1;
            site.PosCoordinates = 1;
            site.PosError = model.LLError;
            site.PosSource = model.LLSource;

            return site;
        }



    }

}
