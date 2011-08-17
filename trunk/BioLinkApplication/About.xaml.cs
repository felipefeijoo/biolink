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
using System.Reflection;
using BioLink.Client.Extensibility;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window {

        public About() {
            InitializeComponent();
            var v = this.GetType().Assembly.GetName().Version;
            var version = String.Format("Version {0}.{1} (build {2}) {3}", v.Major, v.Minor, v.Revision, BuildLabel.BUILD_LABEL);
            lblVersion.Content = version;

            var model = new List<PluginVersionInfo>();
            PluginManager.Instance.TraversePlugins((plugin) => {
                model.Add(plugin.Version);
            });

            lvw.ItemsSource = model;
        }

        private void label2_MouseUp(object sender, MouseButtonEventArgs e) {            
            ShowCredits();
        }

        private void ShowCredits() {
            var frm = new ThirdPartyLicenses();
            frm.Owner = this;
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
        }

    }
    
}
