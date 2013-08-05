/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governingm
 * rights and limitations under the License.
 ******************************************************************************/
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

    public partial class MaterialExplorer : ChangeContainerControl {

        private MaterialFavorites _favorites;

        private ViewModelPlaceholder _siteTemplatesRoot;
        private ViewModelPlaceholder _siteVisitTemplatesRoot;
        private ViewModelPlaceholder _materialTemplatesRoot;

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
            AddToMap(SiteExplorerNodeType.Region, SiteExplorerNodeType.Region, AskMoveMergeNode);
            AddToMap(SiteExplorerNodeType.SiteGroup, SiteExplorerNodeType.SiteGroup, AskMoveMergeNode);
            AddToMap(SiteExplorerNodeType.Site, SiteExplorerNodeType.Site, MergeSites);

            AddToMap(SiteExplorerNodeType.Trap, SiteExplorerNodeType.Trap, MergeNodes);
            AddToMap(SiteExplorerNodeType.SiteVisit, SiteExplorerNodeType.SiteVisit, MergeNodes);
            AddToMap(SiteExplorerNodeType.Material, SiteExplorerNodeType.Material, MergeNodes);


            AddToMap(SiteExplorerNodeType.Site, SiteExplorerNodeType.SiteGroup, MoveNode);
            AddToMap(SiteExplorerNodeType.Site, SiteExplorerNodeType.Region, MoveNode);

            AddToMap(SiteExplorerNodeType.Trap, SiteExplorerNodeType.Site, MoveNode);

            AddToMap(SiteExplorerNodeType.SiteGroup, SiteExplorerNodeType.Region, MoveNode);

            AddToMap(SiteExplorerNodeType.SiteVisit, SiteExplorerNodeType.Site, MoveNode);

            AddToMap(SiteExplorerNodeType.Material, SiteExplorerNodeType.SiteVisit, MoveNode);
        }

        #endregion

        #region Designer constructor

        public MaterialExplorer() {
            InitializeComponent();
        }

        #endregion

        public MaterialExplorer(MaterialPlugin owner)
            : base(owner.User) {

            InitializeComponent();

            BuildDropMap();

            this.ChangeRegistered += new PendingChangedRegisteredHandler((s, action) => {
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
            cmbFindScope.PreviewKeyDown += new KeyEventHandler(cmbFindScope_PreviewKeyDown);

            int lastSelectedIndex = Config.GetUser(User, "Material.Find.LastFilter", -1);
            if (lastSelectedIndex < 0 || lastSelectedIndex >= findScopes.Count) {
                cmbFindScope.SelectedIndex = 0;
            } else {
                cmbFindScope.SelectedIndex = lastSelectedIndex;
            }

            TreeViewDragHelper.Bind(tvwFind, CreatePinnableDragObject);
            TreeViewDragHelper.Bind(tvwMaterial, CreatePinnableDragObject);

            tvwMaterial.PreviewDragOver += new DragEventHandler(tvwMaterial_PreviewDragOver);
            tvwMaterial.PreviewDragEnter += new DragEventHandler(tvwMaterial_PreviewDragOver);

            tvwFind.PreviewDragOver += new DragEventHandler(tvwFind_PreviewDragOver);
            tvwFind.PreviewDragEnter += new DragEventHandler(tvwFind_PreviewDragOver);

            this.Drop += new DragEventHandler(MaterialExplorer_Drop);

            var service = new MaterialService(User);

            tvwMaterial.AllowDrop = true;
            _favorites = new MaterialFavorites(User, this);
            tabMaterial.AddTabItem("Favorites", _favorites);

            txtFind.PreviewKeyDown += new KeyEventHandler(txtFind_PreviewKeyDown);
        }

        void cmbFindScope_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DoFind();
                e.Handled = true;
            }
        }

        void tvwFind_PreviewDragOver(object sender, DragEventArgs e) {
            var dest = GetHoveredTreeViewItem(e, tvwFind);
            var source = e.Data.GetData("SiteExplorerNodeViewModel") as SiteExplorerNodeViewModel;

            e.Effects = DragDropEffects.None;

            if (dest != null && source != null && dest != source) {
                string key = MakeDropMapKey(source, dest);
                if (_DropMap.ContainsKey(key)) {
                    e.Effects = DragDropEffects.Move;
                }
            }

            e.Handled = true;
        }

        void txtFind_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                tvwFind.Focus();                
                e.Handled = true;
            }

            if (e.Key == Key.Enter) {
                DoFind();
                e.Handled = true;
            }

        }

        private ViewModelPlaceholder CreateTemplateNode(string label, string imagePath, Func<List<SiteExplorerNodeViewModel>> getTemplates) {
            var templateRoot = new ViewModelPlaceholder(label, imagePath);            
            var model = templatesNode.ItemsSource as ObservableCollection<ViewModelPlaceholder>;
            if (model == null) {
                model = new ObservableCollection<ViewModelPlaceholder>();
                templatesNode.ItemsSource = model;
            }

            model.Add(templateRoot);
            templateRoot.Children.Add(new ViewModelPlaceholder("Loading..."));
            
            templateRoot.LazyLoadChildren += new HierarchicalViewModelAction((parent) => {
                using (new OverrideCursor(Cursors.Wait)) {
                    parent.Children.Clear();
                    foreach (SiteExplorerNodeViewModel child in getTemplates()) {
                        parent.Children.Add(child);
                    }
                }
            });
            return templateRoot;
        }

        void MaterialExplorer_Drop(object sender, DragEventArgs e) {

            var tvw = GetCurrentTree();

            if (tvw == null) {
                return;
            }

            var dest = tvw.SelectedItem as SiteExplorerNodeViewModel;
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
            DatabaseCommand moveAction = null;
            if (source.Parent == dest) {
                ErrorMessage.Show("'{0}' is already a child of '{1}'.", source.Name, dest.Name);
                return;
            }

            switch (source.NodeType) {
                case SiteExplorerNodeType.Site:
                    moveAction = new MoveSiteCommand(source.Model, dest.Model);
                    break;
                case SiteExplorerNodeType.SiteGroup:
                    moveAction = new MoveSiteGroupCommand(source.Model, dest.Model);
                    break;
                case SiteExplorerNodeType.Region:
                    moveAction = new MoveRegionCommand(source.Model, dest.Model);
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    moveAction = new MoveSiteVisitCommand(source.Model, dest.Model);
                    break;
                case SiteExplorerNodeType.Material:
                    moveAction = new MoveMaterialCommand(source.Model, dest.Model);
                    break;
                case SiteExplorerNodeType.Trap:
                    moveAction = new MoveTrapCommand(source.Model, dest.Model);
                    break;
            }

            if (moveAction != null) {
                if (source.Parent != null) {
                    source.Parent.Children.Remove(source);
                }
                dest.IsChanged = true;
                dest.IsExpanded = true;
                dest.Children.Add(source);
                RegisterPendingChange(moveAction, this);
            }

        }

        private void MergeSites(SiteExplorerNodeViewModel oldNode, SiteExplorerNodeViewModel newNode) {
            var frm = new MergeSiteOptions(this.FindParentWindow(), User, oldNode, newNode);
            if (frm.ShowDialog().GetValueOrDefault(false)) {
                newNode.IsChanged = true;
                foreach (SiteExplorerNodeViewModel other in frm.SelectedNodes) {
                    other.IsDeleted = true;
                    RegisterPendingChange(new MergeSiteCommand(other.Model, newNode.Model), this);
                }
            }
        }

        private void MergeNodes(SiteExplorerNodeViewModel oldNode, SiteExplorerNodeViewModel newNode) {
            DatabaseCommand mergeAction = null;
            switch (oldNode.NodeType) {
                case SiteExplorerNodeType.SiteGroup:
                    mergeAction = new MergeSiteGroupCommand(oldNode.Model, newNode.Model);
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    mergeAction = new MergeSiteVisitCommand(oldNode.Model, newNode.Model);
                    break;
                case SiteExplorerNodeType.Material:
                    mergeAction = new MergeMaterialCommand(oldNode.Model, newNode.Model);
                    break;
                case SiteExplorerNodeType.Trap:
                    mergeAction = new MergeTrapCommand(oldNode.Model, newNode.Model);
                    break;
                case SiteExplorerNodeType.Region:
                    mergeAction = new MergeRegionCommand(oldNode.Model, newNode.Model);
                    break;
            }

            if (mergeAction != null) {
                if (this.Question(string.Format("Are you sure you want to merge '{0}' with '{1}'?", oldNode.DisplayLabel, newNode.DisplayLabel), "Merge " + oldNode.NodeType)) {
                    oldNode.IsDeleted = true;
                    newNode.IsChanged = true;
                    RegisterPendingChange(mergeAction, this);
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

            if (dest != null && source != null && dest != source) {
                string key = MakeDropMapKey(source, dest);
                if (_DropMap.ContainsKey(key)) {
                    e.Effects = DragDropEffects.Move;
                }
            }

            e.Handled = true;

        }

        private DataObject CreatePinnableDragObject(ViewModelBase selected) {
            var node = selected as SiteExplorerNodeViewModel;
            if (node != null) {
                var data = new DataObject("Pinnable", node);
                var pinnable = CreatePinnable(node);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);
                data.SetData("SiteExplorerNodeViewModel", selected);

                return data;
            }

            return null;
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

        public void ShowInContents(SiteExplorerNodeViewModel node) {

            if (node == null) {
                return;
            }

            var parentage = new MaterialService(User).GetSiteExplorerNodeParentage(node.ElemID, node.NodeType);
            var childlist = RegionsModel;

            tabMaterial.InvokeIfRequired(() => {
                tabMaterial.SelectedIndex = 0; // ensure contents tab is showing
            });

            tvwMaterial.InvokeIfRequired(() => {                
                tvwMaterial.Focus();
            });

            tvwMaterial.InvokeIfRequired(() => {
                HierarchicalViewModelBase child = null;
                foreach (SiteExplorerNode element in parentage) {
                    child = childlist.FirstOrDefault((vm) => (vm.ObjectID.HasValue ? vm.ObjectID.Value : 0) == element.ElemID);
                    if (child == null) {
                        break;
                    }
                    if (child.Parent != null && !child.Parent.IsExpanded) {
                        child.Parent.IsExpanded = true;

                    }

                    child.IsExpanded = true;                    
                    childlist = child.Children;
                }

                if (child != null) {
                    tvwMaterial.BringModelToView(child, regionsNode);
                    child.IsSelected = true;
                }
                
            });


        }

        private void LoadExplorerModel() {
            var service = new MaterialService(User);

            // Region explorer...
            var list = service.GetTopLevelExplorerItems();
            RegionsModel = BuildExplorerModel(list, false);

            regionsNode.ItemsSource = RegionsModel;
            regionsNode.IsExpanded = true;

            // Unplaced sites (Sites with no region)...
            list = service.GetTopLevelExplorerItems(SiteExplorerNodeType.Unplaced);
            UnplacedModel = BuildExplorerModel(list, false);
            unplacedNode.ItemsSource = UnplacedModel;

            // Templates...

            templatesNode.ItemsSource = null;

            _siteTemplatesRoot = CreateTemplateNode("Site Templates", @"images\Site.png", () => {
                var templates = service.GetSiteTemplates();
                return templates.ConvertAll((m) => {
                    return new SiteExplorerNodeViewModel(m);
                });
            });

            _siteVisitTemplatesRoot = CreateTemplateNode("Site Visit Templates", @"images\SiteVisit.png", () => {
                var templates = service.GetSiteVisitTemplates();
                return templates.ConvertAll((m) => {
                    return new SiteExplorerNodeViewModel(m);
                });
            });

            _materialTemplatesRoot = CreateTemplateNode("Material Templates", @"images\Material.png", () => {
                var templates = service.GetMaterialTemplates();
                return templates.ConvertAll((m) => {
                    return new SiteExplorerNodeViewModel(m);
                });
            });


        }

        private void ReloadModel() {

            List<String> expanded = null;
            if (RememberExpanded) {
                expanded = GetExpandedParentages(RegionsModel);
            }

            LoadExplorerModel();

            if (_favorites.IsPopulated) {
                _favorites.ReloadFavorites();
            }

            if (_favorites.IsPopulated) {
                // _favorites.ReloadFavorites();
            }

            if (expanded != null && expanded.Count > 0) {
                ExpandParentages(RegionsModel, expanded);
            }

            btnApply.IsEnabled = false;
            btnCancel.IsEnabled = false;

            ClearPendingChanges();
        }

        public MaterialFavorites Favorites { 
            get { return _favorites; } 
        }

        private ObservableCollection<HierarchicalViewModelBase> BuildExplorerModel(List<SiteExplorerNode> list, bool isFindModel) {

            list.Sort((item1, item2) => {
                int compare = item1.ElemType.CompareTo(item2.ElemType);
                if (compare == 0) {
                    return item1.Name.CompareTo(item2.Name);
                }
                return compare;
            });


            var regionsModel = new ObservableCollection<HierarchicalViewModelBase>(list.ConvertAll((model) => {
                var viewModel = new SiteExplorerNodeViewModel(model, isFindModel);

                if (model.NumChildren > 0) {
                    viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                    viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                }
                return viewModel;
            }));
            return regionsModel;
        }

        void viewModel_LazyLoadChildren(HierarchicalViewModelBase item) {
            using (new OverrideCursor(Cursors.Wait)) {
                var parent = item as SiteExplorerNodeViewModel;
                if (parent != null) {
                    parent.Children.Clear();
                    var service = new MaterialService(User);
                    var list = service.GetExplorerElementsForParent(parent.ElemID, parent.ElemType);
                    var viewModel = BuildExplorerModel(list, parent.IsFindViewModel);
                    foreach (HierarchicalViewModelBase childViewModel in viewModel) {
                        childViewModel.Parent = parent;
                        parent.Children.Add(childViewModel);
                    }
                }
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e) {
        }

        private TreeView GetCurrentTree() {

            if (_favorites.IsVisible) {
                return _favorites.Provider.FavoritesTree;
            }

            if (tvwMaterial.IsVisible) {
                return tvwMaterial;
            }

            if (tvwFind.IsVisible) {
                return tvwFind;
            }

            return null;
        }

        private void EditableTextBlock_EditingComplete(object sender, string text) {
            var treeView = GetCurrentTree();
            if (treeView != null) {
                var selected = treeView.SelectedItem as SiteExplorerNodeViewModel;
                var action = GetRenameActionForNode(selected);
                if (action == null) {
                    throw new NotImplementedException(selected.NodeType.ToString());
                } else {
                    selected.Name = text;
                    RegisterPendingChange(action, this);
                }
            }
        }

        public DatabaseCommand GetRenameActionForNode(SiteExplorerNodeViewModel selected) {
            if (selected != null) {
                switch (selected.NodeType) {
                    case SiteExplorerNodeType.Region:
                        return new RenameRegionCommand(selected.Model);
                    case SiteExplorerNodeType.SiteGroup:
                        return new RenameSiteGroupCommand(selected.Model);
                    case SiteExplorerNodeType.Site:
                        return new RenameSiteCommand(selected.Model);
                    case SiteExplorerNodeType.SiteVisit:
                        return new RenameSiteVisitCommand(selected.Model);
                    case SiteExplorerNodeType.Trap:
                        return new RenameTrapCommand(selected.Model);
                    case SiteExplorerNodeType.Material:
                        return new RenameMaterialCommand(selected.Model);
                }
            }
            return null;
        }

        private void EditableTextBlock_EditingCancelled(object sender, string oldtext) {
        }

        private void treeview_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowContextMenu(sender as TreeView);
        }

        public int? ChooseTemplate(SiteExplorerNodeType templateType) {

            var picklist = new PickListWindow(User, "Select Template", () => {
                var service = new MaterialService(User);
                List<SiteExplorerNode> list = new List<SiteExplorerNode>();
                switch (templateType) {
                    case SiteExplorerNodeType.Site:
                        list.AddRange(service.GetSiteTemplates());
                        break;
                    case SiteExplorerNodeType.SiteVisit:
                        list.AddRange(service.GetSiteVisitTemplates());
                        break;
                    case SiteExplorerNodeType.Material:
                        list.AddRange(service.GetMaterialTemplates());
                        break;
                }                
                return list.ConvertAll((node) => {
                    return new SiteExplorerNodeViewModel(node);
                });
            }, null);

            if (picklist.ShowDialog().GetValueOrDefault(false)) {
                var selected = picklist.SelectedValue as SiteExplorerNodeViewModel;
                if (selected != null) {
                    return selected.ElemID;
                }
            }

            return null;
        }

        private void ShowContextMenu(TreeView tvw) {
            var selected = tvw.SelectedItem as SiteExplorerNodeViewModel;
            var builder = new ContextMenuBuilder(null);
            if (selected != null) {
                tvw.ContextMenu = SiteExplorerMenuBuilder.Build(selected, this);
            } else if (tvw.SelectedItem == this.regionsNode) {                
                builder.New("Add _Region").Handler(() => { 
                    var newNode = AddRegion(null);
                    RegionsModel.Insert(0, newNode);
                }).End();
                builder.Separator();
                builder.AddMenuItem(SiteExplorerMenuBuilder.CreateTemplateItems(this));
                builder.Separator();
                builder.New("Refresh").Handler(() => { Refresh(); }).End();
                tvw.ContextMenu = builder.ContextMenu;
            } else if (tvw.SelectedItem == unplacedNode) {
                builder.New("Add Site _Group").Handler(() => {
                    var newNode = AddSiteGroup(null);
                    UnplacedModel.Insert(0, newNode);
                }).End();

                MenuItemBuilder b = new MenuItemBuilder();
                MenuItem addSite = b.New("Add Site").MenuItem;
                addSite.Items.Add(b.New("Blank").Handler(() => {
                    var newNode = AddSite(null);
                    UnplacedModel.Insert(0, newNode);
                }).MenuItem);

                addSite.Items.Add(b.New("From _Template").Handler(() => {
                    int? templateId = ChooseTemplate(SiteExplorerNodeType.Site);
                    if (templateId != null && templateId.HasValue) {
                        var newNode = AddSite(null, templateId.Value);
                        UnplacedModel.Insert(0, newNode);
                    }
                }).MenuItem);


                builder.AddMenuItem(addSite);

                builder.Separator();
                builder.AddMenuItem(SiteExplorerMenuBuilder.CreateTemplateItems(this));
                builder.Separator();
                builder.New("Refresh").Handler(() => { Refresh(); }).End();
                tvw.ContextMenu = builder.ContextMenu;
            } else if (tvw.SelectedItem is ViewModelPlaceholder) {
                var ph = tvw.SelectedItem as ViewModelPlaceholder;
                switch (ph.DisplayLabel.ToLower()) {
                    case "site templates":
                        builder.New("Add new _Site Template").Handler(() => { AddSiteTemplate(); }).End();
                        break;
                    case "site visit templates":
                        builder.New("Add new Site _Visit Template").Handler(() => { AddSiteVisitTemplate(); }).End();
                        break;
                    case "material templates":
                        builder.New("Add new _Material Template").Handler(() => { AddMaterialTemplate(); }).End();
                        break;
                }

                tvw.ContextMenu = builder.ContextMenu;
            } else if (tvw.SelectedItem == templatesNode) {
                builder.AddMenuItem(SiteExplorerMenuBuilder.CreateTemplateItems(this));
                tvw.ContextMenu = builder.ContextMenu;
            } else {
                tvw.ContextMenu = null;
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

        internal void RefreshNode(SiteExplorerNodeViewModel parent) {
            var surrogateModel = new ObservableCollection<HierarchicalViewModelBase>();
            surrogateModel.Add(parent);
            var expanded = GetExpandedParentages(surrogateModel);
            parent.IsChildrenLoaded = false;
            parent.IsExpanded = false;
            ExpandParentages(surrogateModel, expanded);
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

        internal SiteExplorerNodeViewModel AddNewNode(HierarchicalViewModelBase parent, SiteExplorerNodeType nodeType, Func<SiteExplorerNodeViewModel, DatabaseCommand> commandFactoryFunction) {

            if (parent != null) {
                parent.IsExpanded = true;
            }

            var model = new SiteExplorerNode();
            model.Name = string.Format("<New {0}>", nodeType.ToString());
            if (parent is SiteExplorerNodeViewModel) {
                model.ParentID = (parent as SiteExplorerNodeViewModel).ElemID;
            }
            model.ElemType = nodeType.ToString();
            model.ElemID = -1;
            model.RegionID = -1;

            var viewModel = new SiteExplorerNodeViewModel(model);
            if (parent != null) {
                viewModel.Parent = parent;
                parent.Children.Add(viewModel);
            }

            viewModel.IsSelected = true;
            viewModel.IsRenaming = true;

            if (commandFactoryFunction != null) {
                RegisterPendingChange(commandFactoryFunction(viewModel), this);
            }

            return viewModel;
        }

        internal SiteExplorerNodeViewModel AddRegion(HierarchicalViewModelBase parent) {
            return AddNewNode(parent, SiteExplorerNodeType.Region, (viewModel) => { return new InsertRegionCommand(viewModel.Model, viewModel); });
        }

        internal SiteExplorerNodeViewModel AddSiteGroup(SiteExplorerNodeViewModel parent) {
            return AddNewNode(parent, SiteExplorerNodeType.SiteGroup, (viewModel) => { return new InsertSiteGroupCommand(viewModel.Model, viewModel); });
        }

        internal SiteExplorerNodeViewModel AddSite(SiteExplorerNodeViewModel parent, int templateId = 0) {
            return AddNewNode(parent, SiteExplorerNodeType.Site, (viewModel) => { return new InsertSiteCommand(viewModel.Model, viewModel, templateId); });
        }

        internal void AddSiteVisit(SiteExplorerNodeViewModel parent, int templateId = 0) {
            AddNewNode(parent, SiteExplorerNodeType.SiteVisit, (viewModel) => { return new InsertSiteVisitCommand(viewModel.Model, viewModel, templateId); });
        }

        internal void AddTrap(SiteExplorerNodeViewModel parent) {
            AddNewNode(parent, SiteExplorerNodeType.Trap, (viewModel) => { return new InsertTrapCommand(viewModel.Model, viewModel); });
        }

        internal void AddMaterial(SiteExplorerNodeViewModel parent, int templateId = 0) {
            AddNewNode(parent, SiteExplorerNodeType.Material, (viewModel) => { return new InsertMaterialCommand(viewModel.Model, viewModel, templateId); });
        }

        internal void AddSiteTemplate() {
            AddNewNode(_siteTemplatesRoot, SiteExplorerNodeType.Site, (viewModel) => {
                return new InsertSiteTemplateCommand(viewModel.Model); 
            });
        }

        internal void AddSiteVisitTemplate() {
            AddNewNode(_siteVisitTemplatesRoot, SiteExplorerNodeType.SiteVisit, (viewModel) => {
                return new InsertSiteVisitTemplateCommand(viewModel.Model);
            });
        }

        internal void AddMaterialTemplate() {
            AddNewNode(_materialTemplatesRoot, SiteExplorerNodeType.Material, (viewModel) => {
                return new InsertMaterialTemplateCommand(viewModel.Model);
            });

        }


        private void EditNode(SiteExplorerNodeViewModel node, Func<bool, DatabaseCommandControl> editorFactory, PermissionCategory permCategory) {
            if (node.ElemID < 0) {
                ErrorMessage.Show("You must first apply the changes before editing the details of this item!");
                return;
            } else {
                var readOnly = !User.HasPermission(permCategory, PERMISSION_MASK.UPDATE);
                var editor = editorFactory(readOnly);
                string caption = "";

                var readOnlyStr = readOnly ? "(Read Only)" : "";
                if (node.IsTemplate) {
                    caption = string.Format("{0} Template [{1}] {2}", node.NodeType.ToString(), node.ElemID, readOnlyStr);
                } else {
                    caption = string.Format("{0} Detail {1} [{2}] {3}", node.NodeType.ToString(), node.Name, node.ElemID, readOnlyStr);
                }
                
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
            EditNode(sitevisit, (readOnly) => { 
                var form = new SiteVisitDetails(User, sitevisit.ElemID, readOnly);
                form.ChangesCommitted += new PendingChangesCommittedHandler((s) => {
                    Refresh();
                });
                return form;
            }, PermissionCategory.SPARC_SITEVISIT);
        }

        internal void EditMaterial(SiteExplorerNodeViewModel material) {
            EditNode(material, (readOnly) => { 
                var form = new MaterialDetails(User, material.ElemID, readOnly);
                form.ChangesCommitted += new PendingChangesCommittedHandler((s) => {
                    Refresh();
                });
                return form;
            }, PermissionCategory.SPARC_MATERIAL);
        }

        internal void EditTrap(SiteExplorerNodeViewModel trap) {
            EditNode(trap, (readOnly) => { return new TrapDetails(User, trap.ElemID, readOnly); }, PermissionCategory.SPARC_TRAP);
        }

        internal void EditRegion(SiteExplorerNodeViewModel region) {
            EditNode(region, (readOnly) => { return new RegionDetails(User, region.ElemID, readOnly); }, PermissionCategory.SPARC_REGION);
        }

        internal void EditSite(SiteExplorerNodeViewModel site) {
            EditNode(site, (readOnly) => { return new SiteDetails(User, site.ElemID, readOnly); }, PermissionCategory.SPARC_SITE);
        }

        internal void DeleteNode(SiteExplorerNodeViewModel node, Func<DatabaseCommand> actionFactory, PermissionCategory permCategory) {

            if (!node.IsDeleted) {

                if (!User.HasPermission(permCategory, PERMISSION_MASK.DELETE)) {
                    ErrorMessage.Show("You do not have permission to delete this item!");
                    return;
                }

                node.Traverse((child) => {
                    child.IsDeleted = true;
                });

                if (actionFactory != null) {
                    RegisterPendingChange(actionFactory(), this);
                }
            }
        }

        internal void DeleteRegion(SiteExplorerNodeViewModel region) {
            DeleteNode(region, () => { return new DeleteRegionCommand(region.ElemID); }, PermissionCategory.SPARC_REGION);
        }

        internal void DeleteSiteGroup(SiteExplorerNodeViewModel group) {
            DeleteNode(group, () => { return new DeleteSiteGroupCommand(group.ElemID); }, PermissionCategory.SPARC_SITEGROUP);
        }

        internal void DeleteSite(SiteExplorerNodeViewModel group) {
            DeleteNode(group, () => { return new DeleteSiteCommand(group.ElemID); }, PermissionCategory.SPARC_SITE);
        }

        internal void DeleteSiteVisit(SiteExplorerNodeViewModel group) {
            DeleteNode(group, () => { return new DeleteSiteVisitCommand(group.ElemID); }, PermissionCategory.SPARC_SITEVISIT);
        }

        internal void DeleteTrap(SiteExplorerNodeViewModel trap) {
            DeleteNode(trap, () => { return new DeleteTrapCommand(trap.ElemID); }, PermissionCategory.SPARC_TRAP);
        }

        internal void DeleteMaterial(SiteExplorerNodeViewModel material) {
            DeleteNode(material, () => { return new DeleteMaterialCommand(material.ElemID); }, PermissionCategory.SPARC_MATERIAL);
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
            using (new OverrideCursor(Cursors.Wait)) {
                CommitPendingChanges();
            }
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
                if (this.DiscardChangesQuestion("You have unsaved changes. Refreshing will cause those changes to be discarded. Are you sure you want to discard unsaved changes?")) {
                    ReloadModel();
                }
            } else {
                ReloadModel();
            }
        }


        public static LookupType GetLookupTypeFromElemType(string elemType) {            
            var nodeType = (SiteExplorerNodeType) Enum.Parse(typeof(SiteExplorerNodeType), elemType);
            return GetLookupTypeFromNodeType(nodeType);
        }

        public static LookupType GetLookupTypeFromNodeType(SiteExplorerNodeType nodeType) {
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
            return CreatePinnable(node.ElemType, node.ElemID);
        }

        internal PinnableObject CreatePinnable(string elemType, int elemID) {
            SiteExplorerNodeType nodeType = (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), elemType);
            return new PinnableObject(MaterialPlugin.MATERIAL_PLUGIN_NAME, GetLookupTypeFromNodeType(nodeType), elemID);
        }


        private void btnFind_Click(object sender, RoutedEventArgs e) {
            DoFind();
        }

        private void DoFind() {

            int resultsCount = 0;
            var text = txtFind.Text;
            var scope = cmbFindScope.SelectedItem as MaterialFindScope;

            using (new OverrideCursor(Cursors.Wait)) {

                if (string.IsNullOrWhiteSpace(text)) {
                    return;
                }

                var service = new MaterialService(User);

                string limitations = "";
                if (scope != null) {
                    limitations = scope.Value;
                }
                var list = service.FindNodesByName(text, limitations);

                if (!PluginManager.Instance.CheckSearchResults(list)) {
                    return;
                }

                resultsCount = list.Count;
                var findModel = BuildExplorerModel(list, true);
                tvwFind.ItemsSource = findModel;
                tvwFind.UpdateLayout();

                if (resultsCount == 0) {
                    InfoBox.Show("No matching elements found.", "No results", this);
                }                
            }

        }

        private void txtFind_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                DoFind();
                e.Handled = true;
            }
        }

        private void FavoriteName_EditingComplete(object sender, string text) {

        }

        internal void EditRDE(SiteExplorerNodeViewModel node) {
            RapidDataEntry control = null;

            if (node != null && (node.NodeType == SiteExplorerNodeType.Site || node.NodeType == SiteExplorerNodeType.SiteVisit || node.NodeType == SiteExplorerNodeType.Material)) {
                control = new RapidDataEntry(this, User, node.ElemID, node.NodeType, node);
            } else {
                control = new RapidDataEntry(this, User, -1, SiteExplorerNodeType.Site, null);
            }

            control.ChangesCommitted += new PendingChangesCommittedHandler((s) => {
                Refresh();
            });

            PluginManager.Instance.AddNonDockableContent(Owner, control, "RDE", SizeToContent.Manual);            
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
