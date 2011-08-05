using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public abstract class TraitDatabaseCommandBase : DatabaseCommand {

        public TraitDatabaseCommandBase(Trait trait, ViewModelBase owner) {
            this.Trait = trait;
            this.Owner = owner;
        }

        public Trait Trait { get; private set; }
        public ViewModelBase Owner { get; private set; }

        
    }

    public class UpdateTraitDatabaseCommand : TraitDatabaseCommandBase {

        public UpdateTraitDatabaseCommand(Trait trait, ViewModelBase owner) : base(trait, owner) { }

        protected override void ProcessImpl(User user) {
            SupportService service = new SupportService(user);
            Trait.IntraCatID = Owner.ObjectID.Value;
            Trait.TraitID = service.InsertOrUpdateTrait(Trait);
        }

        public override bool Equals(object obj) {
            var other = obj as UpdateTraitDatabaseCommand;
            if (other != null) {
                return Trait == other.Trait;
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            return string.Format("Insert/Update Trait: ID={0} Cat={1} Value={2}", Trait.TraitID, Trait.Category, Trait.Value);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class DeleteTraitDatabaseCommand : TraitDatabaseCommandBase {

        public DeleteTraitDatabaseCommand(Trait trait, ViewModelBase owner) : base(trait, owner) { }

        protected override void ProcessImpl(User user) {
            SupportService service = new SupportService(user);
            service.DeleteTrait(Trait.TraitID);
        }

        public override string ToString() {
            return string.Format("Delete Trait: ID={0} Cat={1} Value={2}", Trait.TraitID, Trait.Category, Trait.Value);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

}
