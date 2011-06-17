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
            _selfChange = true;
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
            _selfChange = false;
        }

        private void CalculateUTM() {
            double easting, northing;
            string zone;
            var ellipsoid = GeoUtils.FindEllipsoidByName(Datum);
            if (ellipsoid != null && !_selfChange) {
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
            control.CalculateUTM();
        }

        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(EastingsNorthingsControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnYChanged)));

        public double Y {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        private static void OnYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (EastingsNorthingsControl)obj;
            control.CalculateUTM();
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



    }


}
