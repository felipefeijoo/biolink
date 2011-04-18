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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for SingleModelOptionsControl.xaml
    /// </summary>
    public partial class SingleModelOptionsControl : UserControl {
        public SingleModelOptionsControl() {
            InitializeComponent();

            var models = PluginManager.Instance.GetExtensionsOfType<DistributionModel>();
            cmbModelType.ItemsSource = models;
            cmbModelType.SelectedIndex = 0;
            txtFilename.Text = TempFileManager.NewTempFilename("grd", "model");
        }
    }
}
