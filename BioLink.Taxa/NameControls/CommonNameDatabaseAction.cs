using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class UpdateCommonNameAction : GenericDatabaseCommand<CommonName> {

        public UpdateCommonNameAction(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.UpdateCommonName(Model);
        }

    }

    public class InsertCommonNameAction : GenericDatabaseCommand<CommonName> {

        public InsertCommonNameAction(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertCommonName(Model);
        }
    }

    public class DeleteCommonNameAction : GenericDatabaseCommand<CommonName> {

        public DeleteCommonNameAction(CommonName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.DeleteCommonName(Model.CommonNameID);
        }
    }




}
