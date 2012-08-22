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
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ReferencesReportOptions.xaml
    /// </summary>
    public partial class ReferencesReportOptions : Window {

        public ReferencesReportOptions() {
            InitializeComponent();
            DataContext = this;
            User = PluginManager.Instance.User;

            var sortableColumns = new List<String> { "RefCode", "Author", "Title", "BookTitle" };
            cmbSortBy.ItemsSource = sortableColumns;

            var indexStyles = new List<BibliographyIndexStyle> { BibliographyIndexStyle.Number, BibliographyIndexStyle.RefCode, BibliographyIndexStyle.None };
            cmbIndexStyle.ItemsSource = indexStyles;

            // Initialise options
            BibliographyTitle = Config.GetUser(User, "ReferencesReport.Title", "References");
            HonourIncludeInReportsFlag = Config.GetUser(User, "ReferencesReport.HonourIncludeInReportsFlag", true);
            IncludeQualification = Config.GetUser(User, "ReferencesReport.IncludeQualification", true);
            SortColumn = Config.GetUser(User, "ReferencesReport.SortColumn", "RefCode");
            SortAscending = Config.GetUser(User, "ReferencesReport.SortAscending", true);
            BibliographyIndexStyle = Config.GetUser(User, "ReferencesReport.BibliographyIndexStyle", BibliographyIndexStyle.RefCode); 
            GroupByReferenceType = Config.GetUser(User, "ReferencesReport.GroupByReferenceType", false);
            IncludeChildReferences = Config.GetUser(User, "ReferencesReport.IncludeChildReferences", false);
        }

        private void SaveSettings() {
            Config.SetUser(User, "ReferencesReport.Title", BibliographyTitle);
            Config.SetUser(User, "ReferencesReport.HonourIncludeInReportsFlag", HonourIncludeInReportsFlag);
            Config.SetUser(User, "ReferencesReport.IncludeQualification", IncludeQualification);
            Config.SetUser(User, "ReferencesReport.SortColumn", SortColumn);
            Config.SetUser(User, "ReferencesReport.SortAscending", SortAscending);
            Config.SetUser(User, "ReferencesReport.BibliographyIndexStyle", BibliographyIndexStyle);
            Config.SetUser(User, "ReferencesReport.GroupByReferenceType", GroupByReferenceType);
            Config.SetUser(User, "ReferencesReport.IncludeChildReferences", IncludeChildReferences);
        }

        protected User User { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();

        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            SaveSettings();
            this.Hide();
        }

        public string BibliographyTitle { get; set; }
        public bool HonourIncludeInReportsFlag { get; set; }
        public bool IncludeQualification { get; set; }
        public string SortColumn { get; set; }
        public bool SortAscending { get; set; }
        public BibliographyIndexStyle BibliographyIndexStyle { get; set; }
        public bool GroupByReferenceType { get; set; }
        public bool IncludeChildReferences { get; set; }

    }

    public enum BibliographyIndexStyle {
        Number,
        RefCode,
        None
    }

}
