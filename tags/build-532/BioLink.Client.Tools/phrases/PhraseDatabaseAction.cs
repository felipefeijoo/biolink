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
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Tools {

    abstract class PhraseDatabaseCommand : DatabaseCommand {

        protected PhraseDatabaseCommand(Phrase phrase) {
            this.Phrase = phrase;
        }

        public Phrase Phrase { get; private set; }
    }

    class RenamePhraseCommand : PhraseDatabaseCommand {

        public RenamePhraseCommand(Phrase phrase, string newvalue)
            : base(phrase) {
            this.NewValue = newvalue;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.RenamePhrase(Phrase.PhraseID, NewValue);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_PHRASES, PERMISSION_MASK.UPDATE);
        }


        public string NewValue { get; set; }
    }

    class DeletePhraseCommand : PhraseDatabaseCommand {

        public DeletePhraseCommand(Phrase phrase)
            : base(phrase) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeletePhrase(Phrase.PhraseID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_PHRASES, PERMISSION_MASK.DELETE);
        }

    }

    class InsertPhraseCommand : PhraseDatabaseCommand {
        public InsertPhraseCommand(Phrase phrase)
            : base(phrase) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.AddPhrase(this.Phrase);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_PHRASES, PERMISSION_MASK.INSERT);
        }


    }

    class DeletePhraseCategoryCommand : DatabaseCommand {

        public DeletePhraseCategoryCommand(PhraseCategory category) {
            this.PhraseCategory = category;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeletePhraseCategory(PhraseCategory.CategoryID);
        }

        public PhraseCategory PhraseCategory { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SUPPORT_PHRASECATEGORIES, PERMISSION_MASK.DELETE);
        }

    }

}
