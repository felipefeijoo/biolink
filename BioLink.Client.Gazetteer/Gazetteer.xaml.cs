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

namespace BioLink.Client.Gazetteer {
    /// <summary>
    /// Interaction logic for Gazetteer.xaml
    /// </summary>
    public partial class Gazetteer : UserControl {
        public Gazetteer() {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e) {
            GazetteerService svc = new GazetteerService("c:/zz/Auslig.sqlite");
            List<PlaceName> names = svc.FindPlaceNames("Wagga");
            Logger.Debug("Place names: {0}", names);
        }
    }
}
