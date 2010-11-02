using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BioLink.Client.Extensibility {

    public class AutoNumberBox : EllipsisTextBox {

        public AutoNumberBox() {
            InitializeComponent();
            Click += new System.Windows.RoutedEventHandler(AutoNumberBox_Click);
        }

        void AutoNumberBox_Click(object sender, System.Windows.RoutedEventArgs e) {
            ShowAutoNumber();
        }

        private void ShowAutoNumber() {
        }

    }
}
