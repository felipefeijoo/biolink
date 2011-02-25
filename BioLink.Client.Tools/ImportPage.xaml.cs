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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ImportPage.xaml
    /// </summary>
    public partial class ImportPage : WizardPage, IProgressObserver {

        private ObservableCollection<ImportStatusMessage> _messages;

        public ImportPage() {
            InitializeComponent();
        }

        public override string PageTitle {
            get {
                return "Import data";
            }
        }

        private void btnSaveTemplate_Click(object sender, RoutedEventArgs e) {
            var inifile = new IniFile();
            inifile.SetValue("Import", "ProfileStr", ImportContext.Importer.CreateProfileString());
            inifile.SetValue("Import", "ImportFilter", ImportContext.Importer.Name);
            inifile.SetValue("Import", "FieldCount", ImportContext.FieldMappings.Count());

            int i = 0;
            foreach (ImportFieldMapping mapping in ImportContext.FieldMappings) {
                var ep = new EntryPoint(mapping.SourceColumn);
                ep.AddParameter("Mapping", mapping.TargetColumn);
                ep.AddParameter("Default", mapping.DefaultValue);
                ep.AddParameter("IsFixed", mapping.IsFixed.ToString());

                inifile.SetValue("Mappings", string.Format("Field{0}", i++), ep.ToString());
            }

            inifile.Save("c:\\zz\\test.bip");
        }

        protected ImportWizardContext ImportContext { get { return WizardContext as ImportWizardContext; } }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartImportAsync();
        }

        private void StartImportAsync() {

            JobExecutor.QueueJob(() => {
                StartImport();
            });
        }

        private void StartImport() {
            _messages = new ObservableCollection<ImportStatusMessage>();
            lvw.ItemsSource = _messages;
            StatusMsg("Import started");
        }

        public void StatusMsg(string message) {
            var msg = new ImportStatusMessage { Timestamp = DateTime.Now, Message = message };
            _messages.Add(msg);
            lvw.SelectedItem = msg;
        }


        public void ProgressStart(string message, bool indeterminate = false) {
            this.InvokeIfRequired(() => {
                progressBar.IsIndeterminate = indeterminate;
                progressBar.Minimum = 0;
                progressBar.Maximum = 100;
                progressBar.Value = 0;
                lblStatus.Content = message;
            });
        }

        public void ProgressMessage(string message, double? percentComplete = null) {
            this.InvokeIfRequired(() => {
                if (percentComplete.HasValue) {
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = percentComplete.Value;
                }
                lblStatus.Content = message;
            });
            
        }

        public void ProgressEnd(string message) {
            this.InvokeIfRequired(() => {
                progressBar.IsIndeterminate = false;
                progressBar.Value = progressBar.Maximum;
                lblStatus.Content = message;
            });            
        }
    }

    class ImportStatusMessage {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }

    class ImportProcessor {



        public ImportProcessor(TabularDataImporter importer, List<ImportFieldMapping> mappings, IProgressObserver progress, Action<string> logFunc) {
            this.Importer = importer;
            this.Mappings = mappings;
            this.Progress = progress;
            this.LogFunc = logFunc;
        }

        public void Import() {

            if (Progress != null) {
                Progress.ProgressStart("Initialising...");
            }

            if (!InitImport()) {
                return;
            }

            ProgressMsg("Importing rows - Stage 1", 0);
            if (!DoStage1()) {
                return;
            }

            ProgressMsg("Importing rows - Stage 2", 10);
            int rowCount = 0;
            while (RowSource.MoveNext()) {
                rowCount++;
                var percent = (double) ((double) RowSource.RowCount / (double) rowCount) * 90;

                if (rowCount % 100 == 0) {
                    ProgressMsg("Importing rows - Stage 2", percent + 10);
                }

                ImportCurrentRow();
            }




        }

        private void ImportCurrentRow() {
        }

        private bool DoStage1() {

            LogMsg("Stage 1 - preparing staging database");
            LogMsg("Stage 1 - Preprocessing rows...");
            this.RowSource = Importer.CreateRowSource();
            LogMsg("Stage 1 Complete, {0} rows staged for import.", RowSource.RowCount);

            return true;
        }

        private bool InitImport() {
            LogMsg("Initializing...");

            return true;
        }

        private void ProgressMsg(string message, double? percent = null) {
            if (Progress != null) {
                Progress.ProgressMessage(message, percent);
            }
        }

        private void LogMsg(String format, params object[] args) {
            if (LogFunc != null) {
                if (args.Length > 0) {
                    LogFunc(string.Format(format, args));
                } else {
                    LogFunc(format);
                }
            }
        }

        protected IProgressObserver Progress { get; private set; }

        protected Action<string> LogFunc { get; private set; }

        protected TabularDataImporter Importer { get; private set; }

        protected List<ImportFieldMapping> Mappings { get; private set; }

        protected ImportRowSource RowSource { get; private set; }

        public bool Cancelled { get; set; }

    }

}
