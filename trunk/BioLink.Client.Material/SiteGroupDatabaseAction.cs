using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Material {

    public class RenameSiteGroupAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public RenameSiteGroupAction(SiteExplorerNodeViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.RenameSiteGroup(Model.ElemID, Model.Name);
        }

    }

    public class InsertSiteGroupAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public InsertSiteGroupAction(SiteExplorerNodeViewModel model, SiteExplorerNodeViewModel parent)
            : base(model) {
            this.Parent = parent;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            var parentID = Parent.ElemID;
            var parentType = 2;
            if (Parent.NodeType == SiteExplorerNodeType.Region) {
                // Weird! Need to do this otherwise stored proc crashes
                parentID = 0;
                parentType = 1;
            }
            var regionID = FindRegionID(Model);
            Model.ElemID = service.InsertSiteGroup(Model.Name, parentType, parentID, regionID);
            foreach (SiteExplorerNodeViewModel child in Model.Children) {
                child.ParentID = Model.ElemID;
            }
        }

        private int FindRegionID(SiteExplorerNodeViewModel node) {
            HierarchicalViewModelBase p = node;

            while (p != null && (p as SiteExplorerNodeViewModel).NodeType != SiteExplorerNodeType.Region) {
                p = p.Parent;
            }
            if (p != null) {
                return (p as SiteExplorerNodeViewModel).ElemID;
            }

            return -1;
        }

        internal SiteExplorerNodeViewModel Parent { get; private set; }

    }

    public class DeleteSiteGroupAction : DatabaseAction {

        public DeleteSiteGroupAction(int siteGroupID) {
            this.SiteGroupID = siteGroupID;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.DeleteSiteGroup(SiteGroupID);
        }

        public int SiteGroupID { get; private set; }
    }

}
