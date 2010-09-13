using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public abstract class TraitDatabaseActionBase : DatabaseAction<SupportService> {

        public TraitDatabaseActionBase(Trait trait) {
            this.Trait = trait;
        }

        public Trait Trait { get; set; }
    }

    public class UpdateTraitDatabaseAction : TraitDatabaseActionBase {

        public UpdateTraitDatabaseAction(Trait trait)
            : base(trait) {
        }

        protected override void ProcessImpl(SupportService service) {
            Trait.TraitID = service.InsertOrUpdateTrait(Trait);
        }

        public override bool Equals(object obj) {
            var other = obj as UpdateTraitDatabaseAction;
            if (other != null) {
                return Trait == other.Trait;
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

    }

    public class DeleteTraitDatabaseAction : TraitDatabaseActionBase {

        public DeleteTraitDatabaseAction(Trait trait)
            : base(trait) {
        }

        protected override void ProcessImpl(SupportService service) {
            service.DeleteTrait(Trait.TraitID);
        }

    }

}
