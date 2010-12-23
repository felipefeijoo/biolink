using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameMaterialAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public RenameMaterialAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameMaterial(Model.ElemID, Model.Name);
        }

    }

    public class InsertMaterialAction : AbstractSiteExplorerAction {

        public InsertMaterialAction(SiteExplorerNodeViewModel model, int templateID = 0)
            : base(model) {
            this.TemplateID = templateID;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertMaterial(Model.ParentID, TemplateID);
            UpdateChildrenParentID();
        }

        public int TemplateID { get; private set; }
    }

    public class DeleteMaterialAction : DatabaseAction {

        public DeleteMaterialAction(int materialId) {
            this.MaterialID = materialId;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteMaterial(MaterialID);
        }

        public int MaterialID { get; private set; }
    }

    public class UpdateMaterialAction : GenericDatabaseAction<MaterialViewModel> {

        public UpdateMaterialAction(MaterialViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateMaterial(Model.Model);
        }
    }

    public class MergeMaterialAction : GenericDatabaseAction<SiteExplorerNode> {

        public MergeMaterialAction(SiteExplorerNode source, SiteExplorerNode dest)
            : base(source) {
            Dest = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MergeMaterial(Model.ElemID, Dest.ElemID);
        }

        public SiteExplorerNode Dest { get; private set; }
    }

    public class MoveMaterialAction : GenericDatabaseAction<SiteExplorerNode> {
        public MoveMaterialAction(SiteExplorerNode model, SiteExplorerNode dest)
            : base(model) {
            this.Destination = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MoveMaterial(Model.ElemID, Destination.ElemID);
        }

        public SiteExplorerNode Destination { get; private set; }
    }

    public class InsertMaterialTemplateAction : GenericDatabaseAction<SiteExplorerNode> {
        public InsertMaterialTemplateAction(SiteExplorerNode model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertMaterialTemplate();
        }
    }

}
