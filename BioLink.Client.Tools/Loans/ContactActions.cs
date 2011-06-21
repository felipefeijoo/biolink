using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class UpdateContactAction : GenericDatabaseAction<Contact> {

        public UpdateContactAction(Contact model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateContact(Model);
        }
    }

    public class DeleteContactAction : GenericDatabaseAction<Contact> {

        public DeleteContactAction(Contact contact) : base(contact) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.DeleteContact(Model.ContactID);            
        }
        
    }

    public class InsertContactAction : GenericDatabaseAction<Contact> {

        public InsertContactAction(Contact model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            Model.ContactID = service.InsertContact(Model);
        }
    }
}
