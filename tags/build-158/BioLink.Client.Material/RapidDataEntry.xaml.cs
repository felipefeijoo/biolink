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
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for RapidDataEntry.xaml
    /// </summary>
    public partial class RapidDataEntry : UserControl {

        public RapidDataEntry() {
            InitializeComponent();

            int[] ids = new int[] { 3132, 3215, 3878, 4606 };

            var user = PluginManager.Instance.User;

            var service = new MaterialService(user);
            var sites = service.GetRDESites(ids);

            grpSites.Items = new ObservableCollection<ViewModelBase>(sites.ConvertAll((m) => { return (ViewModelBase)new RDESiteViewModel(m); }));
            grpSites.Content = new SiteRDEControl(user);

        }

        private void grpSites_AddNewClicked(object sender, RoutedEventArgs e) {
            var g = sender as ItemsGroupBox;
            if (g != null) {
                var model = new RDESite();
                model.SiteID = -1;
                model.Locality = "<New site>";

                var viewModel = new RDESiteViewModel(model);
                g.Items.Add(viewModel);
                g.SelectedItem = viewModel;
            }

        }

    }
}
