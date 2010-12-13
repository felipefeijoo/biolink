using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class DeleteNoteAction : GenericDatabaseAction<Note> {

        public DeleteNoteAction(Note model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteNote(Model.NoteID);
        }
    }

    public class InsertNoteAction : GenericDatabaseAction<Note> {
        public InsertNoteAction(Note model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.NoteID = service.InsertNote(Model.NoteCategory.ToString(), Model.IntraCatID, Model.NoteType, Model.NoteRTF, Model.Author, Model.Comments, Model.UseInReports, Model.RefID, Model.RefPages);
        }
    }

    public class UpdateNoteAction : GenericDatabaseAction<Note> {

        public UpdateNoteAction(Note model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Debug.Assert(Model.NoteID >= 0);
            service.UpdateNote(Model.NoteID, Model.NoteCategory.ToString(), Model.NoteType, Model.NoteRTF, Model.Author, Model.Comments, Model.UseInReports, Model.RefID, Model.RefPages);
        }
    }
}
