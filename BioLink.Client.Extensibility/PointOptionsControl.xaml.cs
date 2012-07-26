/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using System.Windows.Input;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PointOptionsControl.xaml
    /// </summary>
    public partial class PointOptionsControl : UserControl {

        internal static List<PointShapeViewModel> _shapeModel = new List<PointShapeViewModel>();

        static PointOptionsControl() {
            _shapeModel.Add(new PointShapeViewModel(MapPointShape.Circle));
            _shapeModel.Add(new PointShapeViewModel(MapPointShape.Square));
            _shapeModel.Add(new PointShapeViewModel(MapPointShape.Triangle));
        }

        public PointOptionsControl() {
            init(null);
        }

        public PointOptionsControl(SymbolInfo info) {
            init(info);
        }

        private void init(SymbolInfo info) {
            InitializeComponent();
            
            cmbShape.ItemsSource = _shapeModel;            
            cmbShape.SelectionChanged += cmbShape_SelectionChanged;
            Loaded += PointOptionsControl_Loaded;

            if (info == null) {
                sizeSlider.Value = 7;
                cmbShape.SelectedIndex = 0;
                ctlColor.SelectedColor = Colors.Red;
                Shape = MapPointShape.Circle;
                chkDrawOutline.IsChecked = true;
            } else {
                sizeSlider.Value = info.Size;                
                cmbShape.SelectedItem = _shapeModel.Find(vm => vm.Shape == info.Shape);
                ctlColor.SelectedColor = Color.FromArgb(info.Color.A, info.Color.R, info.Color.G, info.Color.B);
                Shape = info.Shape;
                chkDrawOutline.IsChecked = info.DrawOutline;
            }

            ctlColor.SelectedColorChanged += ctlColor_SelectedColorChanged;
        }

        void ctlColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e) {
            UpdatePreview();
        }

        void PointOptionsControl_Loaded(object sender, RoutedEventArgs e) {
            UpdatePreview();
        }

        void cmbShape_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var vm = cmbShape.SelectedItem as PointShapeViewModel;
            if (vm != null) {
                Shape = vm.Shape;
            }
            UpdatePreview();
        }

        private void UpdatePreview() {
            if (IsLoaded) {
                var image = MapSymbolGenerator.GetSymbol(Shape, (int)sizeSlider.Value, ctlColor.SelectedColor, chkDrawOutline.IsChecked.ValueOrFalse(), Colors.Black);
                BitmapSource s = GraphicsUtils.SystemDrawingImageToBitmapSource(image);
                imgPreview.Source = s;
                if (ValuesChanged != null) {
                    ValuesChanged(this);
                }
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

        private MapPointShape _shape;

        public MapPointShape Shape {
            get { return _shape; }
            set {
                _shape = value;
                cmbShape.SelectedItem = _shapeModel.Find(vm => vm.Shape == value);                
            }
        }

        public int Size {
            get { return (int)sizeSlider.Value; }
            set { sizeSlider.Value = value; }
        }

        public Color Color {
            get { return ctlColor.SelectedColor; }
            set { ctlColor.SelectedColor = value; }
        }

        public bool DrawOutline {
            get { return chkDrawOutline.IsChecked.ValueOrFalse(); }
            set { chkDrawOutline.IsChecked = value; }                 
        }

        protected IMapPointSetGenerator Generator { get; private set; }

        public event Action<PointOptionsControl> ValuesChanged;
    }

    public class PointShapeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var s = (MapPointShape?)value;
            var vm = PointOptionsControl._shapeModel.Find(cand => cand.Shape == s);
            return vm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var vm = value as PointShapeViewModel;
            if (vm != null) {
                return vm.Shape;
            }
            return null;
        }
    }

    public sealed class PointShapeViewModel : ViewModelBase {

        public PointShapeViewModel(MapPointShape shape) {
            Shape = shape;
            Icon = GraphicsUtils.SystemDrawingImageToBitmapSource(MapSymbolGenerator.GetSymbol(shape, 10, Colors.Black, true));
        }

        public MapPointShape Shape { get; set; }


        public override int? ObjectID {
            get { return 0; }
        }
    }

}
