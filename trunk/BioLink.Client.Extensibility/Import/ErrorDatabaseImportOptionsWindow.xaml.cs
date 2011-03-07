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

        public ErrorDatabaseImportOptionsWindow(ErrorDatabaseImporterOptions options, ImportWizardContext context) {
            InitializeComponent();
            _service = new ImportStagingService(options.Filename);
            Options = options;
            Context = context;

            dataGrid.AutoGenerateColumns = true;
            var ds = _service.GetErrorsDataSet();
            dataGrid.ItemsSource = ds.Tables[0].DefaultView;

        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;

            Context.FieldMappings = _service.GetMappings();

            _service.Disconnect();            
            this.Close();
        }

        protected ErrorDatabaseImporterOptions Options { get; private set; }
        protected ImportWizardContext Context { get; private set; }
    }
}
