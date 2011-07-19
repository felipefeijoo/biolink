using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public class UpdateMultimediaAction : GenericDatabaseCommand<Multimedia> {

        public UpdateMultimediaAction(Multimedia model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateMultimedia(Model.MultimediaID, Model.Name, Model.Number, Model.Artist, Model.DateRecorded, Model.Owner, Model.Copyright);
        }
    }

    public class DeleteMultimediaLinkAction : GenericDatabaseCommand<MultimediaLink> {

        public DeleteMultimediaLinkAction(MultimediaLink model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteMultimediaLink(Model.MultimediaLinkID);
        }

    }

    public class InsertMultimediaAction : GenericDatabaseCommand<MultimediaLink> {

        public InsertMultimediaAction(MultimediaLink model, string filename) : base(model) {
            this.Filename = filename;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var bytes = SystemUtils.GetBytesFromFile(Filename);
            int newId = service.InsertMultimedia(Model.Name, Model.Extension, bytes);
            Model.MultimediaID = newId;
        }

        public string Filename { get; private set; }        
    }

    public class InsertMultimediaLinkAction : GenericDatabaseCommand<MultimediaLink> {

        public InsertMultimediaLinkAction(MultimediaLink model, TraitCategoryType category, ViewModelBase owner) : base(model) {
            this.Category = category;
            this.Owner = owner;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            BioLink.Client.Utilities.Debug.Assert(Model.MultimediaID >= 0, "Not a valid multimedia ID!");
            var newId = service.InsertMultimediaLink(Category.ToString(), Owner.ObjectID.Value, Model.MultimediaType, Model.MultimediaID, Model.Caption);
            Model.MultimediaLinkID = newId;
        }

        public TraitCategoryType Category { get; private set; }

        public ViewModelBase Owner{ get; private set; }
    }

    public class UpdateMultimediaLinkAction : GenericDatabaseCommand<MultimediaLink> {

        public UpdateMultimediaLinkAction(MultimediaLink model, TraitCategoryType category)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateMultimediaLink(Model.MultimediaLinkID, TraitCategory.ToString() , Model.MultimediaType, Model.Caption);
        }

        public TraitCategoryType TraitCategory { get; private set; }
    }

    public class UpdateMultimediaBytesAction : GenericDatabaseCommand<MultimediaLink> {

        public UpdateMultimediaBytesAction(MultimediaLink model, string filename)
            : base(model) {
            this.Filename = filename;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var bytes = SystemUtils.GetBytesFromFile(Filename);
            service.UpdateMultimediaBytes(Model.MultimediaID, bytes);
        }

        public string Filename { get; private set; }
    }

}
