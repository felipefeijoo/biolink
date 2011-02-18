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

        static ImportMappingPage() {
            MapField = new RoutedCommand("MapFieldCommand", typeof(ImportMappingPage));
            UnmapField = new RoutedCommand("UnmapFieldCommand", typeof(ImportMappingPage));
            UnmapAll = new RoutedCommand("UnmapAllCommand", typeof(ImportMappingPage));
            AutoMap = new RoutedCommand("AutoMapCommand", typeof(ImportMappingPage));
        }


        public ImportMappingPage() {
            // Command bindings...
            this.CommandBindings.Add(new CommandBinding(MapField, ExecutedMapField, CanExecuteMapField));
            this.CommandBindings.Add(new CommandBinding(UnmapField, ExecutedUnmapField, CanExecuteUnmapField));
            this.CommandBindings.Add(new CommandBinding(UnmapAll, ExecutedUnmapAll, CanExecuteUnmapAll));
            this.CommandBindings.Add(new CommandBinding(AutoMap, ExecutedAutoMap, CanExecuteAutoMap));

            InitializeComponent();
        }

        public override void OnPageEnter(WizardDirection fromdirection) {
            var service = new SupportService(User);
            _fields = service.GetImportFields();
            lvwFields.ItemsSource = _fields;

            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lvwFields.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            myView.GroupDescriptions.Add(groupDescription);
            var model = new List<ImportFieldMapping>();
            var columns = ImportContext.ImporterOptions.ColumnNames;
            var existingMappings = ImportContext.FieldMappings;
            foreach (string columnName in columns) {            
                var mapping = new ImportFieldMapping { SourceColumn = columnName };
                if (existingMappings != null) {
                    var existing = existingMappings.Find((m) => {
                        return m.SourceColumn == columnName;
                    });
                    if (existing != null) {
                        mapping.TargetColumn = existing.TargetColumn;
                        mapping.DefaultValue = existing.TargetColumn;
                    }
                }
                model.Add(mapping);
            }
            ImportContext.FieldMappings = model;

            var mappingModel = new ObservableCollection<ImportFieldMappingViewModel>(model.Select((m) => {
                return new ImportFieldMappingViewModel(m);
            }));

            lvwMappings.ItemsSource = mappingModel;

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

        private void MapSelectedField() {
            var mapping = lvwMappings.SelectedItem as ImportFieldMappingViewModel;
            var field = lvwFields.SelectedItem as FieldDescriptor;

            if (mapping != null && field != null) {
                mapping.TargetColumn = field.DisplayName;
                mapping.DefaultValue = null; // Should we clear any default values?
            }
        }

        private void UnmapSelectedMapping() {
            var mapping = lvwMappings.SelectedItem as ImportFieldMappingViewModel;
            if (mapping != null) {
                mapping.TargetColumn = "";
                mapping.DefaultValue = null; 
            }
        }

        private void UnmapAllMappings() {
            if (lvwMappings.ItemsSource != null) {

                foreach (ImportFieldMappingViewModel vm in lvwMappings.ItemsSource) {
                    vm.TargetColumn = "";
                    vm.DefaultValue = null;
                }

            }
        }

        private void DoAutoMap() {
            MessageBox.Show("Automap!");
        }

        public static RoutedCommand MapField { get; private set; }

        private void CanExecuteMapField(object target, CanExecuteRoutedEventArgs e) {

            ImportMappingPage page = target as ImportMappingPage;

            e.CanExecute = false;

            if (page != null) {
                e.CanExecute = page.lvwFields != null && page.lvwFields.SelectedItem != null && lvwMappings.SelectedItem != null;
            }
        }

        private void ExecutedMapField(object target, ExecutedRoutedEventArgs e) {
            ImportMappingPage page = target as ImportMappingPage;
            if (page != null) {
                page.MapSelectedField();
            }
        }

        public static RoutedCommand UnmapField { get; private set; }

        private void CanExecuteUnmapField(object target, CanExecuteRoutedEventArgs e) {

            ImportMappingPage page = target as ImportMappingPage;

            e.CanExecute = false;

            if (page != null) {
                e.CanExecute = lvwMappings.SelectedItem != null;
            }
        }

        private void ExecutedUnmapField(object target, ExecutedRoutedEventArgs e) {
            ImportMappingPage page = target as ImportMappingPage;
            if (page != null) {
                page.UnmapSelectedMapping();
            }
        }

        public static RoutedCommand UnmapAll{ get; private set; }

        private void CanExecuteUnmapAll(object target, CanExecuteRoutedEventArgs e) {

            ImportMappingPage page = target as ImportMappingPage;

            e.CanExecute = true;
        }

        private void ExecutedUnmapAll(object target, ExecutedRoutedEventArgs e) {
            ImportMappingPage page = target as ImportMappingPage;
            if (page != null) {
                page.UnmapAllMappings();
            }
        }

        public static RoutedCommand AutoMap { get; private set; }

        private void CanExecuteAutoMap(object target, CanExecuteRoutedEventArgs e) {

            ImportMappingPage page = target as ImportMappingPage;

            e.CanExecute = true;
        }

        private void ExecutedAutoMap(object target, ExecutedRoutedEventArgs e) {
            ImportMappingPage page = target as ImportMappingPage;
            if (page != null) {
                page.DoAutoMap();
            }
        }




    }
}
