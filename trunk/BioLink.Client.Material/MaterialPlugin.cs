using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Text.RegularExpressions;

namespace BioLink.Client.Material {

    public class MaterialPlugin : BiolinkPluginBase {

        public const string MATERIAL_PLUGIN_NAME = "Material";

        private MaterialExplorer _explorer;

        public MaterialPlugin() {
        }

        public override string Name {
            get { return MATERIAL_PLUGIN_NAME; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

            contrib.Add(new MenuWorkspaceContribution(this, "ShowExplorer", (obj, e) => { PluginManager.EnsureVisible(this, "MaterialExplorer"); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Material.Menu.View")),
                String.Format("{{'Name':'ShowMaterialExplorer', 'Header':'{0}'}}", _R("Material.Menu.ShowExplorer"))
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ShowRDE", (obj, e) => { ShowRDE(); },
                "{'Name':'Tools', 'Header':'_Tools','InsertAfter':'File'}",
                "{'Name':'ShowMaterialExplorer', 'Header':'_Rapid Data Entry'}"
            ));


            _explorer = new MaterialExplorer(this);
            contrib.Add(new ExplorerWorkspaceContribution<MaterialExplorer>(this, "MaterialExplorer", _explorer, _R("MaterialExplorer.Title"), (explorer) => {
                explorer.InitializeMaterialExplorer();
                // initializer
            }));

            return contrib;
        }

        public override bool RequestShutdown() {
            if (!_explorer.RequestClose()) {
                return false;
            }

            return true;
        }

        public override void Dispose() {
            base.Dispose();
            if (_explorer != null && _explorer.Content != null) {

                if (Config.GetGlobal<bool>("Material.RememberExpandedNodes", true)) {
                    List<string> expandedElements = _explorer.GetExpandedParentages(_explorer.RegionsModel);
                    if (expandedElements != null) {
                        Config.SetProfile(User, "Material.Explorer.ExpandedNodes.Region", expandedElements);
                    }
                }

            }
        }

        public override bool CanSelect<T>() {
            var t = typeof(T);
            return typeof(Region).IsAssignableFrom(t) || typeof(Trap).IsAssignableFrom(t) || typeof(BioLink.Data.Model.Material).IsAssignableFrom(t) || typeof(SiteExplorerNode).IsAssignableFrom(t);
        }

        public override void Select<T>(LookupOptions options, Action<SelectionResult> success) {
            var t = typeof(T);
            RegionSelectorContentProvider selectionContent = null;
            if (typeof(Region).IsAssignableFrom(t)) {
                selectionContent = new RegionSelectorContentProvider(User, (n) => { return n.ElemType != "Region"; }, "Region", SiteExplorerNodeType.Region);
            } else if (typeof(Site).IsAssignableFrom(t)) {
                selectionContent = new RegionSelectorContentProvider(User, (n) => { return n.ElemType != "Site"; }, "Site", SiteExplorerNodeType.Site);
            } else if (typeof(Trap).IsAssignableFrom(t)) {
                selectionContent = new RegionSelectorContentProvider(User, (n) => { return n.ElemType == "Material" || n.ElemType == "SiteVisit"; }, "Trap", SiteExplorerNodeType.Trap);
            } else if (typeof(Data.Model.Material).IsAssignableFrom(t)) {
                selectionContent = new RegionSelectorContentProvider(User, (n) => { return false; }, "", SiteExplorerNodeType.Material);
            } else if (typeof(SiteExplorerNode).IsAssignableFrom(t)) {
                selectionContent = new RegionSelectorContentProvider(User, (n) => { 
                    return n.ElemType != "Region" && n.ElemType != "Site" && n.ElemType != "SiteGroup"; 
                }, "", SiteExplorerNodeType.Region, SiteExplorerNodeType.Site);
            } else {                
                throw new Exception("Unhandled Selection Type: " + t.Name);
            }

            if (selectionContent != null) {
                var frm = new HierarchicalSelector(User, selectionContent, success);
                frm.Owner = ParentWindow;
                frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                frm.ShowDialog();
            }
        }

        public override ViewModelBase CreatePinnableViewModel(PinnableObject pinnable) {
            SiteExplorerNodeType nodeType;
            if (Enum.TryParse(pinnable.LookupType.ToString(), out nodeType)) {
                return ViewModelFromObjectID(nodeType, pinnable.ObjectID);
            }
            return null;
        }

        public List<IBioLinkReport> GetReportsForNode(SiteExplorerNodeViewModel node) {
            List<IBioLinkReport> list = new List<IBioLinkReport>();

            switch (node.NodeType) {
                case SiteExplorerNodeType.Trap:
                    list.Add(new MaterialForTrapReport(User, node));
                    break;
                case SiteExplorerNodeType.Site:
                case SiteExplorerNodeType.Region:
                    list.Add(new TaxaForSiteReport(User, node.Model));
                    break;
            }

            return list;
        }


        private SiteExplorerNodeViewModel ViewModelFromObjectID(SiteExplorerNodeType nodeType, int objectID) {
            var service = new MaterialService(User);
            SiteExplorerNode model = new SiteExplorerNode();
            model.ElemType = nodeType.ToString();
            switch (nodeType) {
                case SiteExplorerNodeType.Region:
                    var region = service.GetRegion(objectID);
                    if (region != null) {
                        model.ElemID = region.PoliticalRegionID;                            
                        model.Name = region.Name;
                        model.RegionID = region.PoliticalRegionID;
                        return new SiteExplorerNodeViewModel(model);
                    }
                    break;
                case SiteExplorerNodeType.Site:
                    var site = service.GetSite(objectID);
                    if (site != null) {
                        model.ElemID = site.SiteID;
                        model.Name = site.SiteName;
                        model.RegionID = site.PoliticalRegionID;
                        return new SiteExplorerNodeViewModel(model);
                    }
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    var sitevisit = service.GetSiteVisit(objectID);
                    if (sitevisit != null) {
                        model.ElemID = sitevisit.SiteVisitID;
                        model.Name = sitevisit.SiteName;
                        return new SiteExplorerNodeViewModel(model);
                    }
                    break;
                case SiteExplorerNodeType.Trap:
                    var trap = service.GetTrap(objectID);
                    if (trap != null) {
                        model.ElemID = trap.TrapID;
                        model.Name = trap.TrapName;
                        return new SiteExplorerNodeViewModel(model);
                    }
                    break;
                case SiteExplorerNodeType.Material:                    
                    var material = service.GetMaterial(objectID);
                    if (material != null) {
                        model.ElemID = material.MaterialID;
                        model.Name = material.MaterialName;
                        return new SiteExplorerNodeViewModel(model);
                    }
                    break;
                default:
                    throw new Exception("Unhandled node type: " + nodeType);
            }
            return null;
        }

        public override List<Command> GetCommandsForSelected(List<ViewModelBase> selected) {

            var list = new List<Command>();

            if (selected == null || selected.Count == 0) {
                return list;
            }

            var obj = selected[0];

            if (obj is SiteExplorerNodeViewModel) {
                var node = obj as SiteExplorerNodeViewModel;

                // list.Add(new Command("Show in explorer", (dataobj) => { _explorer.ShowInExplorer(node.ElemID); }));
                // list.Add(new CommandSeparator());
                list.Add(new Command("Edit details...", (dataobj) => { _explorer.EditNode(node); }));
                if (node.NodeType == SiteExplorerNodeType.Site || node.NodeType == SiteExplorerNodeType.SiteVisit || node.NodeType == SiteExplorerNodeType.Material) {
                    list.Add(new Command("Edit in Rapid Data Entry...", (dataobj) => { _explorer.EditRDE(node); }));                    
                }
            }

            return list;
        }

        public void ShowRDE() {            
            _explorer.EditRDE(null);
        }

        public override bool CanEditObjectType(LookupType type) {
            switch (type) {
                case LookupType.Material:
                case LookupType.Region:
                case LookupType.Site:
                case LookupType.SiteVisit:
                case LookupType.Trap:
                    return true;
            }
            return false;
        }

        public override void EditObject(LookupType type, int objectID) {
            SiteExplorerNodeViewModel viewModel = null;
            switch (type) {
                case LookupType.Material:
                    viewModel = ViewModelFromObjectID(SiteExplorerNodeType.Material, objectID);
                    break;
                case LookupType.Region:
                    viewModel = ViewModelFromObjectID(SiteExplorerNodeType.Region, objectID);
                    break;
                case LookupType.Site:
                    viewModel = ViewModelFromObjectID(SiteExplorerNodeType.Site, objectID);
                    break;
                case LookupType.SiteVisit:
                    viewModel = ViewModelFromObjectID(SiteExplorerNodeType.SiteVisit, objectID);
                    break;
                case LookupType.Trap:
                    viewModel = ViewModelFromObjectID(SiteExplorerNodeType.Trap, objectID);
                    break;
            }

            if (viewModel != null) {
                _explorer.EditNode(viewModel);
            }
        }
    }

    internal class RegionSelectorContentProvider : IHierarchicalSelectorContentProvider {

        public RegionSelectorContentProvider(User user, Predicate<SiteExplorerNode> filter, string searchLimit, params SiteExplorerNodeType[] targetTypes) {
            this.User = user;
            this.TargetTypes = targetTypes;
            this.Filter = filter;
            this.SearchLimitation = searchLimit;
        }

        public string Caption {
            get { return "Select Region"; }
        }

        public List<HierarchicalViewModelBase> LoadModel(HierarchicalViewModelBase parent) {
            var list = new List<SiteExplorerNode>();
            var service = new MaterialService(User);
            if (parent == null) {
                list.AddRange(service.GetTopLevelExplorerItems());
            } else {
                var parentElem = parent as SiteExplorerNodeViewModel;
                list.AddRange(service.GetExplorerElementsForParent(parentElem.ElemID, parentElem.NodeType));
            }

            if (Filter != null) {
                list.RemoveAll(Filter);
            }

            list.Sort((item1, item2) => {
                int compare = item1.ElemType.CompareTo(item2.ElemType);
                if (compare == 0) {
                    return item1.Name.CompareTo(item2.Name);
                }
                return compare;
            });

            return list.ConvertAll((model) => {
                var viewModel = (HierarchicalViewModelBase)new SiteExplorerNodeViewModel(model);
                return viewModel;
            });

        }

        public List<HierarchicalViewModelBase> Search(string searchTerm) {
            var service = new MaterialService(User);
            var results = service.FindNodesByName(searchTerm, SearchLimitation);

            if (Filter != null) {
                results = results.Where((n) => !Filter(n)).ToList();
            }

            var converted = results.ConvertAll((node) => {                
                return new SiteExplorerNodeViewModel(node);
            });
            return new List<HierarchicalViewModelBase>(converted);            
        }

        public bool CanSelectItem(HierarchicalViewModelBase candidate) {
            var item = candidate as SiteExplorerNodeViewModel;
            if (item == null) {
                return false;
            }

            foreach (SiteExplorerNodeType nodeType in TargetTypes) {
                if (item.NodeType == nodeType) {
                    return true;
                }
            }

            return false;
        }


        public SelectionResult CreateSelectionResult(HierarchicalViewModelBase selectedItem) {
            var item = selectedItem as SiteExplorerNodeViewModel;
            if (item == null) {
                return null;
            }
            var result = new SelectionResult();

            result.ObjectID = item.ElemID;
            result.Description = item.Name;
            result.DataObject = item.Model;
            result.LookupType = MaterialExplorer.GetLookupTypeFromNodeType(item.NodeType);

            return result;
        }

        #region Properties

        public User User { get; private set; }

        public bool CanAddNewItem {
            get { return true; }
        }

        public bool CanDeleteItem {
            get { return true; }
        }

        public bool CanRenameItem {
            get { return true; }
        }        

        public SiteExplorerNodeType[] TargetTypes { get; private set; }

        public Predicate<SiteExplorerNode> Filter { get; private set; }

        public string SearchLimitation { get; private set; }

        #endregion

        public DatabaseAction AddNewItem(HierarchicalViewModelBase selectedItem) {
            var parent = selectedItem as SiteExplorerNodeViewModel;

            Debug.Assert(parent != null);
            
            var model = new SiteExplorerNode();
            model.ElemID = -1;
            model.ElemType = "Region";
            model.Name = "<New Region>";
            model.ParentID = parent.ElemID;
            model.RegionID = parent.RegionID;

            var viewModel = new SiteExplorerNodeViewModel(model);

            parent.Children.Add(viewModel);

            viewModel.IsSelected = true;
            viewModel.IsRenaming = true;

            return new InsertRegionAction(viewModel.Model, viewModel);
        }

        public DatabaseAction RenameItem(HierarchicalViewModelBase selectedItem, string newName) {
            var item = selectedItem as SiteExplorerNodeViewModel;
            if (item != null) {
                item.Name = newName;
                return new RenameRegionAction(item.Model);
            }
            return null;
        }

        public DatabaseAction DeleteItem(HierarchicalViewModelBase selectedItem) {
            var item = selectedItem as SiteExplorerNodeViewModel;
            if (item != null) {                
                return new DeleteRegionAction(item.ElemID);
            }
            return null;
        }


        public int? GetElementIDForViewModel(HierarchicalViewModelBase item) {
            var viewmodel = item as SiteExplorerNodeViewModel;
            if (viewmodel != null) {
                return viewmodel.ElemID;
            }
            return null;
        }

    }
}
