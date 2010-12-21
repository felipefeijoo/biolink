using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using BioLink.Data;
using System;

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

        protected string Escape(string str) {
            return System.Security.SecurityElement.Escape(str);
        }

        protected bool FileExistsAndNotOverwrite(string filename) {
            FileInfo file = new FileInfo(filename);
            if (file.Exists) {

                bool retval = false;
                PluginManager.Instance.ParentWindow.InvokeIfRequired(() => {                    
                    retval = PluginManager.Instance.ParentWindow.Question(string.Format("The file {0} already exists. Do you wish to overwrite it?", file.FullName), "Overwrite existing file?");
                });

                if (!retval) {
                    return true;
                }

                
                if (file.IsReadOnly) {
                    ErrorMessage.Show("{0} is not writable. Please ensure that it is not marked as read-only and that you have sufficient priviledges to write to it before trying again", file.FullName);
                    return true;
                }

                try {
                    file.Delete();
                } catch (Exception) {
                    ErrorMessage.Show("{0} could not be deleted. Please ensure that it is not marked as read-only and that you have sufficient priviledges to write to it before trying again", file.FullName);
                    return true;
                }
            }

            return false;
        }


        protected abstract object GetOptions(Window parentWindow);

        public abstract void ExportImpl(Window parentWindow, DataMatrix matrix, object options);

        public abstract void Dispose();

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

        #region Properties

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract BitmapSource Icon { get; }

        #endregion

    }
}
