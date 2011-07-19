using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class DeleteNoteAction : GenericDatabaseCommand<Note> {
    
        public DeleteNoteAction(Note model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.IntraCatID = Owner.ObjectID.Value;
            service.DeleteNote(Model.NoteID);
        }

        protected ViewModelBase Owner { get; set; }
    }

    public class InsertNoteAction : GenericDatabaseCommand<Note> {

        public InsertNoteAction(Note model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.IntraCatID = Owner.ObjectID.Value;
            Model.NoteID = service.InsertNote(Model.NoteCategory.ToString(), Model.IntraCatID, Model.NoteType, Model.NoteRTF, Model.Author, Model.Comments, Model.UseInReports, Model.RefID, Model.RefPages);
        }

        protected ViewModelBase Owner { get; private set; }

    }

    public class UpdateNoteAction : GenericDatabaseCommand<Note> {

        public UpdateNoteAction(Note model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.IntraCatID = Owner.ObjectID.Value;            
            service.UpdateNote(Model.NoteID, Model.NoteCategory.ToString(), Model.NoteType, Model.NoteRTF, Model.Author, Model.Comments, Model.UseInReports, Model.RefID, Model.RefPages);
        }

        protected ViewModelBase Owner { get; private set; }
    }
}
