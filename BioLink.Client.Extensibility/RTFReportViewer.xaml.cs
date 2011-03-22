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
using System.IO;
using Microsoft.Win32;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for RTFReportViewer.xaml
    /// </summary>
    public partial class RTFReportViewer : UserControl {
        public RTFReportViewer() {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            SaveRTF();
        }

        private void SaveRTF() {
            var dlg = new SaveFileDialog();
            dlg.Filter = "RTF files (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (dlg.ShowDialog().GetValueOrDefault(false)) {

                try {
                    this.Cursor = Cursors.Wait;
                    using (StreamWriter writer = new StreamWriter(dlg.FileName)) {
                        writer.Write(RTF);
                    }
                } finally {
                    this.Cursor = Cursors.Arrow;
                }
            }
        }

        public String RTF {
            get {
                var range = new TextRange(txtRTF.Document.ContentStart, txtRTF.Document.ContentEnd);
                string rtf;

                using (var stream = new MemoryStream()) {
                    range.Save(stream, DataFormats.Rtf);
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream)) {
                        rtf = reader.ReadToEnd();
                    }
                }
                return rtf;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e) {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true) {
                dialog.PrintDocument((((IDocumentPaginatorSource)txtRTF.Document).DocumentPaginator), ReportName);                
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(RTF, TextDataFormat.Rtf);
        }

        public String ReportName { get; set; }
    }

    public class RTFReportViewerSource : IReportViewerSource {
        public string Name {
            get { return "RTF Report Viewer"; }
        }

        public FrameworkElement ConstructView(IBioLinkReport report, Data.DataMatrix reportData, Utilities.IProgressObserver progress) {
            var viewer = new RTFReportViewer();
            viewer.ReportName = report.Name;
            var doc = viewer.txtRTF.Document;
            using (var stream = new MemoryStream((new UTF8Encoding()).GetBytes(reportData.Rows[0][0] as string))) {
                var text = new TextRange(doc.ContentStart, doc.ContentEnd);
                text.Load(stream, DataFormats.Rtf);
            }

            return viewer;            
        }
    }
}
