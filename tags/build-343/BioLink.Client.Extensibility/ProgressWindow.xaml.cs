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
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window, IProgressObserver {

        public ProgressWindow() {
            InitializeComponent();
        }

        public void ProgressStart(string message, bool indeterminate = false) {
            this.InvokeIfRequired(() => {                
                lblProgress.Content = message;                
                progressBar.IsIndeterminate = indeterminate;
                this.Show();
            });
        }

        public void ProgressMessage(string message, double? percentComplete) {
            this.InvokeIfRequired(() => {
                lblProgress.Content = message;
                if (!progressBar.IsIndeterminate && percentComplete.HasValue) {
                    progressBar.Value = percentComplete.Value;
                }
            });
        }

        public void ProgressEnd(string message) {
            this.InvokeIfRequired(() => {
                lblProgress.Content = message;
                if (!progressBar.IsIndeterminate) {
                    progressBar.Value = 100;
                }
                this.Hide();
            });
        }

    }
}
