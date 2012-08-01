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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public abstract class AssociateDatabaseCommand : GenericDatabaseCommand<Associate> {

        protected AssociateDatabaseCommand(Associate model) : base(model) { }

        protected string CategoryIDToString(int? catId) {
            if (!catId.HasValue) {
                return "";
            }

            if (catId.Value == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Material)) {
                return "Material";
            }

            if (catId.Value == TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Taxon)) {
                return "Taxon";
            }

            return "";
        }

    }

    public class InsertAssociateCommand : AssociateDatabaseCommand {

        public InsertAssociateCommand(Associate model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service =new SupportService(user);
            Model.FromIntraCatID = Owner.ObjectID.Value;
            Model.ToCategory = CategoryIDToString(Model.ToCatID);
            Model.AssociateID = service.InsertAssociate(Model);
        }

        public override string ToString() {
            return string.Format("Insert Associate: Name={0}", Model.AssocName);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

        protected ViewModelBase Owner { get; private set; }

    }

    public class UpdateAssociateCommand : AssociateDatabaseCommand {

        public UpdateAssociateCommand(Associate model) : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.ToCategory = CategoryIDToString(Model.ToCatID);
            service.UpdateAssociate(Model);
        }

        public override string ToString() {
            return string.Format("Update Associate: Name={0}", Model.AssocName);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteAssociateCommand : GenericDatabaseCommand<Associate> {

        public DeleteAssociateCommand(Associate model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteAssociate(Model.AssociateID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
        }

    }
}
