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
 * implied. See the License for the specific language governing
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

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonFavorites.xaml
    /// </summary>
    public partial class TaxonFavorites : DatabaseCommandControl {

        private ObservableCollection<HierarchicalViewModelBase> _model;
        private HierarchicalViewModelBase _userRoot;
        private HierarchicalViewModelBase _globalRoot;
        private bool _IsDragging;
        private Point _startPoint;

        public TaxonFavorites() {
            InitializeComponent();
            tvwFavorites.PreviewMouseLeftButtonDown +=new MouseButtonEventHandler(tvwFavorites_PreviewMouseLeftButtonDown);
            tvwFavorites.PreviewMouseMove += new MouseEventHandler(tvwFavorites_PreviewMouseMove);
            this.ChangeRegistered += new Action<IList<DatabaseCommand>>((changes) => {
                EnableButtons();
            });
        }

        void tvwFavorites_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(tvwFavorites);
        }

        void tvwFavorites_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, tvwFavorites);
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

        private UIElement _dropScope;

        private void StartDrag(MouseEventArgs mouseEventArgs, TreeView treeView, TreeViewItem item) {

            // Can only drag actual favorites (or favorite groups)...
            var selected = treeView.SelectedItem as TaxonFavoriteViewModel;
            if (selected != null) {
                
                string desc = "";
                var data = new DataObject("TaxonFavorite", selected);

                if (selected.IsGroup) {
                    desc = String.Format("Favorite Folder: Name={0} [FavoriteID={1}]", selected.TaxaFullName, selected.FavoriteID);
                } else {
                    desc = String.Format("Taxon Favorite: Name={0} [TaxonID={1}, FavoriteID={2}]", selected.TaxaFullName, selected.TaxaID, selected.FavoriteID);
                    data.SetData(PinnableObject.DRAG_FORMAT_NAME, TaxonExplorer.Owner.CreatePinnableTaxon(selected.TaxaID));                        
                }

                
                data.SetData(DataFormats.Text, desc);
                

                _dropScope = treeView;
                _dropScope.AllowDrop = true;

                GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DropScope_GiveFeedback);
                item.GiveFeedback += feedbackhandler;

                var handler = new DragEventHandler((s, e) => {
                    TreeViewItem destItem = GetHoveredTreeViewItem(e);
                    e.Effects = DragDropEffects.None;                    
                    if (destItem != null) {
                        destItem.IsSelected = true;                                                
                        var destModel = destItem.Header as TaxonFavoriteViewModel;
                        if (destModel != null) {
                            if (destModel.IsGroup) {
                                e.Effects = DragDropEffects.Move;
                            }
                        } else {
                            if (destItem.Header is ViewModelPlaceholder) {
                                e.Effects = DragDropEffects.Move;
                            }
                        }
                    }
                    e.Handled = true;
                });

                treeView.PreviewDragEnter += handler;
                treeView.PreviewDragOver += handler;

                var dropHandler = new DragEventHandler(treeView_Drop);
                treeView.Drop += dropHandler;

                
                try {
                    _IsDragging = true;
                    DragDrop.DoDragDrop(item, data, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                } finally {
                    _IsDragging = false;
                    treeView.PreviewDragEnter -= handler;
                    treeView.PreviewDragOver -= handler;
                    treeView.Drop -= dropHandler;
                }                
            }

            InvalidateVisual();
        }

        void treeView_Drop(object sender, DragEventArgs e) {
            var source = e.Data.GetData("TaxonFavorite") as TaxonFavoriteViewModel;

            if (source != null) {
                var target = tvwFavorites.SelectedItem as TaxonFavoriteViewModel;
                if (target != null && target.IsGroup) {
                    target.IsExpanded = true;
                    if (source.Parent != null) {
                        source.Parent.Children.Remove(source);
                    }
                    target.Children.Add(source);
                    source.FavoriteParentID = target.FavoriteID;
                    source.IsGroup = target.IsGlobal;       // May have changed root
                    source.Parent = target;
                    if (source.FavoriteID >= 0) {
                        RegisterPendingChange(new MoveFavoriteCommand(source.Model, target.Model));
                    }
                } else {
                    if (tvwFavorites.SelectedItem is ViewModelPlaceholder) {
                        var root = tvwFavorites.SelectedItem as ViewModelPlaceholder;
                        root.IsExpanded = true;
                        if (source.Parent != null) {
                            source.Parent.Children.Remove(source);
                        }
                        root.Children.Add(source);
                        source.FavoriteParentID = 0;
                        source.IsGroup = (bool) root.Tag;
                        source.Parent = root;
                        // If it is a new favorite no need to move, because the insert will occur with the latest (correct) linkages.
                        if (source.FavoriteID >= 0) {
                            RegisterPendingChange(new MoveFavoriteCommand(source.Model, null));
                        }

                    }
                }
            }
        }

        void DropScope_GiveFeedback(object sender, GiveFeedbackEventArgs mouseEventArgs) {
            mouseEventArgs.UseDefaultCursors = true;
            mouseEventArgs.Handled = true;
        }

        private TreeViewItem GetHoveredTreeViewItem(DragEventArgs e) {
            TreeView tvw = _dropScope as TreeView;
            DependencyObject elem = tvw.InputHitTest(e.GetPosition(tvw)) as DependencyObject;
            while (elem != null && !(elem is TreeViewItem)) {
                elem = VisualTreeHelper.GetParent(elem);
            }
            return elem as TreeViewItem;
        }


        public void BindUser(User user, TaxonExplorer explorer) {
            User = user;
            this.TaxonExplorer = explorer;
        }

        public void LoadFavorites() {

            if (_model == null) {
                _model = new ObservableCollection<HierarchicalViewModelBase>();
                _userRoot = new ViewModelPlaceholder("User Favorites");
                _globalRoot = new ViewModelPlaceholder("Global Favorites");

                BuildFavoritesModel(_userRoot, false);
                BuildFavoritesModel(_globalRoot, true);

                _model.Add(_userRoot);
                _model.Add(_globalRoot);

                tvwFavorites.ItemsSource = _model;

                btnCancel.IsEnabled = false;
                btnApply.IsEnabled = false;
            }
        }

        private void BuildFavoritesModel(HierarchicalViewModelBase root, bool global) {
            var service = new SupportService(User);
            var list = service.GetTopTaxaFavorites(global);

            foreach (TaxonFavorite item in list) {
                var viewModel = new TaxonFavoriteViewModel(item);
                viewModel.Parent = root;
                if (item.NumChildren > 0) {
                    viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                    viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                }
                root.Children.Add(viewModel);
                root.IsExpanded = true;
                root.Tag = global;
            }
        }

        public void ReloadFavorites() {
            _model = null;
            LoadFavorites();
        }

        void viewModel_LazyLoadChildren(HierarchicalViewModelBase item) {
            var vm = item as TaxonFavoriteViewModel;
            if (vm != null) {
                if (vm.IsGroup) {
                    // Load the children of this favorites group...
                    var service = new SupportService(User);
                    var list = service.GetTaxaFavorites(vm.FavoriteID, vm.IsGlobal);
                    vm.Children.Clear();
                    list.ForEach((tf) => {
                        var viewModel = new TaxonFavoriteViewModel(tf);
                        viewModel.Parent = item;
                        if (tf.NumChildren > 0) {
                            viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                            viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                        }
                        vm.Children.Add(viewModel);
                    });
                } else {                    
                    BuildTaxaChildrenViewModel(item, vm.TaxaID);
                }
            } else {
                if (item is TaxonViewModel) {
                    var tvm = item as TaxonViewModel;
                    BuildTaxaChildrenViewModel(item, tvm.TaxaID.Value);
                }
            }
        }

        public bool IsFavoritesLoaded {
            get { return _model != null; }
        }

        private void BuildTaxaChildrenViewModel(HierarchicalViewModelBase item, int taxaID) {
            // The model node is a Taxon favorites, so we can get the 'real' taxon children for it...
            item.Children.Clear();

            var taxaService = new TaxaService(User);
            List<Taxon> taxa = taxaService.GetTaxaForParent(taxaID);
            foreach (Taxon taxon in taxa) {
                TaxonViewModel child = new TaxonViewModel(item, taxon, null);
                if (child.NumChildren > 0) {
                    child.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                    child.Children.Add(new ViewModelPlaceholder("Loading..."));
                }
                item.Children.Add(child);
            }

        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e) {
            TreeViewItem item = sender as TreeViewItem;

            if (item == null) {
                return;
            }

            item.Focus();
            e.Handled = true;

            var model = tvwFavorites.SelectedItem as HierarchicalViewModelBase;

            Debug.Assert(model != null);

            int? favoriteId = null;

            TaxonViewModel tvm = null;


            bool isGroup = false;

            if (model is TaxonFavoriteViewModel) {
                var fav = model as TaxonFavoriteViewModel;
                favoriteId = fav.FavoriteID;
                if (!fav.IsGroup) {
                    var taxon = new TaxaService(User).GetTaxon(fav.TaxaID);
                    tvm = new TaxonViewModel(null, taxon, TaxonExplorer.GenerateTaxonDisplayLabel);
                } else {
                    isGroup = true;
                }

            } else if (model is TaxonViewModel) {
                tvm = model as TaxonViewModel;
            }

            if (tvm != null) {
                TaxonMenuFactory f = new TaxonMenuFactory(tvm, TaxonExplorer, TaxonExplorer._R);
                tvwFavorites.ContextMenu = f.BuildFavoritesMenu(model);
            } else {
                var builder = new ContextMenuBuilder(null);

                builder.New("Add favorite group").Handler(() => { AddFavoriteGroup(model); }).End();
                if (isGroup) {
                    builder.New("Rename group").Handler(() => { RenameFavoriteGroup(model as TaxonFavoriteViewModel); }).End();
                    builder.New("Remove favorite group").Handler(() => { DeleteFavoriteGroup(model); }).End();
                }

                tvwFavorites.ContextMenu = builder.ContextMenu;
            }
            
        }

        private void DeleteFavoriteGroup(HierarchicalViewModelBase model) {

            var favorite = model as TaxonFavoriteViewModel;
            if (favorite == null) {
                return;
            }

            if (favorite.IsDeleted) {
                return;
            }

            model.IsDeleted = true;
            RegisterUniquePendingChange(new DeleteFavoriteCommand(favorite.FavoriteID));
        }

        public TaxonFavoriteViewModel FindFavorite(int favoriteId) {
            var result =  SearchModel(_model, favoriteId);
            return result;
        }

        private TaxonFavoriteViewModel SearchModel(System.Collections.IList list, int favoriteID) {
            foreach (object model in list) {
                if (model is TaxonFavoriteViewModel) {
                    var favModel = model as TaxonFavoriteViewModel;
                    if (favModel.FavoriteID == favoriteID) {
                        return favModel;
                    }
                    if (favModel.IsGroup) {
                        var result = SearchModel(favModel.Children, favoriteID);
                        if (result != null) {
                            return result;
                        }
                    }
                } else if (model is HierarchicalViewModelBase) {
                    var result = SearchModel((model as HierarchicalViewModelBase).Children, favoriteID);
                    if (result != null) {
                        return result;
                    }
                }
            }
            return null;
        }

        internal TaxonExplorer TaxonExplorer { get; private set; }

        private void TaxonName_EditingComplete(object sender, string text) {
            if (sender is EditableTextBlock) {
                var viewModel = (sender as EditableTextBlock).ViewModel;
                if (viewModel is TaxonFavoriteViewModel) {
                    ProcessRename(viewModel as TaxonFavoriteViewModel, text);            
                }
            }            
        }

        private void ProcessRename(TaxonFavoriteViewModel viewModel, string name) {
            viewModel.GroupName = name;
            RegisterUniquePendingChange(new RenameFavoriteGroupCommand(viewModel.Model));
        }

        internal void DeleteFavorite(int favoriteId) {
            var viewModel = FindFavorite(favoriteId);
            if (favoriteId < 0) {
                viewModel.IsDeleted = true;
            } else {
                if (viewModel != null && !viewModel.IsDeleted) {
                    viewModel.IsDeleted = true;
                    RegisterUniquePendingChange(new DeleteFavoriteCommand(viewModel.FavoriteID));
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            if (this.DiscardChangesQuestion()) {
                ReloadFavorites();
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

        private void ApplyChanges() {
            CommitPendingChanges(() => {
                ReloadFavorites();
            });
        }

        internal void AddToFavorites(TaxonViewModel Taxon, bool global) {

            TaxonFavorite model = new TaxonFavorite();
            model.IsGroup = false;
            model.ChgComb = Taxon.ChgComb ?? false;
            model.ElemType = Taxon.ElemType;
            model.Epithet = Taxon.Epithet;
            model.FavoriteID = -1;
            model.FavoriteParentID = 0;
            model.IsGlobal = global;
            model.KingdomCode = Taxon.KingdomCode;
            model.NameStatus = Taxon.NameStatus;
            model.Rank = Taxon.Rank;
            model.TaxaFullName = Taxon.TaxaFullName;
            model.TaxaID = Taxon.TaxaID ?? -1;
            model.TaxaParentID = Taxon.TaxaParentID ?? -1;
            model.Unplaced = Taxon.Unplaced ?? false;
            model.Unverified = Taxon.Unverified ?? false;
            model.Username = User.Username;
            model.YearOfPub = Taxon.YearOfPub;

            LoadFavorites();

            TaxonFavoriteViewModel viewModel = new TaxonFavoriteViewModel(model);
            if (global) {
                _globalRoot.IsExpanded = true;
                _globalRoot.Children.Add(viewModel);
                viewModel.Parent = _globalRoot;
            } else {
                _userRoot.IsExpanded = true;
                _userRoot.Children.Add(viewModel);
                viewModel.Parent = _userRoot;
            }

            viewModel.IsSelected = true;

            RegisterPendingChange(new InsertTaxonFavoriteCommand(viewModel.Model));
        }

        internal void AddFavoriteGroup(HierarchicalViewModelBase parent) {

            int parentGroupID = 0;
            if (parent == null) {
                return;
            }

            bool isGlobal = false;

            if (parent is ViewModelPlaceholder) {
                isGlobal = (bool) (parent as ViewModelPlaceholder).Tag;
            } else if (parent is TaxonFavoriteViewModel) {
                var parentViewModel = parent as TaxonFavoriteViewModel;
                isGlobal = parentViewModel.IsGlobal;
                parentGroupID = parentViewModel.FavoriteID;
            }

            TaxonFavorite model = new TaxonFavorite();

            model.IsGroup = true;
            model.GroupName = "<New Folder>";
            model.IsGlobal = isGlobal;
            model.FavoriteParentID = parentGroupID;

            TaxonFavoriteViewModel viewModel = new TaxonFavoriteViewModel(model);
            viewModel.Parent = parent;

            parent.Children.Add(viewModel);

            RegisterUniquePendingChange(new InsertFavoriteGroupCommand(model));
            viewModel.IsRenaming = true;
        }

        private void EnableButtons() {
            btnApply.IsEnabled = true;
            btnCancel.IsEnabled = true;
        }

        internal void RenameFavoriteGroup(TaxonFavoriteViewModel taxonFavoriteViewModel) {
            if (taxonFavoriteViewModel == null) {
                return;
            }

            taxonFavoriteViewModel.IsRenaming = true;
        }

        public ObservableCollection<HierarchicalViewModelBase> Model { 
            get { return _model; } 
        }

    }

}
