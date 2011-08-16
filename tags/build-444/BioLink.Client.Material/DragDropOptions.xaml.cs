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

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for DragDropOptions.xaml
    /// </summary>
    public partial class DragDropOptions : Window {

        #region Designer Constructor
        public DragDropOptions() {
            InitializeComponent();
        }
        #endregion

        public DragDropOptions(Window owner) {
            this.Owner = owner;
            InitializeComponent();

            optCreateAsChild.IsChecked = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            if (optCreateAsChild.IsChecked.GetValueOrDefault(false)) {
                this.DragDropOption = Material.DragDropOption.CreateAsChild;
            } else {
                this.DragDropOption = Material.DragDropOption.Merge;
            }
            this.DialogResult = true;
            Hide();
        }

        public DragDropOption DragDropOption { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Hide();
        }

    }

    public enum DragDropOption {
        CreateAsChild,
        Merge
    }

}
