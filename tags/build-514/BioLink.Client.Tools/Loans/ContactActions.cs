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
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class UpdateContactCommand : GenericDatabaseCommand<Contact> {

        public UpdateContactCommand(Contact model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateContact(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class DeleteContactCommand : GenericDatabaseCommand<Contact> {

        public DeleteContactCommand(Contact contact) : base(contact) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.DeleteContact(Model.ContactID);            
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
        
    }

    public class InsertContactCommand : GenericDatabaseCommand<Contact> {

        public InsertContactCommand(Contact model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            Model.ContactID = service.InsertContact(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }
}
