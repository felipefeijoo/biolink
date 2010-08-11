using System.Windows;
using System.Windows.Controls;
using BioLink.Client.Utilities;
using BioLink.Data;
using System;
using System.Windows.Input;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ReportResults.xaml
    /// </summary>
    public partial class ReportResults : UserControl, IProgressObserver, IDisposable {

        public ReportResults() {
            InitializeComponent();
        }

        public ReportResults(IBioLinkReport report) {
            InitializeComponent();
            progressBar.Visibility = System.Windows.Visibility.Hidden;
            this.Report = report;
            this.Loaded += new RoutedEventHandler(ReportResults_Loaded);
        }

        void ReportResults_Loaded(object sender, RoutedEventArgs e) {
            JobExecutor.QueueJob(() => {
                try {
                    this.WaitCursor();                    
                    StatusMessage("Running report...");
                    DataMatrix data = Report.ExtractReportData(this);
                    this.InvokeIfRequired(() => { DisplayReportResults(data); });
                } finally {
                    this.NormalCursor();
                }
            });
        }

        internal void DisplayReportResults(DataMatrix data) {

            StatusMessage("Preparing view...");

            if (this.Report != null) {
                reportContent.Children.Clear();
                if (Report.Viewers.Count == 0 || Report.Viewers.Count == 1) {
                    IReportViewerSource viewerSource = null;
                    if (Report.Viewers.Count == 0) {
                        viewerSource = new TabularDataViewerSource();
                    } else {
                        viewerSource = Report.Viewers[0];
                    }
                    FrameworkElement control = viewerSource.ConstructView(Report, data, this);
                    reportContent.Children.Add(control);                    
                } else {
                    TabControl tab = new TabControl();
                    foreach (IReportViewerSource viewerSource in Report.Viewers) {
                        FrameworkElement control = viewerSource.ConstructView(Report, data, this);
                        tab.Items.Add(control);
                    }                    
                    reportContent.Children.Add(tab);    
                }
            }

            StatusMessage("{0} records retrieved.", data.Rows.Count);
        }

        private void StatusMessage(string format, params object[] args) {
            string message = String.Format(format, args);
            statusMessage.InvokeIfRequired(() => {
                statusMessage.Text = message;
            });
        }

        #region ProgressObserver

        public void ProgressStart(string message, bool indeterminate) {
            StatusMessage(message);
            progressBar.InvokeIfRequired(() => {
                progressBar.Value = 0;
                progressBar.Visibility = System.Windows.Visibility.Visible;
                progressBar.IsIndeterminate = indeterminate;
            });
        }

        public void ProgressMessage(string message, double percentComplete) {
            StatusMessage(message);
            progressBar.InvokeIfRequired(() => {
                progressBar.Value = percentComplete;
            });
        }

        public void ProgressEnd(string message) {
            progressBar.InvokeIfRequired(() => {
                progressBar.Value = 100;
                progressBar.Visibility = System.Windows.Visibility.Hidden;
            });
        }

        #endregion

        #region properties

        protected IBioLinkReport Report { get; private set; }

        #endregion

        public void Dispose() {
            foreach (FrameworkElement elem in reportContent.Children) {
                if (elem is IDisposable) {
                    IDisposable disposable = elem as IDisposable;
                    disposable.Dispose();
                }
            }
        }


        //private void mnuFileExit_Click(object sender, RoutedEventArgs e) {
        //    PluginManager.Instance.CloseContent(this);
        //}

    }
}
