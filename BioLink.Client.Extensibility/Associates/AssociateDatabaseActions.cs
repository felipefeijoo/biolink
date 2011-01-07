using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public class InsertAssociateAction : GenericDatabaseAction<Associate> {

        public InsertAssociateAction(Associate model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service =new SupportService(user);
            Model.FromIntraCatID = Owner.ObjectID.Value;

            switch (Model.ToCatID.GetValueOrDefault(-1)) {
                case 1:
                    Model.ToCategory = "Material";
                    break;
                case 2:
                    Model.ToCategory = "Taxon";
                    break;
                default:
                    Model.ToCategory = "";
                    break;
            }

            Model.AssociateID = service.InsertAssociate(Model);
        }

        public override string ToString() {
            return string.Format("Insert Associate: Name={0}", Model.AssocName);
        }

        protected ViewModelBase Owner { get; private set; }

    }

    public class UpdateAssociateAction : GenericDatabaseAction<Associate> {
        public UpdateAssociateAction(Associate model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);

            switch (Model.ToCatID.GetValueOrDefault(-1)) {
                case 1:
                    Model.ToCategory = "Material";
                    break;
                case 2:
                    Model.ToCategory = "Taxon";
                    break;
                default:
                    Model.ToCategory = "";
                    break;
            }

            service.UpdateAssociate(Model);
        }

        public override string ToString() {
            return string.Format("Update Associate: Name={0}", Model.AssocName);
        }

    }

    public class DeleteAssociateAction : GenericDatabaseAction<Associate> {
        public DeleteAssociateAction(Associate model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteAssociate(Model.AssociateID);
        }
    }
}
