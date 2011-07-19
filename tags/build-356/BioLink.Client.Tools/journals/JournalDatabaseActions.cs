using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Tools {

    public class InsertJournalAction : GenericDatabaseCommand<Journal> {

        public InsertJournalAction(Journal model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.JournalID = service.InsertJournal(Model);
        }
    }

    public class UpdateJournalAction : GenericDatabaseCommand<Journal> {

        public UpdateJournalAction(Journal model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateJournal(Model);
        }
    }

    public class DeleteJournalAction : GenericDatabaseCommand<Journal> {

        public DeleteJournalAction(Journal model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteJournal(Model.JournalID);
        }
    }
}
