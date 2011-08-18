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
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Data;
using System.Collections.ObjectModel;

namespace BioLink.DatabaseUtilities {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private ObservableCollection<HierarchicalViewModelBase> _model;

        public MainWindow() {
            InitializeComponent();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            _model = new ObservableCollection<HierarchicalViewModelBase>(DiscoverServers(true));
            tvw.ItemsSource = _model;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {

        }

        private List<ServerViewModel> DiscoverServers(bool search) {

            var list = new List<ServerViewModel>();

            var profiles = Config.GetGlobal<List<ConnectionProfile>>("connection.profiles", new List<ConnectionProfile>());
            foreach (ConnectionProfile profile in profiles) {
                var existing = list.FirstOrDefault((sv) => {
                    return sv.Name.Equals(profile.Server, StringComparison.CurrentCultureIgnoreCase);
                });
                if (existing == null) {
                    list.Add(CreateServerViewModel(profile.Server));
                }
            }

            LegacySettings.TraverseSubKeys("Client", "UserProfiles", (key) => {
                ConnectionProfile profile = new ConnectionProfile();
                var server = key.GetValue("DatabaseServer") as string;

                var existing = list.FirstOrDefault((sv) => {
                    return sv.Name.Equals(server, StringComparison.CurrentCultureIgnoreCase);
                });

                if (server != null && existing == null) {
                    list.Add(CreateServerViewModel(server));                    
                }
            });

            if (search) {
                DataTable dt = SmoApplication.EnumAvailableSqlServers(false);
                if (dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        var server = dr["Name"] as string;
                        var existing = list.FirstOrDefault((sv) => {
                            return sv.Name.Equals(server, StringComparison.CurrentCultureIgnoreCase);
                        });

                        if (!string.IsNullOrEmpty(server) && existing == null) {
                            list.Add(CreateServerViewModel(server));
                        }
                    }
                }
            }

            return list;
        }

        private void tvw_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null) {
                item.Focus();
                e.Handled = true;
            }
        }

        private ServerViewModel CreateServerViewModel(string name) {

            var model = new ServerViewModel { Name = name };

            return model;
        }

    }

    public class ServerViewModel : HierarchicalViewModelBase {

        public ServerViewModel() : base() { }

        public override ImageSource Icon {
            get {
                return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/DistributionRegion.png");
            }
            set {
                base.Icon = value;
            }
        }

        public override string DisplayLabel {
            get { return Name; }
        }

        public String Name { get; set; }

        public override int? ObjectID {
            get { return null; }
        }

    }
}
