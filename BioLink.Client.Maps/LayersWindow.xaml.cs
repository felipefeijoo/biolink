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

        private Map _map;
        private ObservableCollection<VectorLayerViewModel> _model;
        private List<StyleViewModel> _styleModel;

        public LayersWindow(Map map) {
            InitializeComponent();
            _map = map;            
            _model = new ObservableCollection<VectorLayerViewModel>();
            foreach (ILayer layer in map.Layers) {
                if (layer is VectorLayer) {
                    _model.Add(new VectorLayerViewModel(layer as VectorLayer));
                }
            }

            lstLayers.ItemsSource = _model;

            _styleModel = new List<StyleViewModel>();
            _styleModel.Add(new StyleViewModel(null));

            foreach (System.Drawing.Drawing2D.HatchStyle item in Enum.GetValues(typeof(System.Drawing.Drawing2D.HatchStyle))) {
                _styleModel.Add(new StyleViewModel(item));
            }

            cmbStyle.ItemsSource = _styleModel;
        }

        private void lstLayers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = lstLayers.SelectedItem as VectorLayerViewModel;
            grpLayer.DataContext = null;
        }
    }

    internal class StyleViewModel : ViewModelBase {

        protected int _iconSize = 15;

        public StyleViewModel(System.Drawing.Drawing2D.HatchStyle? hatchStyle) {
            this.HatchStyle = HatchStyle;
            this.Icon = MakeHatchIcon(hatchStyle);
        }

        private BitmapSource MakeHatchIcon(System.Drawing.Drawing2D.HatchStyle? hatchStyle) {

            System.Drawing.Color foreColor = System.Drawing.Color.Black;
            System.Drawing.Color backColor = System.Drawing.Color.White;
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(_iconSize, _iconSize);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm);

            var rect = new System.Drawing.Rectangle(0, 0, _iconSize - 1, _iconSize - 1);
            if (hatchStyle != null) {
                g.FillRectangle(new System.Drawing.Drawing2D.HatchBrush(HatchStyle.Value, foreColor, backColor), rect);
                g.DrawRectangle(new System.Drawing.Pen(foreColor), rect);
            } else {
                g.FillRectangle(new System.Drawing.SolidBrush(foreColor), rect);
            }

            return GraphicsUtils.SystemDrawingImageToBitmapSource(bm);
        }

        public System.Drawing.Drawing2D.HatchStyle? HatchStyle { get; set;  }
    }

    internal class VectorLayerViewModel : ViewModelBase {

        private VectorLayer _model;
        
        private System.Drawing.Pen _Line;
        private bool _outlineEnabled;
        private System.Drawing.Bitmap _symbol;
        private System.Drawing.Color _fillColor;
        private System.Drawing.Drawing2D.HatchStyle? _hatchStyle;

        public VectorLayerViewModel(VectorLayer model) {
            _model = model;            

            _fillColor = ExtractColorFromBrush(model.Style.Fill);
            _hatchStyle = ExtractStyleFromBrush(model.Style.Fill);
            _Line = model.Style.Line;
            _outlineEnabled = model.Style.EnableOutline;
            _symbol = model.Style.Symbol;
        }

        private System.Drawing.Drawing2D.HatchStyle? ExtractStyleFromBrush(System.Drawing.Brush brush) {
            if (brush is System.Drawing.Drawing2D.HatchBrush) {
                return ((System.Drawing.Drawing2D.HatchBrush)brush).HatchStyle;
            }

            return null;            
        }

        private System.Drawing.Color ExtractColorFromBrush(System.Drawing.Brush brush) {
            if (brush is System.Drawing.SolidBrush) {
                return ((System.Drawing.SolidBrush)brush).Color;
            }

            if (brush is System.Drawing.Drawing2D.HatchBrush) {
                return ((System.Drawing.Drawing2D.HatchBrush)brush).ForegroundColor;
            }

            throw new Exception("Unhandled brush type! Could not extract brush color");
        }

        public string Name {
            get { return _model.LayerName; }
        }

        public System.Drawing.Color FillColor {
            get { return _fillColor; }
            set { SetProperty("FillColor", ref _fillColor, value); }
        }

        public System.Drawing.Drawing2D.HatchStyle? HatchStyle {
            get { return _hatchStyle; }
            set { SetProperty("HatchStyle", ref _hatchStyle, value); }
        }

    }
}
