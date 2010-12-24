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
            int parentID = 0;
            var parentType = 2;
            if (Parent != null) {
                parentID = Parent.ElemID;
                if (Parent.NodeType == SiteExplorerNodeType.Region) {
                    // Weird! Need to do this otherwise stored proc crashes
                    parentID = 0;
                    parentType = 1;
                }
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

    public class MergeSiteGroupAction : GenericDatabaseAction<SiteExplorerNode> {

        public MergeSiteGroupAction(SiteExplorerNode source, SiteExplorerNode dest) 
            : base(source) {
            Dest = dest;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            service.MergeSiteGroup(Model.ElemID, Dest.ElemID);
        }

        public SiteExplorerNode Dest { get; private set; }
    }

    public class MoveSiteGroupAction : GenericDatabaseAction<SiteExplorerNode> {

        public MoveSiteGroupAction(SiteExplorerNode source, SiteExplorerNode newParent)
            : base(source) {
                NewParent = newParent;
        }

        protected override void ProcessImpl(User user) {
            var service = new MaterialService(user);
            int parentType = 0;
            int parentID = 0;            
            if (NewParent.ElemType == "Region") {
                parentType = 1;
                parentID = 0;
            } else {
                parentType = 2;
                parentID = NewParent.ElemID;
            }
            service.MoveSiteGroup(Model.ElemID, parentType, parentID, NewParent.RegionID);
        }


        public SiteExplorerNode NewParent { get; private set; }
    }

}
