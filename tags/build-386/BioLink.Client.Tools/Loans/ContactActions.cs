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
