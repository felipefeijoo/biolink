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
        public InsertMaterialAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.ElemID = service.InsertMaterial(Model.ParentID);
            UpdateChildrenParentID();
        }
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

}
