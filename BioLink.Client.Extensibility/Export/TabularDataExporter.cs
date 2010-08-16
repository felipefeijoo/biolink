using BioLink.Client.Utilities;
using BioLink.Data;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Extensibility {

    public abstract class TabularDataExporter : IBioLinkExtension {

        public TabularDataExporter() {
        }

        public IProgressObserver ProgressObserver { get; set; }

        public void Export(Window parentWindow, DataMatrix matrix, IProgressObserver progress) {
            this.ProgressObserver = progress;
            object options = GetOptions(parentWindow);
            JobExecutor.QueueJob(() => {
                this.ExportImpl(parentWindow, matrix, options);
                ProgressEnd("");
            });

        }

        protected abstract object GetOptions(Window parentWindow);

        public abstract void ExportImpl(Window parentWindow, DataMatrix matrix, object options);

        public abstract void Dispose();

        protected void ProgressStart(string message, bool indeterminate = false) {
            if (ProgressObserver != null) {
                ProgressObserver.ProgressStart(message, indeterminate);
            }
        }

        protected void ProgressMessage(string message, double percent) {
            if (ProgressObserver != null) {
                ProgressObserver.ProgressMessage(message, percent);
            }
        }

        protected void ProgressEnd(string message) {
            if (ProgressObserver != null) {
                ProgressObserver.ProgressEnd(message);                
            }
        }

        #region Properties

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract BitmapSource Icon { get; }

        #endregion

    }
}
