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
using System.Windows.Shapes;
using SharpMap;
using SharpMap.Layers;
using SharpMap.Data;
using System.Collections.ObjectModel;
using System.Data;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for PointFeaturesOptions.xaml
    /// </summary>
    public partial class PointFeaturesOptions : Window {

        public PointFeaturesOptions(List<VectorLayer> pointLayers, List<VectorLayer> featureLayers) {
            InitializeComponent();
            this.PointLayers = pointLayers;
            this.FeatureLayers = featureLayers;
            
            lstPointLayers.ItemsSource = new ObservableCollection<VectorLayerViewModel>(pointLayers.Select((m) => {
                var vm = new VectorLayerViewModel(m);
                vm.IsSelected = true; // select all point layers by default...
                return vm;
            }));


            var model = new ObservableCollection<VectorLayerViewModel>(featureLayers.Select((m) => {
                return new VectorLayerViewModel(m);
            }).Reverse());
            cmbFeatureLayer.ItemsSource = model;

            cmbFeatureLayer.SelectionChanged += new SelectionChangedEventHandler(cmbFeatureLayer_SelectionChanged);

            if (featureLayers.Count > 0) {
                cmbFeatureLayer.SelectedIndex = 0;
            }

        }

        void cmbFeatureLayer_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = cmbFeatureLayer.SelectedItem as VectorLayerViewModel;
            if (selected != null) {
                var layer = selected.Model as VectorLayer;
                SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();
                layer.DataSource.Open();
                layer.DataSource.ExecuteIntersectionQuery(layer.DataSource.GetExtents(), ds);
                DataTable tbl = ds.Tables[0] as SharpMap.Data.FeatureDataTable;

                var list = new ObservableCollection<DataColumnViewModel>();

                var ignore = new String[] { "BLREGHIER", "BLAUTH" };

                foreach (DataColumn col in tbl.Columns) {                    
                    var vm = new DataColumnViewModel(col);
                    if (!ignore.Contains(col.ColumnName)) {
                        vm.IsSelected = true;
                    }
                    list.Add(vm);
                }

                lstFields.ItemsSource = list;
            }
        }

        protected List<VectorLayer> PointLayers { get; private set; }

        protected List<VectorLayer> FeatureLayers { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {

            if (cmbFeatureLayer.SelectedItem == null) {
                ErrorMessage.Show("Please select a feature layer before continuing");
                return;
            }

            if (SelectedPointLayers.Count == 0) {
                ErrorMessage.Show("You must select at least one point layer!");
                return;
            }

            if (SelectedColumns.Count == 0) {
                ErrorMessage.Show("You must select at least one column!");
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        public VectorLayer SelectedFeatureLayer {
            get {
                var vm = cmbFeatureLayer.SelectedItem as VectorLayerViewModel;
                if (vm != null) {
                    return vm.Model as VectorLayer;
                }
                return null;
            }
        }

        public List<VectorLayer> SelectedPointLayers {
            get {
                var list = lstPointLayers.ItemsSource as ObservableCollection<VectorLayerViewModel>;
                if (list != null) {
                    var selected = list.Where((vm) => {
                        return vm.IsSelected;
                    });
                    return new List<VectorLayer>(selected.Select((vm) => {
                        return vm.Model as VectorLayer;
                    }));
                }

                return new List<VectorLayer>();
            }
        }

        public List<String> SelectedColumns {
            get {
                var list = lstFields.ItemsSource as ObservableCollection<DataColumnViewModel>;
                if (list != null) {
                    var selected = list.Where((vm) => {
                        return vm.IsSelected;
                    });
                    return new List<String>(selected.Select((vm) => {
                        return vm.Model.ColumnName;
                    }));
                }

                return new List<String>();
            }
        }

        public bool IncludeRowForUnmatchedPoints {
            get {
                return chkIncludeUnmatchedPoints.IsChecked == true;
            }
        }

        public bool IsGroupedByTaxonName {
            get {
                return chkGroupByTaxon.IsChecked == true;
            }
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e) {
            SelectAllColumns(true);
        }

        private void SelectAllColumns(bool value) {
            var model = lstFields.ItemsSource as ObservableCollection<DataColumnViewModel>;
            if (model != null) {
                foreach (DataColumnViewModel vm in model) {
                    vm.IsSelected = value;
                }
            }
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e) {
            SelectAllColumns(false);
        }
            
    }

    public class DataColumnViewModel : BioLink.Client.Extensibility.GenericViewModelBase<DataColumn> {

        public DataColumnViewModel(DataColumn model) : base(model, null) { }

        public override string DisplayLabel {
            get { return Model.ColumnName; }
        }


    }

}
