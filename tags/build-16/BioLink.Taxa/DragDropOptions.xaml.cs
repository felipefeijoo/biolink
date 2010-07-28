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
using System.Windows.Shapes;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for ConvertTaxon.xaml
    /// </summary>
    public partial class DragDropOptions : Window {

        private TaxaPlugin _owner;

        public DragDropOptions() {
            InitializeComponent();
        }

        public DragDropOptions(TaxaPlugin owner) {
            _owner = owner;
            InitializeComponent();
        }


        public TaxonRank ShowChooseConversion(TaxonRank sourceRank, List<TaxonRank> choices) {
            lblConvert.Content = _owner.GetCaption("DragDropOptions.lblConvert", sourceRank.LongName);
            cmbRanks.ItemsSource = choices;
            cmbRanks.SelectedIndex = 0;
            if (ShowDialog().GetValueOrDefault(false)) {
                return cmbRanks.SelectedItem as TaxonRank;
            }
            return null;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Hide();
        }
    }
}
