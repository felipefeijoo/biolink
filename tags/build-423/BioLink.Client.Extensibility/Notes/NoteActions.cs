using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class DeleteNoteCommand : GenericDatabaseCommand<Note> {
    
        public DeleteNoteCommand(Note model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.IntraCatID = Owner.ObjectID.Value;
            service.DeleteNote(Model.NoteID);
        }

        protected ViewModelBase Owner { get; set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class InsertNoteCommand : GenericDatabaseCommand<Note> {

        public InsertNoteCommand(Note model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.IntraCatID = Owner.ObjectID.Value;
            Model.NoteID = service.InsertNote(Model.NoteCategory.ToString(), Model.IntraCatID, Model.NoteType, Model.NoteRTF, Model.Author, Model.Comments, Model.UseInReports, Model.RefID, Model.RefPages);
        }

        protected ViewModelBase Owner { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class UpdateNoteCommand : GenericDatabaseCommand<Note> {

        public UpdateNoteCommand(Note model, ViewModelBase owner) : base(model) {
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.IntraCatID = Owner.ObjectID.Value;            
            service.UpdateNote(Model.NoteID, Model.NoteCategory.ToString(), Model.NoteType, Model.NoteRTF, Model.Author, Model.Comments, Model.UseInReports, Model.RefID, Model.RefPages);
        }

        protected ViewModelBase Owner { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }
}
