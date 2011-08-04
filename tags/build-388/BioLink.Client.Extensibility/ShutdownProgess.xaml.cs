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
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ShutdownProgess.xaml
    /// </summary>
    public partial class ShutdownProgess : Window {

        public ShutdownProgess() {
            InitializeComponent();
        }

        public void StatusMessage(string message) {
            this.InvokeIfRequired(() => {
                lblMessage.Content = message;
            });
        }

    }
}
