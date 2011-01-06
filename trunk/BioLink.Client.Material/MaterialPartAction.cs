using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class InsertMaterialPartAction : GenericDatabaseAction<MaterialPart> {

        public InsertMaterialPartAction(MaterialPart model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.MaterialID = Owner.ObjectID.Value;
            Model.MaterialPartID = service.InsertMaterialPart(Model);
        }

        protected ViewModelBase Owner { get; private set; }
    }

    public class UpdateMaterialPartAction : GenericDatabaseAction<MaterialPart> {

        public UpdateMaterialPartAction(MaterialPart model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateMaterialPart(Model);
        }
    }

    public class DeleteMaterialPartAction : GenericDatabaseAction<MaterialPart> {

        public DeleteMaterialPartAction(MaterialPart model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteMaterialPart(Model.MaterialPartID);
        }
    }

}
