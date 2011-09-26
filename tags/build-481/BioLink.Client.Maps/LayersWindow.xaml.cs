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
        private ObservableCollection<LayerViewModel> _model;

        public LayersWindow(MapControl mapControl) {
            InitializeComponent();
            _mapControl = mapControl;            
            _model = new ObservableCollection<LayerViewModel>();
            if (mapControl.mapBox.Map.Layers != null && mapControl.mapBox.Map.Layers.Count > 0) {
                foreach (ILayer layer in mapControl.mapBox.Map.Layers) {
                    if (layer is VectorLayer) {
                        _model.Insert(0, new VectorLayerViewModel(layer as VectorLayer));
                    } else if (layer is MyGdalRasterLayer) {
                        _model.Insert(0, new RasterLayerViewModel(layer as MyGdalRasterLayer));
                    }
                }
            }

            lstLayers.ItemsSource = _model;

            if (_model.Count > 0) {
                lstLayers.SelectedIndex = 0;
            }

            this.MapBackColor = mapControl.mapBox.BackColor;

            backgroundColorPicker.DataContext = this;
        }

        public System.Drawing.Color MapBackColor { get; set; }

        private void lstLayers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = lstLayers.SelectedItem as LayerViewModel;
            grpLayer.IsEnabled = (selected != null);
            if (selected != null && selected is VectorLayerViewModel) {
                var vector = selected as VectorLayerViewModel;
                if (vector.Symbol != null) {
                    var info = vector.Tag as SymbolInfo;
                    var control = new PointOptionsControl(info);                    
                    control.ValuesChanged += new Action<PointOptionsControl>((ctl) => {
                        vector.Symbol = MapSymbolGenerator.GetSymbol(ctl);
                    });
                    grpLayer.Content = control;
                } else {                    
                    grpLayer.Content = new VectorOptionsControl(vector);
                }
            } else {
                grpLayer.Content = null;
            }
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
                        if (vl.Style.Symbol != null) {
                            vl.Style.Symbol = (m as VectorLayerViewModel).Symbol;
                        } else {
                            vl.Style.Fill = (m as VectorLayerViewModel).FillBrush;
                            vl.Style.EnableOutline = (m as VectorLayerViewModel).DrawOutline;
                        }
                    }
                } else {
                    throw new Exception("Could not load layer " + (m as VectorLayerViewModel).ConnectionID + "!");
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
            var vm = lstLayers.SelectedItem as LayerViewModel;
            if (vm != null) {
                _model.Remove(vm);
            }
        }

        private void MoveLayerUp() {
            var vm = lstLayers.SelectedItem as LayerViewModel;
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
            var vm = lstLayers.SelectedItem as LayerViewModel;
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

}
