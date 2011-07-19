using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public class UpdateMultimediaCommand : GenericDatabaseCommand<Multimedia> {

        public UpdateMultimediaCommand(Multimedia model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateMultimedia(Model.MultimediaID, Model.Name, Model.Number, Model.Artist, Model.DateRecorded, Model.Owner, Model.Copyright);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class DeleteMultimediaLinkCommand : GenericDatabaseCommand<MultimediaLink> {

        public DeleteMultimediaLinkCommand(MultimediaLink model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteMultimediaLink(Model.MultimediaLinkID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }


    }

    public class InsertMultimediaCommand : GenericDatabaseCommand<MultimediaLink> {

        public InsertMultimediaCommand(MultimediaLink model, string filename) : base(model) {
            this.Filename = filename;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var bytes = SystemUtils.GetBytesFromFile(Filename);
            int newId = service.InsertMultimedia(Model.Name, Model.Extension, bytes);
            Model.MultimediaID = newId;
        }

        public string Filename { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class InsertMultimediaLinkCommand : GenericDatabaseCommand<MultimediaLink> {

        public InsertMultimediaLinkCommand(MultimediaLink model, TraitCategoryType category, ViewModelBase owner) : base(model) {
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

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class UpdateMultimediaLinkCommand : GenericDatabaseCommand<MultimediaLink> {

        public UpdateMultimediaLinkCommand(MultimediaLink model, TraitCategoryType category)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateMultimediaLink(Model.MultimediaLinkID, TraitCategory.ToString() , Model.MultimediaType, Model.Caption);
        }

        public TraitCategoryType TraitCategory { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class UpdateMultimediaBytesCommand : GenericDatabaseCommand<MultimediaLink> {

        public UpdateMultimediaBytesCommand(MultimediaLink model, string filename)
            : base(model) {
            this.Filename = filename;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var bytes = SystemUtils.GetBytesFromFile(Filename);
            service.UpdateMultimediaBytes(Model.MultimediaID, bytes);
        }

        public string Filename { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

}
