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
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PositionControl.xaml
    /// </summary>
    public partial class PositionControl : UserControl {

        public PositionControl() {
            InitializeComponent();
            lon.CoordinateValueChanged += new CoordinateValueChangedHandler(lon_CoordinateValueChanged);
            lat.CoordinateValueChanged += new CoordinateValueChangedHandler(lat_CoordinateValueChanged);

            AllowDrop = true;

            this.PreviewDragOver += new DragEventHandler(PositionControl_PreviewDragEnter);
            this.PreviewDragEnter += new DragEventHandler(PositionControl_PreviewDragEnter);
            this.Drop += new DragEventHandler(PositionControl_Drop);
            this.DragEnter += new DragEventHandler(PositionControl_DragEnter);
            this.DragOver += new DragEventHandler(PositionControl_DragEnter);

            HookLatLongControl(lat);
            HookLatLongControl(lon);

            grid.RowDefinitions[0].Height = new GridLength(0);
            grid.RowDefinitions[1].Height = new GridLength(0);
            grid.ColumnDefinitions[1].Width = new GridLength(68);
            lblLon.Visibility = System.Windows.Visibility.Visible;

            if (GoogleEarth.IsInstalled()) {
                grid.ColumnDefinitions[5].Width = new GridLength(4);
                grid.ColumnDefinitions[6].Width = new GridLength(23);
            } else {
                grid.ColumnDefinitions[5].Width = new GridLength(0);
                grid.ColumnDefinitions[6].Width = new GridLength(0);
            }

        }

        private void HookLatLongControl(LatLongInput ctl) {
            HookTextBox(ctl.txtDegrees);
            HookTextBox(ctl.txtMinutes);
            HookTextBox(ctl.txtSeconds);
        }

        public void Clear() {
            lon.Clear();
            lat.Clear();
        }

        private void HookTextBox(System.Windows.Controls.TextBox box) {
            box.AllowDrop = true;

            box.PreviewDragEnter += new DragEventHandler(PositionControl_PreviewDragEnter);
            box.PreviewDragOver += new DragEventHandler(PositionControl_PreviewDragEnter);
            box.PreviewDrop += new DragEventHandler(PositionControl_Drop);
        }

        void PositionControl_DragEnter(object sender, DragEventArgs e) {
            e.Handled = true;
        }

        void PositionControl_PreviewDragEnter(object sender, DragEventArgs e) {

            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null) {
                if (pinnable.LookupType == LookupType.PlaceName) {
                    e.Effects = DragDropEffects.Link;
                }
            } else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        void PositionControl_Drop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null && pinnable.LookupType == LookupType.PlaceName) {
                PlaceName placeName = pinnable.GetState<PlaceName>();
                this.lat.Value = placeName.Latitude;
                lon.Value = placeName.Longitude;
            }
            e.Handled = true;
        }

        void lat_CoordinateValueChanged(object source, double value) {
            this.Latitude = lat.Value;
        }

        void lon_CoordinateValueChanged(object source, double value) {
            this.Longitude = lon.Value;
        }

        public LatLongMode Mode {
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
                control.btnGoogleCode.IsEnabled = !val;
            }

        }

        public static readonly DependencyProperty ShowHeaderLabelsProperty = DependencyProperty.Register("ShowHeaderLabels", typeof(bool), typeof(PositionControl), new FrameworkPropertyMetadata(false, OnShowHeaderLabelsChanged));

        public bool ShowHeaderLabels {
            get { return (bool)GetValue(ShowHeaderLabelsProperty); }
            set { SetValue(ShowHeaderLabelsProperty, value); }
        }

        private static void OnShowHeaderLabelsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {

            var control = obj as PositionControl;
            if (control != null) {
                bool val = (bool)args.NewValue;

                if (val) {
                    control.grid.RowDefinitions[0].Height = new GridLength(18);
                    control.grid.RowDefinitions[1].Height = new GridLength(2);
                    control.grid.ColumnDefinitions[1].Width = new GridLength(4);
                    control.lblLon.Visibility = System.Windows.Visibility.Collapsed;
                } else {
                    control.grid.RowDefinitions[0].Height = new GridLength(0);
                    control.grid.RowDefinitions[1].Height = new GridLength(0);
                    control.grid.ColumnDefinitions[1].Width = new GridLength(68);
                    control.lblLon.Visibility = System.Windows.Visibility.Visible;
                }
            }

        }


        private void btnEgaz_Click(object sender, RoutedEventArgs e) {
            LaunchGazetteer();
        }

        private void LaunchGazetteer() {
            PluginManager.Instance.StartSelect<PlaceName>((result) => {
                var place = result.DataObject as PlaceName;
                if (place != null) {
                    lat.Value = place.Latitude;
                    lon.Value = place.Longitude;
                    if (PositionChanged != null) {
                        PositionChanged(place, "EGaz");
                    }
                }
            });
        }

        public event Action<PlaceName, string> PositionChanged;

        private void btnGoogleCode_Click(object sender, RoutedEventArgs e) {
            GoogleEarth.GeoTag((lat, lon, altitude) => {
                this.lat.Value = lat;
                this.lon.Value = lon;
            }, this.lat.Value, this.lon.Value);
        }

    }

}
