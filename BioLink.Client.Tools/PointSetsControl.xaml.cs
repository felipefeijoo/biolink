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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for PointSetsControl.xaml
    /// </summary>
    public partial class PointSetsControl : UserControl {

        private ObservableCollection<PointSetViewModel> _model;

        public PointSetsControl() {
            InitializeComponent();

            tvw.DragOver += new DragEventHandler(tvw_DragOver);
            tvw.Drop += new DragEventHandler(tvw_Drop);

            _model = new ObservableCollection<PointSetViewModel>();
            tvw.ItemsSource = _model;
        }

        void tvw_Drop(object sender, DragEventArgs e) {
            var pointGenerator = e.Data.GetData(MapPointSetGenerator.DRAG_FORMAT_NAME) as IMapPointSetGenerator;
            if (pointGenerator != null) {
                MapPointSet points = pointGenerator.GeneratePoints();
                if (points != null) {
                    AddPointSet(points);
                }
            }
        }

        private void AddPointSet(MapPointSet set) {
            var viewModel = new PointSetViewModel(set);
            _model.Add(viewModel);
            foreach (MapPoint p in set) {
                var childVM = new PointViewModel(p);
                childVM.Parent = viewModel;
                viewModel.Children.Add(childVM);
            }
        }

        void tvw_DragOver(object sender, DragEventArgs e) {
            var pointGenerator = e.Data.GetData(MapPointSetGenerator.DRAG_FORMAT_NAME) as IMapPointSetGenerator;

            e.Effects = DragDropEffects.None;

            if (pointGenerator != null) {
                e.Effects = DragDropEffects.Copy;
            }            
        }

        private void elemName_EditingComplete(object sender, string text) {

        }

        private void elemName_EditingCancelled(object sender, string oldtext) {

        }

        private void PlotPointSet(MapPointSet points) {
            var map = PluginManager.Instance.GetMap();
            if (map != null) {
                map.Show();
                map.PlotPoints(points);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            PlotAll();
        }

        private PointSetViewModel GetSelectedPointSet() {
            var pointSet = tvw.SelectedItem as PointSetViewModel;
            if (pointSet == null) {
                var point = tvw.SelectedItem as PointViewModel;
                if (point != null) {
                    pointSet = point.Parent as PointSetViewModel;
                }
            }

            return pointSet;
        }

        private void PlotSelected() {
            var pointSet = GetSelectedPointSet();
            if (pointSet != null) {
                PlotPointSet(pointSet.Model);
            }
        }

        private void PlotAll() {
            var map = PluginManager.Instance.GetMap();
            if (map != null) {
                map.Show();
                foreach (PointSetViewModel set in _model) {
                    map.PlotPoints(set.Model);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            ClearPoints();
        }

        private void ClearPoints() {
            if (this.Question("Are you sure you want to clear all the point sets from the list?", "Clear all points?")) {
                _model.Clear();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            RemoveSelectedPointSet();
        }

        private void RemoveSelectedPointSet() {
            var pointSet = GetSelectedPointSet();
            if (pointSet != null) {
                if (this.Question(string.Format("Are you sure you wish to remove point set '{0}' from the list?", pointSet.Name), string.Format("Remove {0}?", pointSet.Name))) {
                    _model.Remove(pointSet);
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            ExportSelected();
        }

        private DataMatrix BuildPointSetMatrix(PointSetViewModel vm) {
            if (vm == null) {
                return null;
            }
            var result = new DataMatrix();
            result.Columns.Add(new MatrixColumn { Name = "Long" });
            result.Columns.Add(new MatrixColumn { Name = "Lat" });
            

            foreach (MapPoint p in vm.Model) {
                var row = result.AddRow();                
                row[0] = p.Longitude;
                row[1] = p.Latitude;
            }

            return result;
        }


        private void ExportSelected() {
            var selected = GetSelectedPointSet();
            if (selected != null) {
                var Data = BuildPointSetMatrix(selected);
                ExportData exporter = new ExportData(Data, null);
                exporter.Owner = PluginManager.Instance.ParentWindow;
                exporter.ShowDialog();
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e) {
            LoadPointFile();
        }

        private Regex _pointLineRegex = new Regex(@"^(-?[\d.]+)[^\d.-]+(-?[\d.]+)\s*(.*)$");

        private void LoadPointFile() {
            var frm = new OpenFileDialog();
            frm.Filter = "XY Files (*.xy)|*.xy|All Files (*.*)|*.*";
            if (frm.ShowDialog(this.FindParentWindow()) == true) {
                StreamReader reader = new StreamReader(frm.FileName);
                string line;
                var set = new ListMapPointSet(frm.SafeFileName);
                while ((line = reader.ReadLine()) != null) {
                    var m = _pointLineRegex.Match(line);
                    if (m.Success) {
                        var lon = double.Parse(m.Groups[1].Value);
                        var lat = double.Parse(m.Groups[2].Value);
                        var label = m.Groups[3].Value;
                        var point = new MapPoint { Longitude = lon, Latitude = lat, Label = label };
                        set.Add(point);
                    }                    
                }

                AddPointSet(set);
            }

            
        }

        private void Button_Click_5(object sender, RoutedEventArgs e) {
            PlotSelected();
        }

    }

    public class PointViewModel : GenericHierarchicalViewModelBase<MapPoint> {

        public PointViewModel(MapPoint model) : base(model, ()=>0) { }

        public override string DisplayLabel {
            get {
                return String.Format("{0:0.###}, {1:0.###}", Model.Longitude, Model.Latitude);
            }
        }

        public double Latitude {
            get { return Model.Latitude; }
            set { SetProperty(() => Model.Latitude, value); }
        }

        public double Longitude {
            get { return Model.Longitude; }
            set { SetProperty(() => Model.Longitude, value); }
        }

        public string Label {
            get { return Model.Label; }
            set { SetProperty(() => Model.Label, value); }
        }
        
    }

    public class PointSetViewModel : GenericHierarchicalViewModelBase<MapPointSet> {
        public PointSetViewModel(MapPointSet model) : base(model, () => 0) { }

        public String Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public override string DisplayLabel {
            get { return String.Format("{0} ({1} points)", Name, Model.Count()); }
        }

        public override ImageSource Icon {
            get {
                var image = MapSymbolGenerator.GetSymbol(Model.PointShape, Model.Size, Model.PointColor, Model.DrawOutline);
                return GraphicsUtils.SystemDrawingImageToBitmapSource(image);
            }
            set {
                base.Icon = value;
            }
        }

        //public Color PointColor { get; set; }
        //public MapPointShape PointShape { get; set; }
        //public bool DrawOutline { get; set; }
        //public Color OutlineColor { get; set; }
        //public int Size { get; set; }
        //public bool DrawLabels { get; set; }

    }
}
