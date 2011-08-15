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

namespace BioLink.Client.Gazetteer {
    /// <summary>
    /// Interaction logic for DistanceDirectionControl.xaml
    /// </summary>
    public partial class DistanceDirectionControl : UserControl {
        public DistanceDirectionControl() {
            InitializeComponent();

            string[] units = new String[] { "km", "miles" };
            cmbUnits.ItemsSource = units;
            cmbUnits.SelectedItem = units[0];

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(DistanceDirectionControl_DataContextChanged);
            txtLatitude.TextChanged += new TextChangedEventHandler(txtLatitude_TextChanged);
            txtLongitude.TextChanged += new TextChangedEventHandler(txtLongitude_TextChanged);
            cmbUnits.SelectionChanged += new SelectionChangedEventHandler(cmbUnits_SelectionChanged);                
        }

        void cmbUnits_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Recalculate();
        }

        void txtLongitude_TextChanged(object sender, TextChangedEventArgs e) {
            Recalculate();
        }

        void txtLatitude_TextChanged(object sender, TextChangedEventArgs e) {
            Recalculate();
        }

        void DistanceDirectionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Clear();
        }

        public void Clear() {
            txtLatitude.Text = "";
            txtLongitude.Text = "";
        }

        private void Recalculate() {
            txtResults.Text = "";
           var place = this.DataContext as PlaceNameViewModel;
           if (place != null) {
               double lat, lon;
               string error = "";
               if (GeoUtils.DMSStrToDecDeg(txtLatitude.Text, CoordinateType.Latitude, out lat, out error)) {
                   if (GeoUtils.DMSStrToDecDeg(txtLongitude.Text, CoordinateType.Longitude, out lon, out error)) {
                       string direction = GeoUtils.GreatCircleArcDirection(place.Latitude, place.Longitude, lat, lon, 32);
                       string distance = String.Format("{0:0.00}", GeoUtils.GreatCircleArcLength(place.Latitude, place.Longitude, lat, lon, cmbUnits.SelectedItem as string));
                       txtResults.Text = String.Format("{0} {1} {2}", distance, cmbUnits.SelectedItem as string, direction);
                   }
               }

               if (!String.IsNullOrEmpty(error)) {
                   txtResults.Text = error;
               }
           } else {
               txtResults.Text = "You must select a place name first";
           }
        }
    }
}
