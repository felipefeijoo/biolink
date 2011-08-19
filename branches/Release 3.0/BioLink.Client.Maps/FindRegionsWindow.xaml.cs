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
using SharpMap.Data;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Data.Providers;


namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for FindRegionsWindow.xaml
    /// </summary>
    public partial class FindRegionsWindow : Window {

        public FindRegionsWindow(VectorLayer layer, Action<string> selectHandler) {
            InitializeComponent();
            Layer = layer;
            SelectionHandler = selectHandler;                
        }

        private void txtFind_TypingPaused(string text) {
            DoFind(text);
        }

        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ApplySelection();
        }

        private void DoFind(string filter) {
            var results = new List<string>();
            if (Layer != null) {
                if (!Layer.DataSource.IsOpen) {
                    Layer.DataSource.Open();
                }
                var ds = Layer.DataSource;
                var count = ds.GetFeatureCount();
                for (uint i = 0; i < count; ++i) {
                    var feature = ds.GetFeature(i);
                    var path = feature["BLREGHIER"] as string;
                    if (!string.IsNullOrWhiteSpace(path) && path.ToLower().Contains(filter.ToLower())) {
                        results.Add(path);
                    }

                }
            }
            lst.ItemsSource = results;
        }

        private bool ApplySelection() {
            var selectedPath = lst.SelectedItem as string;
            if (!string.IsNullOrWhiteSpace(selectedPath)) {
                if (SelectionHandler != null) {
                    SelectionHandler(selectedPath);
                    return true;
                }
            }
            return false;
        }

        protected Action<string> SelectionHandler { get; private set; }

        protected VectorLayer Layer { get; private set; }

        private void btnFind_Click(object sender, RoutedEventArgs e) {
            DoFind(txtFind.Text);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplySelection();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (ApplySelection()) {
                this.DialogResult = true;
                Close();
            }
        }

    }
}
