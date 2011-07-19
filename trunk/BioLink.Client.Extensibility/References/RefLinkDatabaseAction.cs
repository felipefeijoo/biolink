using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public abstract class RefLinkDatabaseCommand : GenericDatabaseCommand<RefLink> {

        public RefLinkDatabaseCommand(RefLink model, string categoryName)
            : base(model) {
            this.CategoryName = categoryName;
        }

        #region Properties

        public string CategoryName { get; private set; }

        #endregion

    }

    public class UpdateRefLinkCommand : RefLinkDatabaseCommand {

        public UpdateRefLinkCommand(RefLink model, string categoryName)
            : base(model, categoryName) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateRefLink(Model, CategoryName);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class InsertRefLinkCommand : RefLinkDatabaseCommand {

        public InsertRefLinkCommand(RefLink model, string categoryName)
            : base(model, categoryName) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.InsertRefLink(Model, CategoryName);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class DeleteRefLinkCommand : GenericDatabaseCommand<RefLink> {

        public DeleteRefLinkCommand(RefLink model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteRefLink(Model.RefLinkID);
        }

        public int RefLinkID { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }
}
