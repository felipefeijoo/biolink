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

        internal class GridLayout {

            private object[] _widths;           

            public GridLayout() {
                // default layout for DegreesMinutesSeconds
                _widths = new object[] { STAR, 15, STAR, 15, STAR, 15, 35 };                
            }

            public void Widths(params object[] widths) {
                _widths = widths;
                Debug.Assert(_widths.Length == 7);
            }

            public void Traverse(TraverseLayoutDelegate func) {
                for (int i = 0; i < 7; ++i) {
                    object w = _widths[i];
                    GridLength gl;
                    if (w is GridLength) {
                        gl = (GridLength) w;
                    } else {                        
                        gl = new GridLength((double) (int) w);
                    }
                    func(i, gl);
                }
            }

            public delegate void TraverseLayoutDelegate(int index, GridLength length);
        }


        private void LayoutControl() {
            // Construct a layout with the default values (for DMS);
            GridLayout layout = new GridLayout();
            switch (Mode) {
                case LatLongMode.DecimalDegrees:
                    layout.Widths(STAR, 15, 0, 0, 0, 0, 0);
                    break;
                case LatLongMode.DegreesDecimalMinutes:
                    layout.Widths(STAR, 15, STAR, 15, 0, 0, 0);
                    break;
                case LatLongMode.DegreesDecimalMinutesDirection:
                    layout.Widths(STAR, 15, STAR, 15, 0, 0, 35);
                    break;
                default: // DMS
                    break;
            }

            // Now apply the column widths
            layout.Traverse((index, width) => {
                grid.ColumnDefinitions[index].Width = width;
            });
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
