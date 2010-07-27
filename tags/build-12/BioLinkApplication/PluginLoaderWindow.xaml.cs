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
using BioLink.Client.Extensibility;


namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for PluginLoaderWindow.xaml
    /// </summary>
    public partial class PluginLoaderWindow : Window, IProgressObserver {
        public PluginLoaderWindow() {
            InitializeComponent();
            progressBar.SetValue(ProgressBar.MinimumProperty, 0.0);
            progressBar.SetValue(ProgressBar.MaximumProperty, 100.0);
        }

        public void ProgressStart(string message) {
            lblProgress.Content = message;
        }

        public void ProgressMessage(string message, double percentComplete) {
            lblProgress.Content = message;
            progressBar.SetValue(ProgressBar.ValueProperty, percentComplete) ;
        }

        public void ProgressEnd(string message) {
            lblProgress.Content = message;
        }
    }
}
