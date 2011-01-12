using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameMaterialAction : GenericDatabaseAction<SiteExplorerNode> {

        public RenameMaterialAction(SiteExplorerNode model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameMaterial(Model.ElemID, Model.Name);
        }

    }

    public class InsertMaterialAction : AbstractSiteExplorerAction {

        public InsertMaterialAction(SiteExplorerNode model, SiteExplorerNodeViewModel viewModel, int templateID = 0) : base(model, viewModel) {
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

    public class UpdateMaterialAction : GenericDatabaseAction<BioLink.Data.Model.Material> {

        public UpdateMaterialAction(BioLink.Data.Model.Material model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateMaterial(Model);
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

    public class InsertRDEMaterialAction : GenericDatabaseAction<RDEMaterial> {

        public InsertRDEMaterialAction(RDEMaterial model, RDESiteVisit owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.SiteVisitID = Owner.SiteVisitID;
            Model.MaterialID = service.InsertMaterial(Model.SiteVisitID);
        }

        protected RDESiteVisit Owner { get; private set; }
    }

    public class UpdateRDEMaterialAction : GenericDatabaseAction<RDEMaterial> {

        public UpdateRDEMaterialAction(RDEMaterial model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateMaterial(MapToMaterial(Model));
        }

        private static Data.Model.Material MapToMaterial(RDEMaterial s) {
            var m = new Data.Model.Material();
            m.MaterialID = s.MaterialID;
            m.SiteVisitID = s.SiteVisitID;

            if (String.IsNullOrEmpty(s.MaterialName)) {
                var name = new StringBuilder();
                if (!string.IsNullOrEmpty(s.AccessionNo)) {
                    name.Append(s.AccessionNo).Append("; ");
                } else {
                    if (!string.IsNullOrEmpty(s.RegNo)) {
                        name.Append(s.RegNo).Append("; ");
                    }
                }

                name.Append(s.TaxaDesc);

                s.MaterialName = name.ToString();
            }

            m.MaterialName = s.MaterialName;


            m.AccessionNumber = s.AccessionNo;
            m.BiotaID = s.BiotaID;
            m.CollectionMethod = s.CollectionMethod;
            m.CollectorNumber = s.CollectorNo;
            m.IdentificationDate = s.IDDate;
            m.IdentifiedBy = s.ClassifiedBy;
            m.Institution = s.Institution;
            m.MacroHabitat = s.MacroHabitat;
            m.MicroHabitat = s.MicroHabitat;
            m.TrapID = s.TrapID.GetValueOrDefault(-1);
            return m;
        }

    }

    public class MoveRDEMaterialAction : GenericDatabaseAction<RDEMaterial> {

        public MoveRDEMaterialAction(RDEMaterial model, RDESiteVisit newParent) : base(model) {
            this.NewParent = newParent;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MoveMaterial(Model.MaterialID, NewParent.SiteVisitID);
        }

        protected RDESiteVisit NewParent { get; private set; }
    }


}
