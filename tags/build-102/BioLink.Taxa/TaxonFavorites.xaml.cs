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
    public partial class TaxonFavorites : DatabaseActionControl {

        private ObservableCollection<HierarchicalViewModelBase> _model;
        private HierarchicalViewModelBase _userRoot;
        private HierarchicalViewModelBase _globalRoot;

        public TaxonFavorites() {
            InitializeComponent();
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

        private void ModelChanged(object source, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            EnableButtons();
        }

        private void BuildFavoritesModel(HierarchicalViewModelBase root, bool global) {
            var service = new SupportService(User);
            var list = service.GetTopTaxaFavorites(global);

            foreach (TaxonFavorite item in list) {
                var viewModel = new TaxonFavoriteViewModel(item);
                if (item.NumChildren > 0) {
                    viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                    viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                    viewModel.DataChanged += new DataChangedHandler((m) => {
                        EnableButtons();
                    });
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
                        if (tf.NumChildren > 0) {
                            viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                            viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                            viewModel.DataChanged += new DataChangedHandler((m) => {
                                EnableButtons();
                            });
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
            RegisterUniquePendingChange(new DeleteFavoriteAction(favorite.FavoriteID));
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
            RegisterUniquePendingChange(new RenameFavoriteGroupAction(viewModel.Model));
        }

        private void TaxonName_EditingCancelled(object sender, string oldtext) {
            var tvm = (sender as EditableTextBlock).ViewModel as HierarchicalViewModelBase;
            if (tvm != null) {
                tvm.DisplayLabel = null;
            }
        }

        internal void DeleteFavorite(int favoriteId) {
            var viewModel = FindFavorite(favoriteId);
            if (favoriteId < 0) {
                viewModel.IsDeleted = true;
            } else {
                if (viewModel != null && !viewModel.IsDeleted) {
                    viewModel.IsDeleted = true;
                    RegisterUniquePendingChange(new DeleteFavoriteAction(viewModel.FavoriteID));
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            if (this.Question("You have unsaved changes. Are you sure you want to discard those changes?", "Discard changes?")) {
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
            } else {
                _userRoot.IsExpanded = true;
                _userRoot.Children.Add(viewModel);
            }

            viewModel.IsSelected = true;
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

            parent.Children.Add(viewModel);

            RegisterUniquePendingChange(new InsertFavoriteGroupAction(model, FavoriteType.Taxa));

            viewModel.IsRenaming = true;

            EnableButtons();            
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
    }

}
