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
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class InsertMaterialPartCommand : GenericDatabaseCommand<MaterialPart> {

        public InsertMaterialPartCommand(MaterialPart model, ViewModelBase owner) : base(model) {
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

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }


        protected ViewModelBase Owner { get; private set; }
    }

    public class UpdateMaterialPartCommand : GenericDatabaseCommand<MaterialPart> {

        public UpdateMaterialPartCommand(MaterialPart model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);

            if (String.IsNullOrWhiteSpace(Model.PartName) || Preferences.AutoGenerateMaterialNames.Value) {
                Model.PartName = InsertMaterialPartCommand.GeneratePartName(Model);
            }
            service.UpdateMaterialPart(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteMaterialPartCommand : GenericDatabaseCommand<MaterialPart> {

        public DeleteMaterialPartCommand(MaterialPart model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteMaterialPart(Model.MaterialPartID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

}
