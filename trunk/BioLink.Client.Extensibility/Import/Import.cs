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

        public abstract ImportRowSource CreateRowSource(IProgressObserver progress);

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

}
