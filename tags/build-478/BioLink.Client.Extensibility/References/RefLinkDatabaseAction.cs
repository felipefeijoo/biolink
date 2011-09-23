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

    public abstract class RefLinkDatabaseCommand : GenericDatabaseCommand<RefLink> {

        public RefLinkDatabaseCommand(RefLink model, string categoryName)
            : base(model) {
            this.CategoryName = categoryName;
        }

        #region Properties

        public string CategoryName { get; private set; }

        #endregion

    }

    public class UpdateRefLinkCommand : RefLinkDatabaseCommand {

        public UpdateRefLinkCommand(RefLink model, string categoryName)
            : base(model, categoryName) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateRefLink(Model, CategoryName);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class InsertRefLinkCommand : RefLinkDatabaseCommand {

        public InsertRefLinkCommand(RefLink model, string categoryName)
            : base(model, categoryName) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.InsertRefLink(Model, CategoryName);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class DeleteRefLinkCommand : GenericDatabaseCommand<RefLink> {

        public DeleteRefLinkCommand(RefLink model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteRefLink(Model.RefLinkID);
        }

        public int RefLinkID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }
}
