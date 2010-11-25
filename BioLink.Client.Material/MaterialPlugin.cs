using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class MaterialPlugin : BiolinkPluginBase {

        private MaterialExplorer _explorer;

        public MaterialPlugin() {            
        }

        public override string Name {
            get { return "MATERIAL"; }
        }

        public override List<IWorkspaceContribution> GetContributions() {
            
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

            contrib.Add(new MenuWorkspaceContribution(this, "ShowExplorer", (obj, e) => { PluginManager.EnsureVisible(this, "MaterialExplorer"); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("MATERIAL.Menu.View")),
                String.Format("{{'Name':'ShowMaterialExplorer', 'Header':'{0}'}}", _R("MATERIAL.Menu.ShowExplorer"))
            ));

            _explorer = new MaterialExplorer(this);
            contrib.Add(new ExplorerWorkspaceContribution<MaterialExplorer>(this, "MaterialExplorer", _explorer, _R("MaterialExplorer.Title"), (explorer) => {
                explorer.InitializeMaterialExplorer();
                // initializer
            }));

            return contrib;
        }

        public override bool RequestShutdown() {
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

        public override bool CanSelect(Type t) {
            return typeof(Region).IsAssignableFrom(t);
        }

        public override void Select(Type t, Action<SelectionResult> success) {
            if (typeof(Region).IsAssignableFrom(t)) {
                var selectionContent = new RegionSelectorContentProvider(User);
                var frm = new HierarchicalSelector(User, selectionContent, success);
                frm.Owner = ParentWindow;
                frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                frm.ShowDialog();
            } else {
                throw new Exception("Unhandled Selection Type: " + t.Name);
            }

        }

        public override List<Command> GetCommandsForObject(ViewModelBase obj) {
            return null;
        }
    }

    internal class RegionSelectorContentProvider : IHierarchicalSelectorContentProvider {

        public RegionSelectorContentProvider(User user) {
            this.User = user;
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

            list.RemoveAll((item) => {
                return item.ElemType != "Region";
            });

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
            var results = new MaterialService(User).FindRegions(searchTerm);
            var converted = results.ConvertAll((sr) => {
                var model = new SiteExplorerNode();
                model.ElemType = "Region";
                model.ElemID = sr.RegionID;
                model.ParentID = sr.ParentID;
                model.Name = sr.Region;
                model.NumChildren = sr.NumChildren;
                return new SiteExplorerNodeViewModel(model);
            });
            return new List<HierarchicalViewModelBase>(converted);            
        }

        public void OnSelected(SelectionResult result) {
            throw new NotImplementedException();
        }

        public SelectionResult CreateSelectionResult(HierarchicalViewModelBase selectedItem) {
            var item = selectedItem as SiteExplorerNodeViewModel;
            if (item == null) {
                return null;
            }
            var result = new SelectionResult();

            result.ObjectID = item.ElemID;
            result.Description = item.Name;
            result.DataObject = item;

            return result;
        }

        #region Properties

        public User User { get; private set; }

        #endregion

    }
}
