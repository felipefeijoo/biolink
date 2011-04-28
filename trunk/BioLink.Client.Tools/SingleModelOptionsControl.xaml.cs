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

            cmbModelType.SelectionChanged += new SelectionChangedEventHandler(cmbModelType_SelectionChanged);
        }

        void cmbModelType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = cmbModelType.SelectedItem as DistributionModel;
            if (selected != null) {
                txtCutOff.IsEnabled = !selected.PresetCutOff.HasValue;
                txtIntervals.IsEnabled = !selected.PresetIntervals.HasValue;
            }
        }

        public int Intervals {
            get {
                var selected = cmbModelType.SelectedItem as DistributionModel;
                if (selected != null && selected.PresetIntervals.HasValue) {
                    return selected.PresetIntervals.Value;                    
                }
                return Int32.Parse(txtIntervals.Text);
            }
        }

        public double CutOff {
            get {
                var selected = cmbModelType.SelectedItem as DistributionModel;
                if (selected != null && selected.PresetCutOff.HasValue) {
                    return selected.PresetCutOff.Value;
                }
                return Double.Parse(txtCutOff.Text);
            }
        }
    }
}
