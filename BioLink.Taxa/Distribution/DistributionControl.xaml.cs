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
    public partial class DistributionControl : DatabaseActionControl {

        private ObservableCollection<HierarchicalViewModelBase> _model;

        #region Design constructor

        public DistributionControl() {
            InitializeComponent();
        }

        #endregion

        public DistributionControl(Data.User user, TaxonViewModel taxon)
            : base(user, String.Format("Taxon::DistributionControl::{0}", taxon.TaxaID.Value)) {
            InitializeComponent();                
            this.Taxon = taxon;
            txtDistribution.DataContext = taxon;

            var list = Service.GetDistribution(taxon.TaxaID);
            list.Sort((a, b) => {
                return a.DistRegionFullPath.CompareTo(b.DistRegionFullPath);
            });

            _model = CreateModelFromList(list);
            tvwDist.ItemsSource = _model;
            ExpandAll(_model);
            grpDist.IsEnabled = false;

            taxon.DataChanged += new DataChangedHandler((t) => {
                RegisterUniquePendingChange(new UpdateDistQualDatabaseAction(taxon.Taxon));
            });
            
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
                AddViewModelByPath(viewmodel, model);
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
            var regions = new List<RegionDescriptor>();

            foreach (HierarchicalViewModelBase vm in _model) {
                vm.Traverse((item) => {
                    if (item is DistributionViewModel) {
                        var model = item as DistributionViewModel;
                        regions.Add(new RegionDescriptor(model.DistRegionFullPath, model.ThroughoutRegion));
                    }
                });
            }

            PluginManager.Instance.ShowRegionSelector(regions, (selectedRegions) => {

                var newModel = new ObservableCollection<HierarchicalViewModelBase>();

                // rebuild the model from the new list of regions...
                foreach (RegionDescriptor region in selectedRegions) {
                    var existing = FindDistributionByPath(region.Path);
                    TaxonDistribution model = null;
                    if (existing != null) {
                        model = existing.Model;
                    } else {
                        model = new TaxonDistribution();
                        model.DistRegionFullPath = region.Path;
                        model.BiotaDistID = -1;                        
                        model.ThroughoutRegion = region.IsThroughoutRegion;
                    }
                    AddViewModelByPath(newModel, model);
                }

                _model.Clear();
                foreach (HierarchicalViewModelBase item in newModel) {
                    _model.Add(item);
                }
                RegisterUniquePendingChange(new SaveDistributionRegionsAction(Taxon.Taxon, _model));
                ExpandAll(_model);
            });
        }

        private void AddViewModelByPath(ObservableCollection<HierarchicalViewModelBase> collection, TaxonDistribution model) {            
            String[] bits = model.DistRegionFullPath.Split('\\');
            var pCol = collection;
            HierarchicalViewModelBase parent = null;
            for (int i = 0; i < bits.Length; ++i) {
                string bit = bits[i];                
                var current = pCol.FirstOrDefault((candidate) => { return candidate.DisplayLabel == bit; });
                if (current == null) {
                    if (i == bits.Length - 1) {
                        current = new DistributionViewModel(model, bit);                        
                        current.Parent = parent;                        
                        current.DataChanged += new DataChangedHandler((d) => {
                            RegisterUniquePendingChange(new SaveDistributionRegionsAction(Taxon.Taxon, _model));
                        });
                    } else {
                        current = new DistributionPlaceholder(bit);
                        current.Parent = parent;
                        parent = current;                        
                    }
                    pCol.Add(current);
                } else {
                    parent = current;
                    if (i == bits.Length - 1) {
                        // This region exists already, but will be overridden by this one...
                        (current as DistributionViewModel).Model = model;
                    }
                }
                pCol = current.Children;
            }
            // return result;
        }

        private DistributionViewModel FindDistributionByPath(string path) {

            foreach (HierarchicalViewModelBase model in _model) {
                DistributionViewModel found = null;
                model.Traverse((item) => {
                    var dvm = item as DistributionViewModel;
                    if (dvm != null && dvm.DistRegionFullPath == path) {
                        found = dvm;
                    }                    
                });
                if (found != null) {
                    return found as DistributionViewModel;
                }
            }
            return null;
        }

        #region Properties        

        protected TaxonViewModel Taxon { get; private set; }

        #endregion

        private void btnRemove_Click(object sender, RoutedEventArgs e) {
            DeleteCurrentRegion();
        }

        private void DeleteCurrentRegion() {
            var item = tvwDist.SelectedItem as DistributionViewModel;
            item.Parent.Children.Remove(item);
            RegisterUniquePendingChange(new SaveDistributionRegionsAction(Taxon.Taxon, _model));
        }

    }
}
