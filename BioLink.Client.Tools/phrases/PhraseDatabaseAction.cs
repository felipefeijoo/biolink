using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Tools {

    abstract class PhraseDatabaseAction : DatabaseAction {

        protected PhraseDatabaseAction(Phrase phrase) {
            this.Phrase = phrase;
        }

        public Phrase Phrase { get; private set; }
    }

    class RenamePhraseAction : PhraseDatabaseAction {

        public RenamePhraseAction(Phrase phrase, string newvalue)
            : base(phrase) {
            this.NewValue = newvalue;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.RenamePhrase(Phrase.PhraseID, NewValue);
        }

        public string NewValue { get; set; }
    }

    class DeletePhraseAction : PhraseDatabaseAction {

        public DeletePhraseAction(Phrase phrase)
            : base(phrase) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeletePhrase(Phrase.PhraseID);
        }
    }

    class AddPhraseAction : PhraseDatabaseAction {
        public AddPhraseAction(Phrase phrase)
            : base(phrase) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.AddPhrase(this.Phrase);
        }

    }

    class DeletePhraseCategoryAction : DatabaseAction {

        public DeletePhraseCategoryAction(PhraseCategory category) {
            this.PhraseCategory = category;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeletePhraseCategory(PhraseCategory.CategoryID);
        }

        public PhraseCategory PhraseCategory { get; private set; }
    }

}
