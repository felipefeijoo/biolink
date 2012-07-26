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
using System.Windows;
using System.Windows.Controls;
using BioLink.Client.Utilities;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for PluginLoaderWindow.xaml
    /// </summary>
    public partial class PluginLoaderWindow : Window, IProgressObserver {
        public PluginLoaderWindow() {
            InitializeComponent();
            progressBar.SetValue(ProgressBar.MinimumProperty, 0.0);
            progressBar.SetValue(ProgressBar.MaximumProperty, 100.0);
        }

        public void ProgressStart(string message, bool indeterminate) {
            lblProgress.Content = message;
        }

        public void ProgressMessage(string message, double? percentComplete) {
            lblProgress.Content = message;
            if (percentComplete.HasValue) {
                progressBar.Value = percentComplete.Value;                
            }
        }

        public void ProgressEnd(string message) {
            lblProgress.Content = message;
        }

    }
}
