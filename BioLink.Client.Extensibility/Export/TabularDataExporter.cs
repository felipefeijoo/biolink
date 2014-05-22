/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using BioLink.Data;
using System;
using System.Windows.Input;

namespace BioLink.Client.Extensibility {

    public abstract class TabularDataExporter : IBioLinkExtension {

        public TabularDataExporter() {
        }

        public IProgressObserver ProgressObserver { get; set; }

        public void Export(Window parentWindow, DataMatrix matrix, String datasetName, IProgressObserver progress) {
            this.ProgressObserver = progress;
            object options = GetOptions(parentWindow, matrix);
            JobExecutor.QueueJob(() => {
                this.ExportImpl(parentWindow, matrix, datasetName, options);
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

        public abstract bool CanExport(DataMatrix matrix);

        protected abstract object GetOptions(Window parentWindow, DataMatrix matrix);

        public abstract void ExportImpl(Window parentWindow, DataMatrix matrix, String datasetName, object options);

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

        #region Properties

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract BitmapSource Icon { get; }

        #endregion

    }
}
