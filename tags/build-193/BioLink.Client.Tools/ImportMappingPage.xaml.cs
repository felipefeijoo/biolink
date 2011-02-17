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
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ImportMappingPage.xaml
    /// </summary>
    public partial class ImportMappingPage : WizardPage {

        private List<FieldDescriptor> _fields;

        public ImportMappingPage() {
            InitializeComponent();
        }

        public override void OnPageEnter(WizardDirection fromdirection) {
            var service = new SupportService(User);
            _fields = service.GetImportFields();
            lvwFields.ItemsSource = _fields;

            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lvwFields.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            myView.GroupDescriptions.Add(groupDescription);

            var rs = ImportContext.Importer.CreateRowSource(ImportContext.ImporterOptions);

            var columns = new List<ImportFieldMapping>();
            for (int i = 0; i < rs.ColumnCount; ++i) {
                var mapping = new ImportFieldMapping { SourceColumn = rs.ColumnName(i) };
                columns.Add(mapping);
            }

            lvwMappings.ItemsSource = columns;

        }

        protected ImportWizardContext ImportContext { get { return WizardContext as ImportWizardContext; } }

        public override string PageTitle {
            get { return "Field mapping"; }
        }

        private void lvwFields_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void lvwFields_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }

        private void delayedTriggerTextbox1_TypingPaused(string text) {
            FilterFields(text);
        }

        private void FilterFields(string text) {
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwFields.ItemsSource) as ListCollectionView;

            if (String.IsNullOrEmpty(text)) {
                dataView.Filter = null;
                return;
            }

            text = text.ToLower();
            dataView.Filter = (obj) => {
                var field = obj as FieldDescriptor;

                if (field != null) {
                    if (field.DisplayName.ToLower().Contains(text)) {
                        return true;
                    }

                    if (field.Description.ToLower().Contains(text)) {
                        return true;
                    }

                    return false;
                }
                return true;
            };

            dataView.Refresh();

        }
    }
}
