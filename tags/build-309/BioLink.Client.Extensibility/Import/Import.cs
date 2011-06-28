using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Client.Extensibility.Import;

namespace BioLink.Client.Extensibility {

    public abstract class TabularDataImporter : IBioLinkExtension {

        public abstract bool GetOptions(Window parentWindow, ImportWizardContext context);

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
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Import"; // Default file name
            dlg.DefaultExt = extension; // Default file extension            
            dlg.Filter = filter + "|All files (*.*)|*.*"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {
                return dlg.FileName;
            }

            return null;
        }

        public abstract ImportRowSource CreateRowSource();

        #region Properties

        public IProgressObserver ProgressObserver { get; set; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract BitmapSource Icon { get; }

        #endregion


        public void Dispose() {            
        }

        public string CreateProfileString() {
            var ep = new EntryPoint("ImportProfile");
            WriteEntryPoint(ep);
            return ep.ToString();
        }

        public void InitFromProfileString(string profileString) {
            var ep = EntryPoint.Parse(profileString);
            ReadEntryPoint(ep);
        }

        public abstract List<string> GetColumnNames();

        protected abstract void WriteEntryPoint(EntryPoint ep);

        protected abstract void ReadEntryPoint(EntryPoint ep);

    }

    public class ImportFieldMapping {

        public string SourceColumn { get; set; }
        public string TargetColumn { get; set; }
        public string DefaultValue { get; set; }
        public bool IsFixed { get; set; }

    }

    public class ImportFieldMappingViewModel : GenericViewModelBase<ImportFieldMapping> {

        public ImportFieldMappingViewModel(ImportFieldMapping model) : base(model, ()=>0) { }

        public override string DisplayLabel {
            get {
                if (IsFixed) {
                    return string.Format("\"{0}\"", DefaultValue);
                } else {
                    return SourceColumn;
                }
            }
        }

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

        public bool IsFixed {
            get { return Model.IsFixed; }
            set { SetProperty(() => Model.IsFixed, value); }
        }

    }

}
