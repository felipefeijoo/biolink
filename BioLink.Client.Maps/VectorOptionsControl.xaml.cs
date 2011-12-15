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
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Drawing.Drawing2D;
using SharpMap.Layers;

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for VectorOptionsControl.xaml
    /// </summary>
    public partial class VectorOptionsControl : UserControl {

        internal static List<StyleViewModel> _styleModel;

        static VectorOptionsControl() {
            _styleModel = new List<StyleViewModel> {new StyleViewModel(new SolidBrush(Color.FromArgb(0, 0, 0, 0))), new StyleViewModel(new SolidBrush(Color.FromArgb(255, 0, 0, 0)))};

            foreach (HatchStyle item in Enum.GetValues(typeof(HatchStyle))) {
                _styleModel.Add(new StyleViewModel(new HatchBrush(item, Color.Black, Color.White)));
            }

        }


        public VectorOptionsControl(VectorLayerViewModel layer) {
            InitializeComponent();
            DataContext = layer;
            cmbStyle.ItemsSource = _styleModel;
            cmbStyle.SelectionChanged += cmbStyle_SelectionChanged;
        }

        void cmbStyle_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            var currentLayer = DataContext as VectorLayerViewModel;
            if (currentLayer == null) {
                return;
            }

            var vm = e.AddedItems[0] as StyleViewModel;
            if (vm != null) {
                if (vm.Brush is SolidBrush) {
                    var sb = vm.Brush as SolidBrush;
                    if (sb.Color.A > 0) {
                        currentLayer.FillColor = Color.FromArgb(255, currentLayer.FillColor);
                    }
                } else if (vm.Brush is HatchBrush && currentLayer.FillColor.A == 0) {                    
                    currentLayer.FillColor = Color.FromArgb(255, currentLayer.FillColor);
                }
            }
        }

    }

    internal sealed class StyleViewModel : ViewModelBase {

        private const int _iconSize = 15;

        public StyleViewModel(Brush brush) {
            Brush = brush;
            Icon = MakeHatchIcon(brush);
            
        }

        private BitmapSource MakeHatchIcon(Brush brush) {

            var bm = new Bitmap(_iconSize, _iconSize);
            using (var g = Graphics.FromImage(bm)) {
                var rect = new Rectangle(0, 0, _iconSize - 1, _iconSize - 1);
                g.FillRectangle(brush, rect);
                g.DrawRectangle(new Pen(Color.Black), rect);
            }

            return GraphicsUtils.SystemDrawingImageToBitmapSource(bm);
        }

        public Brush Brush { get; set; }

        public override int? ObjectID {
            get { return null; }
        }
    }



    public class RasterLayerViewModel : LayerViewModel {

        public RasterLayerViewModel(MyGdalRasterLayer model) : base(model) { }

        public override FrameworkElement TooltipContent {
            get { return new RasterLayerTooltipContent(this); }
        }

        public string Name {
            get { return Model.LayerName; }
        }

    }

    public class RasterLayerTooltipContent : TooltipContentBase {

        public RasterLayerTooltipContent(RasterLayerViewModel viewModel) : base(0, viewModel) { }

        protected override void GetDetailText(Data.Model.BioLinkDataObject model, TextTableBuilder builder) {
            var vm = ViewModel as LayerViewModel;
            if (vm != null) {
                var m = vm.Model as GdalRasterLayer;
                if (m != null) {
                    builder.Add("Layer type", "Raster");
                    builder.Add("Filename", m.Filename);
                }
            }
        }

        protected override Data.Model.BioLinkDataObject GetModel() {
            return null;
        }
    }


    public class VectorLayerTooltipContent : TooltipContentBase {

        public VectorLayerTooltipContent(VectorLayerViewModel viewModel) : base(0, viewModel) { }

        protected override void GetDetailText(Data.Model.BioLinkDataObject model, TextTableBuilder builder) {
            var vm = ViewModel as LayerViewModel;
            if (vm != null) {
                var m = vm.Model as VectorLayer;
                if (m != null) {
                    builder.Add("Layer type", "Vector");
                    builder.Add("Filename", m.DataSource.ConnectionID);
                }
            }
        }

        protected override Data.Model.BioLinkDataObject GetModel() {
            return null;
        }
    }

    public class VectorLayerViewModel : LayerViewModel {
        private bool _drawOutline;
        private System.Drawing.Image _symbol;
        private Color _fillColor;
        private Brush _brush;

        public VectorLayerViewModel(VectorLayer model) : base(model) {            
            ConnectionID = model.DataSource.ConnectionID;

            SymbolInfo symbolInfo = null;
            if (model.Style.Symbol != null) {
                symbolInfo = model.Style.Symbol.Tag as SymbolInfo;    
            }
            
            if (symbolInfo != null) {
                _fillColor = symbolInfo.Color;
                _brush = new SolidBrush(_fillColor);
                _drawOutline = symbolInfo.DrawOutline;
            } else {
                var brush = model.Style.Fill;
                _fillColor = GraphicsUtils.GetColorFromBrush(brush);
                _brush = model.Style.Fill;
                _drawOutline = model.Style.EnableOutline;                
            }
            _symbol = model.Style.Symbol;
        }

        public override string DisplayLabel {
            get { return Name; }
        }

        public override FrameworkElement TooltipContent {
            get { return new VectorLayerTooltipContent(this); }
        }

        public string Name {
            get { return Model.LayerName; }
        }

        public Color FillColor {
            get { return _fillColor; }
            set { SetProperty("FillColor", ref _fillColor, value); }
        }

        public Brush FillBrush {
            get {
                if (_brush is HatchBrush) {
                    var hb = _brush as HatchBrush;
                    return new HatchBrush(hb.HatchStyle, Color.FromArgb(255, FillColor.R, FillColor.G, FillColor.B), Color.FromArgb(0, FillColor.R, FillColor.G, FillColor.B));
                }

                if (_brush is SolidBrush) {
                    var sb = _brush as SolidBrush;
                    var color = sb.Color;
                    if (color.A > 0) {
                        return new SolidBrush(FillColor);
                    }
                    return new SolidBrush(Color.FromArgb(0, FillColor));
                }

                return _brush; 
            }
            set {
                SetProperty("FillBrush", ref _brush, value); 
            }
        }

        public bool DrawOutline {
            get { return _drawOutline; }
            set { SetProperty("DrawOutline", ref _drawOutline, value); }
        }

        public string ConnectionID { get; private set; }

        public System.Drawing.Image Symbol {
            get { return _symbol; }
            set { SetProperty("Symbol", ref _symbol, value); }
        }

        public override string ToString() {
            return DisplayLabel;
        }

    }

    public class HatchStyleConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var hb = value as HatchBrush;
            if (hb != null) {
                foreach (StyleViewModel vm in VectorOptionsControl._styleModel) {
                    if (vm.Brush is HatchBrush) {
                        var otherHb = vm.Brush as HatchBrush;
                        if (otherHb.HatchStyle == hb.HatchStyle) {
                            return vm;
                        }
                    }
                }
            }

            var sb = value as SolidBrush;
            if (sb != null) {
                foreach (StyleViewModel vm in VectorOptionsControl._styleModel) {
                    if (vm.Brush is SolidBrush) {
                        var otherSb = vm.Brush as SolidBrush;
                        if (otherSb.Color.A == sb.Color.A) {
                            return vm;
                        }
                    }
                }
                
            }

            return new StyleViewModel(hb);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var vm = value as StyleViewModel;
            if (vm != null) {
                return vm.Brush;
            }
            return new BioLinkHatchStyle(default(HatchStyle));
        }
    }

    public class BioLinkHatchStyle {

        public BioLinkHatchStyle(HatchStyle? style, bool isTransparent = false) {
            HatchStyle = style;
            IsTransparent = isTransparent;
        }

        public HatchStyle? HatchStyle { get; set; }
        public bool IsTransparent { get; set; }
    }

}


