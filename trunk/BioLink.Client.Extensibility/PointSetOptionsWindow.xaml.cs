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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PointSetOptionsWindow.xaml
    /// </summary>
    public partial class PointSetOptionsWindow : Window {

        internal static List<PointShapeViewModel> _shapeModel = new List<PointShapeViewModel>();

        static PointSetOptionsWindow() {
            _shapeModel.Add(new PointShapeViewModel(MapPointShape.Circle));
            _shapeModel.Add(new PointShapeViewModel(MapPointShape.Square));
            _shapeModel.Add(new PointShapeViewModel(MapPointShape.Triangle));
        }

        public PointSetOptionsWindow(string caption, IMapPointSetGenerator generator) {
            InitializeComponent();
            this.Generator = generator;
            this.Caption = caption;
            this.Title = "Point options - " + caption;
            this.Shape = MapPointShape.Circle;
            ctlColor.SelectedColor = Colors.Red;
            cmbShape.ItemsSource = _shapeModel;
            cmbShape.SelectedIndex = 0;

            cmbShape.SelectionChanged += new SelectionChangedEventHandler(cmbShape_SelectionChanged);
            Loaded += new RoutedEventHandler(PointSetOptionsWindow_Loaded);
        }

        void cmbShape_SelectionChanged(object sender, SelectionChangedEventArgs e) {            
            var vm = cmbShape.SelectedItem as PointShapeViewModel;
            if (vm != null) {
                this.Shape = vm.Shape;
            }
            this.UpdatePreview();
        }

        protected IMapPointSetGenerator Generator { get; private set; }

        protected string Caption { get; private set; }

        public MapPointSet Points { get; private set; }

        public MapPointShape Shape { get; set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {

            btnCancel.IsEnabled = false;
            btnOK.IsEnabled = false;
            lblStatus.Content = "Generating points...";
            JobExecutor.QueueJob(() => {
                if (Generator != null) {
                    Points = Generator.GeneratePoints();
                    this.InvokeIfRequired(() => {
                        Points.PointColor = ctlColor.SelectedColor;
                        Points.PointShape = MapPointShape.Triangle;
                        Points.Size = (int) sizeSlider.Value;
                        Points.DrawOutline = chkDrawOutline.IsChecked.ValueOrFalse();
                        Points.PointShape = Shape;
                    });
                }
                this.InvokeIfRequired(() => {
                    lblStatus.Content = "";
                    this.DialogResult = true;
                    this.Close();
                });
            });
            
        }

        void PointSetOptionsWindow_Loaded(object sender, RoutedEventArgs e) {
            UpdatePreview();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void ctlColor_SelectedColorChanged(Color obj) {
            UpdatePreview();
        }

        private void UpdatePreview() {
            if (IsLoaded) {
                var image = MapSymbolGenerator.GetSymbol(Shape, (int)sizeSlider.Value, ctlColor.SelectedColor, chkDrawOutline.IsChecked.ValueOrFalse(), Colors.Black);
                BitmapSource s = GraphicsUtils.SystemDrawingImageToBitmapSource(image);
                imgPreview.Source = s;
            }
        }

        private void chkDrawOutline_Checked(object sender, RoutedEventArgs e) {
            UpdatePreview();
        }

        private void chkDrawOutline_Unchecked(object sender, RoutedEventArgs e) {
            UpdatePreview();
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            UpdatePreview();
        }

    }

    public class PointShapeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            MapPointShape? s = (MapPointShape?)value;
            var vm = PointSetOptionsWindow._shapeModel.Find((cand) => {
                return cand.Shape == s;
            });
            return vm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var vm = value as PointShapeViewModel;
            return vm.Shape;
        }
    }

    public class PointShapeViewModel : ViewModelBase {

        public PointShapeViewModel(MapPointShape shape) {
            this.Shape = shape;
            this.Icon = GraphicsUtils.SystemDrawingImageToBitmapSource(MapSymbolGenerator.GetSymbol(shape, 10, Colors.Black, true));
        }

        public MapPointShape Shape { get; set; }


        public override int? ObjectID {
            get { return 0; }
        }
    }
}
