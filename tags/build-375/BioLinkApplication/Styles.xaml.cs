using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BioLinkApplication {

    partial class Styles : ResourceDictionary {

        public Styles() {
            InitializeComponent();
        }

        public void CloseButtonClicked(object sender, RoutedEventArgs e) {

            MessageBox.Show("Here");
        }
    }
}
