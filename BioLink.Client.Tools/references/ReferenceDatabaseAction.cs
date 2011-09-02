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

namespace BioLink.Client.Tools {

    public class UpdateReferenceCommand : GenericDatabaseCommand<Reference> {

        public UpdateReferenceCommand(Reference model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateReference(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_REFS, PERMISSION_MASK.UPDATE);
        }
    }

    public class InsertReferenceCommand : GenericDatabaseCommand<Reference> {

        public InsertReferenceCommand(Reference model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.RefID = service.InsertReference(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_REFS, PERMISSION_MASK.INSERT);
        }

    }

    public class DeleteReferenceCommand : DatabaseCommand {

        public DeleteReferenceCommand(int refID) {
            this.RefID = refID;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteReference(RefID);
        }

        public override bool Equals(object obj) {
            if (obj is DeleteReferenceCommand) {
                var other = obj as DeleteReferenceCommand;
                if (other.RefID == this.RefID) {
                    return true;
                }
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public int RefID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_REFS, PERMISSION_MASK.DELETE);
        }

    }
}
