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
    /// Interaction logic for OffsetControl.xaml
    /// </summary>
    public partial class OffsetControl : UserControl {

        private PlaceNameViewModel _viewModel;


        public OffsetControl() {
            InitializeComponent();

            string[] directions = new String[] { "N", "NE", "NW", "NNE", "NNW", "NbyE", "NbyW", "NEbyN", "NEbyE", "NWbyN", "NWbyW", "E", "ENE", "ESE", "EbyN", "EbyS", "S", "SE", "SW", "SSE", "SSW", "SbyE", "SbyW", "SEbyS", "SEbyE", "SWbyS", "SWbyW", "W", "WNW", "WSW", "WbyN", "WbyS" };

            cmbDirection.ItemsSource = directions;

            string[] units = new String[] { "km", "miles" };

            cmbUnits.ItemsSource = units;

            cmbUnits.SelectedItem = units[0];
            cmbDirection.SelectedItem = directions[0];

            if (DataContext as PlaceNameViewModel != null) {
                var source = DataContext as PlaceNameViewModel;
                var pinnable = new PinnableObject(GazetterPlugin.GAZETTEER_PLUGIN_NAME, LookupType.PlaceName, 0, source.Model);
                // This actually creates a copy. 
                _viewModel = new PlaceNameViewModel(pinnable.GetState<PlaceName>());

                cmbDirection.DataContext = _viewModel;
                cmbUnits.DataContext = _viewModel;
                txtDistance.DataContext = _viewModel;
            }

        }
    }
}
