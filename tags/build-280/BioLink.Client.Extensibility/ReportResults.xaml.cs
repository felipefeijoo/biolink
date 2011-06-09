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

        private bool _ReportExecuted = false;

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
            if (!_ReportExecuted) {
                _ReportExecuted = true;
                JobExecutor.QueueJob(() => {
                    try {
                        this.WaitCursor();
                        StatusMessage("Running report...");
                        this.InvokeIfRequired(() => { DisplayLoading(); });
                        DataMatrix data = Report.ExtractReportData(this);
                        this.InvokeIfRequired(() => { DisplayReportResults(data); });                        
                    } catch (Exception ex) {
                        this.NormalCursor();
                        this.ProgressEnd("An error occured executing the report");
                        this.InvokeIfRequired(() => { DisplayException(ex); });
                    } finally {                        
                        this.NormalCursor();
                    }
                });
            }
        }

        private string _R(string key, params object[] args) {
            string format = FindResource(key) as string;
            if (format != null) {                
                return String.Format(format, args);
            }
            return string.Format(key, args);
        }

        private void DisplayLoading() {
            string assemblyName = this.GetType().Assembly.GetName().Name;
            string imageFile = PluginManager.Instance.ResourceTempFileManager.ProxyResource(new Uri(String.Format("pack://application:,,,/{0};component/images/loading-big.gif", assemblyName)));
            string html = _R("ReportResults.Loading.Template", Report.Name, imageFile);
            DisplayHTML(html);
        }

        private void DisplayHTML(String html) {
            reportContent.Children.Clear();
            WebBrowser browser = new WebBrowser();
            browser.Navigating += new System.Windows.Navigation.NavigatingCancelEventHandler(browser_Navigating);                        
            browser.NavigateToString(html);
            reportContent.Children.Add(browser);
        }

        private void DisplayException(Exception ex) {
            
            string title = _R("ReportResults.Error.Title", Report.Name);
            string stacktrace = ex.StackTrace;
            string html = _R("ReportResults.Error.Template", title, ex.Message, stacktrace);
            DisplayHTML(html);
        }

        void browser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) {
            Logger.Debug("Navigating to {0}", e.Uri == null ? null : e.Uri.ToString());
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

        internal void StatusMessage(string format, params object[] args) {
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

        public void ProgressMessage(string message, double? percentComplete) {
            StatusMessage(message);
            if (percentComplete.HasValue) {
                progressBar.InvokeIfRequired(() => {
                    progressBar.Value = percentComplete.Value;
                });
            }
        }

        public void ProgressEnd(string message) {
            StatusMessage(message);
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
