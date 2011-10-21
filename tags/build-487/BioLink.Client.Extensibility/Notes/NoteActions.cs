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
