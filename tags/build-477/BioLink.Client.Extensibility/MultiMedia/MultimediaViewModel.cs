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
using BioLink.Client.Utilities;


namespace BioLink.Client.Extensibility {

    public class MultimediaViewModel : GenericOwnedViewModel<Multimedia> {

        public MultimediaViewModel(Multimedia model) : base(model, ()=>model.MultimediaID) { }

        public int MultimediaID {
            get { return Model.MultimediaID; }
            set { SetProperty(() => Model.MultimediaID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Number {
            get { return Model.Number; }
            set { SetProperty(() => Model.Number, value); }
        }

        public string Artist {
            get { return Model.Artist; }
            set { SetProperty(() => Model.Artist, value); }
        }

        public string DateRecorded {
            get { return Model.DateRecorded; }
            set { SetProperty(() => Model.DateRecorded, value); }
        }

        public string Owner {
            get { return Model.Owner; }
            set { SetProperty(() => Model.Owner, value); }
        }

        public string Copyright {
            get { return Model.Copyright; }
            set { SetProperty(() => Model.Copyright, value); } 
        }

        public string FileExtension {
            get { return Model.FileExtension; }
            set { SetProperty(() => Model.FileExtension, value); }
        }

        public string Fullname {
            get { return string.Format("{0}.{1}", Name, FileExtension); }
        }

    }
}
