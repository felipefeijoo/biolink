using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using BioLink.Client.Utilities;
using BioLink.Data;

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
            var candidates = PluginManager.Instance.GetExtensionsOfType<TabularDataExporter>();
            var exporters = candidates.FindAll((exporter) => {
                return exporter.CanExport(data);
            });

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
