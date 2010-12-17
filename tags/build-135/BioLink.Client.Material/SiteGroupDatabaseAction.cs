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

    public abstract class AbstractSiteExplorerAction : GenericDatabaseAction<SiteExplorerNodeViewModel> {

        public AbstractSiteExplorerAction(SiteExplorerNodeViewModel model)
            : base(model) {
            this.Parent = model.Parent as SiteExplorerNodeViewModel;
        }

        protected int FindRegionID(SiteExplorerNodeViewModel node) {
            return FindIDOfParentType(node, SiteExplorerNodeType.Region);
        }

        protected int FindIDOfParentType(SiteExplorerNodeViewModel node, SiteExplorerNodeType nodeType, int defaultId = -1) {
            HierarchicalViewModelBase p = node;

            while (p != null && (p as SiteExplorerNodeViewModel).NodeType != nodeType) {
                p = p.Parent;
            }
            if (p != null) {
                return (p as SiteExplorerNodeViewModel).ElemID;
            }

            return defaultId;
        }

        protected void UpdateChildrenParentID() {
            foreach (SiteExplorerNodeViewModel child in Model.Children) {
                child.ParentID = Model.ElemID;
            }

        }

        internal SiteExplorerNodeViewModel Parent { get; private set; }

    }

    public class InsertSiteGroupAction : AbstractSiteExplorerAction {

        public InsertSiteGroupAction(SiteExplorerNodeViewModel model)
            : base(model) {            
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
            base.UpdateChildrenParentID();
        }

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
