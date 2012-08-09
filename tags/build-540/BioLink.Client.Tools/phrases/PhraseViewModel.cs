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
