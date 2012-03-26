using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Drawing;

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for LegendOptionsWindow.xaml
    /// </summary>
    public partial class LegendOptionsWindow : Window {

        protected MapLegend Legend { get; private set; }

        public LegendOptionsWindow() {
            InitializeComponent();
            
        }

        public LegendOptionsWindow(MapLegend legend) {
            InitializeComponent();
            this.Legend = legend;

            this.DataContext = this;

            BackgroundColor = (Legend.BackgroundBrush as SolidBrush).Color;
        }

        public Color BackgroundColor { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (ApplyChanges()) {
                this.Close();
            }
        }

        private bool ApplyChanges() {

            Legend.BackgroundBrush = new SolidBrush(BackgroundColor);

            Legend.MapBox.Refresh();

            return true;
        }

    }
}
