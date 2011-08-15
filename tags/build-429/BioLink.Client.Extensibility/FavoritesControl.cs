using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BioLink.Client.Extensibility {

    public class FavoritesControl<T,V> : DatabaseCommandControl, ILazyPopulateControl where T : Favorite, new() where V : HierarchicalViewModelBase {

        private ObservableCollection<HierarchicalViewModelBase> _model;
        private HierarchicalViewModelBase _userRoot;
        private HierarchicalViewModelBase _globalRoot;
        private bool _IsDragging;
        private Point _startPoint;
        private UIElement _dropScope;

        #region Designer ctor
        public FavoritesControl() :base() {
        }
        #endregion

        public FavoritesControl(User user)
            : base(user, typeof(T).Name + "s") {
        }

        public void BindProvider(IFavoritesProvider<T, V> provider) {
            this.Provider = provider;
            var tvw = provider.FavoritesTree;
            tvw.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(tvw_PreviewMouseLeftButtonDown);
            tvw.PreviewMouseMove += new MouseEventHandler(tvw_PreviewMouseMove);

            tvw.MouseRightButtonUp += new MouseButtonEventHandler(tvw_MouseRightButtonUp);
        }

        void tvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = (sender as TreeView).SelectedItem as HierarchicalViewModelBase;
            if (selected != null) {
                ContextMenu menu = Provider.GetContextMenu(selected);

                if (menu == null) {
                    menu = new ContextMenu();
                }

                var builder = new ContextMenuBuilder(null, menu);

                if (selected is FavoriteViewModel<T>) {
                    var favViewModel = selected as FavoriteViewModel<T>;                    
                    if (menu.HasItems) {
                        menu.Items.Add(new Separator());
                    }
                    if (favViewModel.IsGroup) {
                        builder.New("Rename group").Handler(() => { RenameFavoriteGroup(favViewModel); }).End();
                        builder.New("Remove favorite group").Handler(() => { DeleteFavoriteGroup(favViewModel); }).End();
                    } else {
                        builder.New("Remove from favorites").Handler(() => { DeleteFavorite(favViewModel.FavoriteID); }).End();
                    }

                } else if (selected is ViewModelPlaceholder) {
                    builder.New("Add favorite group").Handler(() => { AddFavoriteGroup(selected); }).End();
                }

                if (menu.HasItems) {
                    Provider.FavoritesTree.ContextMenu = menu;
                } else {
                    Provider.FavoritesTree.ContextMenu = null;
                }

            }
        }

        internal void AddFavoriteGroup(HierarchicalViewModelBase parent) {

            int parentGroupID = 0;
            if (parent == null) {
                return;
            }

            bool isGlobal = false;

            if (parent is ViewModelPlaceholder) {
                isGlobal = (bool)(parent as ViewModelPlaceholder).Tag;
            } else if (parent is FavoriteViewModel<T>) {
                var parentViewModel = parent as FavoriteViewModel<T>;
                isGlobal = parentViewModel.IsGlobal;
                parentGroupID = parentViewModel.FavoriteID;
            }

            T model = new T();

            model.IsGroup = true;
            model.GroupName = "<New Folder>";
            model.IsGlobal = isGlobal;
            model.FavoriteParentID = parentGroupID;

            FavoriteViewModel<T> viewModel = Provider.CreateFavoriteViewModel(model);
            viewModel.Parent = parent;
            viewModel.IsSelected = true;

            parent.Children.Add(viewModel);
            

            RegisterUniquePendingChange(new InsertFavoriteGroupCommand(model));
            viewModel.IsRenaming = true;
        }



        internal void RenameFavoriteGroup(FavoriteViewModel<T> fav) {
            if (fav == null) {
                return;
            }

            fav.IsSelected = true;
            fav.IsRenaming = true;            
        }

        protected void CompleteRename(object sender, string text) {
            var control = sender as FrameworkElement;
            if (control != null && control.DataContext is HierarchicalViewModelBase) {
                var selected = control.DataContext as HierarchicalViewModelBase;


                DatabaseCommand command = null;
                if (selected is FavoriteViewModel<T>) {
                    var vm = selected as FavoriteViewModel<T>;
                    if (vm.IsGroup) {
                        vm.GroupName = text;
                        command = new RenameFavoriteGroupCommand(vm.Model);
                    } else {
                        command = Provider.RenameFavorite(vm, text);
                    }
                } else if (selected is V) {
                    command = Provider.RenameViewModel(selected as V, text);
                }

                if (command != null) {
                    RegisterPendingChange(command);
                }
            }

        }

        private void DeleteFavoriteGroup(FavoriteViewModel<T> favorite) {

            if (favorite == null) {
                return;
            }

            if (favorite.IsDeleted) {
                return;
            }

            favorite.IsDeleted = true;
            RegisterUniquePendingChange(new DeleteFavoriteCommand(favorite.FavoriteID));
        }


        public void AddToFavorites(V viewModel, bool global) {

            FavoriteViewModel<T> favViewModel = Provider.CreateFavoriteViewModel(viewModel);

            favViewModel.FavoriteID = -1;
            favViewModel.FavoriteParentID = 0;
            favViewModel.Model.IsGlobal = global;
            favViewModel.Username = User.Username;
            
            Populate();

            if (global) {
                _globalRoot.IsExpanded = true;
                _globalRoot.Children.Add(favViewModel);
                viewModel.Parent = _globalRoot;
            } else {
                _userRoot.IsExpanded = true;
                _userRoot.Children.Add(favViewModel);
                viewModel.Parent = _userRoot;
            }

            viewModel.IsSelected = true;

            RegisterPendingChange(Provider.GetInsertAction(favViewModel));
        }

        public void LoadFavorites() {
            _model = new ObservableCollection<HierarchicalViewModelBase>();
            _userRoot = new ViewModelPlaceholder("User Favorites");
            _globalRoot = new ViewModelPlaceholder("Global Favorites");

            BuildFavoritesModel(_userRoot, false);
            BuildFavoritesModel(_globalRoot, true);

            _model.Add(_userRoot);
            _model.Add(_globalRoot);

            _globalRoot.IsExpanded = true;
            _userRoot.IsExpanded = true;

            Provider.FavoritesTree.ItemsSource = _model;

            IsPopulated = true;
        }

        private void BuildFavoritesModel(HierarchicalViewModelBase root, bool global) {

            var list = Provider.GetTopFavorites(global);

            foreach (T item in list) {

                var viewModel = Provider.CreateViewModel(item);
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

        void viewModel_LazyLoadChildren(HierarchicalViewModelBase item) {
            using (new OverrideCursor(Cursors.Wait)) {
                var vm = item as FavoriteViewModel<T>;
                if (vm != null) {
                    if (vm.IsGroup) {
                        // Load the children of this favorites group...
                        var list = Provider.GetFavoritesForParent(vm.FavoriteID, vm.IsGlobal);
                        vm.Children.Clear();
                        list.ForEach((model) => {
                            var viewModel = Provider.CreateViewModel(model);
                            viewModel.Parent = item;
                            if (model.NumChildren > 0) {
                                viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                                viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                            }
                            vm.Children.Add(viewModel);
                        });
                    } else {
                        DecorateChildViewModels(item, Provider.GetChildViewModels(vm));
                    }
                } else {
                    if (item is V) {
                        DecorateChildViewModels(item, Provider.GetChildViewModels((V)item));
                    }
                }
            }
        }

        private void DecorateChildViewModels(HierarchicalViewModelBase parent, List<HierarchicalViewModelBase> list) {
            parent.Children.Clear();
            foreach (HierarchicalViewModelBase child in list) {
                if (child.NumChildren > 0) {
                    child.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                    child.Children.Add(new ViewModelPlaceholder("Loading..."));
                }
                parent.Children.Add(child);
            }
        }

        void tvw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(Provider.FavoritesTree);
        }

        void tvw_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, Provider.FavoritesTree);
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
            // Can only drop actual favorites (or favorite groups)...
            DataObject data = null;

            var selectedItem = treeView.SelectedItem as HierarchicalViewModelBase;
            
            if (selectedItem is FavoriteViewModel<T>) {
                var selected = treeView.SelectedItem as FavoriteViewModel<T>;
                string desc = "";
                data = new DataObject(typeof(T).Name, selected);

                if (selected.IsGroup) {
                    desc = String.Format("Favorite Folder: Name={0} [FavoriteID={1}]", selected.GroupName, selected.FavoriteID);
                } else {
                    desc = String.Format("{0} : Name={1} [ID1={2}, ID2={3}, FavoriteID={4}]", typeof(T).Name, selected.DisplayLabel, selected.Model.ID1, selected.Model.ID2, selected.FavoriteID);
                    data.SetData(PinnableObject.DRAG_FORMAT_NAME, Provider.CreatePinnableObject(selected));
                }

                data.SetData(DataFormats.Text, desc);
            } else if (selectedItem is V) {
                data = new DataObject(PinnableObject.DRAG_FORMAT_NAME, Provider.CreatePinnableObject(treeView.SelectedItem as V));
            }

            if ( data != null) {

                _dropScope = treeView;
                _dropScope.AllowDrop = true;

                GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DropScope_GiveFeedback);
                item.GiveFeedback += feedbackhandler;

                var handler = new DragEventHandler((s, e) => {
                    TreeViewItem destItem = GetHoveredTreeViewItem(e);
                    e.Effects = DragDropEffects.None;
                    if (destItem != null) {
                        destItem.IsSelected = true;
                        var destModel = destItem.Header as FavoriteViewModel<T>;
                        if (destModel != selectedItem && !selectedItem.IsAncestorOf(destModel)) {
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
            var tvw = Provider.FavoritesTree;

            var source = e.Data.GetData(typeof(T).Name) as FavoriteViewModel<T>;

            if (source != null) {
                var target = tvw.SelectedItem as FavoriteViewModel<T>;
                if (target != null && target.IsGroup) {
                    target.IsExpanded = true;
                    if (source.Parent != null) {
                        source.Parent.Children.Remove(source);
                    }
                    target.Children.Add(source);
                    source.FavoriteParentID = target.FavoriteID;
                    source.IsGlobal = target.IsGlobal;       // May have changed root
                    source.Parent = target;
                    if (source.FavoriteID >= 0) {
                        RegisterPendingChange(new MoveFavoriteCommand(source.Model, target.Model));
                    }
                } else {
                    if (tvw.SelectedItem is ViewModelPlaceholder) {
                        var root = tvw.SelectedItem as ViewModelPlaceholder;
                        root.IsExpanded = true;
                        if (source.Parent != null) {
                            source.Parent.Children.Remove(source);
                        }
                        root.Children.Add(source);
                        source.FavoriteParentID = 0;
                        source.IsGlobal = (bool)root.Tag;
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

        public void DeleteFavorite(int favoriteId) {
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

        public FavoriteViewModel<T> FindFavorite(int favoriteId) {
            var result = SearchModel(_model, favoriteId);
            return result;
        }

        private FavoriteViewModel<T> SearchModel(System.Collections.IList list, int favoriteID) {
            foreach (object model in list) {
                if (model is FavoriteViewModel<T>) {
                    var favModel = model as FavoriteViewModel<T>;
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
                    var hm = model as HierarchicalViewModelBase;
                    var result = SearchModel(hm.Children, favoriteID);
                    if (result != null) {
                        return result;
                    }
                }
            }
            return null;
        }

        public void ReloadFavorites() {
            IsPopulated = false;
            Populate();
        }

        public IFavoritesProvider<T, V> Provider { get; private set; }

        public bool IsPopulated { get; private set; }

        public void Populate() {
            if (!IsPopulated) {
                LoadFavorites();
            }
        }
    }

    public interface IFavoritesProvider<T,V> where T : Favorite where V : HierarchicalViewModelBase {

        PinnableObject CreatePinnableObject(FavoriteViewModel<T> viewModel);

        PinnableObject CreatePinnableObject(V viewModel);

        TreeView FavoritesTree { get; }

        List<T> GetTopFavorites(bool global);

        List<T> GetFavoritesForParent(int parentID, bool global);

        FavoriteViewModel<T> CreateViewModel(T model);

        List<HierarchicalViewModelBase> GetChildViewModels(FavoriteViewModel<T> parent);

        List<HierarchicalViewModelBase> GetChildViewModels(V parent);

        ContextMenu GetContextMenu(HierarchicalViewModelBase selected);

        FavoriteViewModel<T> CreateFavoriteViewModel(V viewModel);

        FavoriteViewModel<T> CreateFavoriteViewModel(T model);

        DatabaseCommand GetInsertAction(FavoriteViewModel<T> favViewModel);

        DatabaseCommand RenameViewModel(V vm, string text);

        DatabaseCommand RenameFavorite(FavoriteViewModel<T> vm, string text);
    }
}
