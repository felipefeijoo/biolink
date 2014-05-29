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
    /// Interaction logic for TransformationPiplineStartControl.xaml
    /// </summary>
    public partial class TransformationPiplineStartControl : UserControl {
        public TransformationPiplineStartControl() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (this.TestClicked != null) {
                TestClicked(this, e);
            }
        }

        public String TestValue {
            get { return txtTestValue.Text; }
        }

        public event RoutedEventHandler TestClicked;
    }
}
