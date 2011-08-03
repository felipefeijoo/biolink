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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for GoogleEarthGeoTag.xaml
    /// </summary>
    public partial class GoogleEarthGeoTag : Window {

        public GoogleEarthGeoTag(Action selectAction) {
            InitializeComponent();
            this.SelectAction = selectAction;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (SelectAction != null) {
                SelectAction();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {            
            this.Close();
        }

        private Action SelectAction { get; set; }

    }
}
