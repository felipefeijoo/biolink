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
        private ObservableCollection<ImportFieldMappingViewModel> _model;
        private Func<List<FieldDescriptor>> _fieldSource;

        static ImportMappingPage() {
            MapField = new RoutedCommand("MapFieldCommand", typeof(ImportMappingPage));
            UnmapField = new RoutedCommand("UnmapFieldCommand", typeof(ImportMappingPage));
            UnmapAll = new RoutedCommand("UnmapAllCommand", typeof(ImportMappingPage));
            AutoMap = new RoutedCommand("AutoMapCommand", typeof(ImportMappingPage));
        }


        public ImportMappingPage(Func<List<FieldDescriptor>> fieldSource) {

            _fieldSource = fieldSource;

            // Command bindings...
            this.CommandBindings.Add(new CommandBinding(MapField, ExecutedMapField, CanExecuteMapField));
            this.CommandBindings.Add(new CommandBinding(UnmapField, ExecutedUnmapField, CanExecuteUnmapField));
            this.CommandBindings.Add(new CommandBinding(UnmapAll, ExecutedUnmapAll, CanExecuteUnmapAll));
            this.CommandBindings.Add(new CommandBinding(AutoMap, ExecutedAutoMap, CanExecuteAutoMap));

            InitializeComponent();

            lvwFields.PreviewKeyDown += new KeyEventHandler(lvwFields_PreviewKeyDown);
        }

        void lvwFields_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                MapSelectedField();
                e.Handled = true;
            }
        }

        public override void OnPageEnter(WizardDirection fromdirection) {
            _fields = _fieldSource();
            lvwFields.ItemsSource = _fields;

            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lvwFields.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            myView.GroupDescriptions.Add(groupDescription);
            var model = new List<ImportFieldMapping>();
            var columns = ImportContext.Importer.GetColumnNames();
            var existingMappings = ImportContext.FieldMappings;

            if (existingMappings != null && existingMappings.Count() > 0) {
                foreach (ImportFieldMapping mapping in existingMappings) {
                    model.Add(mapping);
                }
            } else {
                foreach (string columnName in columns) {
                    var mapping = new ImportFieldMapping { SourceColumn = columnName };
                    model.Add(mapping);
                }
            }

            _model = new ObservableCollection<ImportFieldMappingViewModel>(model.Select((m) => {
                return new ImportFieldMappingViewModel(m);
            }));

            lvwMappings.ItemsSource = _model;

        }

        protected ImportWizardContext ImportContext { get { return WizardContext as ImportWizardContext; } }

        public override string PageTitle {
            get { return "Field mapping"; }
        }

        private void lvwFields_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void lvwFields_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            MapSelectedField();
        }

        public override bool OnPageExit(WizardDirection todirection) {
            ImportContext.FieldMappings = _model.Select((vm) => {
                return vm.Model;
            });
            return true;
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

                // First check to see if this field has been mapped already...
                var targetName = string.Format("{0}.{1}", field.Category, field.DisplayName);

                if (!field.DisplayName.Equals("other", StringComparison.CurrentCultureIgnoreCase)) {
                    var existing = _model.FirstOrDefault((candidate) => {
                        return candidate.TargetColumn != null && candidate.TargetColumn.Equals(targetName);
                    });

                    if (existing != null) {
                        ErrorMessage.Show("The target field {0} has already been mapped to {1}. Fields can only be mapped to one import column.", existing.TargetColumn, existing.SourceColumn);
                        return;
                    }
                }

                mapping.TargetColumn = targetName;
            }
        }

        private void UnmapSelectedMapping() {
            var mapping = lvwMappings.SelectedItem as ImportFieldMappingViewModel;
            if (mapping != null) {
                mapping.TargetColumn = "";
                if (!mapping.IsFixed) {
                    mapping.DefaultValue = null;
                }
            }
        }

        private void UnmapAllMappings() {
            if (lvwMappings.ItemsSource != null) {

                foreach (ImportFieldMappingViewModel vm in lvwMappings.ItemsSource) {
                    vm.TargetColumn = "";
                    if (!vm.IsFixed) {
                        vm.DefaultValue = null;
                    }
                }

            }
        }

        private void DoAutoMap() {
            foreach (ImportFieldMappingViewModel mapping in _model) {
                if (string.IsNullOrEmpty(mapping.TargetColumn)) {
                    var candidate = _fields.Find((field) => {
                        if (!string.IsNullOrEmpty(mapping.SourceColumn)) {
                            // First try a simple match of the name...
                            if (field.DisplayName.Equals(mapping.SourceColumn, StringComparison.CurrentCultureIgnoreCase)) {
                                return true;
                            };
                            // Next convert all underscores to spaces and try that....

                            var test = mapping.SourceColumn.Replace("_", " ");
                            if (field.DisplayName.Equals(test, StringComparison.CurrentCultureIgnoreCase)) {
                                return true;
                            }
                        }
                        return false;
                    });
                    if (candidate != null) {
                        mapping.TargetColumn = string.Format("{0}.{1}", candidate.Category, candidate.DisplayName);
                        mapping.DefaultValue = null;
                    }
                }
            }
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

        private void lvwMappings_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowSourceMenu();
        }

        private void ShowSourceMenu() {

            var selected = lvwMappings.SelectedItem as ImportFieldMappingViewModel;
            if (selected != null) {
                var builder = new ContextMenuBuilder(null);
                bool isFixed = selected.Model.IsFixed;
                builder.New("_Add new fixed field...").Handler(() => { AddFixedField(); }).End();
                builder.New("_Edit fixed field...").Handler(() => { EditFixedField(); }).Enabled(isFixed).End();
                builder.New("_Delete fixed field").Handler(() => { DeleteFixedField(); }).Enabled(isFixed).End();
                builder.Separator();
                builder.New("_Set default value").Handler(() => { SetDefaultValue(); }).Enabled(!isFixed).End();
                builder.New("_Clear default value").Handler(() => { ClearDefaultValue(); }).Enabled(!isFixed).End();

                lvwMappings.ContextMenu = builder.ContextMenu;
            }
        }

        private void AddFixedField() {

            InputBox.Show(this.FindParentWindow(), "New fixed column", "Enter the value you wish to fix:", (val) => {
                var mapping = new ImportFieldMapping { IsFixed = true, DefaultValue = val };
                var vm = new ImportFieldMappingViewModel(mapping);                
                _model.Add(vm);
                lvwMappings.SelectedItem = vm;
            });
            
        }

        private void EditFixedField() {
            var selected = lvwMappings.SelectedItem as ImportFieldMappingViewModel;
            if (selected != null && selected.IsFixed) {
                InputBox.Show(this.FindParentWindow(), "Edit fixed column", "Enter the value you wish to fix:", (string) selected.DefaultValue, (val) => {
                    selected.DefaultValue = val;
                });
            }
        }

        private void DeleteFixedField() {
            var selected = lvwMappings.SelectedItem as ImportFieldMappingViewModel;
            if (selected != null && selected.IsFixed) {
                _model.Remove(selected);
            }
        }

        private void SetDefaultValue() {
            var selected = lvwMappings.SelectedItem as ImportFieldMappingViewModel;
            if (selected != null) {
                InputBox.Show(this.FindParentWindow(), "Default for '" + selected.SourceColumn + "'", "Enter the default value for '" + selected.SourceColumn + "':", (def) => {
                    selected.DefaultValue = def;
                });
            }
        }

        private void ClearDefaultValue() {
            var selected = lvwMappings.SelectedItem as ImportFieldMappingViewModel;
            if (selected != null) {
                selected.DefaultValue = null;
            }
        }

        private void delayedTriggerTextbox1_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                lvwFields.SelectedIndex = 0;
                if (lvwFields.SelectedItem != null) {
                    ListViewItem item = lvwFields.ItemContainerGenerator.ContainerFromItem(lvwFields.SelectedItem) as ListViewItem;
                    item.Focus();
                }
                e.Handled = true;
            }

        }

    }

}
