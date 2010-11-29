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
    /// Interaction logic for LatLongInput.xaml
    /// </summary>
    public partial class LatLongInput : UserControl {

        private CoordinateType _coordType;
        private LatLongMode _mode;

        public LatLongInput() {
            Axis = CoordinateType.Latitude;
            Mode = LatLongMode.DegreesMinutesSeconds;
            InitializeComponent();
            lblAxis.Content = this.Resources["LatLong.lblAxis." + Axis.ToString()];
        }

        public CoordinateType Axis {
            get { return _coordType; }
            set {
                _coordType = value;
                if (lblAxis != null) {
                    lblAxis.Content = this.Resources["LatLong.lblAxis." + value.ToString()];
                }
            }
        }

        public LatLongMode Mode {
            get { return _mode; }
            set {
                _mode = value;
                if (txtDegrees != null) {
                    LayoutControl();
                }
            }
        }

        public double Value {
            get {
                switch (Mode) {
                    case LatLongMode.DecimalDegrees:
                        return Double.Parse(txtDegrees.Text);
                    case LatLongMode.DegreesDecimalMinutes:
                        var deg = Int32.Parse(txtDegrees.Text);
                        var minutes = Double.Parse(txtMinutes.Text);
                        return GeoUtils.DDecMToDecDeg(deg, minutes);
                    case LatLongMode.DegreesDecimalMinutesDirection:
                        deg = Int32.Parse(txtDegrees.Text);
                        minutes = Double.Parse(txtMinutes.Text);
                        return GeoUtils.DDecMDirToDecDeg(deg, minutes, cmbDirection.Text);
                    case LatLongMode.DegreesMinutesSeconds:
                        deg = Int32.Parse(txtDegrees.Text);
                        int min = Int32.Parse(txtMinutes.Text);
                        int seconds = Int32.Parse(txtSeconds.Text);
                        return GeoUtils.DMSToDecDeg(deg, min, seconds, cmbDirection.Text);
                    default:
                        throw new Exception("Mode not handled: " + Mode.ToString());
                }
            }
            set {
                switch (Mode) {
                    case LatLongMode.DegreesMinutesSeconds:
                        string direction;
                        int degrees, minutes, seconds;
                        GeoUtils.DecDegToDMS(value, _coordType, out degrees, out minutes, out seconds, out direction);
                        txtDegrees.Text = degrees + "";
                        txtMinutes.Text = minutes + "";
                        txtSeconds.Text = seconds + "";
                        cmbDirection.Text = direction;
                        break;

                }
            }
        }

        private static GridLength STAR = new GridLength(1, GridUnitType.Star);

        private void LayoutControl() {
            object[] widths = null;
            switch (Mode) {
                case LatLongMode.DecimalDegrees:
                    widths = new object[] { STAR, 15, 0, 0, 0, 0, 0 };
                    break;
                case LatLongMode.DegreesDecimalMinutes:
                    widths = new object[] { STAR, 15, STAR, 15, 0, 0, 0 };
                    break;
                case LatLongMode.DegreesDecimalMinutesDirection:
                    widths = new object[] { STAR, 15, STAR, 15, 0, 0, 35 };
                    break;
                default: // DMS
                    widths = new object[] { STAR, 15, STAR, 15, STAR, 15, 35 };
                    break;
            }

            Debug.AssetNotNull(widths);
            Debug.Assert(widths.Length == 7);

            // Now apply the column widths
            for (int i = 0; i < widths.Length; ++i) {
                object w = widths[i];
                grid.ColumnDefinitions[i].Width = (w is GridLength ? (GridLength)w : new GridLength((int)w));
            }

        }

        public enum LatLongAxis {
            Latitude,
            Longitude
        }

        public enum LatLongMode {
            DegreesMinutesSeconds,
            DecimalDegrees,
            DegreesDecimalMinutes,
            DegreesDecimalMinutesDirection
        }
    }
}
