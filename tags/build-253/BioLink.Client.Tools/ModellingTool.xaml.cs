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
    public partial class ModellingTool : UserControl, IDisposable, IProgressObserver {

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
            frm.Filter = "GRD files (*.grd)|*.grd|ASCII Grid files (*.asc)|*.asc|All files (*.*)|*.*";
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

        private void ShowGridLayerInMap(GridLayer grid, string filename = null) {
            double cutoff = _singleModelOptions.CutOff;
            int intervals = _singleModelOptions.Intervals;
            
            var prefix = grid.Name;
            FileInfo f = new FileInfo(grid.Name);
            if (f.Exists) {
                prefix = f.Name.Substring(0, f.Name.LastIndexOf("."));
            }
            if (filename == null) {
                filename = LayerImageGenerator.GenerateTemporaryImageFile(grid, prefix, _singleModelOptions.ctlLowColor.SelectedColor, _singleModelOptions.ctlHighColor.SelectedColor, _singleModelOptions.ctlNoValColor.SelectedColor, cutoff, intervals);
            } else {
                LayerImageGenerator.CreateImageFileFromGrid(grid, filename, _singleModelOptions.ctlLowColor.SelectedColor, _singleModelOptions.ctlHighColor.SelectedColor, _singleModelOptions.ctlNoValColor.SelectedColor, cutoff, intervals);
            }

            var map = PluginManager.Instance.GetMap();
            map.Show();
            map.AddRasterLayer(filename);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartSingleModel();
        }

        private DistributionModel _currentModel;

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

            _currentModel = _singleModelOptions.cmbModelType.SelectedItem as DistributionModel;
            if (_currentModel != null) {

                _currentModel.ProgressObserver = this;

                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;

                JobExecutor.QueueJob(() => {
                    try {

                        var layers = _layerFilenames.Select((mm) => {
                            ProgressMessage(string.Format("Loading grid layer {0}", mm.Name));
                            return new GridLayer(mm.Name);
                        }).ToList();

                        ProgressMessage("Running model...");

                        var result = _currentModel.RunModel(layers, points);

                        if (_currentModel.IsCancelled) {
                            MessageBox.Show("Model cancelled", "Model cancelled", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        this.InvokeIfRequired(() => {

                            ProgressMessage("Saving file...");
                            result.SaveToGRDFile(_singleModelOptions.txtFilename.Text);

                            if (_singleModelOptions.chkGenerateImage.IsChecked == true) {
                                ProgressMessage("Preparing map...");
                                string imageFilename = SystemUtils.ChangeExtension(_singleModelOptions.txtFilename.Text, "bmp");
                                TempFileManager.Attach(imageFilename);
                                ShowGridLayerInMap(result, imageFilename);
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
                        _currentModel = null;
                    }
                });

            }
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

        private void lstLayers_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowGridContextMenu();
        }

        private void ShowGridContextMenu() {

            var selected = lstLayers.SelectedItem as GridLayerFileViewModel;

            if (selected != null) {
                var builder = new ContextMenuBuilder(null);
                
                builder.New("Save as _GRD file").Handler(() => { SaveAsGRDFile(new GridLayer(selected.Name)); }).End();
                builder.New("Save as _ASCII Grid file").Handler(() => { SaveAsASCIIFile(new GridLayer(selected.Name)); }).End();
                builder.Separator();
                builder.New("Show in _map").Handler(() => { ShowGridLayerInMap(new GridLayer(selected.Name)); }).End();
                builder.Separator();
                builder.New("Layer _properties").Handler(() => { ShowGridLayerProperties(new GridLayer(selected.Name)); }).End();


                lstLayers.ContextMenu = builder.ContextMenu;
            }
        }

        private void ShowGridLayerProperties(GridLayer layer) {
            var frm = new GridLayerProperties(layer);
            frm.Owner = this.FindParentWindow();
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
        }

        private void SaveAsGRDFile(GridLayer grid) {
            if (grid == null) {
                return;
            }

            var dlg = new SaveFileDialog();
            dlg.Filter = "GRD Files (*.grd)|*.grd|All files (*.*)|*.*";
            if (dlg.ShowDialog(this.FindParentWindow()) == true) {
                grid.SaveToGRDFile(dlg.FileName);
            }
        }

        private void SaveAsASCIIFile(GridLayer grid) {
            if (grid == null) {
                return;
            }

            var dlg = new SaveFileDialog();
            dlg.Filter = "ASC Files (*.asc)|*.asc|All files (*.*)|*.*";
            if (dlg.ShowDialog(this.FindParentWindow()) == true) {
                grid.SaveToASCIIFile(dlg.FileName);
            }

        }

        public void ProgressStart(string message, bool indeterminate = false) {
            progressBar.InvokeIfRequired(() => {
                progressBar.IsIndeterminate = indeterminate;
                if (!indeterminate) {
                    progressBar.Minimum = 0;
                    progressBar.Maximum = 100;
                    progressBar.Value = 0;
                }
            });
             
            ProgressMessage(message);
        }

        public void ProgressMessage(string message, double? percentComplete = null) {
            lblProgress.InvokeIfRequired(() => {
                lblProgress.Content = message;
                if (percentComplete.HasValue) {
                    progressBar.Value = percentComplete.Value;
                }
            });
        }

        public void ProgressEnd(string message) {
            progressBar.InvokeIfRequired(() => {
                ProgressMessage(message);
                progressBar.Value = 0;
            });
        }

        private void btnStop_Click(object sender, RoutedEventArgs e) {
            if (_currentModel != null) {
                _currentModel.CancelModel();
            }
        }
    }
}
