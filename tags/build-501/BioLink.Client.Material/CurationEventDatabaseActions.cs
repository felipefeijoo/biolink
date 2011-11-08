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

    public class InsertCurationEventCommand : GenericDatabaseCommand<CurationEvent> {

        public InsertCurationEventCommand(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            Model.CurationEventID = service.InsertCurationEvent(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class UpdateCurationEventCommand : GenericDatabaseCommand<CurationEvent> {

        public UpdateCurationEventCommand(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.UpdateCurationEvent(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

    public class DeleteCurationEventCommand : GenericDatabaseCommand<CurationEvent> {
        public DeleteCurationEventCommand(CurationEvent model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteCurationEvent(Model.CurationEventID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE);
        }

    }

}
