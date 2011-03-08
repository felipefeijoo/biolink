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
using System.Data.SQLite;
using System.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ErrorDatabaseImportOptionsWindow.xaml
    /// </summary>
    public partial class ErrorDatabaseImportOptionsWindow : Window {

        private ImportStagingService _service;
        private List<ImportFieldMapping> _mappings;

        public ErrorDatabaseImportOptionsWindow(ErrorDatabaseImporterOptions options, ImportWizardContext context) {
            InitializeComponent();
            _service = new ImportStagingService(options.Filename);
            Options = options;
            Context = context;

            _mappings = _service.GetMappings();

            dataGrid.AutoGenerateColumns = false;

            foreach (ImportFieldMapping mapping in _mappings) {
                dataGrid.Columns.Add(CreateColumn(mapping));
            }
            
            var ds = _service.GetErrorsDataSet();            
            dataGrid.ItemsSource = ds.Tables[0].DefaultView;            
            dataGrid.IsReadOnly = true;
            dataGrid.MouseDoubleClick += new MouseButtonEventHandler(dataGrid_MouseDoubleClick);
        }

        private DataGridColumn CreateColumn(ImportFieldMapping mapping) {

            var col = new DataGridTextColumn();
            col.Header = mapping.SourceColumn;
            col.Width = DataGridLength.Auto;
            col.Binding = new Binding(mapping.SourceColumn);

            return col;
        }

        void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

            var selected = dataGrid.SelectedItem as DataRowView;
            if (selected != null) {
                var columnName = dataGrid.CurrentCell.Column.Header as string;
                Int64 rowID = (Int64)selected["ROWID"];
                var currentValueObj = selected[columnName];

                var currentValue = currentValueObj as string;
                if (currentValue == null && currentValueObj != null) {
                    currentValue = currentValueObj.ToString();
                }

                var mapping = _mappings.First((candidate) => {
                    return candidate.SourceColumn.Equals(columnName);
                });

                var frm = new CellEditorWindow(mapping, currentValue);
                frm.Owner = this;

                if (frm.ShowDialog().GetValueOrDefault(false)) {
                    _service.UpdateErrorRowField((int) rowID, columnName, frm.NewValue);
                    selected[columnName] = frm.NewValue;
                }
                
            }

        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            Context.FieldMappings = _mappings;
            _service.Disconnect();            
            this.Close();
        }

        protected ErrorDatabaseImporterOptions Options { get; private set; }
        protected ImportWizardContext Context { get; private set; }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            if (btn != null) {
                var row = btn.Tag as DataGridRow;
                if (row != null) {
                    EditRow(row.Item as DataRowView);
                }
            }
        }

        private void EditRow(DataRowView selected) {            

            if (selected != null) {
                Int64 rowID = (Int64)selected["ROWID"];
                var frm = new RowEditorWindow(_service, (int)rowID, _mappings, selected);
                frm.Owner = this;
                frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                if (frm.ShowDialog().GetValueOrDefault(false)) {
                    if (frm.Changes.Count > 0) {
                        foreach (string columnName in frm.Changes.Keys) {
                            var newValue = frm.Changes[columnName];
                            _service.UpdateErrorRowField((int)rowID, columnName, newValue);
                            selected[columnName] = newValue;
                        }
                    }                    
                }
            }

        }
    }
}
