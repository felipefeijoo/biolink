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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Gazetteer {
    /// <summary>
    /// Interaction logic for OffsetControl.xaml
    /// </summary>
    public partial class OffsetControl : UserControl {
        
        private Point _startPoint;
        private bool _IsDragging;
        
        public OffsetControl() {
            InitializeComponent();

            string[] directions = new String[] { "N", "NE", "NW", "NNE", "NNW", "NbyE", "NbyW", "NEbyN", "NEbyE", "NWbyN", "NWbyW", "E", "ENE", "ESE", "EbyN", "EbyS", "S", "SE", "SW", "SSE", "SSW", "SbyE", "SbyW", "SEbyS", "SEbyE", "SWbyS", "SWbyW", "W", "WNW", "WSW", "WbyN", "WbyS" };

            cmbDirection.ItemsSource = directions;

            string[] units = new String[] { "km", "miles" };

            cmbUnits.ItemsSource = units;

            cmbUnits.SelectedItem = units[0];
            cmbDirection.SelectedItem = directions[0];

            cmbDirection.SelectionChanged += new SelectionChangedEventHandler(cmbDirection_SelectionChanged);
            cmbUnits.SelectionChanged += new SelectionChangedEventHandler(cmbUnits_SelectionChanged);
            txtDistance.TextChanged += new TextChangedEventHandler(txtDistance_TextChanged);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(OffsetControl_DataContextChanged);

            txtResults.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(txt_PreviewMouseLeftButtonDown);
            txtResults.PreviewMouseMove += new MouseEventHandler(txt_PreviewMouseMove);

        }

        void txt_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, txtResults);
        }

        void txt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(txtResults);
        }

        private void CommonPreviewMouseMove(MouseEventArgs e, System.Windows.Controls.TextBox txt) {

            if (_startPoint == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(txt);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (txt.Tag != null) {
                        StartDrag(e, txt);
                    }
                }
            }
        }

        private void StartDrag(MouseEventArgs mouseEventArgs, System.Windows.Controls.TextBox txt) {

            var selected = txt.Tag as PlaceNameViewModel;
            if (selected != null) {
                var data = new DataObject("Pinnable", selected);

                var pinnable = new PinnableObject(GazetterPlugin.GAZETTEER_PLUGIN_NAME, LookupType.PlaceName, 0, selected.Model);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);

                try {
                    _IsDragging = true;
                    DragDrop.DoDragDrop(txt, data, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                } finally {
                    _IsDragging = false;
                }
            }

            InvalidateVisual();
        }


        void txtDistance_TextChanged(object sender, TextChangedEventArgs e) {
            CalculateOffsetPosition();
        }

        void cmbUnits_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            CalculateOffsetPosition();
        }

        void cmbDirection_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            CalculateOffsetPosition();
        }

        void OffsetControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Clear();
        }

        private void CalculateOffsetPosition() {
            double dist = 0;
            var place = DataContext as PlaceNameViewModel;
            txtResults.Text = "";
            txtResults.Tag = null;
            if (place != null) {
                if (double.TryParse(txtDistance.Text, out dist)) {
                    OffsetPlace = OffsetLatLong(dist, cmbUnits.SelectedItem as string, cmbDirection.SelectedItem as string, place.Model);
                    if (OffsetPlace != null) {
                        txtResults.Text = OffsetPlace.LatitudeString + " " + OffsetPlace.LongitudeString;
                        txtResults.Tag = new PlaceNameViewModel(OffsetPlace);
                    }

                    if (SelectedPlaceNameChanged != null) {
                        SelectedPlaceNameChanged(OffsetPlace);
                    }
                }
            }
        }

        private PlaceName OffsetLatLong(double offset, string Units, string Direction, PlaceName source) {


            // set degrees per unit as follows:
            // mean radius of earth is 3959 miles or 6371 kilometers.
            // circumference = 2 x Pi x radius.
            // DegsPerUnit = 360 / circumference.
            double DegsPerUnit = 0;
            if (Units.ToUpper()[0] == 'K') { // Units = Kilometers
                DegsPerUnit = 0.0089932;
            } else if (Units.ToUpper().Substring(0, 2) == "MI") { // Units = Miles
                DegsPerUnit = 0.0144722;
            } else {                                          // Units undefined
                return null;
            }

            double DirDeg = 0;
            // change compass directions into degrees.  degrees are measured E & W from N and E & W from S. there are 32 compass points. 360/32 = 11.25 degrees each.
            switch (Direction) {

                case "N":
                case "S":
                    DirDeg = 0 * 11.25;
                    break;
                case "NbyE":
                case "NbyW":
                case "SbyE":
                case "SbyW":
                    DirDeg = 1 * 11.25;
                    break;
                case "NNE":
                case "NNW":
                case "SSE":
                case "SSW":
                    DirDeg = 2 * 11.25;
                    break;
                case "NEbyN":
                case "NWbyN":
                case "SEbyS":
                case "SWbyS":
                    DirDeg = 3 * 11.25;
                    break;
                case "NE":
                case "NW":
                case "SE":
                case "SW":
                    DirDeg = 4 * 11.25;
                    break;
                case "NEbyE":
                case "NWbyW":
                case "SEbyE":
                case "SWbyW":
                    DirDeg = 5 * 11.25;
                    break;
                case "ENE":
                case "WNW":
                case "ESE":
                case "WSW":
                    DirDeg = 6 * 11.25;
                    break;
                case "EbyN":
                case "WbyN":
                case "EbyS":
                case "WbyS":
                    DirDeg = 7 * 11.25;
                    break;
                case "E":
                case "W":
                    DirDeg = 8 * 11.25;
                    break;
                default:
                    // error in direction
                    return null;
            }

            int NSHem = 0;
            int EWHem = 0;
            int NSDir = 0;
            int EWDir = 0;

            string InitLat = source.LatitudeString;
            string InitLong = source.LongitudeString;

            // find what hemisphere we're in & set to positive or negative
            if (InitLat.EndsWith("N")) {
                NSHem = 1;          // set to positive
            } else {
                NSHem = -1;         // set to negative
            }

            if (InitLong.EndsWith("E")) {
                EWHem = 1;          // set to pos
            } else {
                EWHem = -1;        // set to neg
            }

            // find what quadrant we're in & set to positive or negative
            if (Direction.Contains("N")) {
                NSDir = 1;         // set to pos
            } else {
                NSDir = -1;         // set to neg
            }

            if (Direction.Contains("E")) {
                EWDir = 1;         // set to pos
            } else {
                EWDir = -1;        // set to neg
            }


            int NSCor = NSHem * NSDir;   // set correction (adjustment) to pos or neg
            int EWCor = EWHem * EWDir;   // pos*pos=pos, neg*neg=pos, pos*neg=neg

            string ILatDir = InitLat.Substring(InitLat.Length - 1);                     // get lat direction
            string ILonDir = InitLong.Substring(InitLong.Length - 1);                   // get long direction

            string strError;
            double ILat = 0;
            double ILon = 0;

            if (!GeoUtils.DMSStrToDecDeg(InitLat, CoordinateType.Latitude, out ILat, out strError)) {
                ErrorMessage.Show(strError);
                return null;
            }

            if (!GeoUtils.DMSStrToDecDeg(InitLong, CoordinateType.Longitude, out ILon, out strError)) {
                ErrorMessage.Show(strError);
                return null;
            }

            ILon = Math.Abs(ILon);
            ILat = Math.Abs(ILat);

            double CosDir = Math.Cos(DirDeg * 0.017453292);         // get cosine of direction (.017+ is Pi/180, and changes degrees to radians as required by basic)
            double LatOff = CosDir * offset;                        // Latitude adj in units
            double LatAdj = LatOff * DegsPerUnit * NSCor;            // convert to degrees & make + or -
            double NewLat = ILat + LatAdj;                          // new latitude in degrees. a negative is minus
            if (NewLat < 0) {                                       // we've crossed the Equator
                NewLat = Math.Abs(LatAdj) - ILat;                   // put remainder on other side
                if (ILatDir == "N") {                               // change hemispheres
                    ILatDir = "S";
                } else {
                    ILatDir = "N";
                }
            }

            double SinDir = Math.Sin(DirDeg * 0.017453292);         // get sine of direction
            double LonOff = SinDir * offset;                        // Longitude adj in units
            double CosNewLat = Math.Cos(NewLat * 0.017453292);      // get cosine of new latitude
            double NewDPU = DegsPerUnit / CosNewLat;                // correct DPU for new latitude
            double LonAdj = LonOff * NewDPU * EWCor;                // convert to degrees & make + or -
            double NewLon = ILon + LonAdj;                          // new longitude in degrees
            if (NewLon < 0 || NewLon > 180) {                       // we've crossed 0 or 180 meridian
                if (NewLon < 0) {
                    NewLon = Math.Abs(LonAdj) - ILon;               // put diff on other side
                }

                if (NewLon > 180) {
                    NewLon = 180 - (NewLon - 180);                  // subtract overage fm
                }

                if (ILonDir == "E") {                               // change hemispheres          '180
                    ILonDir = "W";
                } else {
                    ILonDir = "E";
                }

            }

            int intLongSign = 1;

            if (ILonDir == "W") {
                intLongSign = -1;
            }

            int intLatSign = 1;

            if (ILatDir == "S") {
                intLatSign = -1;
            }

            var pn = new PlaceName();

            pn.PlaceNameType = PlaceNameType.OffsetAndDirection;
            pn.Name = source.Name;
            pn.Division = source.Division;
            pn.PlaceType = source.PlaceType;

            pn.Latitude = NewLat * intLatSign;
            pn.Longitude = NewLon * intLongSign;

            pn.Offset = offset + "";
            pn.Units = Units;
            pn.Direction = Direction;

            pn.LatitudeString = GeoUtils.DecDegToDMS(pn.Latitude, CoordinateType.Latitude);
            pn.LongitudeString = GeoUtils.DecDegToDMS(pn.Longitude, CoordinateType.Longitude);

            return pn;

        }

        public PlaceName OffsetPlace { get; private set; }


        internal void Clear() {
            txtDistance.Clear();
        }

        public event Action<PlaceName> SelectedPlaceNameChanged;
    }
}
