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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for XMLExportOptions.xaml
    /// </summary>
    public partial class XMLExportOptions : Window, IProgressObserver {

        private bool _isCancelled;

        public XMLExportOptions(User user, List<int> taxonIds) {
            InitializeComponent();
            TaxonIDs = taxonIds;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        protected List<int> TaxonIDs { get; private set; }

        protected User User { get; private set; }

        public string Filename {
            get { return txtFilename.Text; }
        }

        public bool ExportChildTaxa {
            get { return chkChildTaxa.IsChecked.Value; }
        }

        public bool ExportMaterial {
            get { return chkMaterial.IsChecked.Value; }
        }

        public bool ExportMultimedia {
            get { return chkMultimedia.IsChecked.Value; }
        }

        public bool ExportTraits {
            get { return chkTraits.IsChecked.Value; }
        }

        public bool ExportNotes {
            get { return chkNotes.IsChecked.Value; }
        }

        public bool IncludeFullClassification {
            get { return chkClassification.IsChecked.Value; }
        }

        public bool KeepLogFile {
            get { return chkLogFile.IsChecked.Value; }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartExport();
        }

        private void StartExport() {

            _isCancelled = false;

            if (string.IsNullOrEmpty(txtFilename.Text)) {
                ErrorMessage.Show("You must select a destination file before you can continue");
                return;
            }

            btnStart.IsEnabled = false;
            btnCancel.IsEnabled = true;

            var options = new XMLIOExportOptions { Filename = this.Filename, ExportChildTaxa = this.ExportChildTaxa, ExportMaterial = this.ExportMaterial, ExportTraits = this.ExportNotes, ExportMultimedia = this.ExportMultimedia, ExportNotes = this.ExportNotes, IncludeFullClassification = this.IncludeFullClassification, KeepLogFile = this.KeepLogFile };

            var service = new XMLIOService(User);
            JobExecutor.QueueJob(() => {
                service.ExportXML(TaxonIDs, options, this, IsCancelled);
            });

        }

        internal bool IsCancelled() {
            return _isCancelled;
        }

        public void ProgressStart(string message, bool indeterminate = false) {
            progressBar.InvokeIfRequired(() => {
                this.progressBar.Minimum = 0;
                this.progressBar.Maximum = 100;
                this.progressBar.Value = 0;
                lblProgress.Content = message;
                progressBar.IsIndeterminate = indeterminate;
            });
        }

        public void ProgressMessage(string message, double? percentComplete = null) {

            progressBar.InvokeIfRequired(() => {
                lblProgress.Content = message;
                if (percentComplete.HasValue) {
                    progressBar.IsIndeterminate = false;
                    this.progressBar.Value = percentComplete.Value;
                }
            });
        }

        public void ProgressEnd(string message) {
            progressBar.InvokeIfRequired(() => {
                this.progressBar.Value = 0;
                lblProgress.Content = message;
                progressBar.IsIndeterminate = false;
            });

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            _isCancelled = true;
        }
    }

}
