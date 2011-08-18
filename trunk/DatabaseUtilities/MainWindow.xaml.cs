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

            tvw.MouseRightButtonUp += new MouseButtonEventHandler(tvw_MouseRightButtonUp);
            tvw.MouseDoubleClick += new MouseButtonEventHandler(tvw_MouseDoubleClick);
        }

        void tvw_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selected = tvw.SelectedItem as HierarchicalViewModelBase;
            if (selected != null) {
                if (selected is ServerViewModel) {
                    Connect(selected as ServerViewModel);
                }
            }
        }

        void tvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            var selected = tvw.SelectedItem as HierarchicalViewModelBase;
            tvw.ContextMenu = null;
            if (selected != null) {
                var builder = new ContextMenuBuilder(null);
                if (selected is ServerViewModel) {
                    var server = selected as ServerViewModel;
                    builder.New("Connect...").Handler(() => Connect(server)).Enabled(server.Server == null).End();
                    builder.Separator();
                    builder.New("Disconnect").Handler(() => Disconnect(server)).Enabled(server.Server != null).End();

                    tvw.ContextMenu = builder.ContextMenu;
                }
            }
        }

        private void Disconnect(ServerViewModel server) {
            if (server != null && server.Server != null) {
                server.Server.ConnectionContext.Disconnect();
                server.Server = null;
                server.Children.Clear();
            }
        }

        private void Connect(ServerViewModel server) {
            var frm = new LoginWindow(server.Name);
            frm.Owner = this;
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog() == true) {
                server.Server = frm.Server;
                foreach (Database db in server.Server.Databases) {
                    if (db.Tables.Contains("tblMaterial") && db.Tables.Contains("tblBiota")) {
                        var vm = new DatabaseViewModel(db);
                        server.Children.Add(vm);
                    }
                }
                server.IsExpanded = true;
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e) {

            StatusMessage("Please wait while a list of available servers is being built...");

            this.Cursor = Cursors.Wait;

            JobExecutor.QueueJob(() => {
                try {
                    var list = DiscoverServers(true);
                    tvw.InvokeIfRequired(() => {
                        _model = new ObservableCollection<HierarchicalViewModelBase>(list);
                        tvw.ItemsSource = _model;
                    });
                } finally {
                    this.InvokeIfRequired(() => {
                        this.Cursor = Cursors.Arrow;
                        StatusMessage(string.Format("{0} servers found.", _model.Count));
                    });
                }

            });

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            ShutDown();
        }

        private void ShutDown() {
        }

        private void StatusMessage(string message) {
            txtMessage.InvokeIfRequired(() => {
                txtMessage.Text = message;
            });
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

        public Server Server { get; set; }

    }

    public class DatabaseViewModel : HierarchicalViewModelBase {

        public DatabaseViewModel(Database model) {
            this.Database = model;
        }

        public Database Database { get; private set; }

        public override int? ObjectID {
            get { return null; }
        }

        public override string DisplayLabel {
            get { return Database.Name; }            
        }


    }
}
