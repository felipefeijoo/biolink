using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Tools {

    public class PhraseCategoryViewModel : GenericViewModelBase<PhraseCategory> {

        public PhraseCategoryViewModel(PhraseCategory category) : base(category, ()=>category.CategoryID) { }

        public int CategoryID {
            get { return Model.CategoryID; }
            set { SetProperty( () => Model.CategoryID, value ); }
        }

        public string Category {
            get { return Model.Category; }
            set { SetProperty(() => Model.Category, value); }
        }

        public bool Fixed {
            get { return Model.Fixed; }
            set { SetProperty(() => Model.Fixed, value); }
        }

        protected override string RelativeImagePath {
            get { return (Fixed ?  "images/Phrase_fixed.png" :  "images/Phrase.png" ); }
        }

    }

    public class PhraseViewModel : GenericViewModelBase<Phrase> {

        public PhraseViewModel(Phrase phrase) : base(phrase, ()=>phrase.PhraseID) { }

        public string PhraseText {
            get { return Model.PhraseText; }
            set { SetProperty(() => Model.PhraseText, Model, value); }
        }

        public int PhraseID {
            get { return Model.PhraseID; }
        }

        public int PhraseCatID {
            get { return Model.PhraseCatID; }
        }

    }
}
