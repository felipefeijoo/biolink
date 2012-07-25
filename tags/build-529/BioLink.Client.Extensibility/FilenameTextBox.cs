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
using Microsoft.Win32;

namespace BioLink.Client.Extensibility {

    public class FilenameTextBox : EllipsisTextBox {

        public FilenameTextBox() : base() {
            this.Click += new System.Windows.RoutedEventHandler(FilenameTextBox_Click);
            this.Mode = FilenameTextBoxMode.Open;
            this.Filter = "All files (*.*)|*.*";
        }

        void FilenameTextBox_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (Mode == FilenameTextBoxMode.Open) {
                var dlg = new OpenFileDialog();
                dlg.Filter = Filter;
                if (dlg.ShowDialog().GetValueOrDefault(false)) {
                    this.txt.Text = dlg.FileName;
                    FireFileSelected(dlg.FileName);
                }                
            } else {
                var dlg = new SaveFileDialog();
                dlg.Filter = Filter;
                if (dlg.ShowDialog().GetValueOrDefault(false)) {
                    this.txt.Text = dlg.FileName;
                    FireFileSelected(dlg.FileName);
                }
            }
        }

        private void FireFileSelected(string filename) {
            if (FileSelected != null) {
                FileSelected(filename);
            }
        }

        public FilenameTextBoxMode Mode { get; set; }

        public string Filter { get; set; }

        public event Action<string> FileSelected;

    }

    public enum FilenameTextBoxMode {
        Open, Save
    }
}
