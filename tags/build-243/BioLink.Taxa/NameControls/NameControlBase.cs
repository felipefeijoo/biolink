using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Client.Extensibility;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BioLink.Client.Taxa {

    public class NameControlBase : DatabaseActionControl {

        public NameControlBase()
            : base() {
        }

        public NameControlBase(TaxonViewModel taxon, User user, string controlId)
            : base(user, String.Format("Taxon::{0}::{1}", controlId, taxon.TaxaID.Value)) {
            this.Taxon = taxon;
        }

        protected void InsertPhrase(RichTextBox rtb, string phraseCategory) {
            string phrase = PickListControl.ShowPickList(User, PickListType.Phrase, phraseCategory, TraitCategoryType.Taxon);
            if (phrase != null) {
                var tr = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                tr.Text = phrase;                
                rtb.Focus();
            }
        }

        public TaxonViewModel Taxon { get; private set; }

    }
}
