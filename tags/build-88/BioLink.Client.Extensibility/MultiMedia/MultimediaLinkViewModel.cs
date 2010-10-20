using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Extensibility {

    public class MultimediaLinkViewModel : GenericViewModelBase<MultimediaLink> {

        private BitmapSource _thumb;

        public MultimediaLinkViewModel(MultimediaLink model)
            : base(model) {
        }

        public int MultimediaID {
            get { return Model.MultimediaID; }
            set { SetProperty(() => Model.MultimediaID, value); }
        }

        public int MultimediaLinkID {
            get { return Model.MultimediaLinkID; }
            set { SetProperty(() => Model.MultimediaLinkID, value); }
        }

        public string MultimediaType {
            get { return Model.MultimediaType; }
            set { SetProperty(() => Model.MultimediaType, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Caption {
            get { return Model.Caption; }
            set { SetProperty(() => Model.Caption, value); }
        }

        public string Extension {
            get { return Model.Extension; }
            set { SetProperty(() => Model.Extension, value); }
        }

        public int SizeInBytes {
            get { return Model.SizeInBytes; }
            set { SetProperty(() => Model.SizeInBytes, value); }
        }

        public int Changes {
            get { return Model.Changes; }
            set { SetProperty(() => Model.Changes, value); }
        }

        public int BlobChanges {
            get { return Model.BlobChanges; }
            set { SetProperty(() => Model.BlobChanges, value); }
        }

        public BitmapSource Thumbnail {
            get { return _thumb; }
            set { SetProperty("ThumbNail", ref _thumb, value); }
        }

    }
}
