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
using System.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ReportResults.xaml
    /// </summary>
    public partial class ReportResults : Window {

        public ReportResults() {
            InitializeComponent();
        }

        public ReportResults(IBioLinkReport report) {
            InitializeComponent();
            this.Report = report;
            this.Loaded += new RoutedEventHandler(ReportResults_Loaded);

        }

        void ReportResults_Loaded(object sender, RoutedEventArgs e) {
            if (this.Report != null) {
                DataTable data = Report.ExtractReportData();

                reportContent.Children.Clear();

                if (Report.Viewers.Count == 0 || Report.Viewers.Count == 1) {
                    IReportViewerSource viewerSource = null;
                    if (Report.Viewers.Count == 0) {
                        viewerSource = new TabularDataViewerSource();
                    } else {
                        viewerSource = Report.Viewers[0];
                    }
                    FrameworkElement control = viewerSource.ConstructView(data, null);
                    reportContent.Children.Add(control);                    
                } else {
                    TabControl tab = new TabControl();
                    foreach (IReportViewerSource viewerSource in Report.Viewers) {
                        FrameworkElement control = viewerSource.ConstructView(data, null);
                        tab.Items.Add(control);
                    }                    
                    reportContent.Children.Add(tab);    
                }
            }
        }

        #region properties

        protected IBioLinkReport Report { get; private set; }

        #endregion

    }
}
