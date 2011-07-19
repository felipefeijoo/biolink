using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class InsertMaterialPartAction : GenericDatabaseCommand<MaterialPart> {

        public InsertMaterialPartAction(MaterialPart model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.MaterialID = Owner.ObjectID.Value;
            
            if (String.IsNullOrWhiteSpace(Model.PartName)) {
                Model.PartName = GeneratePartName(Model);
            }

            Model.MaterialPartID = service.InsertMaterialPart(Model);
        }

        public static String GeneratePartName(MaterialPart model) {

            var b = new StringBuilder();

            if (model.NoSpecimens.HasValue) {
                b.AppendFormat("{0}", model.NoSpecimens);
            }

            if (!string.IsNullOrWhiteSpace(model.Gender)) {
                if (b.Length > 0) {
                    b.Append(" x ");
                }
                b.Append(model.Gender);
            }

            if (!string.IsNullOrWhiteSpace(model.Lifestage)) {
                if (b.Length > 0) {
                    b.Append(" x ");
                } 
                b.Append(model.Lifestage);

            }

            if (!string.IsNullOrWhiteSpace(model.StorageMethod)) {
                if (b.Length > 0) {
                    b.Append(" x ");
                }
                b.Append(model.StorageMethod);
            }

            return b.ToString();
        }

        protected ViewModelBase Owner { get; private set; }
    }

    public class UpdateMaterialPartAction : GenericDatabaseCommand<MaterialPart> {

        public UpdateMaterialPartAction(MaterialPart model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);

            if (String.IsNullOrWhiteSpace(Model.PartName)) {
                Model.PartName = InsertMaterialPartAction.GeneratePartName(Model);
            }
            service.UpdateMaterialPart(Model);
        }
    }

    public class DeleteMaterialPartAction : GenericDatabaseCommand<MaterialPart> {

        public DeleteMaterialPartAction(MaterialPart model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteMaterialPart(Model.MaterialPartID);
        }
    }

}
