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
        private SpeciesRichnessOptions _richnessOptions;

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
            grdRichness.Children.Add(_richnessOptions = new SpeciesRichnessOptions());
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
                double cutoff = _singleModelOptions.CutOff;
                int intervals = _singleModelOptions.Intervals;
                ShowGridLayerInMap(new GridLayer(selected.Name), intervals, cutoff);
            }
        }

        private void ShowGridLayerInMap(GridLayer grid, int intervals, double cutoff, string filename = null) {

            this.InvokeIfRequired(() => {
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
            });
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
                                double cutoff = _singleModelOptions.CutOff;
                                int intervals = _singleModelOptions.Intervals;
                                ShowGridLayerInMap(result, intervals, cutoff, imageFilename);
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
                builder.New("Show in _map").Handler(() => { ShowGridLayerInMap(new GridLayer(selected.Name), _singleModelOptions.Intervals, _singleModelOptions.CutOff); }).End();
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

        private void btnStartRichness_Click(object sender, RoutedEventArgs e) {
            StartSpeciesRichness();
        }

        private void StartSpeciesRichness() {

            btnStartRichness.IsEnabled = false;
            btnStopRichness.IsEnabled = true;

            var pointsModel = pointSets.PointSets.Select((m) => {
                var vm = new SpeciesRichnessPointSetViewModel(m);
                vm.Status = "Pending";
                return vm;
            });

            lvwRichness.ItemsSource = new ObservableCollection<SpeciesRichnessPointSetViewModel>(pointsModel);

            var cutOff = _richnessOptions.CutOff;

            var processor = new SpeciesRichnessProcessor(_richnessOptions.SelectedModel, pointSets.PointSets, _layerFilenames.Select(m => m.Name), cutOff);
            processor.ProgressObserver = new SpeciesRichnessProgressAdapter(this);
            processor.PointSetModelStarted += new Action<MapPointSet>(processor_PointSetModelStarted);
            processor.PointSetModelCompleted += new Action<MapPointSet>(processor_PointSetModelCompleted);
            processor.PointSetModelProgress += new Action<MapPointSet, string, double?>(processor_PointSetModelProgress);

            JobExecutor.QueueJob(() => {
                try {                    
                    var result = processor.RunSpeciesRichness();
                    if (result != null) {
                        var range = result.GetRange();
                        ShowGridLayerInMap(result, (int) range.Range, 0);
                    }
                } finally {
                    this.InvokeIfRequired(() => {
                        btnStartRichness.IsEnabled = true;
                        btnStopRichness.IsEnabled = false;
                    });
                }
            });
        }

        void processor_PointSetModelProgress(MapPointSet pointSet, string message, double? percent) {
            var vm = ViewModelForModel(pointSet);
            if (vm != null) {
                lvwRichness.InvokeIfRequired(() => {
                    if (percent.HasValue) {
                        vm.Status = string.Format("{0}%", (int)percent.Value);
                    } else {
                        vm.Status = string.Format("-");
                    }
                });
            }
        }

        void processor_PointSetModelCompleted(MapPointSet obj) {
            var vm = ViewModelForModel(obj);
            if (vm != null) {
                lvwRichness.InvokeIfRequired(() => {
                    vm.Status = string.Format("Done.");
                });
            }
        }

        private SpeciesRichnessPointSetViewModel ViewModelForModel(MapPointSet model) {
            var viewmodels = lvwRichness.ItemsSource as ObservableCollection<SpeciesRichnessPointSetViewModel>;
            if (viewmodels != null) {
                var selected = viewmodels.FirstOrDefault((m) => {
                    return m.Name == model.Name;
                });
                return selected;
            }
            return null;
        }

        void processor_PointSetModelStarted(MapPointSet obj) {
            lvwRichness.InvokeIfRequired(() => {
                var vm = ViewModelForModel(obj);
                if (vm != null) {
                    vm.Status = "Running...";
                }
            });
        }

        class SpeciesRichnessProgressAdapter : IProgressObserver {

            public SpeciesRichnessProgressAdapter(ModellingTool tool) {
                this.ModellingTool = tool;
            }

            public void ProgressStart(string message, bool indeterminate = false) {
                ModellingTool.InvokeIfRequired(() => {
                    ModellingTool.progressRichness.Maximum = 100;
                    ModellingTool.progressRichness.Minimum = 0;
                    ModellingTool.progressRichness.Value = 0;
                    ModellingTool.progressRichness.IsIndeterminate = indeterminate;
                    ModellingTool.lblRichnessStatus.Content = message;
                });
            }

            public void ProgressMessage(string message, double? percentComplete = null) {
                ModellingTool.InvokeIfRequired(() => {
                    ModellingTool.lblRichnessStatus.Content = message;
                    if (percentComplete.HasValue) {
                        ModellingTool.progressRichness.Value = percentComplete.Value;
                    }
                });

            }

            public void ProgressEnd(string message) {
                ModellingTool.InvokeIfRequired(() => {
                    ModellingTool.lblRichnessStatus.Content = message;
                    ModellingTool.progressRichness.Value = 0;
                });
            }

            private ModellingTool ModellingTool { get; set; }
        }

    }

    class SpeciesRichnessProcessor : IProgressObserver {

        public SpeciesRichnessProcessor(DistributionModel model, IEnumerable<MapPointSet> pointSets, IEnumerable<string> layerFiles, double cutOff) {
            this.Model = model;
            this.MapPointSets = pointSets;
            this.LayerFiles = layerFiles;
            this.CutOff = cutOff;
        }

        protected MapPointSet _currentPointSet;

        public GridLayer RunSpeciesRichness() {
            // First load each of the layers...

            IsCancelled = false;

            var layers = new List<GridLayer>();

            foreach (string filename in LayerFiles) {
                ProgressMessage("Loading environmental layer {0}...", filename);
                layers.Add(new GridLayer(filename));
            }

            // Now for each point set, run the model...
            var resultLayers = new List<GridLayer>();

            Model.ProgressObserver = this;

            if (ProgressObserver != null) {
                ProgressObserver.ProgressMessage("Running models...");
            }

            var first = layers[0];

            int setIndex = 0;
            foreach (MapPointSet pointset in MapPointSets) {

                _currentPointSet = pointset;

                var percent = ((double)setIndex / (double) MapPointSets.Count()) * 100.0;

                if (ProgressObserver != null) {
                    ProgressObserver.ProgressMessage("Running model on pointset " + pointset.Name, percent);
                }

                FireStartPointSet(pointset);

                var list = new List<MapPointSet>();
                list.Add(pointset);
                var modelLayer = Model.RunModel(layers, list);
                if (modelLayer != null) {
                    resultLayers.Add(modelLayer);
                }

                if (IsCancelled) {
                    return null;
                }

                FireEndPointSet(pointset);

                setIndex++;
            }

            var target = new GridLayer(first.Width, first.Height) { DeltaLatitude = first.DeltaLatitude, DeltaLongitude = first.DeltaLongitude, Flags = first.Flags, Latitude0 = first.Latitude0, Longitude0 = first.Longitude0, NoValueMarker = first.NoValueMarker };            
            target.SetAllCells(0);
            foreach (GridLayer result in resultLayers) {
                for (int y = 0; y < target.Height; ++y) {
                    var lat = target.Latitude0 + (y * target.DeltaLatitude);		// Work out Lat. of this cell.
                    for (int x = 0; x < target.Width; ++x) {
                        var lon = target.Longitude0 + (x * target.DeltaLongitude); // Work out Long. of this cell.
                        var fVal = result.GetValueAt(lat, lon, result.NoValueMarker);
                        if (fVal == result.NoValueMarker) {
                            target.SetCellValue(x, y, target.NoValueMarker);
                        } else {
                            if (fVal > CutOff) {
                                var currentVal = target.GetCellValue(x, y);
                                target.SetCellValue(x, y, currentVal + 1);
                            }
                        }
                    }
                }
            }

            return target;
        }

        public void Cancel() {
            if (Model != null) {
                Model.CancelModel();
            }

            IsCancelled = true;
        }

        protected void ProgressMessage(string format, params object[] args) {
            if (ProgressObserver != null) {
                ProgressObserver.ProgressMessage(string.Format(format, args));
            }
        }

        public IProgressObserver ProgressObserver { get; set; }

        public DistributionModel Model { get; private set; }

        public IEnumerable<MapPointSet> MapPointSets { get; private set; }

        public IEnumerable<string> LayerFiles { get; private set; }

        public double CutOff { get; private set; }

        public bool IsCancelled { get; set; }

        public void ProgressStart(string message, bool indeterminate = false) {
        }

        public void ProgressMessage(string message, double? percentComplete = null) {
            FirePointSetProgressMessage(_currentPointSet, message, percentComplete);
        }

        public void ProgressEnd(string message) {
        }

        private void FireEndPointSet(MapPointSet pointset) {
            if (PointSetModelCompleted != null) {
                PointSetModelCompleted(pointset);
            }
        }

        private void FirePointSetProgressMessage(MapPointSet points, string message, double? percentComplete) {
            if (PointSetModelProgress != null) {
                PointSetModelProgress(points, message, percentComplete);
            }
        }

        private void FireStartPointSet(MapPointSet pointset) {
            if (PointSetModelStarted != null) {
                PointSetModelStarted(pointset);
            }
        }

        public event Action<MapPointSet> PointSetModelStarted;

        public event Action<MapPointSet, string, double?> PointSetModelProgress;

        public event Action<MapPointSet> PointSetModelCompleted;

    }

    class SpeciesRichnessPointSetViewModel : GenericViewModelBase<MapPointSet> {

        private string _status;
        private string _filename;

        public SpeciesRichnessPointSetViewModel(MapPointSet model) : base(model, null) { }

        public string Status {
            get { return _status; }
            set { SetProperty("Status", ref _status, value); }
        }

        public string Filename {
            get { return _filename; }
            set { SetProperty("Filename", ref _filename, value); }
        }

        public String Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

    }

}
