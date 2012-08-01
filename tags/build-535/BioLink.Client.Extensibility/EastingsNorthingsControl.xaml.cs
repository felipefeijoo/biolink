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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for EastingsNorthingsControl.xaml
    /// </summary>
    public partial class EastingsNorthingsControl : UserControl {

        private bool _selfChange = false;

        public EastingsNorthingsControl() {
            InitializeComponent();
            txtEasting.TextChanged += new TextChangedEventHandler(PositionChanged);
            txtNorthing.TextChanged += new TextChangedEventHandler(PositionChanged);
            txtZone.TextChanged +=new TextChangedEventHandler(PositionChanged);
        }

        void PositionChanged(object sender, TextChangedEventArgs e) {
            if (!_selfChange) {
                double lat, lon;
                var ellipsoid = GeoUtils.FindEllipsoidByName(Datum);
                if (ellipsoid != null) {
                    double easting, northing;
                    if (double.TryParse(txtEasting.Text, out easting) && double.TryParse(txtNorthing.Text, out northing)) {
                        GeoUtils.UTMToLatLong(ellipsoid, northing, easting, txtZone.Text, out lat, out lon);
                        this.X = lon;
                        this.Y = lat;
                    }
                }
            }
        }

        private void CalculateUTM() {
            double easting, northing;
            string zone;
            var ellipsoid = GeoUtils.FindEllipsoidByName(Datum);
            if (ellipsoid != null) {
                GeoUtils.LatLongToUTM(ellipsoid, Y, X, out northing, out easting, out zone);
                _selfChange = true;
                txtEasting.Text = string.Format("{0:0.00}", easting);
                txtNorthing.Text = string.Format("{0:0.00}", northing);
                txtZone.Text = zone;
                _selfChange = false;
            }
        }

        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(EastingsNorthingsControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnXChanged)));

        public double X {
            get { return (double) GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        private static void OnXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (EastingsNorthingsControl) obj;
            if (control != null && !control._selfChange) {
                control.CalculateUTM();
            }
        }

        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(EastingsNorthingsControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnYChanged)));

        public double Y {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        private static void OnYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (EastingsNorthingsControl)obj;
            if (control != null && !control._selfChange) {
                control.CalculateUTM();
            }
        }

        public static readonly DependencyProperty DatumProperty = DependencyProperty.Register("Datum", typeof(string), typeof(EastingsNorthingsControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnDatumChanged)));
        public string Datum {
            get { return (string)GetValue(DatumProperty); }
            set { SetValue(DatumProperty, value); }
            
        }

        private static void OnDatumChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (EastingsNorthingsControl)obj;
            control.CalculateUTM();
        }

        public double Easting { 
            get { 
                double n = 0;
                double.TryParse(txtEasting.Text, out n);
                return n; 
            } 
        }

        public double Northing { 
            get {
                double n = 0;
                double.TryParse(txtNorthing.Text, out n);
                return n; 
            } 
        }

        public string Zone { get { return txtZone.Text; } }

    }


}
