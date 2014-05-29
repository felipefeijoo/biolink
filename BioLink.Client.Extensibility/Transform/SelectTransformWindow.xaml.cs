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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for SelectTransformWindow.xaml
    /// </summary>
    public partial class SelectTransformWindow : Window {

        public SelectTransformWindow() {
            InitializeComponent();
            listBox.ItemsSource = TransformFactory.Transforms;
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selected = listBox.SelectedItem as ValueTransformer;
            if (selected != null) {
                SelectedValueTransformerKey = selected.Key;
                this.DialogResult = true;
                this.Hide();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            var selected = listBox.SelectedItem as ValueTransformer;
            if (selected != null) {
                SelectedValueTransformerKey = selected.Key;
                this.DialogResult = true;
                this.Hide();
            }
        }

        public String SelectedValueTransformerKey { get; private set; }

    }
}


