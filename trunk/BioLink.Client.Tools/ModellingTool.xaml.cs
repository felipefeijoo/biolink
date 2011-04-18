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

        private ObservableCollection<GridLayerFileViewModel> _layerFilenames;
        private SingleModelOptionsControl _singleModelOptions;

        public ModellingTool(User user, ToolsPlugin owner) {
            InitializeComponent();
            this.User = user;
            this.Owner = owner;
            _layerFilenames = new ObservableCollection<GridLayerFileViewModel>();
            lstLayers.ItemsSource = _layerFilenames;

            List<String> filelist = Config.GetUser(owner.User, "Modelling.EnvironmentalLayers", new List<string>());
            if (filelist != null && filelist.Count > 0) {
                foreach (string filename in filelist) {
                    AddLayerFile(filename);
                }
            }

            gridSingle.Children.Add(_singleModelOptions = new SingleModelOptionsControl());
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
            frm.Multiselect = true;
            if (frm.ShowDialog(this.FindParentWindow()) == true) {
                foreach (string filename in frm.FileNames) {
                    AddLayerFile(filename);
                }
            }
        }

        private void AddLayerFile(string filename) {            
            var viewModel = new GridLayerFileViewModel(filename);
            _layerFilenames.Add(viewModel);
        }

        public void Dispose() {
            if (_layerFilenames != null) {
                var filelist = _layerFilenames.Select((vm) => {
                    return vm.Name;
                });
                Config.SetUser(Owner.User, "Modelling.EnvironmentalLayers", filelist);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e) {


        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var selected = lstLayers.SelectedItem as GridLayerFileViewModel;
            if (selected != null) {
                ShowGridLayerInMap(new GridLayer(selected.Name));
            }
        }

        private void ShowGridLayerInMap(GridLayer grid) {
            double cutoff = double.Parse(_singleModelOptions.txtCutOff.Text);
            int intervals = Int32.Parse(_singleModelOptions.txtIntervals.Text);

            var filename = LayerImageGenerator.GenerateTemporaryImageFile(grid, _singleModelOptions.ctlLowColor.SelectedColor, _singleModelOptions.ctlHighColor.SelectedColor, _singleModelOptions.ctlNoValColor.SelectedColor, cutoff, intervals);
            var map = PluginManager.Instance.GetMap();
            map.Show();
            map.AddRasterLayer(filename);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartSingleModel();
        }

        private void StartSingleModel() {

            var points = pointSets.PointSets;
            if (points == null || points.Count() == 0) {
                ErrorMessage.Show("There are no training points to model from!");
                return;
            }

            int totalPoints = 0;
            points.ForEach((set) => {
                totalPoints += set.Count();
            });

            if (totalPoints == 0) {
                ErrorMessage.Show("There are no training points to model from!");
                return;
            }

            if (_layerFilenames.Count == 0) {
                ErrorMessage.Show("There are no environmental grid layers specified!");
                return;
            }

            var model = _singleModelOptions.cmbModelType.SelectedItem as DistributionModel;
            if (model != null) {
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;

                JobExecutor.QueueJob(() => {
                    try {

                        var layers = _layerFilenames.Select((mm) => {
                            ProgressMessage("Loading grid layer {0}", mm.Name);
                            return new GridLayer(mm.Name);
                        }).ToList();

                        ProgressMessage("Running model...");

                        var result = model.RunModel(layers, points);

                        this.InvokeIfRequired(() => {

                            ProgressMessage("Saving file...");
                            result.SaveToGRDFile(_singleModelOptions.txtFilename.Text);

                            if (_singleModelOptions.chkGenerateImage.IsChecked == true) {
                                ProgressMessage("Preparing map...");
                                ShowGridLayerInMap(result);
                            }
                        });

                        

                        ProgressMessage("Model complete.");
                    } catch (Exception ex) {
                        MessageBox.Show(ex.ToString());
                    } finally {
                        this.InvokeIfRequired(() => {
                            btnStart.IsEnabled = true;
                            btnStop.IsEnabled = false;
                        });
                    }
                });

            }
        }

        private void ProgressMessage(string format, params object[] args) {
            lblProgress.InvokeIfRequired(() => {
                lblProgress.Content = string.Format(format, args);
            });
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e) {
            RemoveSelectedFile();
        }

        private void RemoveSelectedFile() {
            var selected = lstLayers.SelectedItem as GridLayerFileViewModel;
            if (selected != null) {
                _layerFilenames.Remove(selected);
            }
        }
    }
}
