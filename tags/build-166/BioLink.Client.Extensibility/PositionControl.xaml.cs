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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PositionControl.xaml
    /// </summary>
    public partial class PositionControl : UserControl {

        public PositionControl() {
            InitializeComponent();
            lon.CoordinateValueChanged += new CoordinateValueChangedHandler(lon_CoordinateValueChanged);
            lat.CoordinateValueChanged += new CoordinateValueChangedHandler(lat_CoordinateValueChanged);
        }

        void lat_CoordinateValueChanged(object source, double value) {
            this.Latitude = lat.Value;
        }

        void lon_CoordinateValueChanged(object source, double value) {
            this.Longitude = lon.Value;
        }

        public bool HeaderLabels {
            get {
                return grid.ColumnDefinitions[1].Width.Value != 0;
            }

            set {

                if (value) {
                    grid.RowDefinitions[0].Height = new GridLength(18);
                    grid.RowDefinitions[1].Height = new GridLength(2);
                    grid.ColumnDefinitions[1].Width = new GridLength(4);
                    lblLon.Visibility = System.Windows.Visibility.Collapsed;
                } else {
                    grid.RowDefinitions[0].Height = new GridLength(0);
                    grid.RowDefinitions[1].Height = new GridLength(0);
                    grid.ColumnDefinitions[1].Width = new GridLength(68);
                    lblLon.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public LatLongInput.LatLongMode Mode {
            get { return lon.Mode; }

            set {
                lon.Mode = value;
                lat.Mode = value;
            }
        }

        public static readonly DependencyProperty LatitudeProperty = DependencyProperty.Register("Latitude", typeof(double), typeof(PositionControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(LatChanged)));

        public double Latitude {
            get { return (double) GetValue(LatitudeProperty); }
            set { SetValue(LatitudeProperty, value); }
        }

        private static void LatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = obj as PositionControl;
            if (control != null) {
                control.lat.Value = (double)args.NewValue;
            }

        }

        public static readonly DependencyProperty LongitudeProperty = DependencyProperty.Register("Longitude", typeof(double), typeof(PositionControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(LonChanged)));

        public double Longitude {
            get { return (double)GetValue(LongitudeProperty); }
            set { SetValue(LongitudeProperty, value); }
        }

        private static void LonChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = obj as PositionControl;
            if (control != null) {
                control.lon.Value = (double)args.NewValue;
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PositionControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsReadOnlyChanged));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {

            var control = obj as PositionControl;
            if (control != null) {
                bool val = (bool)args.NewValue;
                control.lat.IsReadOnly = val;
                control.lon.IsReadOnly = val;
                control.btnEgaz.IsEnabled = !val;
            }

        }

    }

}
