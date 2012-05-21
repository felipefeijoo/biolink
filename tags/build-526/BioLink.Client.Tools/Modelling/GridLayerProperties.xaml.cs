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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for GridLayerProperties.xaml
    /// </summary>
    public partial class GridLayerProperties : Window {

        private List<KeyValuePair<string, object>> _model;

        public GridLayerProperties(GridLayer layer) {
            InitializeComponent();
            this.Layer = layer;
            _model = new List<KeyValuePair<string, object>>();

            _model.Add(new KeyValuePair<string, object>("Width", layer.Width));
            _model.Add(new KeyValuePair<string, object>("Height", layer.Height));
            _model.Add(new KeyValuePair<string, object>("Top (Latitude)", layer.Latitude0)); 
            _model.Add(new KeyValuePair<string, object>("Left (Longitude)", layer.Longitude0));
            _model.Add(new KeyValuePair<string, object>("Cell size X", layer.DeltaLongitude));
            _model.Add(new KeyValuePair<string, object>("Cell size Y", layer.DeltaLatitude));

            var range = layer.GetRange();

            _model.Add(new KeyValuePair<string, object>("Minimum value", range.Min));
            _model.Add(new KeyValuePair<string, object>("Maximum value", range.Max));
            _model.Add(new KeyValuePair<string, object>("Range", range.Range));
            _model.Add(new KeyValuePair<string, object>("'No Value' value", layer.NoValueMarker));

            lvw.ItemsSource = _model;

        }

        protected GridLayer Layer { get; private set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
    
}
