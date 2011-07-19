using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public abstract class RefLinkDatabaseAction : GenericDatabaseCommand<RefLink> {

        public RefLinkDatabaseAction(RefLink model, string categoryName)
            : base(model) {
            this.CategoryName = categoryName;
        }

        #region Properties

        public string CategoryName { get; private set; }

        #endregion

    }

    public class UpdateRefLinkAction : RefLinkDatabaseAction {

        public UpdateRefLinkAction(RefLink model, string categoryName)
            : base(model, categoryName) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateRefLink(Model, CategoryName);
        }

    }

    public class InsertRefLinkAction : RefLinkDatabaseAction {

        public InsertRefLinkAction(RefLink model, string categoryName)
            : base(model, categoryName) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.InsertRefLink(Model, CategoryName);
        }

    }

    public class DeleteRefLinkAction : GenericDatabaseCommand<RefLink> {

        public DeleteRefLinkAction(RefLink model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteRefLink(Model.RefLinkID);
        }

        public int RefLinkID { get; private set; }
    }
}
