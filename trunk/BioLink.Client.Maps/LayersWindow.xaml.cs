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
using SharpMap.Layers;
using System.Collections.ObjectModel;
using BioLink.Client.Extensibility;
using SharpMap.Styles;
using SharpMap;
using BioLink.Client.Utilities;
using Microsoft.Win32;
using System.Drawing.Drawing2D;

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for LayersWindow.xaml
    /// </summary>
    public partial class LayersWindow : Window {

        #region Design Constructor
        public LayersWindow() {
            InitializeComponent();
        }
        #endregion

        private MapControl _mapControl;
        private ObservableCollection<VectorLayerViewModel> _model;

        internal static List<StyleViewModel> _styleModel;

        static LayersWindow() {
            _styleModel = new List<StyleViewModel>();
            _styleModel.Add(new StyleViewModel(null));

            foreach (HatchStyle item in Enum.GetValues(typeof(HatchStyle))) {
                _styleModel.Add(new StyleViewModel(item));
            }

        }

        public LayersWindow(MapControl mapControl) {
            InitializeComponent();
            _mapControl = mapControl;            
            _model = new ObservableCollection<VectorLayerViewModel>();
            foreach (ILayer layer in mapControl.mapBox.Map.Layers) {
                if (layer is VectorLayer) {
                    _model.Insert(0, new VectorLayerViewModel(layer as VectorLayer));
                }
            }

            lstLayers.ItemsSource = _model;

            this.MapBackColor = mapControl.mapBox.BackColor;

            backgroundColorPicker.DataContext = this;

            cmbStyle.ItemsSource = _styleModel;
        }

        public System.Drawing.Color MapBackColor { get; set; }

        private void lstLayers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = lstLayers.SelectedItem as VectorLayerViewModel;
            grpLayer.IsEnabled = (selected != null);
            grpLayer.DataContext = selected;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

        private void ApplyChanges() {

            var map = _mapControl.mapBox.Map;

            map.Layers.Clear();

            for (int i = _model.Count -1; i >=0; i--) {
                var m = _model[i];
                var layer = m.Model;          
                if (layer != null) {
                    map.Layers.Add(layer);
                    if (layer is VectorLayer) {
                        var vl = layer as VectorLayer;
                        vl.Style.Fill = m.FillBrush();
                        vl.Style.EnableOutline = m.DrawOutline;
                    }
                } else {                    
                    throw new Exception("Could not load layer " + m.ConnectionID + "!");
                }
            }

            _mapControl.mapBox.BackColor = MapBackColor;

            _mapControl.mapBox.Refresh();

        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
            this.DialogResult = true;
            this.Close();
        }

        //private void cmbStyle_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var layer = grpLayer.DataContext as VectorLayerViewModel;
        //    if (layer != null) {
        //        var selected = cmbStyle.SelectedItem as StyleViewModel;
        //        layer.HatchStyle = selected.HatchStyle;
        //    }
        //}

        private void AddNewLayer() {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Shape Files (*.shp)|*.shp|All files (*.*)|*.*";
            if (dlg.ShowDialog().ValueOrFalse()) {
                ILayer layer = LayerFileLoader.LoadLayer(dlg.FileName);
                if (layer != null && layer is VectorLayer) {
                    var viewModel = new VectorLayerViewModel(layer as VectorLayer);
                    _model.Add(viewModel);
                }
            }
        }

        private void RemoveSelectedLayer() {
            var vm = lstLayers.SelectedItem as VectorLayerViewModel;
            if (vm != null) {
                _model.Remove(vm);
            }
        }

        private void MoveLayerUp() {
            var vm = lstLayers.SelectedItem as VectorLayerViewModel;
            if (vm != null) {
                int index = _model.IndexOf(vm);
                if (_model.IndexOf(vm) > 0) {
                    _model.RemoveAt(index);
                    _model.Insert(index - 1, vm);
                }
                lstLayers.SelectedItem = vm;
            }
        }

        private void MoveLayerDown() {
            var vm = lstLayers.SelectedItem as VectorLayerViewModel;
            if (vm != null) {
                int index = _model.IndexOf(vm);
                if (_model.IndexOf(vm) < _model.Count - 1) {
                    _model.RemoveAt(index);
                    _model.Insert(index + 1, vm);
                }
                lstLayers.SelectedItem = vm;
            }

        }

        private void btnAddLayer_Click(object sender, RoutedEventArgs e) {
            AddNewLayer();
        }

        private void btnDeleteLayer_Click(object sender, RoutedEventArgs e) {
            RemoveSelectedLayer();
        }

        private void btnLayerUp_Click(object sender, RoutedEventArgs e) {
            MoveLayerUp();
        }

        private void btnLayerDown_Click(object sender, RoutedEventArgs e) {
            MoveLayerDown();
        }

    }

    internal class StyleViewModel : ViewModelBase {

        protected int _iconSize = 15;

        public StyleViewModel(HatchStyle? hatchStyle) {
            this.HatchStyle = hatchStyle;
            this.Icon = MakeHatchIcon(hatchStyle);
        }

        private BitmapSource MakeHatchIcon(HatchStyle? hatchStyle) {

            System.Drawing.Color foreColor = System.Drawing.Color.Black;
            System.Drawing.Color backColor = System.Drawing.Color.White;
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(_iconSize, _iconSize);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm)) {

                var rect = new System.Drawing.Rectangle(0, 0, _iconSize - 1, _iconSize - 1);
                if (hatchStyle != null && hatchStyle.HasValue) {
                    g.FillRectangle(new HatchBrush(hatchStyle.Value, foreColor, backColor), rect);
                    g.DrawRectangle(new System.Drawing.Pen(foreColor), rect);
                } else {
                    g.FillRectangle(new System.Drawing.SolidBrush(foreColor), rect);
                }
            }

            return GraphicsUtils.SystemDrawingImageToBitmapSource(bm);
        }

        public HatchStyle? HatchStyle { get; set;  }

        public override int? ObjectID {
            get { return null; }
        }
    }

    internal class VectorLayerViewModel : ViewModelBase {
        
        private System.Drawing.Pen _Line;
        private bool _drawOutline;
        private System.Drawing.Image _symbol;
        private System.Drawing.Color _fillColor;
        private HatchStyle? _hatchStyle;
        

        public VectorLayerViewModel(VectorLayer model) {
            Model = model;
            ConnectionID = model.DataSource.ConnectionID;

            _fillColor = GraphicsUtils.GetColorFromBrush(model.Style.Fill);
            _hatchStyle = GraphicsUtils.GetHatchStyleFromBrush(model.Style.Fill);
            _Line = model.Style.Line;
            _drawOutline = model.Style.EnableOutline;
            _symbol = model.Style.Symbol;
        }

        public string Name {
            get { return Model.LayerName; }
        }

        public System.Drawing.Color FillColor {
            get { return _fillColor; }
            set { SetProperty("FillColor", ref _fillColor, value); }
        }

        public HatchStyle? HatchStyle {
            get { return _hatchStyle; }
            set { SetProperty("HatchStyle", ref _hatchStyle, value); }
        }

        public bool DrawOutline {
            get { return _drawOutline; }
            set { SetProperty("DrawOutline", ref _drawOutline, value); }
        }

        public string ConnectionID { get; private set; }


        internal System.Drawing.Brush FillBrush() {
            return GraphicsUtils.CreateBrush(FillColor, HatchStyle);
        }

        public ILayer Model { get; private set; }

        public override int? ObjectID {
            get { return null; }
        }

    }

    public class HatchStyleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var hs = value as HatchStyle?;
            foreach (StyleViewModel vm in LayersWindow._styleModel) {
                if (vm.HatchStyle == hs) {
                    return vm;
                }
            }
            return new StyleViewModel(hs);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var vm = value as StyleViewModel;
            if (vm != null) {
                return vm.HatchStyle;
            }
            return default(HatchStyle);
        }
    }

}
