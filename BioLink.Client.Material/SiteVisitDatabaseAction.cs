using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameSiteVisitAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public RenameSiteVisitAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameSiteVisit(Model.ElemID, Model.Name);
        }
    }

}
