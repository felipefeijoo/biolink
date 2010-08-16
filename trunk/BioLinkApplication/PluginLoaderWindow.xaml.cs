using System.Windows;
using System.Windows.Controls;
using BioLink.Client.Utilities;


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

        public void ProgressStart(string message, bool indeterminate) {
            lblProgress.Content = message;
        }

        public void ProgressMessage(string message, double? percentComplete) {
            lblProgress.Content = message;
            if (percentComplete.HasValue) {
                progressBar.Value = percentComplete.Value;                
            }
        }

        public void ProgressEnd(string message) {
            lblProgress.Content = message;
        }

    }
}
