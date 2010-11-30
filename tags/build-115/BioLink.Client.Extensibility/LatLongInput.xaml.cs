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
        private bool _selfChanged = false;

        public LatLongInput() {
            InitializeComponent();
            Axis = CoordinateType.Latitude;
            Mode = LatLongMode.DegreesMinutesSeconds;
            lblAxis.Content = this.Resources["LatLong.lblAxis." + Axis.ToString()];
            txtDegrees.TextChanged += new TextChangedEventHandler((s, e) => { RecalculateValue(); });
            txtMinutes.TextChanged += new TextChangedEventHandler((s, e) => { RecalculateValue(); });
            txtSeconds.TextChanged += new TextChangedEventHandler((s, e) => { RecalculateValue(); });
            cmbDirection.SelectionChanged += new SelectionChangedEventHandler((s, e) => { RecalculateValue(); });
        }

        public CoordinateType Axis {
            get { return _coordType; }
            set {
                _coordType = value;
                if (lblAxis != null) {
                    lblAxis.Content = this.Resources["LatLong.lblAxis." + value.ToString()];
                }
                if (cmbDirection != null) {
                    var list = new String[] { "E", "W" };
                    if (_coordType == CoordinateType.Latitude) {
                        list = new String[] { "N", "S" };
                    }
                    cmbDirection.ItemsSource = list;
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

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", 
            typeof(double), 
            typeof(LatLongInput), 
            new FrameworkPropertyMetadata((double) 0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged)
        );

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            
            var control = obj as LatLongInput;
            if (control != null) {
                double newValue = (double)args.NewValue;
                if (!control._selfChanged) {
                    
                    switch (control.Mode) {
                        case LatLongMode.DegreesMinutesSeconds:
                            string direction;
                            int degrees, minutes, seconds;
                            GeoUtils.DecDegToDMS(newValue, control._coordType, out degrees, out minutes, out seconds, out direction);
                            control.txtDegrees.Text = degrees + "";
                            control.txtMinutes.Text = minutes + "";
                            control.txtSeconds.Text = seconds + "";
                            control.cmbDirection.SelectedItem = direction;
                            break;
                    }
                }

                if (control.CoordinateValueChanged != null) {
                    control.CoordinateValueChanged(control, newValue);
                }
            }

        }

        public double Value {
            get { return (double) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void RecalculateValue() {

            double newValue = 0;
            
            switch (Mode) {
                case LatLongMode.DecimalDegrees:
                    newValue = Double.Parse(txtDegrees.Text);
                    break;
                case LatLongMode.DegreesDecimalMinutes:
                    var deg = SafeParse(txtDegrees.Text);
                    var minutes = Double.Parse(txtMinutes.Text);
                    newValue = GeoUtils.DDecMToDecDeg(deg, minutes);
                    break;
                case LatLongMode.DegreesDecimalMinutesDirection:
                    deg = SafeParse(txtDegrees.Text);
                    minutes = Double.Parse(txtMinutes.Text);
                    newValue = GeoUtils.DDecMDirToDecDeg(deg, minutes, cmbDirection.Text);
                    break;
                case LatLongMode.DegreesMinutesSeconds:
                    deg = SafeParse(txtDegrees.Text);
                    int min = SafeParse(txtMinutes.Text);
                    int seconds = SafeParse(txtSeconds.Text);
                    newValue = GeoUtils.DMSToDecDeg(deg, min, seconds, cmbDirection.SelectedItem as string);
                    break;
                default:
                    throw new Exception("Recalculate: Mode not handled: " + Mode.ToString());
            }
            _selfChanged = true;                 
            this.Value = newValue;
            _selfChanged = false;
        }

        private int SafeParse(string text) {
            int result ;
            if (Int32.TryParse(text, out result)) {
                return result;
            } else {
                return 0;
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

        public event CoordinateValueChangedHandler CoordinateValueChanged;

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

    public delegate void CoordinateValueChangedHandler(object source, double value);
}
