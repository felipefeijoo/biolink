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

        private LatLongAxis _axis;
        private LatLongMode _mode;

        public LatLongInput() {
            Axis = LatLongAxis.Latitude;
            Mode = LatLongMode.DegreesMinutesSeconds;
            InitializeComponent();
            lblAxis.Content = this.Resources["LatLong.lblAxis." + Axis.ToString()];
        }

        public LatLongAxis Axis {
            get { return _axis; }
            set {
                _axis = value;
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
