using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class UpdateCommonNameAction : GenericDatabaseAction<CommonName> {

        public UpdateCommonNameAction(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.UpdateCommonName(Model);
        }

    }

    public class InsertCommonNameAction : GenericDatabaseAction<CommonName> {

        public InsertCommonNameAction(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertCommonName(Model);
        }
    }

    public class DeleteCommonNameAction : GenericDatabaseAction<CommonName> {

        public DeleteCommonNameAction(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.DeleteCommonName(Model.CommonNameID);
        }
    }




}
