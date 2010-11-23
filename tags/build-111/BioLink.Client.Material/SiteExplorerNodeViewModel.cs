using System;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class SiteExplorerNodeViewModel : GenericHierarchicalViewModelBase<SiteExplorerNode> {

        public SiteExplorerNodeViewModel(SiteExplorerNode model)
            : base(model) {
            this.DisplayLabel = model.Name;
        }

        public string GetParentage() {
            String parentage = "";
            TraverseToTop((node) => {
                if (node is SiteExplorerNodeViewModel) {
                    parentage = "/" + (node as SiteExplorerNodeViewModel).ElemID + parentage;
                } 
            });
            return parentage;
        }

        protected override string RelativeImagePath {
            get {
                var image = "Region";
                switch (NodeType) {
                    case SiteExplorerNodeType.Region:
                        image = "Region";
                        break;
                    case SiteExplorerNodeType.Site:
                        image = "Site";
                        break;
                    case SiteExplorerNodeType.SiteVisit:
                        image = "SiteVisit";
                        break;
                    case SiteExplorerNodeType.Material:
                        image = "Material";
                        break;
                    case SiteExplorerNodeType.SiteGroup:
                        image = "SiteGroup";
                        break;

                }
                return String.Format(@"images\{0}.png", image);
            }
        }

        public SiteExplorerNodeType NodeType {
            get {
                return (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), Model.ElemType);
            }
        }

        public int ElemID {
            get { return Model.ElemID; }
            set { SetProperty(() => Model.ElemID, value); }
        }

        public string ElemType {
            get { return Model.ElemType; }
            set { SetProperty(() => Model.ElemType, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public int ParentID {
            get { return Model.ParentID; }
            set { SetProperty(() => Model.ParentID, value); }
        }

        public int RegionID {
            get { return Model.RegionID; }
            set { SetProperty(() => Model.RegionID, value); }
        }

        public int NumChildren {
            get { return Model.NumChildren; }
            set { SetProperty(() => Model.NumChildren, value); }
        }

    }
}
