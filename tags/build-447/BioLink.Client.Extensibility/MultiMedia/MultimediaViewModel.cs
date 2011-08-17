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
