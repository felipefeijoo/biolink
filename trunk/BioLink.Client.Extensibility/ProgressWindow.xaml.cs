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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window, IProgressObserver, IDisposable {

        public ProgressWindow() {
            InitializeComponent();
        }

        public ProgressWindow(Window parentWindow, String message, bool indeterminate = false) {
            InitializeComponent();
            this.Owner = parentWindow;
            this.Title = "Please wait...";
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;            
            ProgressStart(message, indeterminate);
        }

        public void ProgressStart(string message, bool indeterminate = false) {
            this.InvokeIfRequired(() => {                
                lblProgress.Content = message;                
                progressBar.IsIndeterminate = indeterminate;
                this.Show();
            });
        }

        public void ProgressMessage(string message, double? percentComplete) {
            this.InvokeIfRequired(() => {
                lblProgress.Content = message;
                if (!progressBar.IsIndeterminate && percentComplete.HasValue) {
                    progressBar.Value = percentComplete.Value;
                }
            });
        }

        public void ProgressEnd(string message) {
            this.InvokeIfRequired(() => {
                lblProgress.Content = message;
                if (!progressBar.IsIndeterminate) {
                    progressBar.Value = 100;
                }
                this.Hide();
            });
        }


        public void Dispose() {
            if (this.IsVisible) {
                this.Hide();
            }
        }
    }
}
