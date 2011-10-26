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
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ControlHostWindow.xaml
    /// </summary>
    public partial class ControlHostWindow : ChangeContainerWindow {

        private Action<SelectionResult> _selectionCallback;

        #region DesignerConstructor
        public ControlHostWindow() {
            InitializeComponent();
        }
        #endregion

        public ControlHostWindow(User user, Control element, SizeToContent sizeToContent, Boolean hideButtonBar = false)
            : base(user) {
            InitializeComponent();

            if (element.MinWidth > 0) {
                this.MinWidth = element.MinWidth + 15;
            }

            if (element.MinHeight > 0) {
                this.MinHeight = element.MinHeight + 15;
            }

            this.Control = element;
            this.SizeToContent = sizeToContent;
            ControlHost.Children.Add(element);
            if (hideButtonBar) {
                buttonBar.Visibility = System.Windows.Visibility.Collapsed;
                grid.RowDefinitions[1].Height = new GridLength(0);
            }

            this.ChangeRegistered += new PendingChangedRegisteredHandler((source, a) => {
                btnApply.IsEnabled = true;
            });

            this.ChangesCommitted += new PendingChangesCommittedHandler((source) => {
                btnApply.IsEnabled = false;
            });

            btnSelect.Click += new RoutedEventHandler(btnSelect_Click);

            this.Closed += new EventHandler(ControlHostWindow_Closed);
        }

        void ControlHostWindow_Closed(object sender, EventArgs e) {
            if (Control != null) {
                if (Control is IDisposable) {
                    (Control as IDisposable).Dispose();
                }

                ControlHost.Children.Remove(Control);
                Control = null;
            }
           
        }

        void btnSelect_Click(object sender, RoutedEventArgs e) {
            DoSelect();
        }

        private void DoSelect() {
            if (_selectionCallback != null && Control is ISelectionHostControl) {
                var ctl = Control as ISelectionHostControl;
                var result = ctl.Select();
                if (result != null) {
                    _selectionCallback(result);
                }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!RequestClose()) {
                e.Cancel = true;
            }
        }

        public void AddCustomButton(FrameworkElement element) {
            element.Margin = new Thickness(3, 0, 3, 0);
            CustomButtonBar.Children.Add(element);
        }

        public bool RequestClose() {
            if (HasPendingChanges) {
                return this.DiscardChangesQuestion();
            }
            return true;
        }

        public void ApplyChanges() {

            if (Control is DatabaseCommandControl) {
                var dac = Control as DatabaseCommandControl;
                var messages = new List<string>();
                if (!dac.Validate(messages)) {
                    var message = String.Format("Changes could not be applied because:\n\n{0}\n", messages.Join("\n"));
                    ErrorMessage.Show(message);
                    return;
                }
            }
            if (HasPendingChanges) {
                using (new OverrideCursor(Cursors.Wait)) {
                    CommitPendingChanges();
                }
            } else {
                btnApply.IsEnabled = false;
            }

        }

        public void Dispose() {
            if (Control != null && Control is IDisposable) {
                (Control as IDisposable).Dispose();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
            try {
                this.DialogResult = true;
            } catch (InvalidOperationException) {
                // this is kind of crap, but you can only set DialogResult if ShowDialog was used to show the Window. 
                // I don'note know how to tell if the ShowDialog method was used (no obvious property), so for now
                // I'll catch the exception and ignore it
            }
            this.Close();
        }

        public void BindSelectCallback(Action<SelectionResult> selectionFunc) {
            if (selectionFunc != null) {
                btnSelect.Visibility = Visibility.Visible;
                btnSelect.IsEnabled = true;
                _selectionCallback = selectionFunc;
            } else {
                ClearSelectCallback();
            }

        }

        public void ClearSelectCallback() {
            _selectionCallback = null;
            btnSelect.Visibility = Visibility.Hidden;
        }

        #region Properties

        public Control Control { get; private set; }

        #endregion

        private void btnDebug_Click(object sender, RoutedEventArgs e) {
            if (btnDebug.IsChecked.ValueOrFalse()) {
                lstPendingChanges.ItemsSource = PendingChanges;
                grid.RowDefinitions[2].Height = new GridLength(200);
            } else {
                grid.RowDefinitions[2].Height = new GridLength(0);
            }
        }

    }

}
