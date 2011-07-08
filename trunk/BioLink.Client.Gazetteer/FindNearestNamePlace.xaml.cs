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
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.ComponentModel;

namespace BioLink.Client.Gazetteer {

    public partial class FindNearestNamePlace : Window {

        public FindNearestNamePlace(GazetterPlugin plugin) {
            InitializeComponent();
            this.Plugin = plugin;

            if (plugin == null || plugin.CurrentGazetteer == null) {
                ErrorMessage.Show("Please select a gazeeteer file before using this tool.");
                this.Loaded += new RoutedEventHandler((source, e) => {
                    this.Close();
                });                
                return;
            }

            lblGaz.Content = "Using " + Service.FileName;

            var places = Service.GetPlaceTypes();
            cmbPlaceType.ItemsSource = places;

            if (plugin.CurrentSelectedPlace != null) {
                var selected = plugin.CurrentSelectedPlace;
                ctlPosition.Latitude = selected.Latitude;
                ctlPosition.Longitude = selected.Longitude;
            }

            ListViewDragHelper.Bind(lstResults, CreateDragData);
        }

        private DataObject CreateDragData(ViewModelBase dragged) {
            var selected = dragged as PlaceNameViewModel;
            if (selected != null) {
                var data = new DataObject("Pinnable", selected);
                var pinnable = new PinnableObject(GazetterPlugin.GAZETTEER_PLUGIN_NAME, LookupType.PlaceName, 0, selected.Model);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);
                return data;
            }

            return null;
        }


        private void btnClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            DoSearch();
        }

        protected GazetterPlugin Plugin { get; private set; }

        protected GazetteerService Service { 
            get { return Plugin.CurrentGazetteer; } 
        }

        protected Boolean Cancelled { get; set; }

        private void DoSearch() {

            if (Service == null) {
                return;
            }

            string division = null;
            if (chkRestrictPlaceType.IsChecked.ValueOrFalse() && !string.IsNullOrEmpty(cmbPlaceType.Text)) {
                division = cmbPlaceType.SelectedItem as string;
            }

            btnCancel.IsEnabled = true;
            btnSearch.IsEnabled = false;

            Cancelled = false;
            var delta = 0.2; // degrees to go out each time...
            var latitude = ctlPosition.Latitude;
            var longitude = ctlPosition.Longitude;
            var info = Service.GetGazetteerInfo();
            int maxResults = Int32.Parse(txtMaxResults.Text);
            maxResults = Math.Min(maxResults, info.RecordCount);
            JobExecutor.QueueJob(() => {                
                double range = 0;
                int count = 0;
                try {
                    double x1, y1, x2, y2;

                    while (count < maxResults && range < 50 && !Cancelled) {
                        StatusMessage("Searching for places near {0} (+/- {1} degrees). {2} places found.", GeoUtils.FormatCoordinates(latitude, longitude), range, count);
                        range += delta;
                        y1 = latitude - range;
                        y2 = latitude + range;
                        x1 = longitude - range;
                        x2 = longitude + range;
                        count = Service.CountPlacesInBoundedBox(x1, y1, x2, y2, division);
                    }


                    this.InvokeIfRequired(() => {
                        this.Cursor = Cursors.Wait;
                    });

                    StatusMessage("Retrieving places near {0} (+/- {1} degrees). {2} places found.", GeoUtils.FormatCoordinates(latitude, longitude), range, count);

                    y1 = latitude - range;
                    y2 = latitude + range;
                    x1 = longitude - range;
                    x2 = longitude + range;

                    var list = Service.GetPlacesInBoundedBox(x1, y1, x2, y2, division);
                    var model = new List<PlaceNameViewModel>( list.Select((m) => {
                        var vm = new PlaceNameViewModel(m);
                        vm.Distance = GeoUtils.GreatCircleArcLength(m.Latitude, m.Longitude, latitude, longitude, "km");
                        string direction = "";
                        var numPoints = (vm.Distance < 10 ? 16 : 32);
                        direction = GeoUtils.GreatCircleArcDirection(m.Latitude, m.Longitude, latitude, longitude, numPoints);
                        vm.Offset = String.Format("{0:0.0} {1} {2}", vm.Distance, "KM", direction);
                        return vm;
                    }));

                    model.Sort(new Comparison<PlaceNameViewModel>((o1, o2) => {
                        return (int) (o1.Distance - o2.Distance);
                    }));

                    model.RemoveRange(maxResults, model.Count - maxResults);

                    lstResults.InvokeIfRequired(() => {
                        lstResults.ItemsSource = model;
                        CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lstResults.ItemsSource);
                        myView.SortDescriptions.Add(new SortDescription("Distance", ListSortDirection.Ascending));
                    });

                    if (!Cancelled) {
                        StatusMessage("{0} places retrieved.", list.Count);
                    } else {
                        StatusMessage("{0} places retrieved (Cancelled).", list.Count);
                    }
                    
                } finally {
                    this.InvokeIfRequired(() => {
                        btnCancel.IsEnabled = false;
                        btnSearch.IsEnabled = true;
                        this.Cursor = Cursors.Arrow;
                    });
                }
                
            });
        }

        private void StatusMessage(string message, params object[] args) {
            lblStatus.InvokeIfRequired(() => {
                lblStatus.Content = string.Format(message, args);
                lblStatus.UpdateLayout();
            });
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Cancelled = true;
        }

        private void btnPlot_Click(object sender, RoutedEventArgs e) {
            ShowOnMap();
        }

        private void ShowOnMap() {
            var map = PluginManager.Instance.GetMap();
            if (map != null) {
                var label = GeoUtils.FormatCoordinates(ctlPosition.Latitude, ctlPosition.Longitude);
                map.Show();                
                var set = new ListMapPointSet("Named places near " + label);                
                foreach (PlaceNameViewModel vm in lstResults.ItemsSource) {
                    set.Add(new MapPoint { Label = vm.Name, Latitude = vm.Latitude, Longitude = vm.Longitude });
                }

                map.PlotPoints(set);
                map.DropAnchor(ctlPosition.Longitude, ctlPosition.Latitude, "");
            }
        }

    }
}
