/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
