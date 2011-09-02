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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;

namespace BioLink.Client.Gazetteer {
    /// <summary>
    /// Interaction logic for CoordinateCalculator.xaml
    /// </summary>
    public partial class CoordinateCalculator : UserControl, IPreferredSizeHolder {



        public CoordinateCalculator() {
            InitializeComponent();


            var model = new ObservableCollection<EllipsoidViewModel>();
            foreach (Ellipsoid e in GeoUtils.ELLIPSOIDS) {
                if (e.ID >= 0) {
                    model.Add(new EllipsoidViewModel(e));
                }
            }
            lstEllipsoids.ItemsSource = model;

            lstEllipsoids.SelectionMode = SelectionMode.Single;
            lstEllipsoids.SelectionChanged += new SelectionChangedEventHandler(lstEllipsoids_SelectionChanged);

            var lastEllipsoid = Config.GetUser(PluginManager.Instance.User, "Gazetteer.CoordinateCalculator.LastEllipsoid", "");
            if (!string.IsNullOrWhiteSpace(lastEllipsoid)) {
                var selected = model.FirstOrDefault((vm) => {
                    return vm.Name.Equals(lastEllipsoid, StringComparison.CurrentCultureIgnoreCase);
                });

                if (selected != null) {
                    lstEllipsoids.SelectedItem = selected;
                } else {
                    lstEllipsoids.SelectedIndex = 0;
                }
                
            }

        }

        void lstEllipsoids_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = lstEllipsoids.SelectedItem as EllipsoidViewModel;
            if (selected != null) {
                Config.SetUser(PluginManager.Instance.User, "Gazetteer.CoordinateCalculator.LastEllipsoid", selected.Name);
                lblUTMEllipsoid.Content = string.Format("(Using ellipsoid '{0}')", selected.Name);
                ctlUTM.Datum = selected.Name;
            }
        }

        private void optDegMinSec_Checked(object sender, RoutedEventArgs e) {
            ctlLatLong.Mode = LatLongMode.DegreesMinutesSeconds;
        }

        private void optDecDeg_Checked(object sender, RoutedEventArgs e) {
            ctlLatLong.Mode = LatLongMode.DecimalDegrees;
        }

        private void optDegDecimalMin_Checked(object sender, RoutedEventArgs e) {
            ctlLatLong.Mode = LatLongMode.DegreesDecimalMinutes;
        }


        public int PreferredHeight {
            get { return 400; }
        }

        public int PreferredWidth {
            get { return 580; }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            var window = this.FindParentWindow();
            if (window != null) {
                window.Close();
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e) {

            var ellipsoid = lstEllipsoids.SelectedItem as EllipsoidViewModel;
            if (ellipsoid == null) {
                ErrorMessage.Show("You must select an ellipsoid to use duing the UTM calculations!");
                return;
            }

            ctlUTM.Datum = ellipsoid.Name;
            ctlUTM.X = ctlLatLong.Longitude;
            ctlUTM.Y = ctlLatLong.Latitude;

        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            var ellipsoid = lstEllipsoids.SelectedItem as EllipsoidViewModel;
            if (ellipsoid == null) {
                ErrorMessage.Show("You must select an ellipsoid to use duing the UTM calculations!");
                return;
            }

            double latitude, longitude;
            GeoUtils.UTMToLatLong(ellipsoid.Model, ctlUTM.Northing, ctlUTM.Easting, ctlUTM.Zone, out latitude, out longitude);

            ctlLatLong.Latitude = latitude;
            ctlLatLong.Longitude = longitude;
        }

        private void btnCopyLatLong_Click(object sender, RoutedEventArgs e) {
            var str = String.Format("{0} {1}", GeoUtils.DecDegToDMS(ctlLatLong.Latitude, CoordinateType.Latitude), GeoUtils.DecDegToDMS(ctlLatLong.Longitude, CoordinateType.Longitude));
            System.Windows.Clipboard.SetText(str);
        }

        private void btnCopyUTM_Click(object sender, RoutedEventArgs e) {
            var str = String.Format("{0} {1} {2}", ctlUTM.Easting, ctlUTM.Northing, ctlUTM.Zone);
            System.Windows.Clipboard.SetText(str);
        }

    }
}
