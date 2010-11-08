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


namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonFavorites.xaml
    /// </summary>
    public partial class TaxonFavorites : UserControl {

        private ObservableCollection<TaxonFavoriteViewModel> _userFavoritesModel;

        public TaxonFavorites() {
            InitializeComponent();
        }

        public void BindUser(User user) {
            this.User = user;
        }

        

        public void LoadFavorites() {
            if (_userFavoritesModel == null) {
                var service = new SupportService(User);
                var list = service.GetTopTaxaFavorites(false);
                _userFavoritesModel = new ObservableCollection<TaxonFavoriteViewModel>(list.ConvertAll((item) => {
                    var viewModel = new TaxonFavoriteViewModel(item);
                    if (item.NumChildren > 0) {
                        viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                        viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                    }
                    return viewModel;
                }));
                tvwUserFavorites.ItemsSource = _userFavoritesModel;
            }
        }

        void viewModel_LazyLoadChildren(HierarchicalViewModelBase item) {
            var vm = item as TaxonFavoriteViewModel;
            if (vm != null) {
                if (vm.IsGroup) {
                    // Load the children of this favorite group...
                    var service = new SupportService(User);
                    var list = service.GetTaxaFavorites(vm.FavoriteID, vm.IsGlobal);
                    vm.Children.Clear();
                    list.ForEach((tf) => {
                        var viewModel = new TaxonFavoriteViewModel(tf);
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

        private void BuildTaxaChildrenViewModel(HierarchicalViewModelBase item, int taxaID) {
            // The selected node is a Taxon favorite, so we can get the 'real' taxon children for it...
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
        }

        internal User User { get; private set; }

        private void TaxonName_EditingComplete(object sender, string text) {

        }

        private void TaxonName_EditingCancelled(object sender, string oldtext) {

        }
    }
}
