using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Tools {

    abstract class PhraseDatabaseAction : DatabaseAction<SupportService> {

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

        protected override void ProcessImpl(SupportService service) {
            service.RenamePhrase(Phrase.PhraseID, NewValue);
        }

        public string NewValue { get; set; }
    }

    class DeletePhraseAction : PhraseDatabaseAction {

        public DeletePhraseAction(Phrase phrase)
            : base(phrase) {
        }

        protected override void ProcessImpl(SupportService service) {
            service.DeletePhrase(Phrase.PhraseID);
        }
    }

    class AddPhraseAction : PhraseDatabaseAction {
        public AddPhraseAction(Phrase phrase)
            : base(phrase) {
        }

        protected override void ProcessImpl(SupportService service) {
            service.AddPhrase(this.Phrase);
        }

    }
}
