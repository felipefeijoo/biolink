using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public abstract class TabularDataImporter : IBioLinkExtension {

        public abstract Object GetOptions(Window parentWindow);

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
                ImportRow(rs.CurrentRow);
            }
        }

        private void ImportRow(ImportRow row) {

        }

        protected abstract ImportRowSource CreateRowSource(Object options);

        #region Properties

        public IProgressObserver ProgressObserver { get; set; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract BitmapSource Icon { get; }

        #endregion


        public void Dispose() {            
        }
    }

    public interface ImportRowSource {
        
        bool MoveNext();
        ImportRow CurrentRow { get; }
        string this[int index] { get; }
        int this[string columnname] { get; }
        int? RowCount { get; }
    }

    public interface ImportRow {
        Object this[int index] { get; }
    }

}
