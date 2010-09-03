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
    /// Interaction logic for ControlHostWindow.xaml
    /// </summary>
    public partial class ControlHostWindow : Window {

        #region DesignerConstructor
        public ControlHostWindow() {
            InitializeComponent();
        }
        #endregion

        public ControlHostWindow(UIElement element) {
            InitializeComponent();
            this.Control = element;
            grid.Children.Add(element);
        }

        public UIElement Control { get; private set; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Control != null && Control is IClosable) {
                var closable = Control as IClosable;
                if (!closable.RequestClose()) {
                    e.Cancel = true;
                }
            }
        }
    }

}
