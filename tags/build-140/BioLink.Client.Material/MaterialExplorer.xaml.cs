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

        private bool _IsDragging;
        private Point _startPoint;

        #region DROP MAP

        private Dictionary<string, Action<SiteExplorerNodeViewModel, SiteExplorerNodeViewModel>> _DropMap = new Dictionary<string, Action<SiteExplorerNodeViewModel, SiteExplorerNodeViewModel>>();

        private void AddToMap(SiteExplorerNodeType sourceType, SiteExplorerNodeType destType, Action<SiteExplorerNodeViewModel, SiteExplorerNodeViewModel> action) {            
            _DropMap[MakeDropMapKey(sourceType, destType)] = action;
        }

        private string MakeDropMapKey(SiteExplorerNodeViewModel source, SiteExplorerNodeViewModel dest) {
            return MakeDropMapKey(source.NodeType, dest.NodeType);
        }

        private string MakeDropMapKey(SiteExplorerNodeType sourceType, SiteExplorerNodeType destType) {
            return string.Format("{0}_{1}", sourceType.ToString(), destType.ToString());
        }

        private void BuildDropMap() {
            AddToMap(SiteExplorerNodeType.Trap, SiteExplorerNodeType.Trap, MergeNodes);
            AddToMap(SiteExplorerNodeType.SiteVisit, SiteExplorerNodeType.SiteVisit, MergeNodes);
            AddToMap(SiteExplorerNodeType.Material, SiteExplorerNodeType.Material, MergeNodes);

            AddToMap(SiteExplorerNodeType.SiteGroup, SiteExplorerNodeType.SiteGroup, AskMoveMergeNode);
            AddToMap(SiteExplorerNodeType.Region, SiteExplorerNodeType.Region, AskMoveMergeNode);

            AddToMap(SiteExplorerNodeType.SiteGroup, SiteExplorerNodeType.Region, MoveNode);
        }

        #endregion

        #region Designer constructor

        public MaterialExplorer() {
            InitializeComponent();
        }

        #endregion

        public MaterialExplorer(MaterialPlugin owner)
            : base(owner.User, "MaterialExplorer") {

            InitializeComponent();

            BuildDropMap();

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

            var findScopes = new List<MaterialFindScope>();
            findScopes.Add(new MaterialFindScope("Find in all", ""));
            findScopes.Add(new MaterialFindScope("Site name", "site"));
            findScopes.Add(new MaterialFindScope("Trap name", "trap"));
            findScopes.Add(new MaterialFindScope("Visit name", "visit"));
            findScopes.Add(new MaterialFindScope("Material name", "material"));
            findScopes.Add(new MaterialFindScope("Accession No.", "accessionno"));
            findScopes.Add(new MaterialFindScope("Registraton No.", "regno"));            
            findScopes.Add(new MaterialFindScope("Region name", "region"));
            findScopes.Add(new MaterialFindScope("Site group name", "group"));
            findScopes.Add(new MaterialFindScope("Collector #", "collector"));

            cmbFindScope.ItemsSource = findScopes;
            cmbFindScope.DisplayMemberPath = "Label";

            int lastSelectedIndex = Config.GetUser(User, "Material.Find.LastFilter", -1);
            if (lastSelectedIndex < 0 || lastSelectedIndex >= findScopes.Count) {
                cmbFindScope.SelectedIndex = 0;
            } else {
                cmbFindScope.SelectedIndex = lastSelectedIndex;
            }

            tvwFind.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(tvwFind_PreviewMouseLeftButtonDown);
            tvwFind.PreviewMouseMove +=new MouseEventHandler(tvwFind_PreviewMouseMove);

            tvwMaterial.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(tvwMaterial_PreviewMouseLeftButtonDown);
            tvwMaterial.PreviewMouseMove += new MouseEventHandler(tvwMaterial_PreviewMouseMove);

            tvwMaterial.PreviewDragOver += new DragEventHandler(tvwMaterial_PreviewDragOver);
            tvwMaterial.PreviewDragEnter += new DragEventHandler(tvwMaterial_PreviewDragOver);


            this.Drop += new DragEventHandler(MaterialExplorer_Drop);

            tvwMaterial.AllowDrop = true;


        }

        void MaterialExplorer_Drop(object sender, DragEventArgs e) {

            var dest = tvwMaterial.SelectedItem as SiteExplorerNodeViewModel;
            var source = e.Data.GetData("SiteExplorerNodeViewModel") as SiteExplorerNodeViewModel;

            e.Effects = DragDropEffects.None;

            if (dest != null && source != null) {
                string key = MakeDropMapKey(source, dest);
                if (_DropMap.ContainsKey(key)) {
                    var action = _DropMap[key];
                    action(source, dest);
                }
            }

            e.Handled = true;
            
        }

        private void AskMoveMergeNode(SiteExplorerNodeViewModel source, SiteExplorerNodeViewModel dest) {
            // Regions and site groups can either be merged or moved. Need to ask...
            var frm = new DragDropOptions(this.FindParentWindow());
            if (frm.ShowDialog().GetValueOrDefault(false)) {
                if (frm.DragDropOption == DragDropOption.Merge) {
                    MergeNodes(source, dest);
                } else {
                    MoveNode(source, dest);
                }
            }
        }

        private void MoveNode(SiteExplorerNodeViewModel source, SiteExplorerNodeViewModel dest) {
            DatabaseAction moveAction = null;
            switch (source.NodeType) {
                case SiteExplorerNodeType.SiteGroup:
                    moveAction = new MoveSiteGroupAction(source.Model, dest.Model);
                    break;
                case SiteExplorerNodeType.Region:
                    
                    break;
            }

            if (moveAction != null) {
                source.Parent.Children.Remove(source);
                dest.IsChanged = true;
                dest.IsExpanded = true;
                dest.Children.Add(source);
                RegisterPendingChange(moveAction);
            }

        }

        private void MergeNodes(SiteExplorerNodeViewModel oldNode, SiteExplorerNodeViewModel newNode) {
            DatabaseAction mergeAction = null;
            switch (oldNode.NodeType) {
                case SiteExplorerNodeType.SiteGroup:
                    mergeAction = new MergeSiteGroupAction(oldNode.Model, newNode.Model);
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    mergeAction = new MergeSiteVisitAction(oldNode.Model, newNode.Model);
                    break;
                case SiteExplorerNodeType.Material:
                    mergeAction = new MergeMaterialAction(oldNode.Model, newNode.Model);
                    break;
                case SiteExplorerNodeType.Trap:
                    mergeAction = new MergeTrapAction(oldNode.Model, newNode.Model);
                    break;
            }

            if (mergeAction != null) {
                if (this.Question(string.Format("Are you sure you want to merge '{0}' with '{1}'?", oldNode.DisplayLabel, newNode.DisplayLabel), "Merge " + oldNode.NodeType)) {
                    oldNode.IsDeleted = true;
                    newNode.IsChanged = true;
                    RegisterPendingChange(mergeAction);
                }
            }
        }

        private SiteExplorerNodeViewModel GetHoveredTreeViewItem(DragEventArgs e, TreeView tvw) {
            DependencyObject elem = tvw.InputHitTest(e.GetPosition(tvw)) as DependencyObject;
            while (elem != null && !(elem is TreeViewItem)) {
                elem = VisualTreeHelper.GetParent(elem);
            }
            var treeItem = elem as TreeViewItem;            
            if (treeItem != null) {
                treeItem.Focus();
                return treeItem.DataContext as SiteExplorerNodeViewModel;
            }
            return null;            
        }

        void tvwMaterial_PreviewDragOver(object sender, DragEventArgs e) {

            var dest = GetHoveredTreeViewItem(e, tvwMaterial);
            var source = e.Data.GetData("SiteExplorerNodeViewModel") as SiteExplorerNodeViewModel;

            e.Effects = DragDropEffects.None;

            if (dest != null && source != null) {
                string key = MakeDropMapKey(source, dest);
                if (_DropMap.ContainsKey(key)) {
                    e.Effects = DragDropEffects.Move;
                }
            }

            e.Handled = true;
            
        }

        void tvwMaterial_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, tvwMaterial);
        }

        void tvwMaterial_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(tvwMaterial);
        }

        void tvwFind_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(tvwFind);
        }

        void tvwFind_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, tvwFind);
        }


        private void CommonPreviewMouseMove(MouseEventArgs e, TreeView treeView) {

            if (_startPoint == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(treeView);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (treeView.SelectedItem != null) {
                        IInputElement hitelement = treeView.InputHitTest(_startPoint);
                        TreeViewItem item = treeView.GetTreeViewItemClicked((FrameworkElement)hitelement);                        
                        if (item != null) {
                            StartDrag(e, treeView, item);
                        }
                    }
                }
            }
        }

        private void StartDrag(MouseEventArgs mouseEventArgs, TreeView treeView, TreeViewItem item) {

            var selected = treeView.SelectedItem as SiteExplorerNodeViewModel;
            if (selected != null) {
                var data = new DataObject("Pinnable", selected);
                var pinnable = CreatePinnable(selected);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);
                data.SetData("SiteExplorerNodeViewModel", selected);


                try {
                    _IsDragging = true;
                    DragDrop.DoDragDrop(item, data, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                } finally {
                    _IsDragging = false;
                }
            }

            InvalidateVisual();
        }


        private void cmbFindScope_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Config.SetUser(User, "Material.Find.LastFilter", cmbFindScope.SelectedIndex);
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

            // Region explorer...
            var list = service.GetTopLevelExplorerItems();
            RegionsModel = BuildExplorerModel(list);
            regionsNode.ItemsSource = RegionsModel;
            regionsNode.IsExpanded = true;

            // Unplaced sites (Sites with no region)...
            list = service.GetTopLevelExplorerItems(SiteExplorerNodeType.Unplaced);
            UnplacedModel = BuildExplorerModel(list);
            unplacedNode.ItemsSource = UnplacedModel;            
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

        private ObservableCollection<HierarchicalViewModelBase> BuildExplorerModel(List<SiteExplorerNode> list) {

            list.Sort((item1, item2) => {
                int compare = item1.ElemType.CompareTo(item2.ElemType);
                if (compare == 0) {
                    return item1.Name.CompareTo(item2.Name);
                }
                return compare;
            });


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
                var viewModel = BuildExplorerModel(list);
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

        private void treeview_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowContextMenu(sender as TreeView);
        }

        private void ShowContextMenu(TreeView tvw) {
            var selected = tvw.SelectedItem as SiteExplorerNodeViewModel;
            if (selected != null) {
                var menu  = SiteExplorerMenuBuilder.Build(selected, this);
                if (menu != null) {
                    tvw.ContextMenu = menu;
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
            AddNewNode(parent, SiteExplorerNodeType.SiteGroup, (viewModel) => { return new InsertSiteGroupAction(viewModel); });
        }

        internal void AddSite(SiteExplorerNodeViewModel parent) {
            AddNewNode(parent, SiteExplorerNodeType.Site, (viewModel) => { return new InsertSiteAction(viewModel); });
        }

        internal void AddSiteVisit(SiteExplorerNodeViewModel parent) {
            AddNewNode(parent, SiteExplorerNodeType.SiteVisit, (viewModel) => { return new InsertSiteVisitAction(viewModel); });
        }

        internal void AddTrap(SiteExplorerNodeViewModel parent) {
            AddNewNode(parent, SiteExplorerNodeType.Trap, (viewModel) => { return new InsertTrapAction(viewModel); });
        }

        internal void AddMaterial(SiteExplorerNodeViewModel parent) {
            AddNewNode(parent, SiteExplorerNodeType.Material, (viewModel) => { return new InsertMaterialAction(viewModel); });
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

        internal void EditSiteVisit(SiteExplorerNodeViewModel sitevisit) {
            EditNode(sitevisit, () => { return new SiteVisitDetails(User, sitevisit.ElemID); });
        }

        internal void EditMaterial(SiteExplorerNodeViewModel material) {
            EditNode(material, () => { return new MaterialDetails(User, material.ElemID); });
        }

        internal void EditTrap(SiteExplorerNodeViewModel trap) {
            EditNode(trap, () => { return new TrapDetails(User, trap.ElemID); });
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

        internal void DeleteSite(SiteExplorerNodeViewModel group) {
            DeleteNode(group, () => { return new DeleteSiteAction(group.ElemID); });
        }

        internal void DeleteSiteVisit(SiteExplorerNodeViewModel group) {
            DeleteNode(group, () => { return new DeleteSiteVisitAction(group.ElemID); });
        }

        internal void DeleteTrap(SiteExplorerNodeViewModel trap) {
            DeleteNode(trap, () => { return new DeleteTrapAction(trap.ElemID); });
        }

        internal void DeleteMaterial(SiteExplorerNodeViewModel material) {
            DeleteNode(material, () => { return new DeleteMaterialAction(material.ElemID); });
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

        private LookupType GetLookupTypeFromNodeType(SiteExplorerNodeType nodeType) {
            switch (nodeType) {
                case SiteExplorerNodeType.Material:
                    return LookupType.Material;
                case SiteExplorerNodeType.Region:
                    return LookupType.Region;
                case SiteExplorerNodeType.Site:
                    return LookupType.Site;
                case SiteExplorerNodeType.SiteVisit:
                    return LookupType.SiteVisit;
                case SiteExplorerNodeType.Trap:
                    return LookupType.Trap;
                default:
                    return LookupType.Unknown;
            }
        }

        #region Properties

        public MaterialPlugin Owner { get; private set; }

        public ObservableCollection<HierarchicalViewModelBase> RegionsModel { get; private set; }

        public ObservableCollection<HierarchicalViewModelBase> UnplacedModel { get; private set; }

        internal bool RememberExpanded { get; private set; }

        #endregion

        internal PinnableObject CreatePinnable(SiteExplorerNodeViewModel node) {
            return new PinnableObject(MaterialPlugin.MATERIAL_PLUGIN_NAME, GetLookupTypeFromNodeType(node.NodeType), node.ElemID);
        }

        private void btnFind_Click(object sender, RoutedEventArgs e) {
            DoFind();
        }

        private void DoFind() {
            var service = new MaterialService(User);
            var scope = cmbFindScope.SelectedItem as MaterialFindScope;
            string limitations = "";
            if (scope != null) {
                limitations = scope.Value;
            }
            var list = service.FindNodesByName(txtFind.Text, limitations);
            var findModel = BuildExplorerModel(list);
            tvwFind.ItemsSource = findModel;
        }

        private void txtFind_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                DoFind();
                e.Handled = true;
            }
        }

    }

    internal class MaterialFindScope {

        public MaterialFindScope(string label, string value) {
            this.Label = label;
            this.Value = value;
        }

        public String Label { get; private set; }
        public String Value { get; private set; }
    }

}
