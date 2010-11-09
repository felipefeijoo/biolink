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

        private ObservableCollection<TaxonFavoriteViewModel> _userFavoritesModel;
        private ObservableCollection<TaxonFavoriteViewModel> _globalFavoritesModel;

        public TaxonFavorites() {
            InitializeComponent();
        }

        public void BindUser(User user, TaxonExplorer explorer) {
            User = user;
            this.TaxonExplorer = explorer;
        }

        public void LoadFavorites() {
            if (_userFavoritesModel == null) {
                _userFavoritesModel = BuildFavoritesModel(false);
                _globalFavoritesModel = BuildFavoritesModel(true);

                tvwUserFavorites.ItemsSource = _userFavoritesModel;
                tvwGlobalFavorites.ItemsSource = _globalFavoritesModel;

                _userFavoritesModel.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ModelChanged);
                _globalFavoritesModel.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ModelChanged);

                btnCancel.IsEnabled = false;
                btnApply.IsEnabled = false;
            }
        }

        private void ModelChanged(object source, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            btnApply.IsEnabled = true;
            btnCancel.IsEnabled = true;
        }

        private ObservableCollection<TaxonFavoriteViewModel> BuildFavoritesModel(bool global) {
            var service = new SupportService(User);
            var list = service.GetTopTaxaFavorites(global);
            var model = new ObservableCollection<TaxonFavoriteViewModel>(list.ConvertAll((item) => {
                var viewModel = new TaxonFavoriteViewModel(item);
                if (item.NumChildren > 0) {
                    viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                    viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                    viewModel.DataChanged += new DataChangedHandler((m) => {
                        btnApply.IsEnabled = true;
                        btnCancel.IsEnabled = true;
                    });
                }
                return viewModel;
            }));
            return model;
        }

        public void ReloadFavorites() {
            _userFavoritesModel = null;
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
                                btnApply.IsEnabled = true;
                                btnCancel.IsEnabled = true;
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
            get { return _userFavoritesModel != null; }
        }

        private void BuildTaxaChildrenViewModel(HierarchicalViewModelBase item, int taxaID) {
            // The selected node is a Taxon favorites, so we can get the 'real' taxon children for it...
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
            if (item != null) {
                item.Focus();
                e.Handled = true;
            }

            var selected = tvwFavorites.SelectedItem;
            int? favoriteId = null;

            TaxonViewModel tvm = null;

            if (selected is TaxonFavoriteViewModel) {
                var fav = selected as TaxonFavoriteViewModel;
                favoriteId = fav.FavoriteID;
                if (!fav.IsGroup) {
                    var taxon = new TaxaService(User).GetTaxon(fav.TaxaID);
                    tvm = new TaxonViewModel(null, taxon, TaxonExplorer.GenerateTaxonDisplayLabel);
                }
            }

            if (selected is TaxonViewModel) {
                tvm = selected as TaxonViewModel;
            }

            if (tvm != null) {
                TaxonMenuFactory f = new TaxonMenuFactory(tvm, TaxonExplorer, TaxonExplorer._R);
                tvwFavorites.ContextMenu = f.BuildFavoritesMenu(favoriteId);
            }
            
        }

        public TaxonFavoriteViewModel FindFavorite(int favoriteId) {
            var result =  SearchModel(_userFavoritesModel, favoriteId);

            if (result == null) {
                result = _globalFavoritesModel.FirstOrDefault((m) => {
                    return m.FavoriteID == favoriteId;
                });
            }

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
        }

        private void TaxonName_EditingCancelled(object sender, string oldtext) {
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
                tvwGlobalFavorites.IsExpanded = true;
                _globalFavoritesModel.Add(viewModel);                
            } else {
                tvwUserFavorites.IsExpanded = true;
                _userFavoritesModel.Add(viewModel);
            }

            viewModel.IsSelected = true;
        }

        internal void AddFavoriteGroup(int? parentFavoriteId) {
            if (parentFavoriteId == null || !parentFavoriteId.HasValue) {
                return;
            }

            var parent = FindFavorite(parentFavoriteId.Value);

            TaxonFavorite model = new TaxonFavorite();
            model.IsGroup = true;
            model.GroupName = "<new group>";
            model.IsGlobal = parent.IsGlobal;

            TaxonFavoriteViewModel viewModel = new TaxonFavoriteViewModel(model);
            // ...

             
        }
    }

}
