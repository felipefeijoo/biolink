using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;
using BioLink.Client.Utilities;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MaterialExplorer : DatabaseActionControl {

        #region Designer constructor

        public MaterialExplorer() {
            InitializeComponent();
        }

        #endregion

        public MaterialExplorer(MaterialPlugin owner)
            : base(owner.User, "MaterialExplorer") {

            InitializeComponent();

            this.ChangeRegistered += new Action<IList<DatabaseAction>>((list) => {
                btnApply.IsEnabled = true;
                btnCancel.IsEnabled = true;
            });

            this.ChangesCommitted += new PendingChangesCommittedHandler((list) => {
                ReloadModel();
                btnApply.IsEnabled = false;
                btnCancel.IsEnabled = false;
            });

            this.Owner = owner;

            this.RememberExpanded = Config.GetGlobal<bool>("Material.RememberExpandedNodes", true);
        }

        public void InitializeMaterialExplorer() {
            this.InvokeIfRequired(() => {
                LoadExplorerModel();
                if (RegionsModel != null && RememberExpanded) {
                    // Now see if we can auto-expand from the last session...                    
                    var expanded = Config.GetProfile<List<String>>(Owner.User, "Material.Explorer.ExpandedNodes.Region", null);
                    ExpandParentages(RegionsModel, expanded);                    
                }
                
            });
        }

        private void LoadExplorerModel() {
            var service = new MaterialService(User);
            var list = service.GetTopLevelExplorerItems();
            list.Sort((item1, item2) => {
                int compare = item1.ElemType.CompareTo(item2.ElemType);
                if (compare == 0) {
                    return item1.Name.CompareTo(item2.Name);
                }
                return compare;
            });
            RegionsModel = BuildRegionsModel(list);
            regionsNode.ItemsSource = RegionsModel;
            regionsNode.IsExpanded = true;
        }

        private void ReloadModel() {

            List<String> expanded = null;
            if (RememberExpanded) {
                expanded = GetExpandedParentages(RegionsModel);
            }

            LoadExplorerModel();

            if (expanded != null && expanded.Count > 0) {
                ExpandParentages(RegionsModel, expanded);
            }

            ClearPendingChanges();
        }

        private ObservableCollection<HierarchicalViewModelBase> BuildRegionsModel(List<SiteExplorerNode> list) {
            var regionsModel = new ObservableCollection<HierarchicalViewModelBase>(list.ConvertAll((model) => {
                var viewModel = new SiteExplorerNodeViewModel(model);

                if (model.NumChildren > 0) {
                    viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                    viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                }
                return viewModel;
            }));
            return regionsModel;
        }

        void viewModel_LazyLoadChildren(HierarchicalViewModelBase item) {
            var parent = item as SiteExplorerNodeViewModel;
            if (parent != null) {
                parent.Children.Clear();
                var service = new MaterialService(User);
                var list = service.GetExplorerElementsForParent(parent.ElemID, parent.ElemType);
                var viewModel = BuildRegionsModel(list);
                foreach (HierarchicalViewModelBase childViewModel in viewModel) {
                    childViewModel.Parent = parent;
                    parent.Children.Add(childViewModel);
                }                
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e) {
        }

        private void EditableTextBlock_EditingComplete(object sender, string text) {
            var selected = tvwMaterial.SelectedItem as SiteExplorerNodeViewModel;
            if (selected != null) {
                selected.Name = text;
                switch (selected.NodeType) {
                    case SiteExplorerNodeType.Region:
                        RegisterPendingChange(new RenameRegionAction(selected));
                        break;
                    case SiteExplorerNodeType.SiteGroup:
                        RegisterPendingChange(new RenameSiteGroupAction(selected));
                        break;
                    case SiteExplorerNodeType.Site:
                        RegisterPendingChange(new RenameSiteAction(selected));
                        break;
                    case SiteExplorerNodeType.SiteVisit:
                        RegisterPendingChange(new RenameSiteVisitAction(selected));
                        break;                
                    case SiteExplorerNodeType.Trap:
                        RegisterPendingChange(new RenameTrapAction(selected));
                        break;
                    case SiteExplorerNodeType.Material:
                        RegisterPendingChange(new RenameMaterialAction(selected));
                        break;
                    default:
                        throw new NotImplementedException(selected.NodeType.ToString());
                }                
            }
        }

        private void EditableTextBlock_EditingCancelled(object sender, string oldtext) {
        }

        private void tvwMaterial_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowContextMenu();
        }

        private void ShowContextMenu() {
            var selected = tvwMaterial.SelectedItem as SiteExplorerNodeViewModel;
            if (selected != null) {
                var menu  = SiteExplorerMenuBuilder.Build(selected, this);
                if (menu != null) {
                    tvwMaterial.ContextMenu = menu;
                }
            }
        }

        public List<string> GetExpandedParentages(ObservableCollection<HierarchicalViewModelBase> model) {
            List<string> list = new List<string>();
            CollectExpandedParentages(model, list);
            return list;
        }

        private void CollectExpandedParentages(ObservableCollection<HierarchicalViewModelBase> model, List<string> list) {
            foreach (SiteExplorerNodeViewModel tvm in model) {
                if (tvm.IsExpanded) {
                    list.Add(tvm.GetParentage());
                    if (tvm.Children != null && tvm.Children.Count > 0) {
                        CollectExpandedParentages(tvm.Children, list);
                    }
                }
            }
        }

        public void ExpandParentages(ObservableCollection<HierarchicalViewModelBase> model, List<string> expanded) {
            if (expanded != null && expanded.Count > 0) {
                var todo = new Stack<HierarchicalViewModelBase>(model);
                while (todo.Count > 0) {
                    var vm = todo.Pop();
                    if (vm is SiteExplorerNodeViewModel) {
                        var tvm = vm as SiteExplorerNodeViewModel;
                        string parentage = tvm.GetParentage();
                        if (expanded.Contains(parentage)) {
                            tvm.IsExpanded = true;
                            expanded.Remove(parentage);
                            tvm.Children.ForEach(child => todo.Push(child));
                        }
                    }
                }
            }
        }

        internal SiteExplorerNodeViewModel AddNewNode(SiteExplorerNodeViewModel parent, SiteExplorerNodeType nodeType, Func<SiteExplorerNodeViewModel, DatabaseAction> actionFactory) {

            parent.IsExpanded = true;

            var model = new SiteExplorerNode();
            model.Name = string.Format("<New {0}>", nodeType.ToString());
            model.ParentID = parent.ElemID;
            model.ElemType = nodeType.ToString();
            model.ElemID = -1;
            model.RegionID = -1;

            var viewModel = new SiteExplorerNodeViewModel(model);
            viewModel.Parent = parent;
            parent.Children.Add(viewModel);
            viewModel.IsSelected = true;
            viewModel.IsRenaming = true;

            if (actionFactory != null) {
                RegisterPendingChange(actionFactory(viewModel));
            }

            return viewModel;
        }

        internal void AddRegion(SiteExplorerNodeViewModel parent) {
            AddNewNode(parent, SiteExplorerNodeType.Region, (viewModel) => { return new InsertRegionAction(viewModel); });
        }

        internal void AddSiteGroup(SiteExplorerNodeViewModel parent) {
            AddNewNode(parent, SiteExplorerNodeType.SiteGroup, (viewModel) => { return new InsertSiteGroupAction(viewModel, parent); });
        }

        private void EditNode(SiteExplorerNodeViewModel node, Func<DatabaseActionControl> editorFactory) {
            if (node.ElemID < 0) {
                ErrorMessage.Show("You must first apply the changes before editing the details of this item!");
                return;
            } else {
                var editor = editorFactory();                
                var caption = string.Format("{0} Detail {1} [{2}]", node.NodeType.ToString(), node.Name, node.ElemID);
                PluginManager.Instance.AddNonDockableContent(Owner, editor, caption, SizeToContent.Manual);
            }
        }

        public void EditNode(SiteExplorerNodeViewModel node) {
            switch (node.NodeType) {
                case SiteExplorerNodeType.Site:
                    EditSite(node);
                    break;
                case SiteExplorerNodeType.Region:
                    EditRegion(node);
                    break;
                case SiteExplorerNodeType.SiteGroup:
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    EditSiteVisit(node);
                    break;
                case SiteExplorerNodeType.Material:
                    EditMaterial(node);
                    break;
                case SiteExplorerNodeType.Trap:
                    EditTrap(node);
                    break;
            }
        }

        internal void EditSiteVisit(SiteExplorerNodeViewModel region) {
            throw new NotImplementedException();
        }

        internal void EditMaterial(SiteExplorerNodeViewModel region) {
            throw new NotImplementedException();
        }

        internal void EditTrap(SiteExplorerNodeViewModel region) {
            throw new NotImplementedException();
        }


        internal void EditRegion(SiteExplorerNodeViewModel region) {
            EditNode(region, () => { return new RegionDetails(User, region.ElemID ); });
        }

        internal void EditSite(SiteExplorerNodeViewModel site) {
            EditNode(site, () => { return new SiteDetails(User, site.ElemID); });
        }

        internal void DeleteNode(SiteExplorerNodeViewModel node, Func<DatabaseAction> actionFactory) {

            if (!node.IsDeleted) {
                node.Traverse((child) => {
                    child.IsDeleted = true;
                });

                if (actionFactory != null) {
                    RegisterPendingChange(actionFactory());
                }
            }
        }

        internal void DeleteRegion(SiteExplorerNodeViewModel region) {
            DeleteNode(region, () => { return new DeleteRegionAction(region.ElemID); });
        }

        internal void DeleteSiteGroup(SiteExplorerNodeViewModel group) {
            DeleteNode(group, () => { return new DeleteSiteGroupAction(group.ElemID); });
        }

        private void tvwMaterial_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null) {
                item.Focus();
                e.Handled = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            ReloadModel();            
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

        private void ApplyChanges() {
            CommitPendingChanges();
        }

        private Action<SelectionResult> _selectionCallback;

        public void BindSelectCallback(Action<SelectionResult> selectionFunc) {
            if (selectionFunc != null) {
                btnSelect.Visibility = Visibility.Visible;
                btnSelect.IsEnabled = true;
                _selectionCallback = selectionFunc;
            } else {
                ClearSelectCallback();
            }

        }

        public void ClearSelectCallback() {
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e) {
            var selected = tvwMaterial.SelectedItem as SiteExplorerNodeViewModel;
            if (selected != null && _selectionCallback != null) {
                var result = new SelectionResult();
                result.ObjectID = selected.ElemID;
                result.Description = selected.Name;
                _selectionCallback(result);
            }
        }


        internal void Refresh() {
            if (HasPendingChanges) {            
                if (this.Question("You have unsaved changes. Refreshing will cause those changes to be discarded. Are you sure you want to discard unsaved changes?", "Discard unsaved changes?")) {
                    ReloadModel();
                }
            } else {
                ReloadModel();
            }            
        }

        #region Properties

        public MaterialPlugin Owner { get; private set; }

        public ObservableCollection<HierarchicalViewModelBase> RegionsModel { get; private set; }

        internal bool RememberExpanded { get; private set; }

        #endregion

        internal PinnableObject CreatePinnable(SiteExplorerNodeViewModel node) {
            return new PinnableObject(MaterialPlugin.MATERIAL_PLUGIN_NAME, string.Format("{0}:{1}", node.NodeType.ToString(), node.ElemID));            
        }
    }

}
