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
    /// Interaction logic for DistributionControl.xaml
    /// </summary>
    public partial class DistributionControl : UserControl {

        private ObservableCollection<HierarchicalViewModelBase> _model;

        #region Design constructor

        public DistributionControl() {
            InitializeComponent();
        }

        #endregion

        public DistributionControl(Data.User user, TaxonViewModel taxon) {
            InitializeComponent();    
            this.User = user;
            this.Taxon = taxon;
            txtDistribution.DataContext = taxon;

            var list = Service.GetDistribution(taxon.TaxaID);
            _model = CreateModelFromList(list);
            tvwDist.ItemsSource = _model;
            ExpandAll(_model);
            grpDist.IsEnabled = false;
            
        }

        private void ExpandAll(ObservableCollection<HierarchicalViewModelBase> l) {
            foreach (HierarchicalViewModelBase model in l) {
                model.IsExpanded = true;
                model.Traverse(item => item.IsExpanded = true );
            }
        }

        private ObservableCollection<HierarchicalViewModelBase> CreateModelFromList(List<TaxonDistribution> list) {

            var viewmodel = new ObservableCollection<HierarchicalViewModelBase>();

            foreach (TaxonDistribution model in list) {
                String[] bits = model.DistRegionFullPath.Split('\\');
                var pCol = viewmodel;
                for (int i = 0; i < bits.Length; ++i) {
                    string bit = bits[i];
                    
                    var parent = pCol.FirstOrDefault((candidate) => { return candidate.DisplayLabel == bit; });
                    if (parent == null) {
                        if (i == bits.Length - 1) {
                            parent = new DistributionViewModel(model, bit);
                        } else {
                            parent = new DistributionPlaceholder(bit);
                        }
                        pCol.Add(parent);
                    } else {
                        if (i == bits.Length - 1) {
                            // This region exists already, but will be overridden by this one...
                            (parent as DistributionViewModel).Model = model;
                        }
                    }
                    pCol = parent.Children;
                }
            }

            return viewmodel;
        }

        private TaxaService Service { get { return new TaxaService(User); } }

        private void tvwDist_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var item = tvwDist.SelectedItem as DistributionViewModel;
            grpDist.IsEnabled = item != null;
            grpDist.DataContext = item;
        }

        private void btnRegionMap_Click(object sender, RoutedEventArgs e) {
            ShowRegionMap();
        }

        private void ShowRegionMap() {
            var regions = new List<string>();

            foreach (HierarchicalViewModelBase vm in _model) {

                if (vm is DistributionViewModel) {
                    regions.Add((vm as DistributionViewModel).DistRegionFullPath);
                }

                vm.Traverse((item) => {
                    if (item is DistributionViewModel) {
                        regions.Add((item as DistributionViewModel).DistRegionFullPath);
                    }
                });
            }

            PluginManager.Instance.ShowRegionMap(regions, (callbackarg) => {
                // rebuild the model from the new list of regions...
            });
        }

        #region Properties

        protected User User { get; private set; }

        protected TaxonViewModel Taxon { get; private set; }

        #endregion

    }
}
