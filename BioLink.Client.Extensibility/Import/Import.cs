using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public abstract class TabularDataImporter : IBioLinkExtension {

        public abstract ImporterOptions GetOptions(Window parentWindow);

        protected void ProgressStart(string message, bool indeterminate = false) {
            if (ProgressObserver != null) {
                ProgressObserver.ProgressStart(message, indeterminate);
            }
        }

        protected void ProgressMessage(string message, double? percent = null) {
            if (ProgressObserver != null) {
                ProgressObserver.ProgressMessage(message, percent);
            }
        }

        protected void ProgressEnd(string message) {
            if (ProgressObserver != null) {
                ProgressObserver.ProgressEnd(message);
            }
        }

        protected string PromptForFilename(string extension, string filter) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Export"; // Default file name
            dlg.DefaultExt = extension; // Default file extension
            dlg.OverwritePrompt = false;
            dlg.Filter = filter + "|All files (*.*)|*.*"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {
                return dlg.FileName;
            }

            return null;
        }

        public void Import(Object options) {

            var rs = CreateRowSource(options);
            while (rs.MoveNext()) {
                ImportRow(rs);
            }
        }

        private void ImportRow(ImportRowSource row) {

        }

        public abstract ImportRowSource CreateRowSource(Object options);

        #region Properties

        public IProgressObserver ProgressObserver { get; set; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract BitmapSource Icon { get; }

        #endregion


        public void Dispose() {            
        }
    }

    public abstract class ImporterOptions {

        public abstract List<String> ColumnNames { get; set; }

    }


    public interface ImportRowSource {        
        bool MoveNext();        
        string ColumnName(int index);
        object this[int index] { get; }
        int? ColumnCount { get; }
        int? RowCount { get; }
    }

    public class ImportFieldMapping {

        public String SourceColumn { get; set; }
        public String TargetColumn { get; set; }
        public object DefaultValue { get; set; }

    }

    public class ImportFieldMappingViewModel : GenericViewModelBase<ImportFieldMapping> {

        public ImportFieldMappingViewModel(ImportFieldMapping model) : base(model, ()=>0) { }

        public String SourceColumn {
            get { return Model.SourceColumn; }
            set { SetProperty(() => Model.SourceColumn, value); }
        }

        public String TargetColumn {
            get { return Model.TargetColumn; }
            set { SetProperty(() => Model.TargetColumn, value); }
        }

        public object DefaultValue {
            get { return Model.DefaultValue; }
            set { SetProperty(() => Model.DefaultValue, value); }
        }

    }

}
