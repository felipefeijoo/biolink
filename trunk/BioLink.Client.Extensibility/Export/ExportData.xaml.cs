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
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ExportData.xaml
    /// </summary>
    public partial class ExportData : Window {

        private DataMatrix _data;
        private IProgressObserver _progress;

        #region designer ctor
        public ExportData() {
            InitializeComponent();
        }
        #endregion

        public ExportData(DataMatrix data, IProgressObserver progress) {
            InitializeComponent();
            _data = data;
            _progress = progress;
            List<TabularDataExporter> exporters = PluginManager.Instance.GetExtensionsOfType<TabularDataExporter>();
            listBox.ItemsSource = exporters;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (listBox.SelectedItem != null) {
                btnOk.IsEnabled = true;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {            
            ExportUsingSelectedExporter();            
        }

        private void ExportUsingSelectedExporter() {
            //ProgressWindow progress = new ProgressWindow();
            //progress.Owner = this.Owner;
            TabularDataExporter exporter = listBox.SelectedItem as TabularDataExporter;
            exporter.Export(this.Owner, _data, _progress);
            this.DialogResult = true;
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (listBox.SelectedItem != null) {
                ExportUsingSelectedExporter();
            }
        }

    }
}
