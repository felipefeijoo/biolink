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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ReferencesReportOptions.xaml
    /// </summary>
    public partial class ReferencesReportOptions : Window {

        public ReferencesReportOptions() {
            InitializeComponent();
            DataContext = this;
            BibliographyTitle = "References";
            HonourIncludeInReportsFlag = true;
            IncludeQualification = true;
            SortColumn = "RefCode";
            SortAscending = true;
            BibliographyIndexStyle = BibliographyIndexStyle.RefCode;
            GroupByReferenceType = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();

        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Hide();
        }

        public string BibliographyTitle { get; set; }
        public bool HonourIncludeInReportsFlag { get; set; }
        public bool IncludeQualification { get; set; }
        public string SortColumn { get; set; }
        public bool SortAscending { get; set; }
        public BibliographyIndexStyle BibliographyIndexStyle { get; set; }
        public bool GroupByReferenceType { get; set; }

    }

    public enum BibliographyIndexStyle {
        Number,
        RefCode
    }

}
