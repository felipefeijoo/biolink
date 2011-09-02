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
using System.Windows.Shapes;
using SharpMap.Data;
using SharpMap.Geometries;
using System.Data;
using System.Collections.ObjectModel;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for FeatureInfoControl.xaml
    /// </summary>
    public partial class FeatureInfoControl : UserControl {

        private ObservableCollection<FeatureDataElement> _model;

        public FeatureInfoControl() {
            InitializeComponent();

        }

        public void DisplayFeatures(SharpMap.Geometries.Point point, List<FeatureDataRowLayerPair> rows) {

            lblPosition.Content = String.Format("Info at: {0} - {1}", GeoUtils.DecDegToDMS(point.X, CoordinateType.Longitude), GeoUtils.DecDegToDMS(point.Y, CoordinateType.Latitude));

            _model = new ObservableCollection<FeatureDataElement>();
            foreach (FeatureDataRowLayerPair info in rows) {
                foreach (DataColumn col in info.FeatureDataRow.Table.Columns) {
                    var item = new FeatureDataElement { Name = col.ColumnName, Value = info.FeatureDataRow[col.ColumnName].ToString(), LayerName = info.Layer.LayerName };
                    _model.Add(item);
                }
            }

            lvw.ItemsSource = _model;

            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lvw.ItemsSource);

            myView.GroupDescriptions.Add(new PropertyGroupDescription("LayerName"));

        }

    }

    public class FeatureDataElement : ViewModelBase {

        public string Name { get; set; }
        public string Value { get; set; }
        public string LayerName { get; set; }

        public override string ToString() {
            return string.Format("{0} = {1} [{2}]", Name, Value, LayerName);
        }

        public override int? ObjectID {
            get { return 0; }
        }
    }
}
