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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ModellingTool.xaml
    /// </summary>
    public partial class ModellingTool : UserControl, IDisposable {

        private ObservableCollection<EnvironmentalLayerViewModel> _layerModel;

        public ModellingTool(User user, ToolsPlugin owner) {
            InitializeComponent();
            this.User = user;
            this.Owner = owner;
            _layerModel = new ObservableCollection<EnvironmentalLayerViewModel>();
            lstLayers.ItemsSource = _layerModel;

            List<String> filelist = Config.GetUser(owner.User, "Modelling.EnvironmentalLayers", new List<string>());
            if (filelist != null && filelist.Count > 0) {
                foreach (string filename in filelist) {
                    AddLayerFile(filename);
                }
            }
        }

        protected User User { get; private set; }

        protected ToolsPlugin Owner { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.FindParentWindow().Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddFile();
        }

        private void AddFile() {
            var frm = new OpenFileDialog();
            frm.Filter = "GRD files (*.grd)|*.grd|All files (*.*)|*.*";
            if (frm.ShowDialog(this.FindParentWindow()) == true) {
                AddLayerFile(frm.FileName);
            }
        }

        private void AddLayerFile(string filename) {
            GRDGridLayer layer = new GRDGridLayer(filename);
            var viewModel = new EnvironmentalLayerViewModel(layer);
            _layerModel.Add(viewModel);
        }

        public void Dispose() {
            if (_layerModel != null) {
                var filelist = _layerModel.Select((vm) => {
                    return vm.Name;
                });
                Config.SetUser(Owner.User, "Modelling.EnvironmentalLayers", filelist);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e) {


        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var selected = lstLayers.SelectedItem as EnvironmentalLayerViewModel;
            if (selected != null && selected.Model is GridLayer) {
                var grid = selected.Model as GridLayer;
                var filename = LayerImageGenerator.GenerateTemporaryImageFile(grid, Colors.White, Colors.Black, Colors.Transparent);
                var map = PluginManager.Instance.GetMap();
                map.Show();
                map.AddRasterLayer(filename);
            }
        }
    }
}
