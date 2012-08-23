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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for UpdateCheckWindow.xaml
    /// </summary>
    public partial class UpdateCheckWindow : Window, IProgressObserver {

        public UpdateCheckWindow() {
            InitializeComponent();
            var v = this.GetType().Assembly.GetName().Version;
            var version = String.Format("Current version {0}.{1}.{2}", v.Major, v.Minor, v.Revision);
            lblCurrentVersion.Content = version;
        }

        public void ProgressStart(string message, bool indeterminate = false) {
            lblMessage.InvokeIfRequired(() => lblMessage.Text = message);
        }

        public void ProgressMessage(string message, double? percentComplete = null) {
            lblMessage.InvokeIfRequired(() => lblMessage.Text = message);
        }

        public void ProgressEnd(string message) {
            lblMessage.InvokeIfRequired(() => lblMessage.Text = "");
        }

        public void ShowUpdateResults(UpdateCheckResults results) {
            if (results.UpdateExists) {
                var message = String.Format("A new version of BioLink ({3}.{4}.{5}) is available for download!\n\nClick the button below to visit the download site.", results.CurrentMajor, results.CurrentMinor, results.CurrentBuild, results.UpdateMajor, results.UpdateMinor, results.UpdateBuild);
                lblMessage.Text = message;
                btnVisitSite.Visibility = System.Windows.Visibility.Visible;
                btnVisitSite.Click +=new RoutedEventHandler((s,e) => {
                   BioLink.Client.Utilities.SystemUtils.ShellExecute(results.UpdateLink);
                });
            } else {
                var message = "There is no new version of BioLink to download at this time.";
                lblMessage.Text = message;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

    }
}
